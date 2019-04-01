using AzureStorageProvider.Azure;

namespace AzureStorageProvider.Helpers
{
    internal class BlobCacheHelper
    {
        internal static string GetCacheKey(string path)
        {
            return nameof(BlobCacheService) + "|" + path;
        }

        internal static string GetCacheDependency(string path)
        {
            return nameof(BlobCacheService) + "dependency|" + path;
        }
    }
}
