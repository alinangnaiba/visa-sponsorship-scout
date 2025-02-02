using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Migrator.Infrastructure.AzureServices;
using Migrator.Infrastructure.Configuration;
using Raven.Client.Documents;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

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
                Database = applicationSettings.RavenDbSettings.Database
            };
            if (TryGetCertificateFromStorage(applicationSettings.AzureFileStorage, applicationSettings.RavenDbSettings.CertificateFileName, out certificate))
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

            new Organisation_ByName().Execute(store);

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
            var ravenConfig = configuration.GetSection("RavenDb");
            var fileStorageConfig = configuration.GetSection("AzureFileStorage");
            applicationSettings.RavenDbSettings = ravenConfig.Get<RavenDbSettings>();
            applicationSettings.AzureFileStorage = fileStorageConfig.Get<AzureFileStorageSettings>();

            return applicationSettings;
        }

        private static bool TryGetCertificateFromStorage(AzureFileStorageSettings settings, string fileName, out X509Certificate2? certificate)
        {
            if (string.IsNullOrWhiteSpace(settings.CertificateDirectoryName) || string.IsNullOrWhiteSpace(settings.ShareName) || string.IsNullOrWhiteSpace(fileName)) 
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
