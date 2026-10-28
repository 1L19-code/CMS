namespace CMS.Editor
{
    internal interface IFileManager
    {
        public void Save(byte[] data, string relativePath);
        public byte[] Load(string relativePath);
        public string LoadUtf8(string relativePath);
        public void Delete(string relativePath);
        public void Move(string sourcePath, string destPath);
        public void CreateDirectory(string relativePath);
        
        public void Save(byte[] data, FileEntry entry);
        public byte[] Load(FileEntry entry);
        public string LoadUtf8(FileEntry entry);
        public void Delete(FileEntry entry);
        
        
    }
}