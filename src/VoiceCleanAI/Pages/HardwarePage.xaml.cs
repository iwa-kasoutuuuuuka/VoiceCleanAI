using Microsoft.UI.Xaml.Controls;
using VoiceCleanAI.ViewModels;

namespace VoiceCleanAI.Pages;

public sealed partial class HardwarePage : Page
{
    public MainPageViewModel ViewModel => App.ViewModel;

    public HardwarePage()
    {
        InitializeComponent();
        this.DataContext = this;
    }
}
