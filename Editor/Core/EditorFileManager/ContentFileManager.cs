using System;
using System.IO;
using System.Linq;

namespace CMS.Editor
{
    public class ContentFileManager : IFileManager, IRootDirectoryChangeObserver
    {
        public event Action Changed;
        
        public void Save(byte[] data, string relativePath)
        {
            RawFileManager.Save(data, relativePath);
            Changed?.Invoke();
        }

        public byte[] Load(string relativePath)
        {
            return RawFileManager.Load(relativePath);
        }

        public void Delete(string relativePath)
        {
            var path = Path.GetFullPath(relativePath);

            if (string.IsNullOrEmpty(path))
            {
                throw new System.ArgumentNullException(relativePath);
            }

            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
            else
            {
                var meta = GetAllMetaFilesPaths(path);

                File.Delete(path);

                foreach (var metaEntry in meta)
                {
                    File.Delete(metaEntry);
                }
            }
            
            Changed?.Invoke();
        }

        public void Move(string sourcePath, string destPath)
        {
            var allMetaFilePaths = GetAllMetaFilesPaths(sourcePath);
            
            RawFileManager.Move(sourcePath, destPath);

            foreach (var meta in allMetaFilePaths)
            {
                var destinationDirectory = Directory.Exists(destPath) ? 
                    destPath : 
                    Path.GetDirectoryName(destPath);
                
                RawFileManager.Move(meta, destinationDirectory);
            }
            
            Changed?.Invoke();
        }

        public void Save(byte[] data, FileEntry entry)
        {
            Save(data, entry.Path);
        }

        public byte[] Load(FileEntry entry)
        {
            return Load(entry.Path);
        }

        public void Delete(FileEntry entry)
        {
            Delete(entry.Path);
        }

        private static string[] GetAllMetaFilesPaths(string filePath)
        {
            var name = Path.GetFileName(filePath);
            var nameWithoutExtensions = name.Split('.').First();
            var directory = Path.GetDirectoryName(filePath);

            var entries =
                RawFileManager.GetEntriesInDirectory(directory, $"*{nameWithoutExtensions}.*");

            return entries
                .Where(entry => !entry.IsDirectory)
                .Where(entry => Path.GetFileName(entry.Path) != name)
                .Select(entry => entry.Path)
                .ToArray();
        }

    }
}
