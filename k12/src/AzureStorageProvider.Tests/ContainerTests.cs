using AzureStorageProvider.Helpers;
using AzureStorageProvider.Models;
using NUnit.Framework;

namespace AzureStorageProvider.Tests
{
    [TestFixture]
    public class ContainerTests
    {
        [Test]
        public void ShouldTestContainerName()
        {
            // uppercase letter
            Assert.IsFalse(BlobContainerHelper.ValidateName("testContainer"));
            // too short
            Assert.IsFalse(BlobContainerHelper.ValidateName("te"));
            // invalid format
            Assert.IsFalse(BlobContainerHelper.ValidateName("te--st"));
            // ok
            Assert.IsTrue(BlobContainerHelper.ValidateName("test-i-7-nice"));
        }
        //[Test]
        //public void ShouldFailIfContainerHasInvalidName()
        //{
        //    Assert.That(() => new BlobContainer("testContainer"), Throws.TypeOf<ArgumentOutOfRangeException>());            
        //}
        //[Test]
        //public void ShouldCreateContainer()
        //{
        //    var container = new BlobContainer("testcontainer");
        //    Assert.IsFalse(container.Exists());

        //    container.Create();

        //    Assert.IsTrue(container.Exists());

        //    // cleanup
        //    container.Delete();
        //}

        //[Test]
        //public void ShouldDeleteContainer()
        //{
        //    var container = new BlobContainer("testcontainer2");
        //    container.Create();

        //    Assert.IsTrue(container.Exists());

        //    container.Delete();

        //    Assert.IsFalse(container.Exists());
        //}
    }
}
