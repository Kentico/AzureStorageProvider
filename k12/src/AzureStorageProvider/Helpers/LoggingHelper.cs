using CMS.Base;
using CMS.DataEngine;
using CMS.EventLog;
using CMS.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorageProvider.Helpers
{
    internal sealed class LoggingHelper
    {
        public static bool LogsEnabled = SettingsKeyInfoProvider.GetBoolValue(nameof(SettingsKeys.AzureStorageProviderEnableLogs));

        public static void Log(string eventCode, string text)
        {
            EventLogProvider.LogEvent(EventType.INFORMATION, "AzureStorageProvider", eventCode, text);
        }
    }
}
