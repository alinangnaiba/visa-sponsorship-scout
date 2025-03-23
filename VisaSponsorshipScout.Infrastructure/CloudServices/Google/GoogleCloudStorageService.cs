using Google.Cloud.Storage.V1;
using VisaSponsorshipScout.Infrastructure.Configuration;

namespace VisaSponsorshipScout.Infrastructure.CloudServices.Google
{
    internal class GoogleCloudStorageService : ICloudStorageService
    {
        private readonly FileStorageSettings _settings;

        internal GoogleCloudStorageService(FileStorageSettings settings)
        {
            _settings = settings;
        }

        public byte[]? DownloadToMemory(string filename)
        {
            StorageClient storage = StorageClient.Create();
            MemoryStream stream = new();
            storage.DownloadObject(_settings.BucketName, filename, stream);

            return stream.ToArray();
        }
    }
}
