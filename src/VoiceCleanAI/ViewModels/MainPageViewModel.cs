using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using VoiceCleanAI.Core.Models;
using VoiceCleanAI.Backend;
using VoiceCleanAI.Services;
using ModelTaskStatus = VoiceCleanAI.Core.Models.TaskStatus;
using System.IO;

namespace VoiceCleanAI.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    private readonly OnnxInferenceService _onnxService;
    private readonly ModelDownloadService _downloadService;
    private readonly InferenceManager _inferenceManager;
    private readonly HardwareService _hardwareService;
    private readonly LogService _logService;
    private readonly WaveformService _waveformService;

    [ObservableProperty]
    public partial string Greeting { get; set; } = "VoiceClean AI";

    [ObservableProperty]
    public partial bool IsProcessing { get; set; }

    [ObservableProperty]
    public partial double GlobalProgress { get; set; }

    [ObservableProperty]
    public partial HardwareInfo? CurrentHardware { get; set; }

    [ObservableProperty]
    public partial string HardwareStatusText { get; set; } = "ハードウェアを診断中... (Diagnosing hardware...)";

    [ObservableProperty]
    public partial string StatusText { get; set; } = string.Empty;

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

    [ObservableProperty]
    public partial bool AutoOpenFolder { get; set; } = true;

    [ObservableProperty]
    public partial int OutputDirectoryMode { get; set; } = 0; // 0: Original, 1: Specific

    [ObservableProperty]
    public partial string SelectedOutputDirectory { get; set; } = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);

    [ObservableProperty]
    public partial string CurrentLanguage { get; set; } = "ja-JP";

    partial void OnCurrentLanguageChanged(string value)
    {
        Microsoft.Windows.Globalization.ApplicationLanguages.PrimaryLanguageOverride = value;
        
        // プリセットを再読み込みして言語を反映
        LoadPresets();

        // MainWindowをリロードしてサイドバーなどのリソースを反映
        MainWindow.Instance?.Reload();

        _logService.Log($"Language changed to: {value}");
    }

    private void LoadPresets()
    {
        var resourceLoader = new Microsoft.Windows.ApplicationModel.Resources.ResourceLoader();
        Presets.Clear();
        
        // 基本的なプリセットを取得
        var defaults = ProcessingPreset.GetDefaultPresets();
        foreach (var p in defaults)
        {
            // 名前と説明をリソースがあれば上書き
            string key = p.Name.Replace(" ", "").Replace("(", "").Replace(")", "");
            // ※ここでは簡易的に元の名前に基づくキー生成を試みるか、
            // 固定のキーマップを使用するのが安全です。
            
            // 安全な方法として固定の翻訳ロジックを導入
            if (p.Name.Contains("AI Auto")) {
                p.Name = resourceLoader.GetString("PresetAuto");
                p.Description = resourceLoader.GetString("PresetAutoDesc");
            }
            else if (p.Name.Contains("Podcast")) {
                p.Name = resourceLoader.GetString("PresetStudio");
                p.Description = resourceLoader.GetString("PresetStudioDesc");
            }
            // 他のプリセットも同様に...
            
            Presets.Add(p);
        }
        
        if (Presets.Count > 0) SelectedPreset = Presets[0];
    }

    public ObservableCollection<TaskViewModel> Tasks { get; } = new();
    public ObservableCollection<ProcessingPreset> Presets { get; } = new();
    public ObservableCollection<string> Encoders { get; } = new() { "Auto", "NVENC (NVIDIA)", "QSV (Intel)", "AMF (AMD)", "libx264 (CPU)" };

    public MainPageViewModel()
    {
        _logService = new LogService();
        _logService.Log("VoiceClean AI started.");

        string modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Models");
        _onnxService = new OnnxInferenceService(modelPath, _logService);
        _downloadService = new ModelDownloadService(modelPath, _logService);
        _inferenceManager = new InferenceManager(_onnxService, _logService);
        
        _hardwareService = new HardwareService();
        _waveformService = new WaveformService(_logService);

        _inferenceManager.ProcessingStateChanged += (s, processing) => IsProcessing = processing;
        _inferenceManager.GlobalProgressChanged += (s, progress) => GlobalProgress = progress;

        LoadPresets();

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
        var taskVm = new TaskViewModel(task);
        Tasks.Add(taskVm);

        // 非同期で波形を生成
        _ = Task.Run(async () =>
        {
            var data = await _waveformService.GetWaveformDataAsync(filePath, 50);
            taskVm.WaveformData = data;
        });
    }

    [RelayCommand]
    private async Task StartAll()
    {
        _logService.Log($"StartAll triggered. Task count: {Tasks.Count}");
        if (Tasks.Count == 0) return;

        // モデルの存在チェック
        if (!_downloadService.AreModelsAvailable())
        {
            _logService.Log("Models are missing. Requesting download via dialog.");
            
            bool shouldDownload = await ShowDownloadConfirmationAsync();
            if (shouldDownload)
            {
                _logService.Log("Transitioning to download state...");
                IsProcessing = true;
                StatusText = "モデルをダウンロード中... (Downloading models...)";
                
                try
                {
                    var progress = new Progress<double>(p => GlobalProgress = p);
                    await Task.Run(async () => await _downloadService.DownloadModelsAsync(progress));
                    _logService.Log("DownloadModelsAsync completed successfully.");
                    StatusText = "ダウンロード完了 (Download completed)";
                }
                catch (Exception ex)
                {
                    _logService.Log($"CRITICAL: Download failed: {ex}", "ERROR");
                    StatusText = $"失敗 (Failed): {ex.Message}";
                    IsProcessing = false;
                    return;
                }
            }
            else
            {
                _logService.Log("User declined download.");
                return;
            }
        }
        
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
        
        _inferenceManager.AutoOpenFolder = AutoOpenFolder;
        _inferenceManager.EnqueueTasks(Tasks, effectiveSettings, OutputDirectoryMode, SelectedOutputDirectory);
        await _inferenceManager.StartProcessingAsync();
    }

    [RelayCommand]
    private async Task PickOutputDirectory()
    {
        var picker = new Windows.Storage.Pickers.FolderPicker();
        picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary;
        picker.FileTypeFilter.Add("*");

        IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var folder = await picker.PickSingleFolderAsync();
        if (folder != null)
        {
            SelectedOutputDirectory = folder.Path;
        }
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
    private void ClearAll() => Tasks.Clear();

    [RelayCommand]
    private void ExportLogs()
    {
        string desktopPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "VoiceCleanAI_Log.txt");
        _logService.ExportLogs(desktopPath);
    }

    public Func<Task<bool>>? RequestDownloadConfirmation { get; set; }

    private async Task<bool> ShowDownloadConfirmationAsync()
    {
        if (RequestDownloadConfirmation != null)
        {
            return await RequestDownloadConfirmation();
        }
        return false;
    }
}
