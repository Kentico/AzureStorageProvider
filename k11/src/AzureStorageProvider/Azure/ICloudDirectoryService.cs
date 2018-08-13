using AzureStorageProvider.Models;
using System.Collections.Generic;

namespace AzureStorageProvider.Azure
{
    public interface ICloudDirectoryService
    {
        List<Blob> GetBlobs(string path);
    }
}
