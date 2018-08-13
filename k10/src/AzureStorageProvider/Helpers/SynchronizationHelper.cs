using CMS.Base;
using CMS.Helpers;

namespace AzureStorageProvider.Helpers
{
    public static class SynchronizationHelper
    {
        public static bool Synchronizing()
        {
            return WebFarmHelper.WebFarmEnabled && !CMSActionContext.CurrentLogWebFarmTasks;
        }
    }
}
