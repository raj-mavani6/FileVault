namespace FileVault.Web.Models.Settings;

public class AppSettings
{
    public string SiteName { get; set; } = "FileVault";
    public int MaxChunkSizeBytes { get; set; } = 5 * 1024 * 1024;
    public int UploadSessionExpiryMinutes { get; set; } = 1440;
    public int ShareLinkDefaultExpiryDays { get; set; } = 7;
    public string TempUploadPath { get; set; } = "temp_uploads";
    public string AllowedFileExtensions { get; set; } = "";
    public string BlockedFileExtensions { get; set; } = ".exe,.bat,.cmd,.com,.msi,.scr,.pif";
    public bool EnableVirusScan { get; set; } = false;
}
