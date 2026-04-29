using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace FileVault.Web.Helpers;

public static class FileHelpers
{
    private static readonly HashSet<string> ImageExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp", ".svg", ".ico" };

    private static readonly HashSet<string> VideoExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".mp4", ".webm", ".ogg", ".mov", ".avi", ".mkv" };

    private static readonly HashSet<string> AudioExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".mp3", ".wav", ".ogg", ".flac", ".aac", ".m4a" };

    private static readonly HashSet<string> TextExtensions = new(StringComparer.OrdinalIgnoreCase)
        { ".txt", ".md", ".csv", ".json", ".xml", ".yaml", ".yml", ".log",
          ".cs", ".js", ".ts", ".html", ".css", ".py", ".java", ".cpp", ".c",
          ".h", ".rb", ".go", ".rs", ".php", ".sql", ".sh", ".bat", ".ps1" };

    public static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName)) return $"file_{DateTime.UtcNow.Ticks}";

        // Explicitly remove control characters (\r, \n, etc.) to prevent header injection errors
        var noControls = new string(fileName.Where(c => !char.IsControl(c)).ToArray());
        
        var invalidChars = Path.GetInvalidFileNameChars();
        var sanitized = new string(noControls.Where(c => !invalidChars.Contains(c)).ToArray());
        
        sanitized = Regex.Replace(sanitized, @"\.{2,}", ".");
        sanitized = sanitized.Trim('.', ' ');
        
        return string.IsNullOrWhiteSpace(sanitized) ? $"file_{DateTime.UtcNow.Ticks}" : sanitized;
    }

    public static string GetContentType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            ".svg" => "image/svg+xml",
            ".ico" => "image/x-icon",
            ".mp4" => "video/mp4",
            ".webm" => "video/webm",
            ".ogg" => "application/ogg",
            ".mov" => "video/quicktime",
            ".mp3" => "audio/mpeg",
            ".wav" => "audio/wav",
            ".flac" => "audio/flac",
            ".aac" => "audio/aac",
            ".pdf" => "application/pdf",
            ".doc" => "application/msword",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".xls" => "application/vnd.ms-excel",
            ".xlsx" => "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            ".ppt" => "application/vnd.ms-powerpoint",
            ".pptx" => "application/vnd.openxmlformats-officedocument.presentationml.presentation",
            ".zip" => "application/zip",
            ".rar" => "application/x-rar-compressed",
            ".7z" => "application/x-7z-compressed",
            ".tar" => "application/x-tar",
            ".gz" => "application/gzip",
            ".txt" => "text/plain",
            ".html" or ".htm" => "text/html",
            ".css" => "text/css",
            ".js" => "application/javascript",
            ".json" => "application/json",
            ".xml" => "application/xml",
            ".csv" => "text/csv",
            ".md" => "text/markdown",
            _ => "application/octet-stream"
        };
    }

    public static bool IsPreviewable(string extension)
    {
        var ext = extension.ToLowerInvariant();
        return IsImage(ext) || IsVideo(ext) || IsAudio(ext) || IsText(ext) || ext == ".pdf";
    }

    public static bool IsImage(string extension) => ImageExtensions.Contains(extension);
    public static bool IsVideo(string extension) => VideoExtensions.Contains(extension);
    public static bool IsAudio(string extension) => AudioExtensions.Contains(extension);
    public static bool IsText(string extension) => TextExtensions.Contains(extension);

    public static string GetFileIcon(string extension)
    {
        var ext = extension.ToLowerInvariant();
        if (IsImage(ext)) return "bi-file-earmark-image";
        if (IsVideo(ext)) return "bi-file-earmark-play";
        if (IsAudio(ext)) return "bi-file-earmark-music";
        if (IsText(ext)) return "bi-file-earmark-code";
        if (ext == ".pdf") return "bi-file-earmark-pdf";
        if (ext is ".doc" or ".docx") return "bi-file-earmark-word";
        if (ext is ".xls" or ".xlsx") return "bi-file-earmark-excel";
        if (ext is ".ppt" or ".pptx") return "bi-file-earmark-ppt";
        if (ext is ".zip" or ".rar" or ".7z" or ".tar" or ".gz") return "bi-file-earmark-zip";
        return "bi-file-earmark";
    }

    public static string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double size = bytes;
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        return $"{size:0.##} {sizes[order]}";
    }
}
