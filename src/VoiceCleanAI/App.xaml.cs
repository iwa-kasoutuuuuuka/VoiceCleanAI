using Microsoft.UI.Xaml;
using System.IO;

namespace VoiceCleanAI;

public partial class App : Application
{
    public App()
    {
        // ログファイルの準備（絶対パスで確実に記録）
        string logDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs");
        if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);
        string startupLog = Path.Combine(logDir, "startup_trace.txt");
        File.AppendAllText(startupLog, $"[{DateTime.Now}] App Constructor started\n");

        try
        {
            this.UnhandledException += (s, e) => {
                File.AppendAllText(startupLog, $"[{DateTime.Now}] Unhandled Exception: {e.Message}\n{e.Exception}\n");
            };

            File.AppendAllText(startupLog, $"[{DateTime.Now}] Initializing Component...\n");
            this.InitializeComponent();
            File.AppendAllText(startupLog, $"[{DateTime.Now}] InitializeComponent finished\n");
        }
        catch (Exception ex)
        {
            File.AppendAllText(startupLog, $"[{DateTime.Now}] CRITICAL ERROR in App Constructor: {ex}\n");
            throw;
        }
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        string startupLog = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "startup_trace.txt");
        File.AppendAllText(startupLog, $"[{DateTime.Now}] OnLaunched started\n");

        try
        {
            m_window = new MainWindow();
            m_window.Activate();
            File.AppendAllText(startupLog, $"[{DateTime.Now}] MainWindow activated\n");
        }
        catch (Exception ex)
        {
            File.AppendAllText(startupLog, $"[{DateTime.Now}] CRITICAL ERROR in OnLaunched: {ex}\n");
            throw;
        }
    }

    public static Window? MainWindow => ((App)Application.Current).m_window;
    public static VoiceCleanAI.ViewModels.MainPageViewModel ViewModel { get; } = new();
    private Window? m_window;
}
