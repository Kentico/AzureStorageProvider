using AzureStorageProvider.Azure;
using AzureStorageProvider.Collections;
using AzureStorageProvider.Models;
using AzureStorageProvider.Tasks;
using CMS.Core;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorageProvider.Tests
{
    [TestFixture]
    public class CacheClearingTaskTests
    {
        private Blob Get(string path)
        {
            return BlobCollection.Instance.GetOrCreate(path);
        }
        private FakeServices.CloudBlobService _cloudBlobService = Service<ICloudBlobService>.Entry() as FakeServices.CloudBlobService;
        private FakeServices.BlobCacheService _blobCacheService = Service<IBlobCacheService>.Entry() as FakeServices.BlobCacheService;
        private FakeServices.CloudDirectoryService _cloudDirectoryService = Service<ICloudDirectoryService>.Entry() as FakeServices.CloudDirectoryService;
        [SetUp]
        public void SetUp()
        {
            _cloudBlobService.History.Clear();
            _cloudDirectoryService.History.Clear();
        }

        [Test]
        public void ShouldRemoveBlobsFileSystem()
        {
            _blobCacheService.SetCacheType(BlobCacheType.FileSystem);

            // add two blobs
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes("string")))
            {
                Get("AddsDataToFileCache/First.txt").Upload(ms);
            }
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes("string")))
            {
                Get("AddsDataToFileCache/NewFolder/AddsDataToFileCache.txt").Upload(ms);
            }
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes("string")))
            {
                Get("AddsDataToFileCache/NewFolder/AddsDataToFileCache2.txt").Upload(ms);
            }
            var test = BlobCollection.Instance;
            // initialize folder blobs
            BlobDirectoryCollection.Instance.GetOrCreate("AddsDataToFileCache/NewFolder").GetBlobs(true);

            Assert.AreEqual(2 + 2 + 2, _cloudBlobService.History.Count);
            Assert.AreEqual(1, _cloudDirectoryService.History.Count);
            // no additional request for exists when initialized
            BlobDirectoryCollection.Instance.GetOrCreate("AddsDataToFileCache/NewFolder").GetBlobs(true);
            BlobDirectoryCollection.Instance.GetOrCreate("AddsDataToFileCache/NewFolder/AdditionalFolder").Exists();
            Assert.AreEqual(1, _cloudDirectoryService.History.Count);

            // blob that will be outdated should be cached
            Assert.IsTrue(_blobCacheService.IsCached("AddsDataToFileCache/NewFolder/AddsDataToFileCache.txt"));

            // make subfolder blob outdated
            var blob = Get("AddsDataToFileCache/NewFolder/AddsDataToFileCache.txt");
            blob
                .GetType()
                .GetField("_lastRefresh", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                .SetValue(blob, DateTime.UtcNow.AddMinutes(-60));
            // and execute task
            new CacheClearingTask().Execute(null);

            // blob cache should be destroyed by now
            Assert.IsFalse(_blobCacheService.IsCached("AddsDataToFileCache/NewFolder/AddsDataToFileCache.txt"));

            // exists should not create request -> there is still one existing blob inside!
            Assert.IsTrue(BlobDirectoryCollection.Instance.GetOrCreate("AddsDataToFileCache/NewFolder").Exists());
            Assert.AreEqual(1, _cloudDirectoryService.History.Count);
            // subfolder should not create request - blobs were initialized in previous Exists() and no change happened in the set
            Assert.IsFalse(BlobDirectoryCollection.Instance.GetOrCreate("AddsDataToFileCache/NewFolder/AdditionalFolder").Exists());
            Assert.AreEqual(1, _cloudDirectoryService.History.Count);

            Assert.IsFalse(BlobDirectoryCollection.Instance.GetOrCreate("AddsDataToFileCache/NewFolder/AdditionalFolder2").Exists());
            Assert.AreEqual(1, _cloudDirectoryService.History.Count);
        }
    }
}
