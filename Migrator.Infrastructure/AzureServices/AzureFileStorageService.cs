using Azure.Identity;
using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Migrator.Infrastructure.Configuration;
using Newtonsoft.Json.Linq;

namespace Migrator.Infrastructure.AzureServices
{
    internal class AzureFileStorageService
    {
        private readonly AzureFileStorageSettings _settings;

        public AzureFileStorageService(AzureFileStorageSettings settings) 
        {
            _settings = settings;
        }

        public byte[]? GetByte(string fileName)
        {
            try
            {
                var shareOptions = new ShareClientOptions
                {
                    ShareTokenIntent = ShareTokenIntent.Backup
                };
                //Not using Key Vault just coz I'm cheap
                var shareClient = new ShareClient(new Uri($"{_settings.Uri}/{_settings.ShareName}"), new DefaultAzureCredential(), shareOptions);
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
