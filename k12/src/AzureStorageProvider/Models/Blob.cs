using System.IO;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using AzureStorageProvider.Helpers;
using AzureStorageProvider.Collections;
using AzureStorageProvider.Azure;
using CMS.Core;

namespace AzureStorageProvider.Models
{
    public class Blob : IObjectWithPath<Blob>
    {
        private object _lock = new object();
        private bool? _exists = null;
        private DateTime? _lastRefresh = null;
        private bool _attributesFetched = false;
        private BlobAttributes _attributes;

        private ICloudBlobService _cloudBlobService = Service.Resolve<ICloudBlobService>();
        private IBlobCacheService _blobCacheService = Service.Resolve<IBlobCacheService>();

        internal DateTime? LastRefresh => _lastRefresh;
        public string Path { get; protected set; }

        public T GetAttribute<T>(Func<BlobAttributes, T> attribute)
        {
            return GetAttribute(attribute, default(T));
        }
        public T GetAttribute<T>(Func<BlobAttributes, T> attribute, T defaultValue)
        {
            if (!Exists())
                return defaultValue;

            lock (_lock)
            {
                return GetAttributeInternal(attribute);
            }
        }

        private T GetAttributeInternal<T>(Func<BlobAttributes, T> attribute)
        {
            if (!_attributesFetched)
            {
                _attributes = _cloudBlobService.FetchAttributes(Path);
                _attributesFetched = true;
                _lastRefresh = DateTime.UtcNow;
            }

            return attribute(_attributes);
        }

        public string GetMetadataAttribute(BlobMetadataEnum attribute)
        {
            var metadata = GetAttribute(a => a.Metadata);

            string value;
            metadata.TryGetValue(attribute.ToString(), out value);

            return value;
        }

        public Blob Initialize(string path)
        {
            Path = path;
            return this;
        }

        public Blob Initialize(CloudBlockBlob blobItem)
        {
            Path = AzurePathHelper.ForceLowercase ? blobItem.Name.ToLowerInvariant() : blobItem.Name;

            lock (_lock)
            {
                _exists = true;
                _attributes = BlobAttributesHelper.MapAttributes(blobItem);
                _attributesFetched = true;
                _lastRefresh = DateTime.UtcNow;
            }

            return this;
        }

        public void Uninitialize()
        {
            lock (_lock)
            {
                _exists = null;
                _lastRefresh = null;
                _attributes = null;
                _attributesFetched = false;

                _blobCacheService.Discard(Path);
            }
        }

        public void Reinitialize()
        {
            Uninitialize();

            lock (_lock)
            {
                _exists = _cloudBlobService.Exists(Path);
                _lastRefresh = DateTime.UtcNow;

                if (_exists.HasValue && _exists.Value)
                {
                    _attributes = _cloudBlobService.FetchAttributes(Path);
                    _attributesFetched = true;
                }
            }
        }

        public bool Exists()
        {
            lock (_lock)
            {
                return ExistsInternal();
            }
        }
        private bool ExistsInternal()
        {
            if (_exists == null)
            {
                // if parent directory has been initialized, this blob can not exist
                var parentDirPath = AzurePathHelper.GetBlobDirectory(Path);
                if (BlobDirectoryCollection.Instance.GetOrCreate(parentDirPath).BlobsInitialized)
                {
                    SetExists(false);
                }
                else
                {
                    SetExists(_cloudBlobService.Exists(Path));
                }
            }

            return _exists.Value;
        }
        public void SetExists(bool exists)
        {
            lock (_lock)
            {
                _exists = exists;
                _lastRefresh = DateTime.UtcNow;

                if (_exists.Value)
                    BlobDirectoryCollection.Instance.GetOrCreate(AzurePathHelper.GetBlobDirectory(Path)).SetExists();
            }
        }


        public void Delete()
        {
            if (Exists())
            {
                lock (_lock)
                {
                    if (!SynchronizationHelper.Synchronizing() || _cloudBlobService.Exists(Path))
                    {
                        _cloudBlobService.Delete(Path);
                    }

                    SetExists(false);
                    _blobCacheService.Discard(Path);

                    BlobDirectoryCollection.Instance.GetOrCreate(AzurePathHelper.GetBlobDirectory(Path)).ResetExists();
                }
            }
        }
        
