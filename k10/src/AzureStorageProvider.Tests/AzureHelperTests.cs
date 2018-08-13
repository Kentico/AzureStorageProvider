using AzureStorageProvider.Helpers;
using AzureStorageProvider.Models;
using CMS.Helpers;
using NUnit.Framework;

namespace AzureStorageProvider.Tests
{
    [TestFixture]
    public class AzureHelperTests
    {
        [Test]
        public void TranslatesPathsToBlob()
        {
            var path = AzurePathHelper.CurrentDirectory + "\\test.txt";
            Assert.AreEqual("test.txt", AzurePathHelper.GetBlobPath(path));
        }

        [Test]
        public void GetsDirectoryForBlob()
        {
            Assert.AreEqual("dir10", AzurePathHelper.GetBlobDirectory("dir10/file.txt"));
            Assert.AreEqual("dir10/dir11", AzurePathHelper.GetBlobDirectory("dir10/dir11/file.txt"));
        }

        [Test]
        public void GetsParentFromPath()
        {
            Assert.AreEqual("dir21", AzurePathHelper.GetBlobDirectory("dir21/test.txt"));
            Assert.AreEqual(string.Empty, AzurePathHelper.GetBlobDirectory("test.txt"));
        }
        [Test]
        public void GetParentFromPathEnsuresForwardSlashes()
        {
            Assert.AreEqual("dir22/dir23", AzurePathHelper.GetBlobDirectory("dir22/dir23/test.txt"));
        }
       

    }
}
