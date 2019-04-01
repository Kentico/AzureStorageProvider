
namespace AzureStorageProvider.Helpers
{
    public interface IObjectWithPath<T>
    {
        string Path { get; }
        T Initialize(string path);
    }
}
