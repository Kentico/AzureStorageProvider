
using CMS;
using Microsoft.WindowsAzure.Storage.Blob;

namespace AzureStorageProvider.Azure
{
    [DefaultImplementation(typeof(CloudBlobContainerService))]
    internal interface ICloudBlobContainerService
    {
        void Create(string path, BlobContainerPublicAccessType? accessType = null);
        bool Exists(string path);
        BlobContainerPublicAccessType? GetPublicAccess(string path);
        void DeleteAsync(string path);
    }
}
