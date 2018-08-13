using AzureStorageProvider.Models;

namespace AzureStorageProvider.Azure
{
    public class CloudBlobClient : ICloudBlobClient
    {
        private Microsoft.WindowsAzure.Storage.Blob.CloudBlobClient _blobClient = InitializeClient();
        private static Microsoft.WindowsAzure.Storage.Blob.CloudBlobClient InitializeClient()
        {
            var accountInfo = AccountInfo.Instance;
            var credentials = new Microsoft.WindowsAzure.Storage.Auth.StorageCredentials(accountInfo.AccountName, accountInfo.SharedKey);
            var account = new Microsoft.WindowsAzure.Storage.CloudStorageAccount(credentials, false);
            return account.CreateCloudBlobClient();
        }

        public Microsoft.WindowsAzure.Storage.Blob.CloudBlobContainer GetContainerReference(string containerName)
        {
            return _blobClient.GetContainerReference(containerName);
        }
    }
}
