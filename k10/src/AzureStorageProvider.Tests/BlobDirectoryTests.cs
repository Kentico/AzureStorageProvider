using AzureStorageProvider.Collections;
using AzureStorageProvider.Models;
using CMS.Core;
using NUnit.Framework;
using System.Linq;

namespace AzureStorageProvider.Tests
{
    [TestFixture]
    public class BlobDirectoryTests
    {
        private Blob GetBlob(string path)
        {
            return BlobCollection.Instance.GetOrCreate(path);
        }
        private BlobDirectory Get(string path)
        {
            return BlobDirectoryCollection.Instance.GetOrCreate(path);
        }
        private FakeServices.CloudDirectoryService _cloudDirectoryService = Service<AzureStorageProvider.Azure.ICloudDirectoryService>.Entry() as FakeServices.CloudDirectoryService;
        private FakeServices.CloudBlobService _cloudBlobService = Service<AzureStorageProvider.Azure.ICloudBlobService>.Entry() as FakeServices.CloudBlobService;
        [SetUp]
        public void SetUp()
        {
            _cloudDirectoryService.History.Clear();
            _cloudBlobService.History.Clear();
        }


        [Test]
        public void ReturnsAllBlobs()
        {
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes("string")))
            {
                GetBlob("dir/dir1.txt").Upload(ms);
                GetBlob("dir/dir1/dir2.txt").Upload(ms);
            }

            var blobs = Get("dir").GetBlobs(true);
            Assert.AreEqual(1, blobs.Count());

            var blobsFlat = Get("dir").GetBlobs(false);
            Assert.AreEqual(2, blobsFlat.Count());

            var blobsSubdirectory = Get("dir/dir1").GetBlobs(true);
            Assert.AreEqual(1, blobsSubdirectory.Count());

            var blob = GetBlob("dir/dir1.txt");
            Assert.IsNotNull(blob);

