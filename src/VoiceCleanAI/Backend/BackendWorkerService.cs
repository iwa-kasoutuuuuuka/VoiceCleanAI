using System.Diagnostics;
using System.IO;
using System.Text.Json;
using VoiceCleanAI.Core.Models;

namespace VoiceCleanAI.Backend;

public class BackendWorkerService
{
    private Process? _process;
    private readonly string _pythonExecutable = "python"; // TODO: Configure path
    private readonly string _scriptPath;

    public BackendWorkerService(string scriptPath)
    {
        _scriptPath = scriptPath;
    }

    public async Task ProcessTaskAsync(AudioTask task, CancellationToken ct, IProgress<double> progress)
    {
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

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();

        try
        {
            await process.WaitForExitAsync(ct);
        }
        catch (OperationCanceledException)
        {
            process.Kill(true);
            throw;
        }
        finally
        {
            _process = null;
        }

        if (process.ExitCode != 0)
        {
            var error = await process.StandardError.ReadToEndAsync();
            throw new Exception($"Backend failed with exit code {process.ExitCode}: {error}");
        }
    }
}
