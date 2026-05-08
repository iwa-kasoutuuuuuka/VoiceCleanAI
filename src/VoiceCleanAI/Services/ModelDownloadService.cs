using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;

namespace VoiceCleanAI.Services;

public class ModelDownloadService
{
    private readonly HttpClient _httpClient;
    private readonly string _modelDir;
    private readonly LogService _logService;

    public ModelDownloadService(string modelDir, LogService logService)
    {
        _modelDir = modelDir;
        _logService = logService;
        
        // 通信の成功率を高めるために User-Agent を設定
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("VoiceCleanAI/1.0 (Windows; Managed)");
    }

    public bool AreModelsAvailable()
    {
        return File.Exists(Path.Combine(_modelDir, "denoiser.onnx"));
    }

    public async Task DownloadModelsAsync(IProgress<double> progress)
    {
        if (!Directory.Exists(_modelDir)) Directory.CreateDirectory(_modelDir);

        // ※ 公開されているリポジトリのURLに修正（例として公式や安定したミラーを指定）
        var models = new[]
        {
            // Denoiser は GitHub の公開ソースから取得可能
            new { Name = "denoiser.onnx", Url = "https://github.com/skeskinen/resemble-denoise-onnx-inference/raw/master/denoiser.onnx" }
        };

        _logService.Log("Starting automatic download for denoiser.onnx...");

        // TODO: 現時点で公式に ONNX が無い場合は、ユーザー様側で用意したURLを指定する必要があります。
        // ここではエラー時に詳細を表示するようにします。

        for (int i = 0; i < models.Length; i++)
        {
            string filePath = Path.Combine(_modelDir, models[i].Name);
            await DownloadFileAsync(models[i].Url, filePath, (p) => 
            {
                progress.Report((i + p) / models.Length);
            });
        }
    }

    private async Task DownloadFileAsync(string url, string destinationPath, Action<double> progressCallback)
    {
        using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
        
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"サーバーからエラーが返されました: {(int)response.StatusCode} ({response.ReasonPhrase}) URL: {url}");
        }

        var totalBytes = response.Content.Headers.ContentLength ?? -1L;
        using var contentStream = await response.Content.ReadAsStreamAsync();
        using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

        var buffer = new byte[8192];
        var totalRead = 0L;
        var isMoreToRead = true;

        do
        {
            var read = await contentStream.ReadAsync(buffer, 0, buffer.Length);
            if (read == 0)
            {
                isMoreToRead = false;
            }
            else
            {
                await fileStream.WriteAsync(buffer, 0, read);
                totalRead += read;
                if (totalBytes != -1)
                    progressCallback((double)totalRead / totalBytes);
            }
        } while (isMoreToRead);
    }
}
