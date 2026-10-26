using System;
using System.IO;
using System.Linq;
using UnityEngine;

namespace CMS.Editor
{
    internal static class RawFileManager
    {
        private const string TempFileExtension = ".tmp";
        
        public static FileEntry[] GetAllEntries()
        {
            var root = Path.GetFullPath(CmsConstants.PreCompilePath);
            
            return Directory
                .GetFileSystemEntries(root, "*", SearchOption.AllDirectories)
                .Select(path => new FileEntry(path, Directory.Exists(path)))
                .ToArray();
        }

        public static FileEntry[] GetEntriesInDirectory(string directory, string searchPattern = "*")
        {
            directory = ValidatePath(directory);
            
            if(!Directory.Exists(directory))
            {
                throw new ArgumentException("Directory must not be null or empty", nameof(directory));
            }

            return Directory
                .GetFileSystemEntries(directory, searchPattern, SearchOption.AllDirectories)
                .Select(path => new FileEntry(path, Directory.Exists(path)))
                .ToArray();
        }

        public static void Save(byte[] data, string relativePath)
        {
            var path = ValidatePath(relativePath);
            var directory = Path.GetDirectoryName(path);
            
            if(string.IsNullOrEmpty(directory))
                throw new ArgumentException("Directory must not be null or empty", nameof(relativePath));
                            
            if(!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
            
            var tempPath = path + TempFileExtension;
            
            using (var stream = new FileStream(tempPath, FileMode.Create))
            {
                stream.Write(data, 0, data.Length);
                stream.Flush();
            }
            
            File.Copy(tempPath, path, true);
            File.Delete(tempPath);
        }

        public static byte[] Load(string relativePath)
        {
            var path = ValidatePath(relativePath);
            
            return File.ReadAllBytes(path);
        }

        public static void Move(string sourcePath, string destPath)
        {
            sourcePath = ValidatePath(sourcePath);
            destPath = ValidatePath(destPath);
           
            var destDirectory = Directory.Exists(destPath) ? 
                destPath : 
                Path.GetDirectoryName(destPath);

            if (string.IsNullOrEmpty(destDirectory))
            {
                throw new ArgumentException("Destination directory must not be null or empty", nameof(destPath));
            }
            
            var name = Path.GetFileName(sourcePath);
            
            File.Copy(sourcePath, Path.Combine(destDirectory, name), true);
            File.Delete(sourcePath);
        }

        private static string ValidatePath(string path)
        {
            if(!path.Contains(CmsConstants.PreCompilePath))
            {
                path = Path.Combine(CmsConstants.PreCompilePath, path);
            }
            
            return Path.GetFullPath(path);
        }
    }
}
