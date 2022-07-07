namespace DocumentService.PolyglotPersistence.Models
{
    public class Dependencies
    {
        public string DatabaseType { get; }
        public DatabaseSettings DatabaseSettings { get; }
    }

    public class DatabaseSettings
    {
        public string ConnectionString { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
    }

    public class AzureBlobStorage
    {
        public string? ConnectionString { get; }
        public string? Container { get; }
        public bool Activated { get; }
    }
}
