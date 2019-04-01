using AzureStorageProvider.Helpers;
using NUnit.Framework;
using System;
using System.Linq;

namespace AzureStorageProvider.Tests
{
    [TestFixture]
    public class DirectoryHelperTests
    {
        [Test]
        public void GetsSinglePath()
        {
            var paths = DirectoryHelper.PathToParent("dir1/dir2", "dir1");
            Assert.AreEqual(1, paths.Count);
            Assert.AreEqual("dir1/dir2", paths.Single());
        }
        [Test]
        public void GetsMultiplePath()
        {
            var paths = DirectoryHelper.PathToParent("dir1/dir2/dir3/dir4", "dir1");
            Assert.AreEqual(3, paths.Count);
            Assert.AreEqual("dir1/dir2/dir3/dir4", paths.First());
            Assert.AreEqual("dir1/dir2", paths.Last());
        }
        [Test]
        public void GetsMultiplePathWithDifferentParent()
        {
            var paths = DirectoryHelper.PathToParent("dir1/dir2/dir3/dir4", "dir1/dir2");
            Assert.AreEqual(2, paths.Count);
            Assert.AreEqual("dir1/dir2/dir3/dir4", paths.First());
            Assert.AreEqual("dir1/dir2/dir3", paths.Last());
        }
        [Test]
        public void GetNullForSamePath()
        {
            var paths = DirectoryHelper.PathToParent("dir1/dir2", "dir1/dir2");
            Assert.AreEqual(0, paths.Count);
        }
    }
}
