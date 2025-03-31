namespace VisaSponsorshipScout.Infrastructure.Configuration
{
    internal class ApplicationSettings
    {
        public DatabaseSettings DatabaseSettings { get; set; } = new DatabaseSettings();
        public FileStorageSettings FileStorage { get; set; } = new FileStorageSettings();
    }

    internal class DatabaseSettings
    {
        public string Database { get; set; }
        public string[] Urls { get; set; }
    }

    internal class FileStorageSettings
    {
        public string BucketName { get; set; }
        public string CertificateDirectoryName { get; set; }
        public string CloudService { get; set; }
        public string ConnectionString { get; set; }
        public string FileName { get; set; }
        public string ShareName { get; set; }
        public string Uri { get; set; }
    }
}