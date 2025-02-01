using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Migrator.Infrastructure.Configuration
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
    }

    internal class RavenDbSettings
    {
        public string CertificateFileName { get; set; }
        public string CertificatePath { get; set; }
        public string DatabaseName { get; set; }
        public string[] Urls { get; set; }
    }
}
