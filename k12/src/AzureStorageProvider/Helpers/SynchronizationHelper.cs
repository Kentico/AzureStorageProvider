using CMS.Base;
using CMS.Core;
using CMS.Helpers;
using CMS.IO;
using System;

namespace AzureStorageProvider.Helpers
{
    internal static class SynchronizationHelper
    {
        public static bool Synchronizing()
        {
            return WebFarmHelper.WebFarmEnabled && !CMSActionContext.CurrentLogWebFarmTasks;
        }

        public static void LogDirectoryDeleteTask(string path)
        {
            if (CMSActionContext.CurrentLogWebFarmTasks)
            {
                path = StorageHelper.GetWebApplicationRelativePath(path);
                if (!String.IsNullOrEmpty(path))
                {
                    CoreServices.WebFarm.CreateIOTask(new DeleteFolderWebFarmTask { Path = path, TaskFilePath = path });
                }
            }
        }
        public static void LogDeleteFileTask(string path)
        {
            if (CMSActionContext.CurrentLogWebFarmTasks)
            {
                path = StorageHelper.GetWebApplicationRelativePath(path);
                if (!String.IsNullOrEmpty(path))
                {
                    CoreServices.WebFarm.CreateIOTask(new DeleteFileWebFarmTask { Path = path, TaskFilePath = path });
                }
            }
        }
        public static void LogUpdateFileTask(string path)
        {
            if (CMSActionContext.CurrentLogWebFarmTasks)
            {
                string relativePath = StorageHelper.GetWebApplicationRelativePath(path);
                if (!String.IsNullOrEmpty(relativePath))
                {
                    if (CMS.IO.File.Exists(path))
                    {
                        using (var str = CMS.IO.File.OpenRead(path))
                        {
                            CoreServices.WebFarm.CreateIOTask(new UpdateFileWebFarmTask
                            {
                                Path = relativePath,
                                TaskFilePath = path,
                                TaskBinaryData = str,
                            });
                        }
                    }
                }
            }
        }
    }
}
