using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System;
using VoiceCleanAI.ViewModels;
using VoiceCleanAI.Pages;

namespace VoiceCleanAI;

public sealed partial class MainWindow : Window
{
    public static MainWindow Instance { get; private set; } = null!;

    public MainWindow()
    {
        Instance = this;
        InitializeComponent();

        // 初期ページを表示
        ContentFrame.Navigate(typeof(MainPage));
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            // 設定ページへ（未実装の場合はプレースホルダ）
            sender.Header = "設定 (Settings)";
        }
        else if (args.SelectedItemContainer != null)
        {
            string tag = args.SelectedItemContainer.Tag.ToString() ?? "";
            sender.Header = args.SelectedItemContainer.Content;

            if (tag == "Home")
            {
                ContentFrame.Navigate(typeof(MainPage));
            }
            else if (tag == "Hardware")
            {
                ContentFrame.Navigate(typeof(HardwarePage));
            }
        }
    }
}
