using AzureStorageProvider.Collections;
using AzureStorageProvider.Helpers;
using AzureStorageProvider.Models;
using NUnit.Framework;

namespace AzureStorageProvider.Tests
{
    [TestFixture]
    public class FileStreamTests
    {
        private Blob Get(string path)
        {
            return BlobCollection.Instance.GetOrCreate(AzurePathHelper.GetBlobPath(path));
        }

        [Test]
        public void ShouldCreateFile()
        {
            var appPath = AzurePathHelper.CurrentDirectory;
            var stream = new FileStream(appPath + "\\ShouldCreateFile.txt", CMS.IO.FileMode.CreateNew);
            stream.Write(EncodingHelper.DefaultEncoding.GetBytes("string".ToCharArray()), 0, 6);
            stream.Close();

            Assert.IsTrue(Get(appPath + "\\ShouldCreateFile.txt").Exists());
        }

        [Test]
        public void ShouldReCreateFileContent()
        {
            var appPath = AzurePathHelper.CurrentDirectory;
            var blob = Get(appPath + "\\ShouldReCreateFileContent.txt");

            var file = new FileInfo(appPath + "\\ShouldReCreateFileContent.txt");
            using (var writer = file.CreateText())
            {
                writer.Write("string");
            }
            Assert.AreEqual("string", EncodingHelper.DefaultEncoding.GetString(blob.Get()));

            using (var fs = new FileStream(appPath + "\\ShouldReCreateFileContent.txt", CMS.IO.FileMode.Create))
            {
                // File stream writer
                using (var sw = CMS.IO.StreamWriter.New(fs))
                {
                    sw.Write("newstring");
                }
            }

            Assert.IsTrue(blob.Exists());
            Assert.AreEqual("newstring", EncodingHelper.DefaultEncoding.GetString(blob.Get()));
        }

        [Test]
        public void ShouldGetContent()
        {
            var appPath = AzurePathHelper.CurrentDirectory;
            var blob = Get(appPath + "\\ShouldGetContent.txt");

            var file = new FileInfo(appPath + "\\ShouldGetContent.txt");
            using (var writer = file.CreateText())
            {
                writer.Write("string");
            }

            string data;
            using (var fs = new FileStream(appPath + "\\ShouldGetContent.txt", CMS.IO.FileMode.Open))
            {
                // File stream writer
                using (var sw = CMS.IO.StreamReader.New(fs))
                {
                    data = sw.ReadToEnd();
                }
            }
            
            Assert.AreEqual("string", data);
        }
    }
}
