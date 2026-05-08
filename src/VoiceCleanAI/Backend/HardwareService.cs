using System.Diagnostics;
using System.Management;
using System.Text.Json;
using VoiceCleanAI.Core.Models;

namespace VoiceCleanAI.Backend;

public class HardwareService
{
    private readonly string _pythonExecutable = "python";
    private readonly string _scriptPath;

    public HardwareService(string scriptPath)
    {
        _scriptPath = scriptPath;
    }

    public async Task<HardwareInfo> GetHardwareInfoAsync()
    {
        var info = new HardwareInfo();

        try
        {
            // Detect CPU and RAM via WMI
            using var searcherCpu = new ManagementObjectSearcher("select Name, NumberOfCores from Win32_Processor");
            foreach (var obj in searcherCpu.Get())
            {
                info.CpuName = obj["Name"]?.ToString() ?? "Unknown CPU";
                info.CpuCores = int.Parse(obj["NumberOfCores"]?.ToString() ?? "0");
                break;
            }

            using var searcherRam = new ManagementObjectSearcher("select TotalPhysicalMemory from Win32_ComputerSystem");
            foreach (var obj in searcherRam.Get())
            {
                info.TotalRamBytes = long.Parse(obj["TotalPhysicalMemory"]?.ToString() ?? "0");
                break;
            }

            // Detect GPU and AI Backend via Python
            var startInfo = new ProcessStartInfo
            {
                FileName = _pythonExecutable,
                Arguments = $"\"{_scriptPath}\" --check-gpu",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };

            using var process = Process.Start(startInfo);
            if (process != null)
            {
                string output = await process.StandardOutput.ReadToEndAsync();
                await process.WaitForExitAsync();

                foreach (var line in output.Split('\n'))
                {
                    if (line.StartsWith("HW_INFO:"))
                    {
                        var json = line.Substring(8);
                        var pyInfo = JsonSerializer.Deserialize<PythonHwInfo>(json);
                        if (pyInfo != null)
                        {
                            info.HasCuda = pyInfo.has_cuda;
                            info.GpuName = pyInfo.gpu_name;
                            info.VramBytes = pyInfo.vram_total;
                            info.HasDirectML = pyInfo.has_directml;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Hardware detection failed: {ex.Message}");
        }

        return info;
    }

    private class PythonHwInfo
    {
        public bool has_cuda { get; set; }
        public string gpu_name { get; set; } = string.Empty;
        public long vram_total { get; set; }
        public bool has_directml { get; set; }
    }
}
