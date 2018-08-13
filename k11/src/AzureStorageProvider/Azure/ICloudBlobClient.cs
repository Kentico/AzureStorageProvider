namespace AzureStorageProvider.Azure
{
    internal interface ICloudBlobClient
    {
        Microsoft.WindowsAzure.Storage.Blob.CloudBlobContainer GetContainerReference(string containerName);
    }
}
