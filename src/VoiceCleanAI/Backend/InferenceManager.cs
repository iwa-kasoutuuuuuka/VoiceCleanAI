using System.Collections.Concurrent;
using System.Diagnostics;
using VoiceCleanAI.Core.Models;
using VoiceCleanAI.Services;
using VoiceCleanAI.ViewModels;
using ModelTaskStatus = VoiceCleanAI.Core.Models.TaskStatus;

namespace VoiceCleanAI.Backend;

public class InferenceManager
{
    private readonly OnnxInferenceService _onnxService;
    private readonly LogService _logService;
    private readonly ConcurrentQueue<TaskViewModel> _taskQueue = new();
    private CancellationTokenSource? _cts;
    private bool _isProcessing;

    public event EventHandler<bool>? ProcessingStateChanged;
    public event EventHandler<double>? GlobalProgressChanged;

    public InferenceManager(OnnxInferenceService onnxService, LogService logService)
    {
        _onnxService = onnxService;
        _logService = logService;
    }

    public void EnqueueTasks(IEnumerable<TaskViewModel> tasks, ProcessingPreset preset, int outputMode, string customDir)
    {
        foreach (var taskViewModel in tasks)
        {
            if (taskViewModel.Status == ModelTaskStatus.Queued || taskViewModel.Status == ModelTaskStatus.Failed)
            {
                taskViewModel.Status = ModelTaskStatus.Queued;
                taskViewModel.Model.ErrorMessage = string.Empty;
                taskViewModel.ProgressValue = 0;
                
                // プリセット値をタスクに適用
                taskViewModel.Model.Lambd = (float)preset.Lambd;
                taskViewModel.Model.Tau = (float)preset.Tau;
                taskViewModel.Model.Nfe = preset.Nfe;

                // 出力パスの設定
                string fileName = Path.GetFileNameWithoutExtension(taskViewModel.Model.InputFilePath);
                string ext = Path.GetExtension(taskViewModel.Model.InputFilePath);
                
                string targetDir;
                if (outputMode == 1 && !string.IsNullOrEmpty(customDir))
                {
                    targetDir = customDir;
                    if (!Directory.Exists(targetDir)) Directory.CreateDirectory(targetDir);
                }
                else
                {
                    targetDir = Path.GetDirectoryName(taskViewModel.Model.InputFilePath) ?? "";
                }

                taskViewModel.Model.OutputFilePath = Path.Combine(targetDir, $"{fileName}_clean{ext}");

                _taskQueue.Enqueue(taskViewModel);
            }
        }
    }

    public bool AutoOpenFolder { get; set; } = true;

    public async Task StartProcessingAsync()
    {
        if (_isProcessing) return;
        _isProcessing = true;
        ProcessingStateChanged?.Invoke(this, true);

        _cts = new CancellationTokenSource();
        int totalTasks = _taskQueue.Count;
        int completedTasks = 0;
        string lastOutputFilePath = string.Empty;

        try
        {
            while (_taskQueue.TryDequeue(out var taskViewModel))
            {
                if (_cts.Token.IsCancellationRequested)
                {
                    taskViewModel.Status = ModelTaskStatus.Cancelled;
                    continue;
                }

                _logService.Log($"Processing task: {taskViewModel.FileName}");
                taskViewModel.Status = ModelTaskStatus.Processing;

                try
                {
                    var progress = new Progress<double>(p => {
                        taskViewModel.ProgressValue = p;
                        double totalProgress = (completedTasks + p) / totalTasks;
                        GlobalProgressChanged?.Invoke(this, totalProgress);
                    });

                    await _onnxService.ProcessAsync(taskViewModel.Model, _cts.Token, progress);
                    
                    lastOutputFilePath = taskViewModel.Model.OutputFilePath;
                    taskViewModel.Status = ModelTaskStatus.Completed;
                    taskViewModel.ProgressValue = 1.0;
                }
                catch (Exception ex)
                {
                    _logService.Log($"Task failed: {taskViewModel.FileName} Error: {ex.Message}", "ERROR");
                    taskViewModel.Status = ModelTaskStatus.Failed;
                    taskViewModel.Model.ErrorMessage = ex.Message;
                }

                completedTasks++;
                GlobalProgressChanged?.Invoke(this, (double)completedTasks / totalTasks);
            }
        }
        finally
        {
            _isProcessing = false;
            ProcessingStateChanged?.Invoke(this, false);
            _logService.Log("Batch processing finished.");

            // 完了時に最後のタスクのフォルダを開く (設定が有効な場合)
            if (completedTasks > 0 && AutoOpenFolder)
            {
                try
                {
                    string? lastOutput = Path.GetDirectoryName(lastOutputFilePath);
                    if (!string.IsNullOrEmpty(lastOutput) && Directory.Exists(lastOutput))
                    {
                        Process.Start("explorer.exe", lastOutput);
                    }
                }
                catch { /* 無視 */ }
            }

            _cts?.Dispose();
            _cts = null;
        }
    }

    public void Cancel()
    {
        _cts?.Cancel();
        _logService.Log("Processing cancelled by user.", "WARN");
    }
}
