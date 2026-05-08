using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using VoiceCleanAI.ViewModels;

namespace VoiceCleanAI;

public sealed partial class MainPage : Page
{
    public MainPageViewModel ViewModel { get; } = new();

    public MainPage()
    {
        InitializeComponent();
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
        e.DragUIOverride.Caption = "ファイルを処理リストに追加";
        e.DragUIOverride.IsCaptionVisible = true;
        e.DragUIOverride.IsContentVisible = true;
    }

    private async void OnDrop(object sender, DragEventArgs e)
    {
        if (e.DataView.Contains(StandardDataFormats.StorageItems))
        {
            var items = await e.DataView.GetStorageItemsAsync();
            foreach (var item in items)
            {
                if (item is StorageFile file)
                {
                    ViewModel.AddTaskCommand.Execute(file.Path);
                }
                else if (item is StorageFolder folder)
                {
                    // Recursive folder scan can be added here
                    var files = await folder.GetFilesAsync();
                    foreach (var f in files)
                    {
                        ViewModel.AddTaskCommand.Execute(f.Path);
                    }
                }
            }
        }
    }
}
