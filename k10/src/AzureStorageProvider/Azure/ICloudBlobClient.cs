using CMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorageProvider.Azure
{
    [DefaultImplementation(typeof(CloudBlobClient))]
    internal interface ICloudBlobClient
    {
        Microsoft.WindowsAzure.Storage.Blob.CloudBlobContainer GetContainerReference(string containerName);
    }
}
