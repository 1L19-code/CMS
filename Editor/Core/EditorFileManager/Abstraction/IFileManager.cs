namespace CMS.Editor
{
    internal interface IFileManager
    {
        public void Save(byte[] data, string relativePath);
        public byte[] Load(string relativePath);
        
        public void Save(byte[] data, FileEntry entry);
        public byte[] Load(FileEntry entry);
    }
}