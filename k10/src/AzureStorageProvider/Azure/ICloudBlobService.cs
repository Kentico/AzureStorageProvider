
using AzureStorageProvider.Models;
using CMS;
using System.Collections.Generic;

namespace AzureStorageProvider.Azure
{
    [DefaultImplementation(typeof(CloudBlobService))]
    public interface ICloudBlobService
    {
        bool Exists(string path);
        BlobAttributes FetchAttributes(string path);
        void SetMetadataAsync(string path, IDictionary<string, string> metadata);
        void Delete(string path);
        void Copy(string path, string targetPath);
        byte[] Download(string path);
        BlobAttributes Upload(string path, IDictionary<string, string> metadata, System.IO.Stream stream);
    }
}
