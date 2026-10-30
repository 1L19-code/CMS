namespace CMS.Editor
{
    public interface IFileManager
    {
        public void Save(byte[] data, string relativePath);
        public byte[] Load(string relativePath);
        public string LoadUtf8(string relativePath);
        public void Delete(string relativePath);
        public void Move(string sourcePath, string destPath);
        public void TryCreateDirectory(string relativePath);
        
        public void Save(byte[] data, FileEntry entry);
        public byte[] Load(FileEntry entry);
        public string LoadUtf8(FileEntry entry);
        public void Delete(FileEntry entry);
        
        public FileEntry[] Find(string searchPattern);
    }
}