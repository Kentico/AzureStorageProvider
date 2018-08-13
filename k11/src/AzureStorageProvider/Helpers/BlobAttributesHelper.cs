using AzureStorageProvider.Models;

namespace AzureStorageProvider.Helpers
{
    internal static class BlobAttributesHelper
    {
        public static BlobAttributes MapAttributes(Microsoft.WindowsAzure.Storage.Blob.CloudBlob blobReference)
        {
            var attributes = new BlobAttributes
            {
                AbsoluteUri = blobReference.Uri.AbsoluteUri,
                Etag = blobReference.Properties.ETag,
                LastModified = blobReference.Properties.LastModified.Value.DateTime,
                Length = blobReference.Properties.Length,
                Metadata = blobReference.Metadata
            };

            return attributes;
        }
    }
}
