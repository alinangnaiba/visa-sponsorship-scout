namespace VisaSponsorshipScout.Infrastructure.CloudServices
{
    internal interface ICloudStorageService
    {
        byte[]? DownloadToMemory(string fileName);
    }
}