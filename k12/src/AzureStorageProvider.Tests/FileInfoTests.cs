using AzureStorageProvider.Collections;
using AzureStorageProvider.Helpers;
using AzureStorageProvider.Models;
using CMS.Base;
using CMS.Helpers;
using NUnit.Framework;
using System;
using System.Linq;
using System.Text;

namespace AzureStorageProvider.Tests
{
    [TestFixture]
    public class FileInfoTests
    {
        [Test]
        public void CreateFile()
        {
            var appPath = AzurePathHelper.CurrentDirectory;

            var file = new FileInfo(appPath + "\\CreateFile.txt");
            using (var writer = file.CreateText())
            {
                writer.Write("This is test");
            }

            Assert.IsTrue(BlobCollection.Instance.GetOrCreate("CreateFile.txt").Exists());
            Assert.AreEqual(EncodingHelper.DefaultEncoding.GetBytes("This is test"), BlobCollection.Instance.GetOrCreate("CreateFile.txt").Get());
            Assert.AreEqual("This is test", EncodingHelper.DefaultEncoding.GetString(BlobCollection.Instance.GetOrCreate("CreateFile.txt").Get()));
        }

        [Test]
        public void MovesFile()
        {
            var appPath = AzurePathHelper.CurrentDirectory;

            var file = new FileInfo(appPath + "\\fileToMove.txt");
            using (var writer = file.CreateText())
            {
                writer.Write("This is test 2");
            }

            file.MoveTo(appPath + "\\fileNowMoved.txt");

            var newFile = new FileInfo(appPath + "\\fileNowMoved.txt");

            Assert.IsTrue(newFile.Exists);
            Assert.IsFalse(file.Exists);

            Assert.Less(newFile.CreationTime, DateTime.Now);
            Assert.Less(newFile.LastAccessTime, DateTime.Now);
            Assert.Less(newFile.LastWriteTime, DateTime.Now);
        }

        [Test]
        public void CopiesFile()
        {
            var appPath = AzurePathHelper.CurrentDirectory;

            var file = new FileInfo(appPath + "\\fileToCopy.txt");
            using (var writer = file.CreateText())
            {
                writer.Write("This is test 3");
            }

            file.CopyTo(appPath + "\\fileCopied.txt");

            var newFile = new FileInfo(appPath + "\\fileCopied.txt");

            Assert.IsTrue(file.Exists);
            Assert.IsTrue(newFile.Exists);
        }

        [Test]
        public void DeletesFiles()
        {
            var appPath = AzurePathHelper.CurrentDirectory;

            var file = new FileInfo(appPath + "\\folderToDelete\\fileToDelete.txt");
            using (var writer = file.CreateText())
            {
                writer.Write("This is delete test");
            }
            
            Assert.AreEqual(1, new Directory().GetFiles(appPath + "\\folderToDelete").Count());
                        
            file.Delete();

            Assert.AreEqual(0, new Directory().GetFiles(appPath + "\\folderToDelete").Count());

            // No more requests!
            Assert.IsFalse(file.Exists);
            Assert.IsFalse(BlobCollection.Instance.GetOrCreate("fileToDelete.txt").Exists());
        }

        [Test]
        public void GetsUrlForFile()
        {
            var appPath = AzurePathHelper.CurrentDirectory;
            var file = new FileInfo(appPath + "\\fileToGetUrl.txt");
            using (var writer = file.CreateText())
            {
                writer.Write("This is URL test");
            }

            var urlRelative = new File().GetFileUrl("~/fileToGetUrl.txt", "DancingGoat");
            var urlAbsolute = new File().GetFileUrl(AzurePathHelper.CurrentDirectory + "/fileToGetUrl.txt", "DancingGoat");
            var urlRelativeWithoutTilda = new File().GetFileUrl("fileToGetUrl.txt", "DancingGoat");
            var urlAbsoluteWithoutCase = new File().GetFileUrl(AzurePathHelper.CurrentDirectory.ToLower() + "/fileToGetUrl.txt", "DancingGoat");

            var expected = AccountInfo.Instance.EndPoint.TrimEnd('/') + '/' + AccountInfo.Instance.RootContainer + "/fileToGetUrl.txt";

            Assert.AreEqual(expected, urlRelative);
            Assert.AreEqual(expected, urlAbsolute);
            Assert.AreEqual(expected, urlAbsoluteWithoutCase);
        }

