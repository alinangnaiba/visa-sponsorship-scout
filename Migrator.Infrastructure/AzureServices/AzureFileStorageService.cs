using Azure.Storage.Files.Shares;
using Azure.Storage.Files.Shares.Models;
using Migrator.Infrastructure.Configuration;

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
            ShareClient share = new ShareClient(_settings.ConnectionString, _settings.ShareName);
            ShareDirectoryClient client = share.GetDirectoryClient(_settings.CertificateDirectoryName);
            var list = client.GetFilesAndDirectories().ToList();
            var fileItem = list.FirstOrDefault(file => !file.IsDirectory && file.Name == fileName);
            if (fileItem is null) 
            {
                return null;
            }
            ShareFileClient file = client.GetFileClient(fileItem.Name);
            ShareFileDownloadInfo download = file.Download();
            using MemoryStream ms = new MemoryStream();
            download.Content.CopyTo(ms);
            ms.Position = 0;

            return ms.ToArray();
        }
    }
}
