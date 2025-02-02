namespace Migrator.Infrastructure.Configuration
{
    [Serializable]
    internal class ApplicationSettings
    {
        public AzureFileStorageSettings AzureFileStorage { get; set; } = new AzureFileStorageSettings();
        public RavenDbSettings RavenDbSettings { get; set; } = new RavenDbSettings();
    }

    [Serializable]
    internal class AzureFileStorageSettings
    {
        public string CertificateDirectoryName { get; set; }
        public string ConnectionString { get; set; }
        public string ShareName { get; set; }
        public string Uri { get; set; }
    }

    [Serializable]
    internal class RavenDbSettings
    {
        public string CertificateFileName { get; set; }
        public string CertificatePath { get; set; }
        public string DatabaseName { get; set; }
        public string[] Urls { get; set; }
    }
}
