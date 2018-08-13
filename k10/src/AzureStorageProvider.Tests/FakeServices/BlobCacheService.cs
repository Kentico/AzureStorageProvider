using AzureStorageProvider.Helpers;
using AzureStorageProvider.Models;
using CMS;
using CMS.Helpers;

[assembly: RegisterImplementation(typeof(AzureStorageProvider.Azure.IBlobCacheService), typeof(AzureStorageProvider.Tests.FakeServices.BlobCacheService))]
namespace AzureStorageProvider.Tests.FakeServices
{
    public class BlobCacheService : AzureStorageProvider.Azure.BlobCacheService
    {
        public object Lock = new object();
        public void SetCacheType(BlobCacheType cacheType)
        {
            _cacheType = cacheType;
        }
        public bool IsCached(string path)
        {
            var tempFilePath = AzurePathHelper.GetTempBlobPath(path);
            return System.IO.File.Exists(tempFilePath);
        }
    }
}
