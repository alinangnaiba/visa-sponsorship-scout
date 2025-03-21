using VisaSponsorshipScout.Infrastructure.Configuration;

namespace VisaSponsorshipScout.Infrastructure.CloudServices
{
    internal static class StorageServiceFactory
    {
        internal static ICloudStorageService Create(FileStorageSettings settings)
        {
            return settings.CloudService switch
            {
                "Azure" => new AzureFileStorageService(settings),
                "Google" => new GoogleCloudStorageService(settings),
                _ => throw new NotSupportedException("Cloud service not supported"),
            };
        }
    }
}