            Assert.AreEqual(2+2, _cloudBlobService.History.Count);
            Assert.AreEqual(1, _cloudDirectoryService.History.Count);
        }


        [Test]
        public void ReinitializesDirectory()
        {
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes("string")))
            {
                GetBlob("BlobDirectoryTests/ReinitializesDirectory/f1.txt").Upload(ms);
                GetBlob("BlobDirectoryTests/ReinitializesDirectory/f2.txt").Upload(ms);
            }

            var blobs = Get("BlobDirectoryTests/ReinitializesDirectory").GetBlobs(true);
            Assert.AreEqual(2, blobs.Count());

            Get("BlobDirectoryTests/ReinitializesDirectory").Reinitialize();

            blobs = Get("BlobDirectoryTests/ReinitializesDirectory").GetBlobs(true);
            Assert.AreEqual(2, blobs.Count());

            Assert.AreEqual(2 + 2, _cloudBlobService.History.Count);
            Assert.AreEqual(2, _cloudDirectoryService.History.Count);
        }
        [Test]
        public void DeletesDirectory()
        {
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes("string")))
            {
                GetBlob("dir2/dir3.txt").Upload(ms);
                GetBlob("dir2/dir3/dir4.txt").Upload(ms);
                GetBlob("dir2/dir3/dir5.txt").Upload(ms);
                GetBlob("dir2/dir3/dir5/dir6.txt").Upload(ms);
            }

            // should delete dir4.txt and dir5.txt
            Get("dir2/dir3").Delete(true);
            Assert.IsFalse(GetBlob("dir2/dir3/dir4.txt").Exists());
            Assert.IsFalse(GetBlob("dir2/dir3/dir5.txt").Exists());
            Assert.IsTrue(GetBlob("dir2/dir3.txt").Exists());
            Assert.IsTrue(GetBlob("dir2/dir3/dir5/dir6.txt").Exists());

            // should delete all
            Get("dir2").Delete(false);
            Assert.IsFalse(GetBlob("dir2/dir3.txt").Exists());
            Assert.IsFalse(GetBlob("dir2/dir3/dir5/dir6.txt").Exists());

            Assert.AreEqual(8+2+2, _cloudBlobService.History.Count);
            Assert.AreEqual(2, _cloudDirectoryService.History.Count);
        }

        [Test]
        public void AddsSubdirectories()
        {
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes("string")))
            {
                GetBlob("dir3/dir1.txt").Upload(ms);
                GetBlob("dir3/dir1/dir2.txt").Upload(ms);
                GetBlob("dir3/dir1/dir2/dir3.txt").Upload(ms);
                GetBlob("dir3/dir2/dir3/dir4.txt").Upload(ms);
            }

            Assert.AreEqual(1, Get("dir3").GetBlobs(true).Count());
            Assert.IsTrue(GetBlob("dir3/dir1.txt").Exists());
            Assert.IsTrue(GetBlob("dir3/dir2/dir3/dir4.txt").Exists());

            Assert.AreEqual(2, Get("dir3/dir1").GetBlobs(false).Count());

            Assert.AreEqual(1, Get("dir3/dir2/dir3").GetBlobs(true).Count());

            Get("dir3").Delete(false);

            Assert.AreEqual(8+4, _cloudBlobService.History.Count);
            Assert.AreEqual(1, _cloudDirectoryService.History.Count);
        }

        [Test]
        public void TestExists()
        {
            Assert.IsFalse(Get("dir10").Exists());
            // must create new request!
            Assert.IsFalse(Get("dir11").Exists());

            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes("string")))
            {
                GetBlob("dir10/file.txt").Upload(ms);
                GetBlob("dir10/dir11/file.txt").Upload(ms);
            }
            
            Assert.IsTrue(Get("dir10").Exists());
            // verify the last call did not make a request
            Assert.AreEqual(2, _cloudDirectoryService.History.Count);

            // init folder
            Get("dir10").GetBlobs(false);
            // must not create request!
            Assert.IsFalse(GetBlob("dir10/fileThatDoesNotExist.txt").Exists());
            // must not create request!
            Assert.IsFalse(Get("dir10/dir12").Exists());

            // must not create request!
            Assert.IsTrue(Get("dir10/dir11").Exists());
            // must not create request
            Assert.IsFalse(Get("dir10/dir11/dir12").Exists());

            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes("string")))
            {
                GetBlob("dir10/dir11/dir12/file.txt").Upload(ms);
            }

            // no more requests!
            Assert.IsTrue(Get("dir10/dir11").Exists());
            Assert.IsTrue(Get("dir10/dir11/dir12").Exists());
            Assert.IsTrue(GetBlob("dir10/dir11/dir12/file.txt").Exists());

            Assert.AreEqual(1+1+1, _cloudBlobService.History.Count);
            Assert.AreEqual(2, _cloudDirectoryService.History.Count);
        }


        [Test]
        public void GetsSubdirectories()
        {
            Assert.IsFalse(Get("dir20").Exists());
            // must create new request!
            Assert.IsFalse(Get("dir21").Exists());
            // 2 requests here

            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes("string")))
            {
                GetBlob("dir20/dir21/dir22/file.txt").Upload(ms);
            }
            // 2 requests for blob upload here

            // no more requests
            Assert.IsTrue(Get("dir20").Exists());
            Assert.IsTrue(Get("dir20/dir21").Exists());

            // verify the last 2 calls did not make a request
            Assert.AreEqual(2, _cloudDirectoryService.History.Count);
            Assert.AreEqual(1, _cloudBlobService.History.Count);

            Assert.AreEqual(1, Get("dir20/dir21").GetSubdirectories(true).Count());
            Assert.AreEqual("dir20/dir21/dir22", Get("dir20/dir21").GetSubdirectories(true).Single().Path);

            // check that after delete of folder, getsubdirectories returns 0
            Get("dir20/dir21/dir22").Delete(true);
            Assert.AreEqual(0, Get("dir20/dir21").GetSubdirectories(true).Count());

            // delete should do 1 delete request on file
            Assert.AreEqual(1 + 1, _cloudBlobService.History.Count);
            // and 0 on folder
            Assert.AreEqual(2, _cloudDirectoryService.History.Count);
        }

        [Test]
        public void DeleteFolder()
        {
            Assert.IsFalse(Get("BlobDirectoryTests/DeleteFolder").Exists());
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes("string")))
            {
                GetBlob("BlobDirectoryTests/DeleteFolder/test.txt").Upload(ms);
            }
            Assert.IsTrue(Get("BlobDirectoryTests/DeleteFolder").Exists());

            Get("BlobDirectoryTests/DeleteFolder").Delete(false);
            Assert.IsFalse(Get("BlobDirectoryTests/DeleteFolder").Exists());
        }

        [Test]
        public void HandlesFolderWithSpecialCharacters()
        {
            using (var ms = new System.IO.MemoryStream(System.Text.Encoding.UTF8.GetBytes("string")))
            {
                GetBlob("BlobDirectoryTests/HandlesFolderWithSpecialCharacters/new folder/test.txt").Upload(ms);
            }
            Assert.IsTrue(Get("BlobDirectoryTests/HandlesFolderWithSpecialCharacters/new folder").Exists());
            Assert.AreEqual("BlobDirectoryTests/HandlesFolderWithSpecialCharacters/new folder", Get("BlobDirectoryTests/HandlesFolderWithSpecialCharacters").GetSubdirectories(false).Single().Path);
        }
    }
}
