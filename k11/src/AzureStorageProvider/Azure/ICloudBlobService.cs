
using AzureStorageProvider.Models;
using System.Collections.Generic;

namespace AzureStorageProvider.Azure
{
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
