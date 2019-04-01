using CMS;
using System;
using System.Collections.Generic;
using AzureStorageProvider.Models;

[assembly: RegisterImplementation(typeof(AzureStorageProvider.Azure.ICloudDirectoryService), typeof(AzureStorageProvider.Tests.FakeServices.CloudDirectoryService))]
namespace AzureStorageProvider.Tests.FakeServices
{
    public class CloudDirectoryService : AzureStorageProvider.Azure.CloudDirectoryService
    {
        public readonly List<KeyValuePair<string, DateTime>> History = new List<KeyValuePair<string, DateTime>>();
        private void Add(string methodName)
        {
            History.Add(new KeyValuePair<string, DateTime>(methodName, DateTime.UtcNow));
        }
        
        public override List<Blob> GetBlobs(string path)
        {
            Add(nameof(GetBlobs));

            return base.GetBlobs(path);
        }
    }
}
