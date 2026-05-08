using System.IO;

namespace VoiceCleanAI.Services;

public class LogService
{
    private readonly string _logFilePath;

    public LogService()
    {
        string logDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "VoiceCleanAI", "logs");
        Directory.CreateDirectory(logDir);
        _logFilePath = Path.Combine(logDir, $"log_{DateTime.Now:yyyyMMdd}.txt");
    }

    public void Log(string message, string level = "INFO")
    {
        string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] [{level}] {message}{Environment.NewLine}";
        File.AppendAllText(_logFilePath, logEntry);
    }

    public string GetLogPath() => _logFilePath;

    public void ExportLogs(string destinationPath)
    {
        File.Copy(_logFilePath, destinationPath, true);
    }
}
