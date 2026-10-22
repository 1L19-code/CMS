using System;
using System.IO;
using System.Linq;

namespace CMS.Editor
{
    internal class RawFileManager : IFileEntryResolver
    {
        private const string TempFileExtension = ".tmp";
        
        public FileEntry[] GetAllEntries()
        {
            var root = Path.GetFullPath(CmsConstants.PreCompilePath);
            
            return Directory
                .GetFileSystemEntries(root, "*", SearchOption.AllDirectories)
                .Select(path => new FileEntry(path, Directory.Exists(path)))
                .ToArray();
        }
        
        public void Save(byte[] data, string relativePath)
        {
            var path = Path.Combine(CmsConstants.PreCompilePath, relativePath);
            var directory = Path.GetDirectoryName(path);
            
            if(string.IsNullOrEmpty(directory))
                throw new ArgumentException("Directory must not be null or empty", nameof(relativePath));
                            
            if(!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            
            var tempPath = Path.Combine(path, TempFileExtension);
            
            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                stream.Write(data, 0, data.Length);
                stream.Flush();
            }
            
            File.Move(tempPath, path);
        }

        public byte[] Load(string relativePath)
        {
            var path = Path.Combine(CmsConstants.PreCompilePath, relativePath);
            
            return File.ReadAllBytes(path);
        }

    }
}