namespace CMS.Editor
{
    internal interface IFileManager
    {
        public void Save(byte[] data, string relativePath);
        public byte[] Load(string relativePath);
    }
}