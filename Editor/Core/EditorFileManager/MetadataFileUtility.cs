using System.IO;

namespace CMS.Editor
{
    internal static class MetadataFileUtility
    {
        public static string GetPath(string path, FileExtension extension)
        {
            return path + "." + extension.Value;
        }

        public static byte[] LoadOrDefault(string path, FileExtension extension)
        {
            try
            {
                return RawFileManager.Load(GetPath(path, extension));
            }
            catch (FileNotFoundException)
            {
                return null;
            }
            catch (DirectoryNotFoundException)
            {
                return null;
            }
        }
    }
}
