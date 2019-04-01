using AzureStorageProvider.Azure;
using AzureStorageProvider.Collections;
using AzureStorageProvider.Helpers;
using CMS.Core;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AzureStorageProvider.Models
{
    public class BlobDirectory : IObjectWithPath<BlobDirectory>
    {
        private object _lock = new object();
        private bool _blobsInitialized = false;
        private bool? _exists = null;
        private ICloudDirectoryService _cloudDirectoryService = Service.Resolve<ICloudDirectoryService>();

        public string Path
        {
            get;
            protected set;
        }

        public bool BlobsInitialized => _blobsInitialized;
        
        public BlobDirectory()
        {
        }

        protected void TryInitializeFromParent()
        {
            if (_blobsInitialized)
                return;

            // if there is initialized folder in the hierarchy, no need to initialize again
            var paths = DirectoryHelper.PathToParent(Path, string.Empty);
            if (paths.Any(p => {
                var dir = BlobDirectoryCollection.Instance.TryGet(p);
                return dir != null && dir.BlobsInitialized;
            }))
            {
                lock (_lock)
                {
                    _blobsInitialized = true;
                }
                return;
            }
        }

        protected void InitializeBlobs(bool forceRefresh = false)
        {
            lock (_lock)
            {
                InitializeBlobsInternal(forceRefresh);
            }
        }
        protected void InitializeBlobsInternal(bool forceRefresh = false)
        {
            if (!forceRefresh)
                TryInitializeFromParent();

            if (!forceRefresh && BlobsInitialized)
            {
                _exists = BlobCollection.Instance.GetStartingWith(Path + "/", false).Any(b => b.Exists());
                return;
            }
            
            // at this point we need to get data from remote
            var blobs = _cloudDirectoryService.GetBlobs(Path);

            if (LoggingHelper.LogsEnabled)
            {
                LoggingHelper.Log($"Blobs for path {Path}", $"{string.Join(",", blobs.Select(b => b.Path))}");
            }

            BlobCollection.Instance.AddRangeDistinct(blobs);
            _blobsInitialized = true;
            _exists = blobs.Any();

            // update directories in collection
            var subDirectoriesPaths = new List<string>();
            blobs
                .Select(b => AzurePathHelper.GetBlobDirectory(b.Path))
                .Distinct()
                .Where(d => d != Path)
                .ToList()
                .ForEach(p => subDirectoriesPaths.AddRange(DirectoryHelper.PathToParent(p, Path)));

            var subDirectories = subDirectoriesPaths
                .Distinct()
                .Select(d => new BlobDirectory().InitializeWithFlag(d))
                .ToList();

            BlobDirectoryCollection.Instance.AddRangeDistinct(subDirectories);
        }

        public BlobDirectory Initialize(string directoryPath)
        {
            Path = directoryPath;
            TryInitializeFromParent();

            return this;
        }
        public void ResetExists()
        {
            if (_exists.HasValue)
            {
                lock (_lock)
                {
                    _exists = null;
                    if (BlobsInitialized)
                    {
                        ExistsInternal();
                    }
                }
                if (!string.IsNullOrEmpty(Path))
                {
                    ResetExistsForParents();
                }
            }
        }
        public void ResetExistsForParents()
        {
            var blobDirectory = AzurePathHelper.GetBlobDirectory(Path);
            if (blobDirectory != string.Empty)
            {
                if (_exists.HasValue && _exists.Value)
                {
                    BlobDirectoryCollection.Instance.GetOrCreate(blobDirectory).SetExists();
                }
                else
                {
                    BlobDirectoryCollection.Instance.GetOrCreate(blobDirectory).ResetExists();
                }
            }
        }
        public void SetExists()
        {
            lock (_lock)
            {
                _exists = true;
            }

            ResetExistsForParents();
        }

        public BlobDirectory Reinitialize()
        {
            GetBlobs(false).ToList().ForEach(b => b.Uninitialize());
            GetSubdirectories(false).ToList().ForEach(d => d.Uninitialize());
            InitializeBlobs(true);
            ResetExistsForParents();

            return this;
        }
        protected void Uninitialize()
        {
            lock (_lock)
            {
                _blobsInitialized = false;
                _exists = false;
            }
        }
        protected BlobDirectory InitializeWithFlag(string directoryPath)
        {
            lock (_lock)
            {
                _blobsInitialized = true;
                _exists = true;
            }

            return Initialize(directoryPath);
        }

        public IEnumerable<Blob> GetBlobs(bool flat)
        {
            if (!_blobsInitialized)
                InitializeBlobs();

            return BlobCollection.Instance.GetStartingWith(Path + "/", flat).Where(b => b.Exists());
        }

        public void Delete(bool flat)
        {
            if (SynchronizationHelper.Synchronizing())
            {
                Reinitialize();
            }
            else
            {
                GetBlobs(flat).ToList().ForEach(b => b.Delete());
                ResetExists();
            }
        }

        public bool Exists()
        {
            lock (_lock)
            {
                return ExistsInternal();
            }
        }

        public bool ExistsInternal()
        {
            if (_exists.HasValue)
                return _exists.Value;

            // if any blob below current directory exists, folder must exist too
            if (BlobCollection.Instance.GetStartingWith(Path + "/", false).Any(b => b.Exists()))
            {
                _exists = true;
                return true;
            }

            // if there is no blob, find out if any parent has already been initialized
            var parents = DirectoryHelper.PathToParent(Path, string.Empty);
            if (parents.Any(p => BlobDirectoryCollection.Instance.GetOrCreate(p).BlobsInitialized))
            {
                _exists = false;
                return false;
            }

            // otherwise we need to initialize
            if (!_blobsInitialized)
            {
                InitializeBlobsInternal(false);
                return _exists.Value;
            }

            // at this point we know the DIR does not exist
            _exists = false;
            return false;
        }

        public IEnumerable<BlobDirectory> GetSubdirectories(bool flat)
        {
            if (!_blobsInitialized)
                InitializeBlobs();

            var dirsPaths = new List<string>();
            BlobCollection.Instance.GetStartingWith(Path + "/", flat)
                .Where(b => b.Exists())
                .Select(b => AzurePathHelper.GetBlobDirectory(b.Path))
                .Union(BlobDirectoryCollection.Instance.GetStartingWith(Path + "/", flat).Where(d => d.Exists()).Select(d => d.Path))
                .Distinct()
                .ToList()
                .ForEach(b => dirsPaths.AddRange(DirectoryHelper.PathToParent(b, Path)));

            dirsPaths = dirsPaths.Distinct().ToList();
            if (flat)
                dirsPaths = dirsPaths.Where(d => AzurePathHelper.GetBlobDirectory(d) == Path).ToList();

            return dirsPaths.Select(p => BlobDirectoryCollection.Instance.GetOrCreate(p));
        }

        public DateTime GetLastWriteTime()
        {
            return GetBlobs(false)
                .Select(b => b.GetAttribute(a => a.LastModified))
                .DefaultIfEmpty(DateTime.MinValue)
                .Max();
        }
    }
}
