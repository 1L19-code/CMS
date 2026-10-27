namespace CMS.Editor
{
    public interface IMetadataResolver<out T>
    {
        T Resolve(FileEntry entry);
        T Resolve(string filePath);
    }
}