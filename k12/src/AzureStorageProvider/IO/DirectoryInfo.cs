using System;
using System.Collections.Generic;
using System.Linq;
using CMS.IO;
using AzureStorageProvider.Helpers;

namespace AzureStorageProvider
{
    /// <summary>
    /// Sample of DirectoryInfo class object of CMS.IO provider.
    /// </summary>
    public class DirectoryInfo : CMS.IO.DirectoryInfo
    {
        private DateTime _creationTime;
        private string _fullName;
        private string _name;
        public override DateTime CreationTime { get { return _creationTime; } set { _creationTime = value; } }
        public override bool Exists
        {
            get
            {
                return new Directory().Exists(FullName);
            }
            set
            {
                return;
            }
        }
        public override string FullName { get { return _fullName; } set { _fullName = value; } }
        public override DateTime LastWriteTime
        {
            get
            {
                return new Directory().GetLastWriteTime(FullName);
            }
            set
            {
                return;
            }
        }
        public override string Name { get { return _name; } set { _name = value; } }

        public override CMS.IO.DirectoryInfo Parent
        {
            get
            {
                var parentPath = CMS.IO.Path.GetDirectoryName(FullName);
                return new DirectoryInfo(parentPath);
            }
        }

        public DirectoryInfo(string path)
        {
            Name = CMS.IO.Path.GetFileName(path);
            FullName = path;
        }

        protected override CMS.IO.DirectoryInfo CreateSubdirectoryInternal(string subdir)
        {
            new Directory().CreateDirectory(FullName + "\\" + subdir);

            return new DirectoryInfo(FullName + "\\" + subdir);
        }

        protected override void DeleteInternal()
        {
            new Directory().Delete(FullName, true);

            SynchronizationHelper.LogDirectoryDeleteTask(FullName);
        }

        protected override CMS.IO.DirectoryInfo[] GetDirectoriesInternal(string searchPattern, CMS.IO.SearchOption searchOption)
        {
            return new Directory().GetDirectories(FullName, searchPattern, searchOption).Select(d => new DirectoryInfo(d)).ToArray();
        }

        protected override CMS.IO.FileInfo[] GetFilesInternal(string searchPattern, CMS.IO.SearchOption searchOption)
        {
            return new Directory().GetFiles(FullName, searchPattern, searchOption).Select(f => new FileInfo(f)).ToArray();
        }

        protected override IEnumerable<CMS.IO.FileInfo> EnumerateFilesInternal(string searchPattern, SearchOption searchOption)
        {
            throw new NotImplementedException();
        }

        protected override IEnumerable<CMS.IO.DirectoryInfo> EnumerateDirectoriesInternal(string searchPattern, SearchOption searchOption)
        {
            throw new NotImplementedException();
        }
    }
}