        [Test]
        public void GetsBlobCaseInvariant()
        {
            var temp = AzurePathHelper.ForceLowercase;
            AzurePathHelper.ForceLowercase = true;

            var appPath = AzurePathHelper.CurrentDirectory;
            var file = new FileInfo(appPath + "\\GetsBlobCaseInvariant\\First.txt");
            using (var writer = file.CreateText())
            {
                writer.Write("This is test");
            }

            // to make sure we will get hit in BLOB storage
            BlobCollection.Instance.GetOrCreate(AzurePathHelper.GetBlobPath(appPath + "\\GetsBlobCaseInvariant\\First.txt")).Uninitialize();

            Assert.IsTrue(new FileInfo(appPath + "\\geTSblobcaseiNVARiant\\first.txt").Exists);

            AzurePathHelper.ForceLowercase = temp;
        }

        [Test]
        public void GetsBlobCaseInvariantIfAlreadyExistsInBlobAsLowercase()
        {
            var temp = AzurePathHelper.ForceLowercase;
            AzurePathHelper.ForceLowercase = false;

            var appPath = AzurePathHelper.CurrentDirectory;
            var file = new FileInfo(appPath + "\\getsblobcaseinvariantifalreadyexistsinblobaslowercase\\first.txt");
            using (var writer = file.CreateText())
            {
                writer.Write("This is test");
            }

            BlobCollection.Instance.GetOrCreate(AzurePathHelper.GetBlobPath(appPath + "\\getsblobcaseinvariantifalreadyexistsinblobaslowercase\\first.txt")).Uninitialize();

            AzurePathHelper.ForceLowercase = true;

            Assert.IsTrue(new FileInfo(appPath + "\\GetsBlobCaseInvariantIfAlreadyExistsInBlobAsLowercase\\First.txt").Exists);

            AzurePathHelper.ForceLowercase = temp;
        }

        [Test]
        public void DoesNotGetsBlobCaseInvariantIfAlreadyExistsInBlob()
        {
            var temp = AzurePathHelper.ForceLowercase;
            AzurePathHelper.ForceLowercase = false;

            var appPath = AzurePathHelper.CurrentDirectory;
            var file = new FileInfo(appPath + "\\GetsBlobCaseInvariantIfAlreadyExistsInBlob\\First.txt");
            using (var writer = file.CreateText())
            {
                writer.Write("This is test");
            }
            BlobCollection.Instance.GetOrCreate(AzurePathHelper.GetBlobPath(appPath + "\\GetsBlobCaseInvariantIfAlreadyExistsInBlob\\First.txt")).Uninitialize();

            AzurePathHelper.ForceLowercase = true;

            Assert.IsFalse(new FileInfo(appPath + "\\getsblobcaseinvariantifalreadyexistsinblob\\first.txt").Exists);

            AzurePathHelper.ForceLowercase = temp;
        }

        [Test]
        public void InitializesBlobAsLowercase()
        {
            var temp = AzurePathHelper.ForceLowercase;
            AzurePathHelper.ForceLowercase = false;

            var appPath = AzurePathHelper.CurrentDirectory;
            var file = new FileInfo(appPath + "\\initializesblobaslowercase\\TEST.txt");
            using (var writer = file.CreateText())
            {
                writer.Write("This is test");
            }
            BlobCollection.Instance.GetOrCreate(AzurePathHelper.GetBlobPath(appPath + "\\initializesblobaslowercase\\TEST.txt")).Uninitialize();

            AzurePathHelper.ForceLowercase = true;
            var files = BlobDirectoryCollection.Instance.GetOrCreate(AzurePathHelper.GetBlobPath(appPath + "\\initializesblobaslowercase")).GetBlobs(false);

            Assert.AreEqual(1, files.Count());
            Assert.AreEqual("test.txt", System.IO.Path.GetFileName(files.Single().Path));

            AzurePathHelper.ForceLowercase = temp;
        }

        [Test]
        public void DoesNotFailForInvalidDateFormat()
        {
            var appPath = AzurePathHelper.CurrentDirectory;
            var file = new FileInfo(appPath + "\\FileInfoTests\\DoesNotFailForInvalidDateFormat\\test.txt");
            using (var writer = file.CreateText())
            {
                writer.Write("This is test");
            }

            file = new FileInfo(appPath + "\\FileInfoTests\\DoesNotFailForInvalidDateFormat\\test.txt");
            Assert.LessOrEqual(file.CreationTime, DateTime.UtcNow);

            BlobCollection.Instance.GetOrCreate(AzurePathHelper.GetBlobPath(appPath + "\\FileInfoTests\\DoesNotFailForInvalidDateFormat\\test.txt"))
                .SetMetadataAttributeAndSave(BlobMetadataEnum.DateCreated, "invalid date");

            Assert.AreEqual(file.CreationTime, DateTime.MinValue);
        }
    }
}
