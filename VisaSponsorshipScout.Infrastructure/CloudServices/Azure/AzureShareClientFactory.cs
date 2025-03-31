using Azure.Identity;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using VisaSponsorshipScout.Infrastructure.Configuration;

namespace VisaSponsorshipScout.Infrastructure.CloudServices.Azure
{
    internal sealed class AzureShareClientFactory
    {
        internal static ShareClient Create(FileStorageSettings settings)
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