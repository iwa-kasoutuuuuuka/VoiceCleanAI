using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace VoiceCleanAI.Launcher;

public static class Program
{
    [STAThread]
    static void Main(string[] args)
    {
        string baseDir = AppDomain.CurrentDomain.BaseDirectory;
        string appPath = Path.Combine(baseDir, "app", "VoiceCleanAI.exe");

        if (!File.Exists(appPath))
        {
            return;
        }

        ProcessStartInfo startInfo = new ProcessStartInfo(appPath)
        {
            WorkingDirectory = Path.Combine(baseDir, "app"),
            UseShellExecute = false
        };

        // 引数があれば引き継ぐ
        foreach (var arg in args)
        {
            startInfo.ArgumentList.Add(arg);
        }

        try
        {
            Process.Start(startInfo);
        }
        catch (Exception)
        {
            // 起動失敗時の処理
        }
    }
}
