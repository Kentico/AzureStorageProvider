using System.Text.RegularExpressions;

namespace AzureStorageProvider.Helpers
{
    public static class BlobContainerHelper
    {
        public static bool ValidateName(string name)
        {
            if (name.Length < 3 || name.Length > 63)
                return false;

            var pattern = @"^[a-z0-9]*(([a-z0-9]-([a-z0-9]-)*[a-z0-9])[a-z0-9]*)*([a-z0-9]*?)$";
            var regex = new Regex(pattern);
            if (!regex.IsMatch(name))
                return false;

            return true;
        }
    }
}
