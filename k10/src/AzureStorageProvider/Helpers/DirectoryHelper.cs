using System;
using System.Collections.Generic;

namespace AzureStorageProvider.Helpers
{
    public static class DirectoryHelper
    {
        public static List<string> PathToParent(string path, string parentPath)
        {
            if (path == parentPath)
                return new List<string>();

            if (!path.StartsWith(parentPath))
                throw new InvalidOperationException("Path " + path + " is not starting with parent path " + parentPath);

            var paths = new List<string> { path };
            var newPath = path;

            while ((newPath = AzurePathHelper.GetBlobDirectory(newPath)) != parentPath)
            {
                paths.Add(newPath);
            }

            return paths;
        }
    }
}
