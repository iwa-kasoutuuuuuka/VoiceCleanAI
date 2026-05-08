using Microsoft.UI.Xaml;
using System.IO;

namespace VoiceCleanAI;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        try
        {
            InitializeComponent();

            ExtendsContentIntoTitleBar = true;
            if (AppTitleBar != null)
            {
                SetTitleBar(AppTitleBar);
            }

            // アイコンの設定（失敗しても続行）
            try 
            { 
                AppWindow.SetIcon(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "StoreLogo.png")); 
            } 
            catch { }

            RootFrame.Navigate(typeof(MainPage));
        }
        catch (Exception ex)
        {
            string crashPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "startup_crash.txt");
            File.WriteAllText(crashPath, ex.ToString());
            throw;
        }
    }
}
