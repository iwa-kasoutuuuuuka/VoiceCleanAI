using System.Collections.Concurrent;
using VoiceCleanAI.Core.Models;
using VoiceCleanAI.ViewModels;
using VoiceCleanAI.Services;
using ModelTaskStatus = VoiceCleanAI.Core.Models.TaskStatus;

namespace VoiceCleanAI.Backend;

public class InferenceManager
{
    private readonly BackendWorkerService _workerService;
    private readonly LogService _logService;
    private readonly ConcurrentQueue<(TaskViewModel ViewModel, ProcessingPreset Preset)> _taskQueue = new();
    private CancellationTokenSource? _cts;
    private bool _isProcessing;

    public event EventHandler<double>? GlobalProgressChanged;
    public event EventHandler<bool>? ProcessingStateChanged;

    public InferenceManager(BackendWorkerService workerService, LogService logService)
    {
        _workerService = workerService;
        _logService = logService;
    }

    public void EnqueueTasks(IEnumerable<TaskViewModel> tasks, ProcessingPreset preset)
    {
        foreach (var task in tasks)
        {
            if (task.Status == ModelTaskStatus.Queued || task.Status == ModelTaskStatus.Failed)
            {
                task.Status = ModelTaskStatus.Queued;
                task.UpdateFromModel();
                _taskQueue.Enqueue((task, preset));
            }
        }
    }

    public async Task StartProcessingAsync()
    {
        if (_isProcessing) return;

        _isProcessing = true;
        ProcessingStateChanged?.Invoke(this, true);
        _cts = new CancellationTokenSource();

        try
        {
            int totalTasks = _taskQueue.Count;
            int completedTasks = 0;

            while (_taskQueue.TryDequeue(out var item))
            {
                var taskViewModel = item.ViewModel;
                var preset = item.Preset;

                if (_cts.Token.IsCancellationRequested) break;

                taskViewModel.Status = ModelTaskStatus.Processing;
                taskViewModel.UpdateFromModel();
                _logService.Log($"Processing task: {taskViewModel.FileName}");

                try
                {
                    string inputPath = taskViewModel.FullFilePath;
                    string outputDir = Path.GetDirectoryName(inputPath) ?? "";
                    string fileName = Path.GetFileNameWithoutExtension(inputPath);
                    string ext = Path.GetExtension(inputPath);
                    string outputPath = Path.Combine(outputDir, $"{fileName}_clean{ext}");

                    var audioTask = new AudioTask
                    {
                        InputFilePath = inputPath,
                        OutputFilePath = outputPath,
                        Lambd = preset.Lambd,
                        Tau = preset.Tau,
                        Nfe = preset.Nfe
                    };

                    var progressHandler = new Progress<double>(p =>
                    {
                        taskViewModel.ProgressValue = p;
                        taskViewModel.UpdateFromModel();
                    });

                    await _workerService.ProcessTaskAsync(audioTask, _cts.Token, progressHandler);

                    taskViewModel.Status = ModelTaskStatus.Completed;
                    taskViewModel.ProgressValue = 1.0;
                    _logService.Log($"Task completed successfully: {taskViewModel.FileName}");
                }
                catch (OperationCanceledException)
                {
                    taskViewModel.Status = ModelTaskStatus.Cancelled;
                    _logService.Log($"Task cancelled: {taskViewModel.FileName}", "WARN");
                }
                catch (Exception ex)
                {
                    taskViewModel.Status = ModelTaskStatus.Failed;
                    taskViewModel.StatusText = $"エラー: {ex.Message}";
                    _logService.Log($"Task failed: {taskViewModel.FileName} Error: {ex.Message}", "ERROR");
                }
                finally
                {
                    taskViewModel.UpdateFromModel();
                    completedTasks++;
                    GlobalProgressChanged?.Invoke(this, (double)completedTasks / totalTasks);
                }
            }
        }
        finally
        {
            _isProcessing = false;
            ProcessingStateChanged?.Invoke(this, false);
            GlobalProgressChanged?.Invoke(this, 1.0);
            _logService.Log("Batch processing finished.");
        }
    }

    public void Cancel()
    {
        _logService.Log("Cancellation requested by user.");
        _cts?.Cancel();
    }
}
