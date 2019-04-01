using System;
using System.Text;
using System.Security.AccessControl;

using CMS.IO;
using AzureStorageProvider.Models;
using AzureStorageProvider.Helpers;
using AzureStorageProvider.Collections;

namespace AzureStorageProvider
{
    /// <summary>
    /// Sample of File class of CMS.IO provider.
    /// </summary>
    public class File : CMS.IO.AbstractFile
    {
        private Blob Get(string path)
        {
            return BlobCollection.Instance.GetOrCreate(AzurePathHelper.GetBlobPath(path));
        }
        #region "Public properties"

        /// <summary>
        /// Determines whether the specified file exists.
        /// </summary>
        /// <param name="path">Path to file.</param>  
        public override bool Exists(string path)
        {
            var exists = Get(path).Exists();

            // weird fallback from original implementation that checks whether file exists on file system
            // should be absolutely removed!!!
            return exists || System.IO.File.Exists(path);
        }


        /// <summary>
        /// Opens an existing UTF-8 encoded text file for reading.
        /// </summary>
        /// <param name="path">Path to file</param>
        public override CMS.IO.StreamReader OpenText(string path)
        {
            var stream = new FileStream(path, FileMode.Open);
            var reader = StreamReader.New(stream);
            return reader;
        }


        /// <summary>
        /// Deletes the specified file. An exception is not thrown if the specified file does not exist.
        /// </summary>
        /// <param name="path">Path to file</param>
        public override void Delete(string path)
        {
            Get(path).Delete();

            SynchronizationHelper.LogDeleteFileTask(path);
        }


        /// <summary>
        /// Copies an existing file to a new file. Overwriting a file of the same name is allowed.
        /// </summary>
        /// <param name="sourceFileName">Path to source file.</param>
        /// <param name="destFileName">Path to destination file.</param>
        /// <param name="overwrite">If destination file should be overwritten.</param>
        public override void Copy(string sourceFileName, string destFileName, bool overwrite)
        {
            Get(sourceFileName).Copy(AzurePathHelper.GetBlobPath(destFileName), overwrite);
        }


        /// <summary>
        /// Copies an existing file to a new file. Overwriting a file of the same name is not allowed.
        /// </summary>
        /// <param name="sourceFileName">Path to source file.</param>
        /// <param name="destFileName">Path to destination file.</param>        
        public override void Copy(string sourceFileName, string destFileName)
        {
            Copy(sourceFileName, destFileName, false);

            SynchronizationHelper.LogUpdateFileTask(destFileName);
        }


        /// <summary>
        /// Opens a binary file, reads the contents of the file into a byte array, and then closes the file.
        /// </summary>
        /// <param name="path">Path to file.</param>
        public override byte[] ReadAllBytes(string path)
        {
            return Get(path).Get();
        }


        /// <summary>
        /// Creates or overwrites a file in the specified path.
        /// </summary>
        /// <param name="path">Path to file.</param> 
        public override CMS.IO.FileStream Create(string path)
        {
            return new FileStream(path, FileMode.Create);
        }


        /// <summary>
        /// Moves a specified file to a new location, providing the option to specify a new file name.
        /// </summary>
        /// <param name="sourceFileName">Source file name.</param>
        /// <param name="destFileName">Destination file name.</param>
        public override void Move(string sourceFileName, string destFileName)
        {
            Get(sourceFileName).Move(AzurePathHelper.GetBlobPath(destFileName));
        }


        /// <summary>
        /// Opens a text file, reads all lines of the file, and then closes the file.
        /// </summary>
        /// <param name="path">Path to file.</param> 
        public override string ReadAllText(string path)
        {
            return ReadAllText(path, EncodingHelper.DefaultEncoding);
        }


        /// <summary>
        /// Opens a text file, reads all lines of the file, and then closes the file.
        /// </summary>
        /// <param name="path">Path to file</param> 
        /// <param name="encoding">The character encoding to use</param>
        public override string ReadAllText(string path, Encoding encoding)
        {
            if (Get(path).Exists())
                return encoding.GetString(Get(path).Get());

            // weird fallback from original implementation
            // if file does not exist, take local file instead
            // should be absolutely removed!!!
            if (System.IO.File.Exists(path))
                return System.IO.File.ReadAllText(path);

            throw new InvalidOperationException("File.ReadAllText: File " + path + " does not exist in neither BLOB nor file system.");
        }


        /// <summary>
        /// Creates a new file, write the contents to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="contents">Content to write.</param>
        public override void WriteAllText(string path, string contents)
        {
            WriteAllText(path, contents, EncodingHelper.DefaultEncoding);
        }


        /// <summary>
        /// Creates a new file, write the contents to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">Path to file</param>
        /// <param name="contents">Content to write</param>
        /// <param name="encoding">The character encoding to use</param>
        public override void WriteAllText(string path, string contents, Encoding encoding)
        {
            using (var stream = new System.IO.MemoryStream(encoding.GetBytes(contents)))
            {
                Get(path).Upload(stream);
            }

            SynchronizationHelper.LogUpdateFileTask(path);
        }


        /// <summary>
        /// Opens a file, appends the specified string to the file, and then closes the file. If the file does not exist, this method creates a file, writes the specified string to the file, then closes the file. 
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="contents">Content to write.</param>
        public override void AppendAllText(string path, string contents)
        {
            AppendAllText(path, contents, EncodingHelper.DefaultEncoding);
        }


