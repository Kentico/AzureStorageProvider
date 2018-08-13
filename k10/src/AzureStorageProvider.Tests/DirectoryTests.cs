using AzureStorageProvider.Collections;
using AzureStorageProvider.Helpers;
using NUnit.Framework;
using System;
using System.Linq;

namespace AzureStorageProvider.Tests
{
    [TestFixture]
    public class DirectoryTests
    {
        /// <summary>
        /// Checks if empty folder on file system will be returned as subfolder of parent
        /// </summary>
        [Test]
        public void CreateAndGetEmptyDirectory()
        {
            var appPath = AzurePathHelper.CurrentDirectory;
            new Directory().CreateDirectory(appPath + "\\DirectoryTests\\CreateDirectory");
            new Directory().CreateDirectory(appPath + "\\DirectoryTests\\CreateDirectory\\SubDir");
            
            Assert.IsTrue(new Directory().Exists(appPath + "\\DirectoryTests\\CreateDirectory"));
            Assert.IsTrue(new Directory().Exists(appPath + "\\DirectoryTests\\CreateDirectory\\SubDir"));

            Assert.AreEqual(1, new Directory().GetDirectories(appPath + "\\DirectoryTests\\CreateDirectory").Length);
            Assert.AreEqual(new Directory().GetDirectories(appPath + "\\DirectoryTests\\CreateDirectory")[0], appPath + "\\DirectoryTests\\CreateDirectory\\SubDir");

            // cleanup
            new Directory().DeleteDirectoryStructure(appPath + "\\DirectoryTests\\CreateDirectory");
        }

        /// <summary>
        /// Checks if also system dirs are returned if there is any dir in BlobDirectoryCollection
        /// </summary>
        [Test]
        public void IncludeSystemDirIfAnyCloudDirExists()
        {
            var appPath = AzurePathHelper.CurrentDirectory;
            // create BLOB directory
            var file = new FileInfo(appPath + "\\DirectoryTests\\IncludeSystemDirIfAnyCloudDirExists\\Dir1\\test.txt");
            using (var writer = file.CreateText())
            {
                writer.Write("This is test");
            }
            
            // create system dir - simulates BLOB directory after app restart
            var systemDir = System.IO.Directory.CreateDirectory(appPath + "\\DirectoryTests\\IncludeSystemDirIfAnyCloudDirExists\\Dir2");
            Assert.AreEqual(2, new Directory().GetDirectories(appPath + "\\DirectoryTests\\IncludeSystemDirIfAnyCloudDirExists").Length);

            // cleanup
            new Directory().DeleteDirectoryStructure(appPath + "\\DirectoryTests\\IncludeSystemDirIfAnyCloudDirExists");
        }

        
        [Test]
        public void DeleteFolder()
        {
            var appPath = AzurePathHelper.CurrentDirectory;
            Assert.IsFalse(new Directory().Exists(appPath + "\\DirectoryTests\\DeleteFolder\\Dir1"));
            var file = new FileInfo(appPath + "\\DirectoryTests\\DeleteFolder\\Dir1\\test.txt");
            using (var writer = file.CreateText())
            {
                writer.Write("This is test");
            }
            Assert.IsTrue(new Directory().Exists(appPath + "\\DirectoryTests\\DeleteFolder\\Dir1"));
            Assert.IsTrue(new File().Exists(appPath + "\\DirectoryTests\\DeleteFolder\\Dir1\\test.txt"));

            new Directory().Delete(appPath + "\\DirectoryTests\\DeleteFolder\\Dir1");
            Assert.IsFalse(new File().Exists(appPath + "\\DirectoryTests\\DeleteFolder\\Dir1\\test.txt"));
            Assert.AreEqual(0, new Directory().GetDirectories(appPath + "\\DirectoryTests\\DeleteFolder").Length);

            Assert.IsFalse(new Directory().Exists(appPath + "\\DirectoryTests\\DeleteFolder\\Dir1"));
        }

