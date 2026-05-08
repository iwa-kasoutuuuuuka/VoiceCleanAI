using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VoiceCleanAI.Core.Models;
using VoiceCleanAI.Backend;
using VoiceCleanAI.Services;
using ModelTaskStatus = VoiceCleanAI.Core.Models.TaskStatus;

namespace VoiceCleanAI.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    private readonly BackendWorkerService _backendWorker;
    private readonly InferenceManager _inferenceManager;
    private readonly HardwareService _hardwareService;
    private readonly LogService _logService;

    [ObservableProperty]
    public partial string Greeting { get; set; } = "VoiceClean AI";

    [ObservableProperty]
    public partial bool IsProcessing { get; set; }

    [ObservableProperty]
    public partial double GlobalProgress { get; set; }

    [ObservableProperty]
    public partial HardwareInfo? CurrentHardware { get; set; }

    [ObservableProperty]
    public partial string HardwareStatusText { get; set; } = "ハードウェアを診断中...";

    [ObservableProperty]
    public partial ProcessingPreset? SelectedPreset { get; set; }

    // Advanced Mode Properties
    [ObservableProperty]
    public partial bool IsAdvancedMode { get; set; }

    [ObservableProperty]
    public partial float ManualLambd { get; set; } = 0.5f;

    [ObservableProperty]
    public partial float ManualTau { get; set; } = 0.5f;

    [ObservableProperty]
    public partial int ManualNfe { get; set; } = 64;

    [ObservableProperty]
    public partial string SelectedEncoder { get; set; } = "Auto";

    public ObservableCollection<TaskViewModel> Tasks { get; } = new();
    public ObservableCollection<ProcessingPreset> Presets { get; } = new();
    public ObservableCollection<string> Encoders { get; } = new() { "Auto", "NVENC (NVIDIA)", "QSV (Intel)", "AMF (AMD)", "libx264 (CPU)" };

    public MainPageViewModel()
    {
        _logService = new LogService();
        _logService.Log("VoiceClean AI started.");

        string scriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Backend", "main.py");
        _backendWorker = new BackendWorkerService(scriptPath);
        _inferenceManager = new InferenceManager(_backendWorker, _logService);
        _hardwareService = new HardwareService(scriptPath);

        _inferenceManager.ProcessingStateChanged += (s, processing) => IsProcessing = processing;
        _inferenceManager.GlobalProgressChanged += (s, progress) => GlobalProgress = progress;

        foreach (var p in ProcessingPreset.GetDefaultPresets()) Presets.Add(p);
        SelectedPreset = Presets[0];

        _ = RunDiagnosticsAsync();
    }

    private async Task RunDiagnosticsAsync()
    {
        CurrentHardware = await _hardwareService.GetHardwareInfoAsync();
        HardwareStatusText = $"Backend: {CurrentHardware.BackendDisplayName} | {CurrentHardware.GpuName} ({CurrentHardware.VramGB:F1} GB VRAM)";
        _logService.Log($"Hardware Diagnostics: {HardwareStatusText}");
        
        // Auto-select encoder based on hardware
        if (CurrentHardware.HasCuda) SelectedEncoder = "NVENC (NVIDIA)";
    }

    [RelayCommand]
    private void AddTask(string filePath)
    {
        var validation = FileValidator.Validate(filePath);
        if (!validation.IsValid)
        {
            _logService.Log($"Validation failed for {filePath}: {validation.ErrorMessage}", "WARN");
            return;
        }

        var task = new AudioTask { InputFilePath = filePath };
        Tasks.Add(new TaskViewModel(task));
    }

    [RelayCommand]
    private async Task StartAll()
    {
        if (Tasks.Count == 0) return;
        
        ProcessingPreset effectiveSettings;
        if (IsAdvancedMode)
        {
            effectiveSettings = new ProcessingPreset 
            { 
                Name = "Manual", 
                Lambd = ManualLambd, 
                Tau = ManualTau, 
                Nfe = ManualNfe 
            };
        }
        else
        {
            effectiveSettings = SelectedPreset ?? Presets[0];
        }

        _logService.Log($"Starting processing with preset: {effectiveSettings.Name} (Lambd={effectiveSettings.Lambd}, Tau={effectiveSettings.Tau}, Nfe={effectiveSettings.Nfe}, Encoder={SelectedEncoder})");
        _inferenceManager.EnqueueTasks(Tasks, effectiveSettings);
        await _inferenceManager.StartProcessingAsync();
    }

    [RelayCommand]
    private void CancelAll() => _inferenceManager.Cancel();

    [RelayCommand]
    private void ClearCompleted()
    {
        var completed = Tasks.Where(t => t.Status == ModelTaskStatus.Completed).ToList();
        foreach (var task in completed) Tasks.Remove(task);
    }

    [RelayCommand]
    private void ExportLogs()
    {
        string desktopPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "VoiceCleanAI_Log.txt");
        _logService.ExportLogs(desktopPath);
    }
}