        public void Copy(string destPath, bool overwrite)
        {
            if (SynchronizationHelper.Synchronizing())
            {
                Reinitialize();
                if (!Exists())
                    return;
            }

            if (!Exists())
                throw new FileNotFoundException($"Blob on path {Path} does not exist.");

            var targetBlob = BlobCollection.Instance.GetOrCreate(destPath);

            if (!overwrite && targetBlob.Exists())
                throw new InvalidOperationException($"Target blob on path {destPath} already exists.");

            // must be synchronous as we delete asynchronously in MOVE
            lock (_lock)
            {
                _cloudBlobService.Copy(Path, targetBlob.Path);
            }
            targetBlob.SetExists(true);
        }

        public void Move(string destPath)
        {
            if (SynchronizationHelper.Synchronizing())
            {
                BlobCollection.Instance.GetOrCreate(destPath).Reinitialize();
            }

            Copy(destPath, false);
            Delete();
        }

        public byte[] Get()
        {
            lock (_lock)
            {
                return GetInternal();
            }
        }
        public byte[] GetInternal()
        {
            if (ExistsInternal())
            {
                var remoteLastModified = GetAttributeInternal(a => a.LastModified);
                return _blobCacheService.Get(Path, remoteLastModified);
            }

            return new byte[0];
        }
        
        public void Upload(Stream stream)
        {
            if (SynchronizationHelper.Synchronizing())
            {
                Reinitialize();
                if (Exists())
                    return;
            }

            IDictionary<string, string> metadata;

            lock (_lock)
            {
                var exists = ExistsInternal();

                if (!exists)
                {
                    _attributes = new BlobAttributes
                    {
                        Metadata = new Dictionary<string, string> {
                            { BlobMetadataEnum.DateCreated.ToString(), DateTime.UtcNow.ToString() }
                        }
                    };
                    metadata = _attributes.Metadata;
                }
                else
                {
                    SetMetadataAttributeInternal(BlobMetadataEnum.DateCreated, DateTime.UtcNow.ToString());
                    metadata = GetAttributeInternal(a => a.Metadata);
                }

                _attributes = _cloudBlobService.Upload(Path, metadata, stream);

                stream.Seek(0, SeekOrigin.Begin);
                _blobCacheService.Add(Path, stream, _attributes.LastModified);

                SetExists(true);
                _attributesFetched = true;
            }
        }

        public void Append(byte[] content)
        {
            lock (_lock)
            {
                var data = GetInternal();

                SetMetadataAttributeInternal(BlobMetadataEnum.LastWriteTime, DateTime.UtcNow.ToString());

                using (var stream = new System.IO.MemoryStream())
                {
                    stream.Write(data, 0, data.Length);
                    stream.Write(content, 0, content.Length);
                    stream.Seek(0, SeekOrigin.Begin);

                    _attributes = _cloudBlobService.Upload(Path, GetAttribute(a => a.Metadata), stream);

                    _lastRefresh = DateTime.UtcNow;
                    stream.Seek(0, SeekOrigin.Begin);
                    _blobCacheService.Add(Path, stream, _attributes.LastModified);
                }
            }
        }

        public string GetUrl()
        {
            return BlobContainerCollection.Instance.GetOrCreate(AccountInfo.Instance.RootContainer).IsPublic() ?
                GetAttribute(a => a.AbsoluteUri) :
                AzurePathHelper.GetDownloadUri(Path);
        }
        
        public void SetMetadataAttributeAndSave(BlobMetadataEnum attribute, string value)
        {
            lock (_lock)
            {
                SetMetadataAttributeInternal(attribute, value);
                _cloudBlobService.SetMetadataAsync(Path, GetAttribute(a => a.Metadata));
            }
        }
        private void SetMetadataAttributeInternal(BlobMetadataEnum attribute, string value)
        {
            var metadata = GetAttributeInternal(a => a.Metadata);

            if (metadata.ContainsKey(attribute.ToString()))
                metadata[attribute.ToString()] = value;
            else
                metadata.Add(new KeyValuePair<string, string>(attribute.ToString(), value));
        }
    }
}
