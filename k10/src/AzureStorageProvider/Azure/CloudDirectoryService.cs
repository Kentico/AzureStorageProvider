using System;
using System.Collections.Generic;
using System.Linq;
using AzureStorageProvider.Models;
using CMS.Core;

namespace AzureStorageProvider.Azure
{
    public class CloudDirectoryService : ICloudDirectoryService
    {
        private ICloudBlobClient _cloudBlobClient = Service<ICloudBlobClient>.Entry();
        protected Microsoft.WindowsAzure.Storage.Blob.CloudBlobDirectory GetDirectoryReference(string path)
        {
            return _cloudBlobClient.GetContainerReference(AccountInfo.Instance.RootContainer).GetDirectoryReference(path);
        }
        public virtual List<Blob> GetBlobs(string path)
        {
            return GetDirectoryReference(path)
                .ListBlobs(true, Microsoft.WindowsAzure.Storage.Blob.BlobListingDetails.Metadata)
                .Where(b => b is Microsoft.WindowsAzure.Storage.Blob.CloudBlockBlob)
                .Cast<Microsoft.WindowsAzure.Storage.Blob.CloudBlockBlob>()
                .Where(b => !b.Name.EndsWith("$cmsfolder$", StringComparison.OrdinalIgnoreCase))
                .Select(cloudBlockBlob => new Blob().Initialize(cloudBlockBlob))
                .ToList();
        }
    }
}
