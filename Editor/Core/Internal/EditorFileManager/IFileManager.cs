namespace CMS.Editor
{
    public interface IFileManager
    {
        public void Save(byte[] data, string relativePath);
        public byte[] Load(string relativePath);
    }
}