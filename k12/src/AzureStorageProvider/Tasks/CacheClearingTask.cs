using AzureStorageProvider.Azure;
using AzureStorageProvider.Collections;
using AzureStorageProvider.Helpers;
using AzureStorageProvider.Models;
using CMS.Core;
using CMS.Scheduler;
using System;
using System.Linq;

namespace AzureStorageProvider.Tasks
{
    public class CacheClearingTask : ITask
    {
        private IBlobCacheService _blobCacheService = Service.Resolve<IBlobCacheService>();

        public string Execute(TaskInfo task)
        {
            if (_blobCacheService.CacheType != BlobCacheType.FileSystem)
                return "Caching of Azure data is disabled or is set to memory. No need to run this task.";

            var blobCacheService = Service.Resolve<IBlobCacheService>();

            var minutes = AccountInfo.Instance.BlobCacheMinutes;
            var dateThreshold = DateTime.UtcNow.AddMinutes(-minutes);
            
            var blobsToDelete = BlobCollection.Instance.GetOutdatedBlobPaths(dateThreshold);

            var blobsDeleted = 0;
            var directoriesUninitialized = 0;

            foreach (var path in blobsToDelete)
            {
                // remove the blob
                blobCacheService.Discard(path);
                blobsDeleted++;
            }

            // clear empty folders in file system
            if (blobsDeleted > 0)
            {
                var folders = System.IO.Directory.GetDirectories(AzurePathHelper.GetTempBlobPath(string.Empty), "*", System.IO.SearchOption.AllDirectories).OrderByDescending(p => p.Length);
                foreach (var subFolder in folders)
                {
                    if (System.IO.Directory.Exists(subFolder) &&
                        !System.IO.Directory.GetFiles(subFolder).Any() &&
                        !System.IO.Directory.EnumerateDirectories(subFolder).Any())
                    {
                        System.IO.Directory.Delete(subFolder, false);
                    }
                }
            }

            return "OK, discarded metadata of " + blobsDeleted + " blobs, " + directoriesUninitialized + " dirs";
        }
    }
}
