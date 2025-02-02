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
                var shareClient = new ShareClient(new Uri(_settings.Uri), new DefaultAzureCredential());
                ShareDirectoryClient client = shareClient.GetDirectoryClient(_settings.CertificateDirectoryName);
                Console.WriteLine($"✅ Share client created!");
                ShareFileClient file = client.GetFileClient(fileName);
                Console.WriteLine($"✅ File client created!");
                Console.WriteLine($"✅ Downloading..........!");
                ShareFileDownloadInfo download = file.DownloadAsync().GetAwaiter().GetResult();
                Console.WriteLine($"✅ Downloading complete!");
                using MemoryStream ms = new MemoryStream();
                download.Content.CopyTo(ms);
                ms.Position = 0;

                return ms.ToArray();
            }
            catch (Exception ex)
            {
                throw;
            }            
        }
    }
}
