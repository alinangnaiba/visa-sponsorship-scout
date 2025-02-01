using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Migrator.Infrastructure.AzureServices;
using Migrator.Infrastructure.Configuration;
using Raven.Client.Documents;
using System.Security.Cryptography.X509Certificates;
using static Raven.Client.Constants;

namespace Migrator.Infrastructure.Extensions
{
    public static class RavenConfigurationExtensions
    {
        public static IServiceCollection ConfigureDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            X509Certificate2? certificate;
            var applicationSettings = GetSettings(configuration);
            // Create and configure the DocumentStore
            var store = new DocumentStore
            {
                Urls = applicationSettings.RavenDbSettings.Urls,
                Database = applicationSettings.RavenDbSettings.DatabaseName
            };
            if (!string.IsNullOrWhiteSpace(applicationSettings.AzureFileStorage.ConnectionString) && 
                TryGetCertificate(applicationSettings.AzureFileStorage, applicationSettings.RavenDbSettings.CertificateFileName, out certificate))
            {
                store.Certificate = certificate;
            }
            else
            {
                certificate = new X509Certificate2(applicationSettings.RavenDbSettings.CertificatePath);
                store.Certificate = certificate;
            }

            store.Initialize();  // Initialize the RavenDB connection

            // Add last updated at in metadata
            store.OnBeforeStore += (sender, eventArgs) =>
            {
                var metadata = eventArgs.DocumentMetadata;
                metadata["Last-Updated-At"] = DateTime.UtcNow.ToString("o");
            };
            services.AddSingleton<IDocumentStore>(store);
            services.AddScoped(provider =>
            {
                var store = provider.GetRequiredService<IDocumentStore>();
                return store.OpenAsyncSession(); // Scoped session for each request
            });

            return services;
        }

        private static ApplicationSettings GetSettings(IConfiguration configuration)
        {
            var applicationSettings = new ApplicationSettings();
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RAVENDB_URL")))
            {
                applicationSettings.RavenDbSettings.Urls = [Environment.GetEnvironmentVariable("RAVENDB_URL")];
                applicationSettings.RavenDbSettings.DatabaseName = Environment.GetEnvironmentVariable("RAVENDB_NAME");
                applicationSettings.RavenDbSettings.CertificateFileName = Environment.GetEnvironmentVariable("RAVENDB_CERTIFICATE_FILE_NAME");
                applicationSettings.AzureFileStorage.ShareName = Environment.GetEnvironmentVariable("AZURE_FILE_STORAGE_SHARE_NAME");
                applicationSettings.AzureFileStorage.ConnectionString = Environment.GetEnvironmentVariable("AZURE_FILE_STORAGE_CONNECTION_STRING");
                applicationSettings.AzureFileStorage.CertificateDirectoryName = Environment.GetEnvironmentVariable("AZURE_FILE_STORAGE_CERTIFICATE_DIRECTORY_NAME");
                applicationSettings.AzureFileStorage.Uri = Environment.GetEnvironmentVariable("AZURE_FILE_STORAGE_URI");
            }
            else
            {
                var ravenConfig = configuration.GetSection("RavenDb");
                applicationSettings.RavenDbSettings.Urls = ravenConfig.GetSection("Urls").Get<string[]>();
                applicationSettings.RavenDbSettings.DatabaseName = ravenConfig.GetValue<string>("Database");
                applicationSettings.RavenDbSettings.CertificatePath = ravenConfig.GetValue<string>("CertificatePath");
                applicationSettings.RavenDbSettings.CertificateFileName = ravenConfig.GetValue<string>("CertificateFileName");

                var fileStorageConfig = configuration.GetSection("AzureFileStorage");
                applicationSettings.AzureFileStorage.ShareName = fileStorageConfig.GetValue<string>("ShareName");
                applicationSettings.AzureFileStorage.ConnectionString = fileStorageConfig.GetValue<string>("ConnectionString");
                applicationSettings.AzureFileStorage.CertificateDirectoryName = fileStorageConfig.GetValue<string>("Directory");
            }

            return applicationSettings;
        }

        private static bool TryGetCertificate(AzureFileStorageSettings settings, string fileName, out X509Certificate2? certificate)
        {
            if (string.IsNullOrWhiteSpace(settings.ConnectionString) || string.IsNullOrWhiteSpace(settings.CertificateDirectoryName) ||
                string.IsNullOrWhiteSpace(settings.ShareName) || string.IsNullOrWhiteSpace(fileName)) 
            {
                certificate = null;
                return false;
            }
            var fileStorageService = new AzureFileStorageService(settings);
            var bytes = fileStorageService.GetByte(fileName);
            if (bytes is not null)
            {
                certificate = new X509Certificate2(bytes);
                return true;
            }
            else
            {
                certificate = null;
                return false;
            }
        }
    }
}
