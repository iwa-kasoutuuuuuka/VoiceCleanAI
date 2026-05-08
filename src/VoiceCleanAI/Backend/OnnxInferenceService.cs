using System.Diagnostics;
using Microsoft.ML.OnnxRuntime;
using Microsoft.ML.OnnxRuntime.Tensors;
using NWaves.Signals;
using NWaves.Transforms;
using NWaves.Windows;
using VoiceCleanAI.Core.Models;
using VoiceCleanAI.Services;

namespace VoiceCleanAI.Backend;

public class OnnxInferenceService
{
    private readonly string _modelPath;
    private readonly LogService _logService;
    private string _ffmpegPath = "ffmpeg";
    private InferenceSession? _denoiserSession;
    private InferenceSession? _enhancerSession;

    public OnnxInferenceService(string modelPath, LogService logService)
    {
        _modelPath = modelPath;
        _logService = logService;
    }

    public async Task InitializeAsync()
    {
        FindFFmpeg();
        
        if (!File.Exists(_ffmpegPath))
        {
            throw new FileNotFoundException("ffmpeg.exe が見つかりません。アプリケーションと同じフォルダに配置してください。\n" +
                                            "ffmpeg.exe not found. Please place it in the same folder as the application.");
        }

        if (_denoiserSession != null) return;

        await Task.Run(() =>
        {
            var options = new SessionOptions();
            string denoiserPath = Path.Combine(_modelPath, "denoiser.onnx");
            if (File.Exists(denoiserPath)) {
                _denoiserSession = new InferenceSession(denoiserPath, options);
            }

            string enhancerPath = Path.Combine(_modelPath, "enhancer.onnx");
            if (File.Exists(enhancerPath)) {
                _enhancerSession = new InferenceSession(enhancerPath, options);
            }
            
            if (_denoiserSession == null) throw new FileNotFoundException("denoiser.onnx not found.");
        });
    }

    private void FindFFmpeg()
    {
        string localFFmpeg = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ffmpeg.exe");
        if (File.Exists(localFFmpeg)) _ffmpegPath = localFFmpeg;
    }

    public async Task ProcessAsync(AudioTask task, CancellationToken ct, IProgress<double> progress)
    {
        try 
        {
            await InitializeAsync();
            _logService.Log($"ONNX Processing started: {task.FileName}");

            float[] audioData = await ExtractAudioAsync(task.InputFilePath);
            if (audioData.Length == 0) throw new Exception("Audio extraction failed.");
            progress.Report(0.1);

            // STFT Parameters (Fixed for resemble style)
            int windowSize = 1680;
            int hopSize = 420;
            int fftSize = 2048; 
            int modelBins = 841;

            var signal = new DiscreteSignal(44100, audioData);
            var stft = new Stft(windowSize, hopSize, WindowType.Hann, fftSize);
            var complexSpec = stft.Direct(signal);

            int frames = complexSpec.Count;
            int nwavesBins = fftSize / 2 + 1;
            _logService.Log($"STFT: {nwavesBins} bins x {frames} frames");

            // Prepare Tensors
            float[] magData = new float[modelBins * frames];
            float[] cosData = new float[modelBins * frames];
            float[] sinData = new float[modelBins * frames];

            for (int f = 0; f < frames; f++)
            {
                var frame = complexSpec[f];
                for (int b = 0; b < modelBins; b++)
                {
                    float real = frame.Item1[b];
                    float imag = frame.Item2[b];
                    float mag = (float)Math.Sqrt(real * real + imag * imag);

                    int idx = b * frames + f;
                    magData[idx] = mag;
                    if (mag > 1e-10f) {
                        cosData[idx] = real / mag;
                        sinData[idx] = imag / mag;
                    } else {
                        cosData[idx] = 1.0f;
                        sinData[idx] = 0.0f;
                    }
                }
            }

            progress.Report(0.3);
            
            // --- Step 1: Denoiser ---
            var denoiserInputs = new List<NamedOnnxValue> {
                NamedOnnxValue.CreateFromTensor("mag", new DenseTensor<float>(magData, new[] { 1, modelBins, frames })),
                NamedOnnxValue.CreateFromTensor("cos", new DenseTensor<float>(cosData, new[] { 1, modelBins, frames })),
                NamedOnnxValue.CreateFromTensor("sin", new DenseTensor<float>(sinData, new[] { 1, modelBins, frames }))
            };

            using var denoiserResults = _denoiserSession!.Run(denoiserInputs);
            float[] denoisedMag = denoiserResults.First().AsTensor<float>().ToArray();
            _logService.Log("Denoiser step completed.");
            progress.Report(0.6);

            // --- Step 2: Enhancer (Optional) ---
            float[] finalMag = denoisedMag;
            if (_enhancerSession != null)
            {
                _logService.Log("Applying Enhancer...");
                var enhancerInputs = new List<NamedOnnxValue> {
                    NamedOnnxValue.CreateFromTensor("mag", new DenseTensor<float>(denoisedMag, new[] { 1, modelBins, frames })),
                    NamedOnnxValue.CreateFromTensor("cos", new DenseTensor<float>(cosData, new[] { 1, modelBins, frames })),
                    NamedOnnxValue.CreateFromTensor("sin", new DenseTensor<float>(sinData, new[] { 1, modelBins, frames }))
                };
                using var enhancerResults = _enhancerSession.Run(enhancerInputs);
                finalMag = enhancerResults.First().AsTensor<float>().ToArray();
                _logService.Log("Enhancer step completed.");
            }
            progress.Report(0.8);

            // Reconstruction
            var processedSpec = new List<(float[], float[])>();
            for (int f = 0; f < frames; f++)
            {
                var re = new float[nwavesBins];
                var im = new float[nwavesBins];
                for (int b = 0; b < modelBins; b++)
                {
                    int outIdx = b * frames + f;
                    float m = finalMag[outIdx];
                    re[b] = m * cosData[outIdx];
                    im[b] = m * sinData[outIdx];
                }
                processedSpec.Add((re, im));
            }

            var processed = stft.Inverse(processedSpec);
            int finalLen = Math.Min(audioData.Length, processed.Length);
            float[] finalAudio = new float[finalLen];
            Array.Copy(processed, finalAudio, finalLen);

            await SaveAudioAsync(finalAudio, task.OutputFilePath, task.InputFilePath);
            _logService.Log("Success!");
            progress.Report(1.0);
        }
        catch (Exception ex)
        {
            _logService.Log($"Error: {ex.Message}\n{ex.StackTrace}", "ERROR");
            throw;
        }
    }

