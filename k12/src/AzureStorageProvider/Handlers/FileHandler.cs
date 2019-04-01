using System.Web;
using CMS.Base;
using CMS.Helpers;
using CMS.IO;
using CMS.Routing.Web;
using AzureStorageProvider.Collections;
using AzureStorageProvider.Helpers;
using System;
using AzureStorageProvider.Handlers;

[assembly: RegisterHttpHandler("CMSPages/GetAzureProviderFile.aspx", typeof(FileHandler), Order = 1)]

namespace AzureStorageProvider.Handlers
{
    internal class FileHandler : AdvancedGetFileHandler
    {
        #region "Properties"

        /// <summary>
        /// Gets or sets whether cache is allowed. By default cache is allowed on live site.
        /// </summary>
        public override bool AllowCache
        {
            get
            {
                if (mAllowCache == null)
                {
                    mAllowCache = IsLiveSite;
                }

                return mAllowCache.Value;
            }
            set
            {
                mAllowCache = value;
            }
        }

        #endregion


        #region "Page events"

        protected override void ProcessRequestInternal(HttpContextBase context)
        {
            var hash = QueryHelper.GetString("hash", string.Empty);
            var path = QueryHelper.GetString("path", string.Empty);

            // Validate hash
            var settings = new HashSettings(string.Empty)
            {
                Redirect = false
            };

            if (!ValidationHelper.ValidateHash("?path=" + URLHelper.EscapeSpecialCharacters(path), hash, settings))
                RequestHelper.Respond403();

            if (path.StartsWithCSafe("~"))
                path = context.Server.MapPath(path);

            var blobPath = AzurePathHelper.GetBlobPath(path);
            var blob = BlobCollection.Instance.GetOrCreate(blobPath);

            if (!blob.Exists())
                RequestHelper.Respond404();

            CookieHelper.ClearResponseCookies();
            Response.Clear();

            SetRevalidation();

            var eTag = blob.GetAttribute(a => a.Etag);
            var lastModified = ValidationHelper.GetDateTime(blob.GetAttribute(a => a.LastModified), DateTimeHelper.ZERO_TIME);

            var mimeType = MimeMapping.GetMimeMapping(path);
            SetResponseContentType(mimeType);

            // Client caching - only on the live site
            if (AllowCache && AllowClientCache && ETagsMatch(eTag, lastModified))
            {
                // Set the file time stamps to allow client caching
                SetTimeStamps(lastModified);

                RespondNotModified(eTag);
                return;
            }

            SetDisposition(Path.GetFileName(path), Path.GetExtension(path));

            // Setup Etag property
            ETag = eTag;

            if (AllowCache)
            {
                // Set the file time stamps to allow client caching
                SetTimeStamps(lastModified);
                Response.Cache.SetETag(eTag);
            }
            else
            {
                SetCacheability();
            }

            WriteFile(path, CacheHelper.CacheImageAllowed(CurrentSiteName, Convert.ToInt32(blob.GetAttribute(a => a.Length))));

            CompleteRequest();
        }

        #endregion
    }
}