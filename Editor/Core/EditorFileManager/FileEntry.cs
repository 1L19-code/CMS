using System;

namespace CMS.Editor
{
    public readonly struct FileEntry : IEquatable<FileEntry>
    {
        public readonly string Path;
        public readonly bool IsDirectory;

        public FileEntry(string path, bool isDirectory)
        {
            Path = path;
            IsDirectory = isDirectory;
        }

        public bool Equals(FileEntry other)
        {
            return Path == other.Path && IsDirectory == other.IsDirectory;
        }

        public override bool Equals(object obj)
        {
            return obj is FileEntry other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Path, IsDirectory);
        }
        
        public override string ToString()
        {
            return $"path: {Path} || is directory: ({IsDirectory})";
        }
    }
}
