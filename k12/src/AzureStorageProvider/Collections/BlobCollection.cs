using System.Collections.Generic;
using AzureStorageProvider.Helpers;
using AzureStorageProvider.Models;
using System;
using System.Linq;

namespace AzureStorageProvider.Collections
{
    public class BlobCollection : Collection<Blob, BlobCollection>
    {
        public List<string> GetOutdatedBlobPaths(DateTime dateThreshold)
        {
            return _items.Where(b => b.Value.LastRefresh.HasValue && b.Value.LastRefresh.Value < dateThreshold).Select(b => b.Key).ToList();
        }
    }
}
