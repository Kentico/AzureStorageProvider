using AzureStorageProvider.Azure;
using AzureStorageProvider.Models;
using CMS;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

[assembly: RegisterImplementation(typeof(ICloudBlobClient), typeof(AzureStorageProvider.Azure.CloudBlobClient))]

namespace AzureStorageProvider.Azure
{
    public class CloudBlobClient : ICloudBlobClient
    {
        private Microsoft.WindowsAzure.Storage.Blob.CloudBlobClient _blobClient = InitializeClient();
        private static Microsoft.WindowsAzure.Storage.Blob.CloudBlobClient InitializeClient()
        {
            var accountInfo = AccountInfo.Instance;
            var credentials = new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(accountInfo.AccountName, accountInfo.SharedKey);
            var account = new CloudStorageAccount(credentials, true);
            return account.CreateCloudBlobClient();
        }

        public Microsoft.WindowsAzure.Storage.Blob.CloudBlobContainer GetContainerReference(string containerName)
        {
            return _blobClient.GetContainerReference(containerName);
        }
    }
}
