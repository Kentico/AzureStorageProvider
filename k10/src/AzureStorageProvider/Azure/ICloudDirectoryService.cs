using AzureStorageProvider.Models;
using CMS;
using System.Collections.Generic;

namespace AzureStorageProvider.Azure
{
    [DefaultImplementation(typeof(CloudDirectoryService))]
    public interface ICloudDirectoryService
    {
        List<Blob> GetBlobs(string path);
    }
}
