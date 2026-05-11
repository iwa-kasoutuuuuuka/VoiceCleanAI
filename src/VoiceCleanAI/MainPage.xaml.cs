using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using Windows.Storage;
using VoiceCleanAI.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace VoiceCleanAI;

public sealed partial class MainPage : Page
{
    public MainPageViewModel ViewModel => App.ViewModel;

    public MainPage()
    {
        InitializeComponent();
        
        ViewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(MainPageViewModel.CurrentLanguage))
            {
                // 言語が切り替わったらページを再読み込みしてリソースを適用する
                DispatcherQueue.TryEnqueue(() => {
                    this.Frame.Navigate(typeof(MainPage));
                });
            }
        };

        ViewModel.RequestDownloadConfirmation = async () =>
        {
            var dialog = new ContentDialog
            {
                Title = "モデルのダウンロード (Download Models)",
                Content = "AI推論に必要なノイズ除去モデルが見つかりません。今すぐダウンロードしますか？（約30MB）\n\n" +
                          "The required AI models for noise removal were not found. Would you like to download them now? (approx. 30MB)\n\n" +
                          "※高音質化モデル(Enhancer)は手動配置が必要です。 (Note: Enhancer model requires manual placement.)",
                PrimaryButtonText = "はい (Yes)",
                CloseButtonText = "いいえ (No)",
                DefaultButton = ContentDialogButton.Primary,
                XamlRoot = this.Content.XamlRoot ?? this.XamlRoot
            };

            try
            {
                var result = await dialog.ShowAsync();
                return result == ContentDialogResult.Primary;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Dialog failed: {ex.Message}");
                return false;
            }
        };
    }

    private void OnDragOver(object sender, DragEventArgs e)
    {
        e.AcceptedOperation = DataPackageOperation.Copy;
        e.DragUIOverride.Caption = "処理リストに追加 (Add to list)";
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
                    await AddFilesFromFolderRecursively(folder);
                }
            }
        }
    }

    private async Task AddFilesFromFolderRecursively(StorageFolder folder)
    {
        var files = await folder.GetFilesAsync();
        foreach (var file in files)
        {
            ViewModel.AddTaskCommand.Execute(file.Path);
        }

        var subFolders = await folder.GetFoldersAsync();
        foreach (var sub in subFolders)
        {
            await AddFilesFromFolderRecursively(sub);
        }
    }

    private async void OnSelectFilesClick(object sender, RoutedEventArgs e)
    {
        var picker = new Windows.Storage.Pickers.FileOpenPicker();
        picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.List;
        picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.MusicLibrary;
        picker.FileTypeFilter.Add(".wav");
        picker.FileTypeFilter.Add(".mp3");
        picker.FileTypeFilter.Add(".flac");
        picker.FileTypeFilter.Add(".m4a");

        IntPtr hwnd = WinRT.Interop.WindowNative.GetWindowHandle(App.MainWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var files = await picker.PickMultipleFilesAsync();
        foreach (var file in files)
        {
            ViewModel.AddTaskCommand.Execute(file.Path);
        }
    }

    private void OnClearClick(object sender, RoutedEventArgs e)
    {
        ViewModel.ClearAllCommand.Execute(null);
    }

    private void OnProcessClick(object sender, RoutedEventArgs e)
    {
        ViewModel.StartAllCommand.Execute(null);
    }
}
