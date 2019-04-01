using AzureStorageProvider.Helpers;
using NUnit.Framework;
using System;
using System.Linq;

namespace AzureStorageProvider.Tests
{
    [TestFixture]
    public class DirectoryInfoTests
    {
        [Test]
        public void DirectoryDoesNotExist()
        {
            var appPath = AzurePathHelper.CurrentDirectory;
            var dir = new DirectoryInfo(appPath + "\\directory");
            Assert.IsFalse(dir.Exists);
            Assert.AreEqual(dir.LastWriteTime, DateTime.MinValue);
        }

        [Test]
        public void DirectoryExists()
        {
            var appPath = AzurePathHelper.CurrentDirectory;

            var dir = new DirectoryInfo(appPath + "\\directory2");
            var file = new FileInfo(appPath + "\\directory2\\file.txt");
            using (var writer = file.CreateText())
            {
                writer.Write("This is test dir 2");
            }

            Assert.IsTrue(dir.Exists);
            Assert.Greater(dir.LastWriteTime, DateTime.MinValue);
        }

        [Test]
        public void ReturnsParent()
        {
            var appPath = AzurePathHelper.CurrentDirectory;

            var file = new FileInfo(appPath + "\\directory3\\directory4\\file.txt");
            using (var writer = file.CreateText())
            {
                writer.Write("This is test 3");
            }

            var dir = new DirectoryInfo(appPath + "\\directory3\\directory4");
            var parent = dir.Parent;

            Assert.AreEqual("directory3", parent.Name);
            Assert.IsTrue(parent.Exists);
        }

        [Test]
        public void DeletesFolder()
        {
            var appPath = AzurePathHelper.CurrentDirectory;

            var file = new FileInfo(appPath + "\\directory5\\directory6\\fileToDelete.txt");
            using (var writer = file.CreateText())
            {
                writer.Write("This is delete test");
            }

            var dir = new DirectoryInfo(appPath + "\\directory5");
            dir.Delete();

            var subDir = new DirectoryInfo(appPath + "\\directory5\\directory6");
            
            Assert.IsFalse(file.Exists);
            Assert.IsFalse(dir.Exists);
            Assert.IsFalse(subDir.Exists);
        }

        [Test]
        public void GetsAllSubdirectoriesAndFiles()
        {
            var appPath = AzurePathHelper.CurrentDirectory;

            var file = new FileInfo(appPath + "\\directory7\\directory8\\file8.txt");
            using (var writer = file.CreateText())
            {
                writer.Write("This is delete test");
            }
            var file2 = new FileInfo(appPath + "\\directory7\\directory9\\file9.txt");
            using (var writer = file2.CreateText())
            {
                writer.Write("This is delete test");
            }
            var file3 = new FileInfo(appPath + "\\directory7\\directory9\\directory10\\file10.txt");
            using (var writer = file3.CreateText())
            {
                writer.Write("This is delete test");
            }

            var dir = new DirectoryInfo(appPath + "\\directory7");
            var subDirs = dir.GetDirectories().ToList().Select(d => d.FullName);
            Assert.IsTrue(subDirs.Contains(dir.FullName + "\\directory8"));
            Assert.IsTrue(subDirs.Contains(dir.FullName + "\\directory9"));
            Assert.IsFalse(subDirs.Contains(dir.FullName + "\\directory9\\directory10"));

            subDirs = dir.GetDirectories(string.Empty, CMS.IO.SearchOption.AllDirectories).ToList().Select(d => d.FullName);
            Assert.IsTrue(subDirs.Contains(dir.FullName + "\\directory9\\directory10"));

            var files = dir.GetFiles(string.Empty, CMS.IO.SearchOption.TopDirectoryOnly).ToList().Select(d => d.FullName);
            Assert.IsFalse(files.Contains(dir.FullName + "\\directory8\\file8.txt"));

            files = dir.GetFiles(string.Empty, CMS.IO.SearchOption.AllDirectories).ToList().Select(d => d.FullName);
            Assert.IsTrue(files.Contains(dir.FullName + "\\directory8\\file8.txt"));
            Assert.IsTrue(files.Contains(dir.FullName + "\\directory9\\file9.txt"));
            Assert.IsTrue(files.Contains(dir.FullName + "\\directory9\\directory10\\file10.txt"));
        }

        [Test]
        public void CreatesDirectory()
        {
            var appPath = AzurePathHelper.CurrentDirectory;
            var dir = new Directory().CreateDirectory(appPath + "\\directoryInfo50");

            Assert.IsTrue(new DirectoryInfo(appPath + "\\directoryInfo50").Exists);

            // cleanup
            new Directory().DeleteDirectoryStructure(appPath + "\\directoryInfo50");
        }

        [Test]
        public void CreatesSubdirectory()
        {
            var appPath = AzurePathHelper.CurrentDirectory;
            var dir = new Directory().CreateDirectory(appPath + "\\directoryInfo51");

            dir.CreateSubdirectory("directoryInfo51.1");

            Assert.IsTrue(new DirectoryInfo(appPath + "\\directoryInfo51\\directoryInfo51.1").Exists);

            // cleanup
            new Directory().DeleteDirectoryStructure(appPath + "\\directoryInfo51");
        }

        [Test]
        public void DeletesDirectory()
        {
            var appPath = AzurePathHelper.CurrentDirectory;
            var dir = new Directory().CreateDirectory(appPath + "\\directoryInfo52");
            dir.CreateSubdirectory("directoryInfo52.1");
            dir.CreateSubdirectory("directoryInfo52.2");

            new Directory().Delete(appPath + "\\directoryInfo52\\directoryInfo52.2");
            Assert.IsFalse(new DirectoryInfo(appPath + "\\directoryInfo52\\directoryInfo52.2").Exists);

            new Directory().DeleteDirectoryStructure(appPath + "\\directoryInfo52");
            Assert.IsFalse(new DirectoryInfo(appPath + "\\directoryInfo52\\directoryInfo52.1").Exists);
            Assert.IsFalse(new DirectoryInfo(appPath + "\\directoryInfo52").Exists);
        }
    }
}
