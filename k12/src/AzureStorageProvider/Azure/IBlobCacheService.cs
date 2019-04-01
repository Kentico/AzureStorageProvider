
using AzureStorageProvider.Models;
using CMS;
using System;

namespace AzureStorageProvider.Azure
{
    public interface IBlobCacheService
    {
        BlobCacheType CacheType { get; }
        void Add(string path, System.IO.Stream stream, DateTime created);
        byte[] Get(string path, DateTime remoteLastModified);
        void Discard(string path);
    }
}
