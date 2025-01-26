using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Raven.Client.Documents;

namespace Migrator.Infrastructure.Extensions
{
    public static class ConfigurationExtensions
    {
        public static IServiceCollection ConfigureDatabase(this IServiceCollection services, IConfiguration configuration)
        {
            string url;
            string[] urls;
            string database;
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("RAVENDB_URL")))
            {
                url = Environment.GetEnvironmentVariable("RAVENDB_URL");
                database = Environment.GetEnvironmentVariable("RAVENDB_DATABASE");
                urls = [url];
            }
            else
            {
                var ravenConfig = configuration.GetSection("RavenDb");
                urls = ravenConfig.GetSection("Urls").Get<string[]>();
                database = ravenConfig.GetValue<string>("Database");
            }

            // Create and configure the DocumentStore
            var store = new DocumentStore
            {
                Urls = urls,
                Database = database
            };

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
    }
}