        [Test]
        public void RenameFolder()
        {
            var appPath = AzurePathHelper.CurrentDirectory;

            var file = new FileInfo(appPath + "\\DirectoryTests\\RenameFolder\\Dir1\\test.txt");
            using (var writer = file.CreateText())
            {
                writer.Write("This is test");
            }
            var file2 = new FileInfo(appPath + "\\DirectoryTests\\RenameFolder\\Dir1\\Subdir1\\test.txt");
            using (var writer = file2.CreateText())
            {
                writer.Write("This is test");
            }

            new Directory().Move(appPath + "\\DirectoryTests\\RenameFolder\\Dir1", appPath + "\\DirectoryTests\\RenameFolder\\Dir2");

            Assert.IsFalse(new Directory().Exists(appPath + "\\DirectoryTests\\RenameFolder\\Dir1"));
            Assert.IsFalse(new Directory().Exists(appPath + "\\DirectoryTests\\RenameFolder\\Dir1\\Subdir1"));

            Assert.IsTrue(new Directory().Exists(appPath + "\\DirectoryTests\\RenameFolder\\Dir2"));
            Assert.IsTrue(new Directory().Exists(appPath + "\\DirectoryTests\\RenameFolder\\Dir2\\Subdir1"));
            Assert.IsTrue(new File().Exists(appPath + "\\DirectoryTests\\RenameFolder\\Dir2\\test.txt"));
            Assert.IsTrue(new File().Exists(appPath + "\\DirectoryTests\\RenameFolder\\Dir2\\Subdir1\\test.txt"));
        }

        [Test]
        public void PrepareForImport()
        {
            var appPath = AzurePathHelper.CurrentDirectory;

            var file = new FileInfo(appPath + "\\DirectoryTests\\PrepareForImport\\test.txt");
            using (var writer = file.CreateText())
            {
                writer.Write("This is test");
            }

            Assert.AreEqual(1, BlobDirectoryCollection.Instance.GetOrCreate("DirectoryTests/PrepareForImport").GetBlobs(false).Count());
            BlobCollection.Instance.GetOrCreate("DirectoryTests/PrepareForImport/test.txt").Uninitialize();
            Assert.AreEqual(0, BlobDirectoryCollection.Instance.GetOrCreate("DirectoryTests/PrepareForImport").GetBlobs(false).Count());

            new Directory().PrepareFilesForImport(appPath + "\\DirectoryTests\\PrepareForImport");
            Assert.AreEqual(1, BlobDirectoryCollection.Instance.GetOrCreate("DirectoryTests/PrepareForImport").GetBlobs(false).Count());
        }

        [Test]
        public void FoldersOnFileSystemAreLowercased()
        {
            var appPath = AzurePathHelper.CurrentDirectory;

            var temp = AzurePathHelper.ForceLowercase;
            AzurePathHelper.ForceLowercase = true;

            var file = new FileInfo(appPath + "\\DirectoryTests\\FoldersOnFileSystemAreLowercased\\Folder1\\Test.txt");
            using (var writer = file.CreateText())
            {
                writer.Write("This is test");
            }

            // at this point we have 2 files in BLOB storage

            System.IO.Directory.CreateDirectory(appPath + "\\DirectoryTests\\FoldersOnFileSystemAreLowercased\\FOLDER1");
            // we created same folder on file system, but uppercased letters

            var subdirectories = new Directory().GetDirectories(appPath + "\\DirectoryTests\\FoldersOnFileSystemAreLowercased");
            Assert.AreEqual(1, subdirectories.Count());
            Assert.AreEqual("folder1", System.IO.Path.GetFileName(subdirectories.Single()));

            AzurePathHelper.ForceLowercase = false;

            var file2 = new FileInfo(appPath + "\\DirectoryTests\\FoldersOnFileSystemAreLowercased\\Folder1\\Test.txt");
            using (var writer = file2.CreateText())
            {
                writer.Write("This is test");
            }

            subdirectories = new Directory().GetDirectories(appPath + "\\DirectoryTests\\FoldersOnFileSystemAreLowercased");
            Assert.AreEqual(2, subdirectories.Count());
            
            AzurePathHelper.ForceLowercase = temp;

            // cleanup
            new Directory().DeleteDirectoryStructure(appPath + "\\DirectoryTests\\FoldersOnFileSystemAreLowercased");
        }
    }
}
