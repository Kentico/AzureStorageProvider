using AzureStorageProvider.Azure;
using AzureStorageProvider.Helpers;
using AzureStorageProvider.Models;
using CMS;
using CMS.Core;
using CMS.Helpers;
using System;
using System.Linq;

[assembly: RegisterImplementation(typeof(IBlobCacheService), typeof(BlobCacheService))]

namespace AzureStorageProvider.Azure
{
    public class BlobCacheService : IBlobCacheService
    {
        private static readonly string[] _excludedPaths = { "App_Data" };
        private ICloudBlobService _cloudBlobService = Service.Resolve<ICloudBlobService>();
        protected BlobCacheType _cacheType = AccountInfo.Instance.BlobCacheType;
        
        public BlobCacheType CacheType => _cacheType;
        public byte[] Get(string path, DateTime remoteLastModified)
        {
            if (_cacheType == BlobCacheType.None || _excludedPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
                return GetFromRemote(path);

            byte[] data = null;
            
            switch (CacheType)
            {
                case BlobCacheType.Memory:
                    data = CacheHelper.Cache(cs => GetFromRemote(cs, path), new CacheSettings(AccountInfo.Instance.BlobCacheMinutes, BlobCacheHelper.GetCacheKey(path)));
                    break;

                case BlobCacheType.FileSystem:
                    data = TryGetFromFileSystemCache(path, remoteLastModified);

                    if (data == null)
                    {
                        data = GetFromRemote(path);
                        AddToCache(path, data, DateTime.UtcNow);
                    }
                    break;
            }

            return data;
        }

        private byte[] GetFromRemote(CacheSettings cs, string path)
        {
            var data = GetFromRemote(path);

            if (cs.Cached)
            {
                cs.CacheDependency = CacheHelper.GetCacheDependency(BlobCacheHelper.GetCacheDependency(path));
            }

            return data;
        }

        private byte[] GetFromRemote(string path)
        {
            return _cloudBlobService.Download(path);
        }

        public void Discard(string path)
        {
            switch (_cacheType)
            {
                case BlobCacheType.Memory:
                    CacheHelper.TouchKey(BlobCacheHelper.GetCacheDependency(path));
                    break;

                case BlobCacheType.FileSystem:
                    var tempFilePath = AzurePathHelper.GetTempBlobPath(path);
                    if (System.IO.File.Exists(tempFilePath))
                        System.IO.File.Delete(tempFilePath);
                    break;
            }
        }

        public void Add(string path, System.IO.Stream stream, DateTime created)
        {
            if (_cacheType == BlobCacheType.None)
                return;

            var data = new byte[stream.Length];
            stream.Read(data, 0, data.Length);

            AddToCache(path, data, created);
        }

        private byte[] TryGetFromFileSystemCache(string path, DateTime remoteLastModified)
        {
            byte[] data = null;
            var fileLastModified = DateTime.MinValue;

            var tempFilePath = AzurePathHelper.GetTempBlobPath(path);

            if (System.IO.File.Exists(tempFilePath))
            {
                fileLastModified = System.IO.File.GetLastWriteTimeUtc(tempFilePath);
                using (var stream = new System.IO.FileStream(tempFilePath, System.IO.FileMode.Open))
                {
                    data = new byte[stream.Length];
                    stream.Read(data, 0, data.Length);
                }
            }

            // no data in cache or outdated
            if (data == null || fileLastModified < remoteLastModified)
            {
                if (data != null)
                    Discard(path);

                return null;
            }

            return data;
        }
        private void AddToCache(string path, byte[] data, DateTime created)
        {
            switch (_cacheType)
            {
                case BlobCacheType.Memory:
                    CacheHelper.Add(BlobCacheHelper.GetCacheKey(path), data, CacheHelper.GetCacheDependency(BlobCacheHelper.GetCacheDependency(path)), DateTime.Now.AddMinutes(AccountInfo.Instance.BlobCacheMinutes), System.Web.Caching.Cache.NoSlidingExpiration);
                    break;

                case BlobCacheType.FileSystem:
                    AddToFileSystem(path, data, created);
                    break;
            }
        }

        private void AddToFileSystem(string path, byte[] data, DateTime created)
        {
            var tempFilePath = AzurePathHelper.GetTempBlobPath(path);
            System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(tempFilePath));

            using (var stream = new System.IO.FileStream(tempFilePath, System.IO.FileMode.Create))
            {
                stream.Write(data, 0, data.Length);
            }

            // make sure the last write time matches cloud timestamp
            System.IO.File.SetLastWriteTimeUtc(tempFilePath, created);
        }
    }
}
