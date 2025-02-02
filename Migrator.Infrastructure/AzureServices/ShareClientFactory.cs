using Azure.Identity;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Migrator.Infrastructure.Configuration;

namespace Migrator.Infrastructure.AzureServices
{
    internal sealed class ShareClientFactory
    {
        internal static ShareClient Create(AzureFileStorageSettings settings)
        {
            var shareOptions = new ShareClientOptions
            {
                ShareTokenIntent = ShareTokenIntent.Backup
            };

            var shareClient = string.IsNullOrWhiteSpace(settings.ConnectionString) ? 
                new ShareClient(new Uri($"{settings.Uri}/{settings.ShareName}"), new DefaultAzureCredential(), shareOptions) :
                new ShareClient(settings.ConnectionString, settings.ShareName); //local

            return shareClient;
        }
    }
}
