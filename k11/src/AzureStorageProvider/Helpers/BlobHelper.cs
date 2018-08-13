using AzureStorageProvider.Collections;
using AzureStorageProvider.Models;

namespace AzureStorageProvider.Helpers
{
    public static class BlobHelper
    {
        public static Blob Get(string path)
        {
            return BlobCollection.Instance.GetOrCreate(AzurePathHelper.GetBlobPath(path));
        }
    }
}
