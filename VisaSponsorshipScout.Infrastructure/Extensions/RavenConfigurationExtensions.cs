using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;
using System.Security.Cryptography.X509Certificates;
using VisaSponsorshipScout.Infrastructure.CloudServices;
using VisaSponsorshipScout.Infrastructure.Configuration;

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

        private static DocumentStore CreateDocumentStore(ApplicationSettings applicationSettings)
        {
            X509Certificate2? certificate;
            var store = new DocumentStore
            {
                Urls = applicationSettings.DatabaseSettings.Urls,
                Database = applicationSettings.DatabaseSettings.Database
            };
            if (TryGetCertificateFromStorage(applicationSettings.FileStorage, out certificate))
            {
                store.Certificate = certificate;
            }
            store.Initialize();
            EnsureDatabaseExists(store, applicationSettings.DatabaseSettings.Database);

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

        private static ApplicationSettings GetSettings(IConfiguration configuration)
        {
            var applicationSettings = new ApplicationSettings();
            var databaseConfig = configuration.GetSection(nameof(DatabaseSettings));
            var fileStorageConfig = configuration.GetSection(nameof(FileStorageSettings));
            applicationSettings.DatabaseSettings = databaseConfig.Get<DatabaseSettings>();
            applicationSettings.FileStorage = fileStorageConfig.Get<FileStorageSettings>();

            return applicationSettings;
        }

        private static bool IsValidSettings(FileStorageSettings settings)
        {
            if (settings is null)
            {
                return false;
            }
            if (settings.CloudService is "Azure")
            {
                return !string.IsNullOrWhiteSpace(settings.CertificateDirectoryName) && !string.IsNullOrWhiteSpace(settings.ShareName) && !string.IsNullOrWhiteSpace(settings.FileName); ;
            }

            return !string.IsNullOrWhiteSpace(settings.BucketName) && !string.IsNullOrWhiteSpace(settings.FileName) && !string.IsNullOrWhiteSpace(settings.FileName);
        }

        private static bool TryGetCertificateFromStorage(FileStorageSettings settings, out X509Certificate2? certificate)
        {
            if (!IsValidSettings(settings))
            {
                certificate = null;
                return false;
            }
            ICloudStorageService fileStorageService = StorageServiceFactory.Create(settings);
            byte[]? bytes = fileStorageService.DownloadToMemory(settings.FileName);
            if (bytes is not null)
            {
                certificate = X509CertificateLoader.LoadPkcs12(bytes.ToArray(), null);
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