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
        ApplyTheme();
        RootFrame.Navigate(typeof(MainPage));
    }

    public void Reload()
    {
        // InitializeComponent() の再呼び出しは WinUI 3 では不正な動作やクラッシュの原因となります。
        // テーマの再適用と、ページの再ナビゲーションのみを行います。
        ApplyTheme();
        // 言語切り替え後はメインページを表示
        RootFrame.Navigate(typeof(MainPage));
    }

    private void ApplyTheme()
    {
        if (RootElement != null)
        {
            RootElement.RequestedTheme = App.CurrentTheme;
        }
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            sender.Header = "設定 (Settings)";
            RootFrame.Navigate(typeof(SettingsPage));
            return;
        }

        if (args.SelectedItemContainer != null)
        {
            string tag = args.SelectedItemContainer.Tag.ToString() ?? "";
            sender.Header = args.SelectedItemContainer.Content;

            if (tag == "Home")
            {
                RootFrame.Navigate(typeof(MainPage));
            }
            else if (tag == "Hardware")
            {
                RootFrame.Navigate(typeof(HardwarePage));
            }
        }
    }
}