        /// <summary>
        /// Opens a file, appends the specified string to the file, and then closes the file. If the file does not exist, this method creates a file, writes the specified string to the file, then closes the file. 
        /// </summary>
        /// <param name="path">Path</param>
        /// <param name="contents">Content to write.</param>
        /// <param name="encoding">The character encoding to use</param>
        public override void AppendAllText(string path, string contents, Encoding encoding)
        {
            var blob = Get(path);
            if (blob.Exists())
            {
                blob.Append(encoding.GetBytes(contents));

                SynchronizationHelper.LogUpdateFileTask(path);
            }
            else
            {
                WriteAllText(path, contents, encoding);
            }
        }


        /// <summary>
        /// Creates a new file, writes the specified byte array to the file, and then closes the file. If the target file already exists, it is overwritten.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <param name="bytes">Bytes to write.</param>
        public override void WriteAllBytes(string path, byte[] bytes)
        {
            using (var stream = new System.IO.MemoryStream(bytes))
            {
                Get(path).Upload(stream);
            }

            SynchronizationHelper.LogUpdateFileTask(path);
        }


        /// <summary>
        /// Opens an existing file for reading.
        /// </summary>
        /// <param name="path">Path to file.</param>
        public override CMS.IO.FileStream OpenRead(string path)
        {
            if (Exists(path))
            {
                return new FileStream(path, FileMode.Open, FileAccess.Read);
            }
            else
            {
                throw new Exception("[File.OpenRead]: Path " + path + " does not exist.");
            }
        }


        /// <summary>
        /// Sets the specified FileAttributes  of the file on the specified path.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <param name="fileAttributes">File attributes.</param>
        public override void SetAttributes(string path, CMS.IO.FileAttributes fileAttributes)
        {
            var blob = Get(path);
            blob.SetMetadataAttributeAndSave(BlobMetadataEnum.Attribute, ((int)fileAttributes).ToString());
        }


        /// <summary>
        /// Opens a FileStream  on the specified path, with the specified mode and access.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <param name="mode">File mode.</param>
        /// <param name="access">File access.</param>
        public override CMS.IO.FileStream Open(string path, CMS.IO.FileMode mode, CMS.IO.FileAccess access)
        {
            return new FileStream(path, mode, access);
        }


        /// <summary>
        /// Sets the date and time, in coordinated universal time (UTC), that the specified file was last written to.
        /// </summary>
        /// <param name="path">Path.</param>
        /// <param name="lastWriteTimeUtc">Specified time.</param>
        public override void SetLastWriteTimeUtc(string path, DateTime lastWriteTimeUtc)
        {
            SetLastWriteTime(path, lastWriteTimeUtc);
        }


        /// <summary>
        /// Creates or opens a file for writing UTF-8 encoded text.
        /// </summary>
        /// <param name="path">Path to file.</param>      
        public override CMS.IO.StreamWriter CreateText(string path)
        {
            var stream = new FileStream(path, FileMode.Create);
            return StreamWriter.New(stream);
        }


        /// <summary>
        /// Gets a FileSecurity object that encapsulates the access control list (ACL) entries for a specified directory.
        /// </summary>
        /// <param name="path">Path to file.</param>
        public override FileSecurity GetAccessControl(string path)
        {
            return new FileSecurity();
        }


        /// <summary>
        /// Returns the date and time the specified file or directory was last written to.
        /// </summary>
        /// <param name="path">Path to file.</param>
        public override DateTime GetLastWriteTime(string path)
        {
            return Get(path).GetAttribute(a => a.LastModified);
        }

        public DateTime GetCreationTime(string path)
        {
            var created = Get(path).GetMetadataAttribute(BlobMetadataEnum.DateCreated);
            if (!string.IsNullOrEmpty(created) && DateTime.TryParse(created, out DateTime createdDate))
                return createdDate;

            return DateTime.MinValue;
        }
        public FileAttributes GetFileAttributes(string path)
        {
            return (FileAttributes)Convert.ToInt32(Get(path).GetMetadataAttribute(BlobMetadataEnum.Attribute));
        }

        public long GetLength(string path)
        {
            return Get(path).GetAttribute(a => a.Length);
        }


        /// <summary>
        /// Sets the date and time that the specified file was last written to.
        /// </summary>
        /// <param name="path">Path to file.</param>
        /// <param name="lastWriteTime">Last write time.</param>
        public override void SetLastWriteTime(string path, DateTime lastWriteTime)
        {
            var blob = Get(path);
            blob.SetMetadataAttributeAndSave(BlobMetadataEnum.LastWriteTime, lastWriteTime.ToString());
        }


        /// <summary>
        /// Returns URL to file. If can be accessed directly then direct URL is generated else URL with GetFile page is generated.
        /// </summary>
        /// <param name="path">Virtual path starting with ~ or absolute path.</param>
        /// <param name="siteName">Site name.</param>
        public override string GetFileUrl(string path, string siteName)
        {
            if (path.StartsWith("~"))
            {
                path = AzurePathHelper.CurrentDirectory + path.Substring(1);
            }
            else if (!path.StartsWith(AzurePathHelper.CurrentDirectory, StringComparison.InvariantCultureIgnoreCase))
            {
                path = AzurePathHelper.CurrentDirectory + "/" + path;
            }
            return Get(path).GetUrl();
        }

        #endregion
    }
}
