using System.IO;
using System.Linq;

namespace VoiceCleanAI.Services;

public class FileValidator
{
    public static (bool IsValid, string ErrorMessage) Validate(string filePath)
    {
        if (!File.Exists(filePath))
            return (false, "ファイルが見つかりません。");

        var info = new FileInfo(filePath);
        if (info.Length == 0)
            return (false, "ファイルサイズが0バイトです。");

        string ext = info.Extension.ToLower();
        string[] supported = { ".wav", ".mp3", ".m4a", ".flac", ".mp4", ".mkv", ".mov", ".avi" };
        
        if (!supported.Contains(ext))
            return (false, $"サポートされていない形式です ({ext})。");

        return (true, string.Empty);
    }
}
