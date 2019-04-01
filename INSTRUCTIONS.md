# Instructions

This provider is designed to replace standard Kentico Azure provider to ensure faster loading times and smooth experience when working with larger sets of data such as Media Library files.

## Web.config keys

* web.config key name
	** value1|value2|*recommended value 3*
	Note about usage of this key
	
---

* CMSAzurePublicContainer
	** True|False
	Pick true or false based on Azure container settings.

* CMSAzureRootContainer
	** {containerName}
	Add name of BLOB container.
	
* CMSAzureCDNEndpoint
	** {endpointURL}
	Add endpoint URL.

* CMSAzureAccountName
	** {accountName}
	Add account name.
	
* CMSAzureSharedKey
	** {accountSharedKey}
	Add account shared key.

* CMSExtenalStorageName
	** {azure}
	Add external storage name – pick one for this provider, e.g. azure.
	
* CMSStorageProviderAssembly
	** AzureStorageProvider

* CMSAzureCachePath
	** {cachePath}
	Folder in root of your website to store cached files from BLOB storage.
	
* AzureStorageProviderCacheType
	** None
	** Memory
	** FileSystem
	Pick one of the cache storage options to cache *data* of BLOBs

* AzureStorageProviderCacheClearMinutes
	** {minutes}
	How long should file contents be cached?

* AzureStorageProviderIgnoreLastWriteTime
	** True|*False*
	Instead of evaluating last write time of folder to max of all blobs last write times, DateTime.MinValue is used. This ensures backwards compatibility with the original provider. Also for Kentico features, this value is not needed, so keep it set to true by default.

* AzureStorageProviderForceLowercase
	** True|*False*
	Before writing/reading files all file names will be lowercased. This ensures backwards compatibility with the original provider.

* AzureStorageProviderInitializeAtAppStart
	** True|*False*
	If true, provider will fetch all metadata at application start.

### Example:
```<add key="CMSAzurePublicContainer" value="true" />
<add key="CMSAzureRootContainer" value="cmsstorage" />
<add key="CMSAzureCDNEndpoint" value="{get from azure}" />
<add key="CMSAzureAccountName" value="{get from azure}" />
<add key="CMSAzureSharedKey" value="{get from azure}" />
<add key="CMSExternalStorageName" value="azure" />
<add key="CMSStorageProviderAssembly" value="AzureStorageProvider" />
<add key="CMSAzureCachePath" value="AzureCache" />
<add key="AzureStorageProviderCacheType" value="None" />
<add key="AzureStorageProviderCacheClearMinutes" value="0" />
<add key="AzureStorageProviderIgnoreLastWriteTime" value="true" />
<add key="AzureStorageProviderForceLowercase" value="false" />
<add key="AzureStorageProviderInitializeAtAppStart" value="false" />```

## How does it work?
This provider works lazily and keeps track of files and folders that were requested to be initialized. Metadata of all files required by the website are stored in application memory and live there throughout the whole life cycle of an application pool.

If caching is enabled, provider also stores actual data of files in designated folder. These data are cleared after specified amount of minutes. However, metadata are still kept in memory. For complete refresh of data restart is required.

Provider supports web farms. If a file is changed on one instance, it is reinitialized on all other instances, i.e. all other instances ask BLOB storage for current data. Folders work similarly.

## Limitations
Due to the nature of this provider’s implementation, direct changes on BLOB storage are not supported. These changes may not be detected easily, therefore to see complete set of data after performing any direct changes on BLOB storage you need to restart the application / recycle the application pool.






