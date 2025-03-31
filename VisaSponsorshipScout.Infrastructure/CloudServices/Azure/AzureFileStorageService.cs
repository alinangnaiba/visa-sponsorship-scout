using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using VisaSponsorshipScout.Infrastructure.Configuration;

namespace VisaSponsorshipScout.Infrastructure.CloudServices.Azure
{
    internal class AzureFileStorageService : ICloudStorageService
    {
        private readonly FileStorageSettings _settings;

        internal AzureFileStorageService(FileStorageSettings settings)
        {
            _settings = settings;
        }

        public byte[]? DownloadToMemory(string fileName)
        {
            try
            {
                var shareClient = AzureShareClientFactory.Create(_settings);
                ShareDirectoryClient client = shareClient.GetDirectoryClient(_settings.CertificateDirectoryName);
                ShareFileClient file = client.GetFileClient(fileName);
                ShareFileDownloadInfo download = file.DownloadAsync().GetAwaiter().GetResult();
                using MemoryStream ms = new();
                download.Content.CopyTo(ms);
                ms.Position = 0;

                return ms.ToArray();
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}