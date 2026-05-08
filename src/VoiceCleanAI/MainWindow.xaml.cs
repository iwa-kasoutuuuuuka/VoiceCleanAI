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
        RootFrame.Navigate(typeof(MainPage));
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
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
