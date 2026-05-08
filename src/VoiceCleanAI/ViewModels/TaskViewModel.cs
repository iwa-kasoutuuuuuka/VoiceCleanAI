using CommunityToolkit.Mvvm.ComponentModel;
using VoiceCleanAI.Core.Models;
using ModelTaskStatus = VoiceCleanAI.Core.Models.TaskStatus;

namespace VoiceCleanAI.ViewModels;

public partial class TaskViewModel : ObservableObject
{
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
    public partial ModelTaskStatus Status { get; set; }

    [ObservableProperty]
    public partial double ProgressValue { get; set; } // Progress との衝突を避けるため名称変更

    [ObservableProperty]
    public partial string StatusText { get; set; } = string.Empty;

    public void UpdateFromModel()
    {
        FileName = _model.FileName;
        FullFilePath = _model.InputFilePath;
        Status = _model.Status;
        ProgressValue = _model.Progress;
        StatusText = Status switch
        {
            ModelTaskStatus.Queued => "待機中",
            ModelTaskStatus.Processing => $"処理中 ({ProgressValue:P0})",
            ModelTaskStatus.Completed => "完了",
            ModelTaskStatus.Failed => "失敗",
            ModelTaskStatus.Cancelled => "キャンセル済み",
            _ => "不明"
        };
    }
}