    private async Task<float[]> ExtractAudioAsync(string inputPath)
    {
        string tempRaw = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".raw");
        try {
            var si = new ProcessStartInfo {
                FileName = _ffmpegPath,
                Arguments = $"-i \"{inputPath}\" -f f32le -ac 1 -ar 44100 -y \"{tempRaw}\"",
                UseShellExecute = false, CreateNoWindow = true
            };
            using var p = Process.Start(si);
            await p!.WaitForExitAsync();
            if (!File.Exists(tempRaw)) return Array.Empty<float>();
            byte[] bytes = await File.ReadAllBytesAsync(tempRaw);
            float[] data = new float[bytes.Length / 4];
            Buffer.BlockCopy(bytes, 0, data, 0, bytes.Length);
            return data;
        } catch { return Array.Empty<float>(); }
        finally { if (File.Exists(tempRaw)) File.Delete(tempRaw); }
    }

    private async Task SaveAudioAsync(float[] data, string path, string? inputVideoPath = null)
    {
        string tempRaw = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".raw");
        byte[] bytes = new byte[data.Length * 4];
        Buffer.BlockCopy(data, 0, bytes, 0, bytes.Length);
        await File.WriteAllBytesAsync(tempRaw, bytes);
        
        try {
            string args;
            bool isVideo = !string.IsNullOrEmpty(inputVideoPath) && 
                          (path.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase) || 
                           path.EndsWith(".mkv", StringComparison.OrdinalIgnoreCase) ||
                           path.EndsWith(".mov", StringComparison.OrdinalIgnoreCase));

            if (isVideo)
            {
                // 映像はコピー (-c:v copy)、音声は再エンコードして合成
                // 入力1: 元動画, 入力2: 生成したRAW音声
                args = $"-i \"{inputVideoPath}\" -f f32le -ac 1 -ar 44100 -i \"{tempRaw}\" " +
                       $"-map 0:v -map 1:a -c:v copy -c:a aac -b:a 192k -shortest -y \"{path}\"";
            }
            else
            {
                // 音声ファイルとして出力
                args = $"-f f32le -ac 1 -ar 44100 -i \"{tempRaw}\" -y \"{path}\"";
            }

            _logService.Log($"FFmpeg Save Command: {args}");
            var si = new ProcessStartInfo {
                FileName = _ffmpegPath,
                Arguments = args,
                UseShellExecute = false, CreateNoWindow = true
            };
            using var p = Process.Start(si);
            await p!.WaitForExitAsync();
        } finally { if (File.Exists(tempRaw)) File.Delete(tempRaw); }
    }
}
