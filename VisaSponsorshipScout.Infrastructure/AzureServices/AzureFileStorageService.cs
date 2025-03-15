using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using VisaSponsorshipScout.Infrastructure.Configuration;

namespace VisaSponsorshipScout.Infrastructure.AzureServices
{
    internal class AzureFileStorageService
    {
        private readonly AzureFileStorageSettings _settings;

        internal AzureFileStorageService(AzureFileStorageSettings settings) 
        {
            _settings = settings;
        }

        internal byte[]? GetByte(string fileName)
        {
            try
            {
                //Not using Key Vault just coz I'm cheap
                var shareClient = ShareClientFactory.Create(_settings);
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
