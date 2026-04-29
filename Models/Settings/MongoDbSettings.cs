namespace FileVault.Web.Models.Settings;

public class MongoDbSettings
{
    public string ConnectionString { get; set; } = "mongodb://localhost:27017";
    public string DatabaseName { get; set; } = "FileVault";
    public string GridFsBucketName { get; set; } = "fileVaultFiles";
}
