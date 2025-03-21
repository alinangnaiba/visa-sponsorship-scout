namespace VisaSponsorshipScout.Infrastructure.CloudServices
{
    interface ICloudStorageService
    {
        byte[]? DownloadToMemory(string fileName);
    }
}
