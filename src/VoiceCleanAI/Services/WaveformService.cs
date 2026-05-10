using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using VoiceCleanAI.Services;

namespace VoiceCleanAI.Services;

public class WaveformService
{
    private readonly string _ffmpegPath;
    private readonly LogService _logService;

    public WaveformService(LogService logService)
    {
        _logService = logService;
        
        string[] searchPaths = {
            Path.GetDirectoryName(Environment.ProcessPath) ?? "",
            AppDomain.CurrentDomain.BaseDirectory,
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, ".."),
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "app")
        };

        _ffmpegPath = "ffmpeg.exe";
        foreach (var dir in searchPaths)
        {
            if (string.IsNullOrEmpty(dir)) continue;
            string path = Path.Combine(dir, "ffmpeg.exe");
            if (File.Exists(path))
            {
                _ffmpegPath = path;
                break;
            }
        }
    }

    public async Task<float[]> GetWaveformDataAsync(string filePath, int points = 100)
    {
        try
        {
            if (!File.Exists(_ffmpegPath)) return Array.Empty<float>();

            // FFmpeg を使って超低サンプリングレート(1000Hz)でモノラル抽出
            string tempRaw = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".raw");
            var si = new ProcessStartInfo
            {
                FileName = _ffmpegPath,
                Arguments = $"-i \"{filePath}\" -f f32le -ac 1 -ar 1000 -y \"{tempRaw}\"",
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var p = Process.Start(si);
            await p!.WaitForExitAsync();

            if (!File.Exists(tempRaw)) return Array.Empty<float>();

            byte[] bytes = await File.ReadAllBytesAsync(tempRaw);
            File.Delete(tempRaw);

            float[] rawData = new float[bytes.Length / 4];
            Buffer.BlockCopy(bytes, 0, rawData, 0, bytes.Length);

            if (rawData.Length == 0) return Array.Empty<float>();

            // 必要なポイント数にダウンサンプリング
            float[] peaks = new float[points];
            int chunkSize = rawData.Length / points;
            if (chunkSize == 0) chunkSize = 1;

            for (int i = 0; i < points; i++)
            {
                int start = i * chunkSize;
                if (start >= rawData.Length) break;
                
                float max = 0;
                for (int j = 0; j < chunkSize && (start + j) < rawData.Length; j++)
                {
                    float val = Math.Abs(rawData[start + j]);
                    if (val > max) max = val;
                }
                peaks[i] = max;
            }

            return peaks;
        }
        catch (Exception ex)
        {
            _logService.Log($"Waveform generation failed: {ex.Message}", "ERROR");
            return Array.Empty<float>();
        }
    }
}
