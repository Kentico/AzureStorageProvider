using System;
using System.Collections.Generic;

namespace AzureStorageProvider.Models
{
    public class BlobAttributes
    {
        public IDictionary<string, string> Metadata;

        public long Length;
        public string Etag;
        public DateTime LastModified;
        public string AbsoluteUri;
    }
}
