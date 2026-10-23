using System.IO;
using System.Linq;
using UnityEditorInternal.Profiling.Memory.Experimental.FileFormat;
using UnityEngine;

namespace CMS.Editor
{
    public class ContentFileManager : IFileManager
    {
        public void Save(byte[] data, string relativePath)
        {
            RawFileManager.Save(data, relativePath);
        }

        public byte[] Load(string relativePath)
        {
            return RawFileManager.Load(relativePath);
        }

        public void Delete(string relativePath)
        {
            var path = Path.GetDirectoryName(relativePath);

            if (string.IsNullOrEmpty(path))
            {
                throw new System.ArgumentNullException(relativePath);
            }

            var meta = GetAllMetaFilePaths(path);

            File.Delete(path);

            foreach (var metaEntry in meta)
            {
                File.Delete(metaEntry);
            }
        }

        public void Move(string sourcePath, string destPath)
        {
            var allMetaFilePaths = GetAllMetaFilePaths(sourcePath);
            
            RawFileManager.Move(sourcePath, destPath);

            foreach (var meta in allMetaFilePaths)
            {
                var destinationDirectory = Directory.Exists(destPath) ? 
                    destPath : 
                    Path.GetDirectoryName(destPath);
                
                RawFileManager.Move(meta, destinationDirectory);
            }
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

        private static string[] GetAllMetaFilePaths(string filePath)
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