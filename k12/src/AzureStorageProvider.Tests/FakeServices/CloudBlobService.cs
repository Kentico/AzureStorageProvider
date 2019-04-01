using System;
using System.Collections.Generic;
using System.IO;
using AzureStorageProvider.Models;
using CMS;

[assembly: RegisterImplementation(typeof(AzureStorageProvider.Azure.ICloudBlobService), typeof(AzureStorageProvider.Tests.FakeServices.CloudBlobService))]
namespace AzureStorageProvider.Tests.FakeServices
{
    public class CloudBlobService : AzureStorageProvider.Azure.CloudBlobService
    {
        public readonly List<KeyValuePair<string, DateTime>> History = new List<KeyValuePair<string, DateTime>>();
        private void Add(string methodName)
        {
            History.Add(new KeyValuePair<string, DateTime>(methodName, DateTime.UtcNow));
        }

        public override bool Exists(string path)
        {
            Add(nameof(Exists));
            return base.Exists(path);
        }
        public override BlobAttributes FetchAttributes(string path)
        {
            Add(nameof(FetchAttributes));
            return base.FetchAttributes(path);
        }
        public override void SetMetadataAsync(string path, IDictionary<string, string> metadata)
        {
            Add(nameof(SetMetadataAsync));
            base.SetMetadataAsync(path, metadata);
        }
        public override void Delete(string path)
        {
            Add(nameof(Delete));
            base.Delete(path);
        }
        public override void Copy(string path, string targetPath)
        {
            Add(nameof(Copy));
            base.Copy(path, targetPath);
        }
        public override byte[] Download(string path)
        {
            Add(nameof(Download));
            return base.Download(path);
        }
        public override BlobAttributes Upload(string path, IDictionary<string, string> metadata, Stream stream)
        {
            Add(nameof(Upload));
            return base.Upload(path, metadata, stream);
        }
    }
}
