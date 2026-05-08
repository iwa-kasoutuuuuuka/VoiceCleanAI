using CommunityToolkit.Mvvm.ComponentModel;
using VoiceCleanAI.Core.Models;
using ModelTaskStatus = VoiceCleanAI.Core.Models.TaskStatus;

namespace VoiceCleanAI.ViewModels;

public partial class TaskViewModel : ObservableObject
{
    public AudioTask Model => _model;
    private readonly AudioTask _model;

    public TaskViewModel(AudioTask model)
    {
        _model = model;
        UpdateFromModel();
    }

    [ObservableProperty]
    public partial string FileName { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string FullFilePath { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsProcessing))]
    [NotifyPropertyChangedFor(nameof(IsCompleted))]
    public partial ModelTaskStatus Status { get; set; }

    [ObservableProperty]
    public partial double ProgressValue { get; set; } // Progress との衝突を避けるため名称変更

    [ObservableProperty]
    public partial string StatusText { get; set; } = string.Empty;

    public bool IsQueued => Status == ModelTaskStatus.Queued;
    public bool IsProcessing => Status == ModelTaskStatus.Processing;
    public bool IsCompleted => Status == ModelTaskStatus.Completed;
    public bool IsFailed => Status == ModelTaskStatus.Failed;
    public bool IsCancelled => Status == ModelTaskStatus.Cancelled;

    public void UpdateFromModel()
    {
        FileName = _model.FileName;
        FullFilePath = _model.InputFilePath;
        Status = _model.Status;
        ProgressValue = _model.Progress;
        StatusText = Status switch
        {
            ModelTaskStatus.Queued => "待機中 (Queued)",
            ModelTaskStatus.Processing => $"処理中 (Processing... {ProgressValue:P0})",
            ModelTaskStatus.Completed => "完了 (Completed)",
            ModelTaskStatus.Failed => "失敗 (Failed)",
            ModelTaskStatus.Cancelled => "キャンセル済み (Cancelled)",
            _ => "不明 (Unknown)"
        };
    }
}
