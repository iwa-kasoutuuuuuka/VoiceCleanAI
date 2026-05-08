using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.Json;
using VoiceCleanAI.Core.Models;

namespace VoiceCleanAI.Backend;

public class BackendWorkerService
{
    private Process? _process;
    private string _pythonExecutable = "python";
    private readonly string _scriptPath;

    public BackendWorkerService(string scriptPath)
    {
        _scriptPath = scriptPath;
        FindPython();
    }

    private void FindPython()
    {
        // 1. アプリケーション直下の python フォルダを確認 (ポータブル構成)
        string localPython = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "python", "python.exe");
        if (File.Exists(localPython))
        {
            _pythonExecutable = localPython;
            return;
        }

        // 2. PATH 上の python を確認
        // デフォルトの "python" を使用
    }

    public async Task ProcessTaskAsync(AudioTask task, CancellationToken ct, IProgress<double> progress)
    {
        if (!File.Exists(_scriptPath))
        {
            throw new FileNotFoundException($"バックエンドスクリプトが見つかりません: {_scriptPath}");
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = _pythonExecutable,
            Arguments = $"\"{_scriptPath}\" --input \"{task.InputFilePath}\" --output \"{task.OutputFilePath}\" --lambd {task.Lambd} --tau {task.Tau} --nfe {task.Nfe}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var process = new Process { StartInfo = startInfo };
        _process = process;

        var errorBuilder = new StringBuilder();

        process.OutputDataReceived += (s, e) =>
        {
            if (e.Data != null && e.Data.StartsWith("PROGRESS:"))
            {
                if (double.TryParse(e.Data.Substring(9), out var p))
                {
                    progress.Report(p);
                }
            }
        };

        process.ErrorDataReceived += (s, e) =>
        {
            if (e.Data != null)
            {
                errorBuilder.AppendLine(e.Data);
            }
        };

        try
        {
            process.Start();
        }
        catch (System.ComponentModel.Win32Exception)
        {
            throw new Exception("Python が見つかりません。Python をインストールするか、アプリフォルダに python.exe を配置してください。");
        }

        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        try
        {
            await process.WaitForExitAsync(ct);
        }
        catch (OperationCanceledException)
        {
            if (!process.HasExited) process.Kill(true);
            throw;
        }
        finally
        {
            _process = null;
        }

        if (process.ExitCode != 0)
        {
            string errorMsg = errorBuilder.ToString();
            if (string.IsNullOrEmpty(errorMsg)) errorMsg = "Python プロセスが異常終了しました。";
            throw new Exception($"Backend failed (Code {process.ExitCode}): {errorMsg}");
        }
    }
}
