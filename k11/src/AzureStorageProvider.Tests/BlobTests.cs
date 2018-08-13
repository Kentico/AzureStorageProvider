using AzureStorageProvider.Collections;
using AzureStorageProvider.Helpers;
using AzureStorageProvider.Models;
using CMS.Core;
using NUnit.Framework;
using System;

namespace AzureStorageProvider.Tests
{
    [TestFixture]
    public class BlobTests
    {
        private Blob Get(string path)
        {
            return BlobCollection.Instance.GetOrCreate(path);
        }
        private FakeServices.CloudBlobService _cloudBlobService = Service<AzureStorageProvider.Azure.ICloudBlobService>.Entry() as FakeServices.CloudBlobService;
        [SetUp]
        public void SetUp()
        {
            _cloudBlobService.History.Clear();
        }


        [Test]
        public void FailsOnNonExistingBlob()
        {
            Assert.IsFalse(Get("FailsOnNonExistingBlob.txt").Exists());
            Assert.IsFalse(Get("FailsOnNonExistingBlob/blob.txt").Exists());

            Assert.AreEqual(2, _cloudBlobService.History.Count);
        }

        [Test]
        public void PassesOnExistingBlob()
        {
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes("string")))
            {
                Get("PassesOnExistingBlob.txt").Upload(ms);
            }

            Assert.IsTrue(Get("PassesOnExistingBlob.txt").Exists());
            Assert.Less(DateTime.Parse(Get("PassesOnExistingBlob.txt").GetMetadataAttribute(BlobMetadataEnum.DateCreated)), DateTime.Now);

            Get("PassesOnExistingBlob.txt").Delete();

            Assert.AreEqual(3, _cloudBlobService.History.Count);
        }

        [Test]
        public void CopiesBlob()
        {
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes("string")))
            {
                Get("CopiesBlob.txt").Upload(ms);
            }

            Assert.IsFalse(Get("CopiesBlob2.txt").Exists());

            Get("CopiesBlob.txt").Copy("CopiesBlob2.txt", false);

            // check that calling Exists on new BLOB does NOT create request
            Assert.AreEqual(2+1+1, _cloudBlobService.History.Count);
            // but metadata will
            Assert.Greater(DateTime.Parse(Get("CopiesBlob2.txt").GetMetadataAttribute(BlobMetadataEnum.DateCreated)), DateTime.MinValue);
            Assert.AreEqual(4+1, _cloudBlobService.History.Count);

            Assert.IsTrue(Get("CopiesBlob.txt").Exists());
            Assert.IsTrue(Get("CopiesBlob2.txt").Exists());

            Assert.AreEqual(5, _cloudBlobService.History.Count);
        }

        [Test]
        public void DeletesBlob()
        {
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes("string")))
            {
                Get("DeletesBlob.txt").Upload(ms);
            }

            Get("DeletesBlob.txt").Delete();

            Assert.IsFalse(Get("DeletesBlob.txt").Exists());
            Assert.IsFalse(Get("DeletesBlob.txt").Exists());

            Assert.AreEqual(2+1, _cloudBlobService.History.Count);
        }

        [Test]
        public void GetsBlobUri()
        {
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes("string")))
            {
                Get("GetsBlobUri/GetsBlobUri.txt").Upload(ms);
            }

            var uri = Get("GetsBlobUri/GetsBlobUri.txt").GetUrl();
            
            Assert.IsNotNull(uri);

            Assert.AreEqual(2, _cloudBlobService.History.Count);
        }
    }
}
