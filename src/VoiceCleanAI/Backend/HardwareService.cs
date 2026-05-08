using System.Diagnostics;
using System.Management;
using System.Text.Json;
using VoiceCleanAI.Core.Models;

namespace VoiceCleanAI.Backend;

public class HardwareService
{
    public HardwareService(string? dummy = null) { }

    public async Task<HardwareInfo> GetHardwareInfoAsync()
    {
        var info = new HardwareInfo();

        try
        {
            await Task.Run(() =>
            {
                // Detect CPU via WMI
                using var searcherCpu = new ManagementObjectSearcher("select Name, NumberOfCores from Win32_Processor");
                foreach (var obj in searcherCpu.Get())
                {
                    info.CpuName = obj["Name"]?.ToString() ?? "Unknown CPU";
                    info.CpuCores = int.Parse(obj["NumberOfCores"]?.ToString() ?? "0");
                    break;
                }

                // Detect RAM via WMI
                using var searcherRam = new ManagementObjectSearcher("select TotalPhysicalMemory from Win32_ComputerSystem");
                foreach (var obj in searcherRam.Get())
                {
                    info.TotalRamBytes = long.Parse(obj["TotalPhysicalMemory"]?.ToString() ?? "0");
                    break;
                }

                // Detect GPU via WMI (Python依存を排除)
                using var searcherGpu = new ManagementObjectSearcher("select Name, AdapterRAM from Win32_VideoController");
                foreach (var obj in searcherGpu.Get())
                {
                    string name = obj["Name"]?.ToString() ?? "";
                    if (name.Contains("Microsoft Remote Display") || name.Contains("Basic Render")) continue;

                    info.GpuName = name;
                    info.VramBytes = Math.Abs(long.Parse(obj["AdapterRAM"]?.ToString() ?? "0"));
                    info.HasDirectML = true; // Win10+ なら基本的に利用可能
                    
                    if (name.Contains("NVIDIA")) info.HasCuda = true;
                    break;
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Hardware detection failed: {ex.Message}");
        }

        return info;
    }
}
