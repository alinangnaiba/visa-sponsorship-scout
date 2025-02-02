using Azure.Identity;
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
            //var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
            //{
            //    ExcludeEnvironmentCredential = true,
            //    ExcludeManagedIdentityCredential = false,
            //    ExcludeVisualStudioCredential = true,
            //    ExcludeSharedTokenCacheCredential = true
            //});
            var credential = new DefaultAzureCredential();
            //try
            //{
            //    var token = credential.GetToken(new Azure.Core.TokenRequestContext(new[] { "https://storage.azure.com//.default" }));
            //    Console.WriteLine($"✅ Managed Identity Authentication Succeeded! Token Expires: {token.ExpiresOn}");
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine($"❌ Managed Identity Authentication Failed: {ex.Message}");
            //}

            try
            {
                var shareClient = new ShareClient(new Uri(_settings.Uri), new DefaultAzureCredential());
                ShareDirectoryClient client = shareClient.GetDirectoryClient(_settings.CertificateDirectoryName);
                //var list = client.GetFilesAndDirectories().ToList();
                //var fileItem = list.FirstOrDefault(file => !file.IsDirectory && file.Name == fileName);
                //if (fileItem is null)
                //{
                //    return null;
                //}
                ShareFileClient txtFileClient = client.GetFileClient("test_text.txt");
                ShareFileDownloadInfo txtdownload = txtFileClient.Download();
                using MemoryStream txtms = new MemoryStream();
                txtdownload.Content.CopyTo(txtms);
                return txtms.ToArray();
                //ShareFileClient file = client.GetFileClient(fileName);
                //ShareFileDownloadInfo download = file.Download();
                //using MemoryStream ms = new MemoryStream();
                //download.Content.CopyTo(ms);
                //ms.Position = 0;

                //return ms.ToArray();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message );
                Console.WriteLine(ex.StackTrace);
                return null;
            }
            //var shareClient = new ShareClient(new Uri(_settings.Uri), new DefaultAzureCredential());
            //ShareDirectoryClient client = shareClient.GetDirectoryClient(_settings.CertificateDirectoryName);
            ////var list = client.GetFilesAndDirectories().ToList();
            ////var fileItem = list.FirstOrDefault(file => !file.IsDirectory && file.Name == fileName);
            ////if (fileItem is null)
            ////{
            ////    return null;
            ////}
            //ShareFileClient file = client.GetFileClient(fileName);
            //ShareFileDownloadInfo download = file.Download();
            
        }
    }
}
