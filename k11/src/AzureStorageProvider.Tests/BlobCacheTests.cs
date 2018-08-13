using AzureStorageProvider.Azure;
using AzureStorageProvider.Collections;
using AzureStorageProvider.Helpers;
using AzureStorageProvider.Models;
using CMS.Core;
using NUnit.Framework;
using System;
using System.Linq;

namespace AzureStorageProvider.Tests
{
    [TestFixture]
    public class BlobCacheTests
    {
        private Blob Get(string path)
        {
            return BlobCollection.Instance.GetOrCreate(path);
        }
        private FakeServices.CloudBlobService _cloudBlobService = Service<ICloudBlobService>.Entry() as FakeServices.CloudBlobService;
        private FakeServices.BlobCacheService _blobCacheService = Service<IBlobCacheService>.Entry() as FakeServices.BlobCacheService;
        [SetUp]
        public void SetUp()
        {
            _cloudBlobService.History.Clear();
        }

        [Test]
        public void AddsDataToMemoryCache()
        {
            _blobCacheService.SetCacheType(BlobCacheType.Memory);
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes("string")))
            {
                Get("AddsDataToMemoryCache.txt").Upload(ms);
            }

            // should be in cache by now
            Assert.AreEqual("string", System.Text.Encoding.Default.GetString(_blobCacheService.Get("AddsDataToMemoryCache.txt", DateTime.UtcNow.AddHours(-1))));
            // must not create any request to BLOB storage
            Assert.AreEqual(2, _cloudBlobService.History.Count);
        }

        [Test]
        public void AddsDataToFileCache()
        {
            _blobCacheService.SetCacheType(BlobCacheType.FileSystem);
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes("string")))
            {
                Get("AddsDataToFileCache.txt").Upload(ms);
            }

            // should be in cache by now
            Assert.AreEqual("string", System.Text.Encoding.Default.GetString(_blobCacheService.Get("AddsDataToFileCache.txt", DateTime.UtcNow.AddHours(-1))));
            // must not create any request to BLOB storage
            Assert.AreEqual(2, _cloudBlobService.History.Count);

            // clean up
            Get("AddsDataToFileCache.txt").Delete();
            _blobCacheService.SetCacheType(BlobCacheType.None);
        }

        [Test]
        public void UpdatesDataMemory()
        {
            _blobCacheService.SetCacheType(BlobCacheType.Memory);

            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes("string")))
            {
                Get("UpdatesDataMemory.txt").Upload(ms);
            }

            Assert.AreEqual("string", EncodingHelper.DefaultEncoding.GetString(_blobCacheService.Get("UpdatesDataMemory.txt", DateTime.UtcNow.AddHours(-1))));
            
            Get("UpdatesDataMemory.txt").Append(System.Text.Encoding.UTF8.GetBytes("new"));

            Assert.AreEqual("stringnew", EncodingHelper.DefaultEncoding.GetString(_blobCacheService.Get("UpdatesDataMemory.txt", DateTime.UtcNow.AddHours(-1))));

            // must not create any request to BLOB storage
            Assert.AreEqual(3, _cloudBlobService.History.Count, string.Join(",", _cloudBlobService.History.Select(k => k.Key)));

            _blobCacheService.SetCacheType(BlobCacheType.None);
        }
        [Test]
        public void UpdatesDataFileSystem()
        {
            lock (_blobCacheService.Lock)
            {
                _blobCacheService.SetCacheType(BlobCacheType.FileSystem);

                using (var ms = new System.IO.MemoryStream(EncodingHelper.DefaultEncoding.GetBytes("string")))
                {
                    Get("UpdatesDataFileSystem.txt").Upload(ms);
                }

                Get("UpdatesDataFileSystem.txt").Append(EncodingHelper.DefaultEncoding.GetBytes("new"));

                Assert.AreEqual("stringnew", EncodingHelper.DefaultEncoding.GetString(_blobCacheService.Get("UpdatesDataFileSystem.txt", DateTime.UtcNow.AddHours(-1))));

                // must not create any request to BLOB storage
                Assert.AreEqual(3, _cloudBlobService.History.Count, string.Join(",", _cloudBlobService.History.Select(k => k.Key)));

                // clean up
                Get("UpdatesDataFileSystem.txt").Delete();
                _blobCacheService.SetCacheType(BlobCacheType.None);
            }
        }
        [Test]
        public void RemovesCachedFile()
        {
            _blobCacheService.SetCacheType(BlobCacheType.FileSystem);

            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes("string")))
            {
                Get("RemovesCachedFile.txt").Upload(ms);
            }

            var cachePath = AzurePathHelper.GetTempBlobPath(Get("RemovesCachedFile.txt").Path);
            // cache was created in file system
            Assert.IsTrue(System.IO.File.Exists(cachePath));

            Get("RemovesCachedFile.txt").Delete();

            // cache was removed
            Assert.IsFalse(System.IO.File.Exists(cachePath));

            Assert.AreEqual(3, _cloudBlobService.History.Count);
        }
    }
}
