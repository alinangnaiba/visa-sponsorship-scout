using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using VisaSponsorshipScout.Infrastructure.AzureServices;
using VisaSponsorshipScout.Infrastructure.Configuration;
using Raven.Client.Documents;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;
using System.Security.Cryptography.X509Certificates;

namespace VisaSponsorshipScout.Infrastructure.Extensions
{
    public static class RavenConfigurationExtensions
    {
        public static IServiceCollection ConfigureDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            var applicationSettings = GetSettings(configuration);

            var store = CreateDocumentStore(applicationSettings);

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
                return store.OpenAsyncSession();
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
            if (settings is null || string.IsNullOrWhiteSpace(settings.CertificateDirectoryName) || string.IsNullOrWhiteSpace(settings.ShareName) || string.IsNullOrWhiteSpace(fileName)) 
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

        private static DocumentStore CreateDocumentStore(ApplicationSettings applicationSettings)
        {
            X509Certificate2? certificate;
            var store = new DocumentStore
            {
                Urls = applicationSettings.RavenDbSettings.Urls,
                Database = applicationSettings.RavenDbSettings.Database
            };
            if (TryGetCertificateFromStorage(applicationSettings.AzureFileStorage, applicationSettings.RavenDbSettings.CertificateFileName, out certificate))
            {
                store.Certificate = certificate;
            }
            store.Initialize();
            EnsureDatabaseExists(store, applicationSettings.RavenDbSettings.Database);

            return store;
        }

        private static void EnsureDatabaseExists(DocumentStore store, string dbName)
        {
            try
            {
                var db = store.Maintenance.Server.Send(new GetDatabaseRecordOperation(dbName));
                if (db is null)
                {
                    var createDbOperation = new CreateDatabaseOperation(new DatabaseRecord(dbName));
                    store.Maintenance.Server.Send(createDbOperation);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to create/verify database: {dbName}", ex);
            }
        }
    }
}
