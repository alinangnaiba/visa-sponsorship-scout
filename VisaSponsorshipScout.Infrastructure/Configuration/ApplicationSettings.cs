namespace VisaSponsorshipScout.Infrastructure.Configuration
{
    internal class ApplicationSettings
    {
        public AzureFileStorageSettings AzureFileStorage { get; set; } = new AzureFileStorageSettings();
        public RavenDbSettings RavenDbSettings { get; set; } = new RavenDbSettings();
    }

    internal class AzureFileStorageSettings
    {
        public string CertificateDirectoryName { get; set; }
        public string ConnectionString { get; set; }
        public string ShareName { get; set; }
        public string Uri { get; set; }
    }

    internal class RavenDbSettings
    {
        public string CertificateFileName { get; set; }
        public string CertificatePath { get; set; }
        public string Database { get; set; }
        public string[] Urls { get; set; }
    }
}
