using AzureStorageProvider.Azure;
using AzureStorageProvider.Collections;
using AzureStorageProvider.Helpers;
using AzureStorageProvider.Models;
using CMS.Base;
using CMS.Core;
using CMS.Helpers;
using Microsoft.WindowsAzure.Storage.Blob;
using NUnit.Framework;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorageProvider.Tests
{
    [SetUpFixture]
    public class SetUpFixture
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            var type = typeof(TypeManager);
            var method = type.GetMethods(BindingFlags.Static | BindingFlags.NonPublic).Where(m => m.Name == "PreInitializeTypes" && m.GetParameters().Count() == 1).Single();
            method.Invoke(null, new[] { typeof(FakeServices.CloudBlobService).Assembly as object });

            AccountInfo.SetUp(
                accountName: ValidationHelper.GetString(SettingsHelper.AppSettings[nameof(WebConfigKeys.CMSAzureAccountName)], string.Empty),
                endPoint: ValidationHelper.GetString(SettingsHelper.AppSettings[nameof(WebConfigKeys.CMSAzureCDNEndpoint)], string.Empty),
                publicContainer: ValidationHelper.GetBoolean(SettingsHelper.AppSettings[nameof(WebConfigKeys.CMSAzurePublicContainer)], false),
                sharedKey: ValidationHelper.GetString(SettingsHelper.AppSettings[nameof(WebConfigKeys.CMSAzureSharedKey)], string.Empty),
                rootContainer: ValidationHelper.GetString(SettingsHelper.AppSettings[nameof(WebConfigKeys.CMSAzureRootContainer)], string.Empty),
                blobCacheType: BlobCacheType.None,
                blobCacheMinutes: 30);
            
            var path = Assembly.GetExecutingAssembly().Location;
            for (var i = 0; i < 4; i++)
            {
                path = System.IO.Directory.GetParent(path).FullName;
            }

            AzurePathHelper.CurrentDirectory = $"{path}\\CMS";
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            foreach (var blob in Service.Entry<ICloudBlobClient>().GetContainerReference(AccountInfo.Instance.RootContainer).ListBlobs())
                DeleteBlob(blob);
        }

        private void DeleteBlob(IListBlobItem blobItem)
        {
            if (blobItem is CloudBlockBlob)
            {
                ((CloudBlockBlob)blobItem).DeleteIfExists();
            }
            else if (blobItem is CloudBlobDirectory)
            {
                foreach (var blob in ((CloudBlobDirectory)blobItem).ListBlobs())
                    DeleteBlob(blob);
            }
        }
    }
}
