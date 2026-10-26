using System;
using System.IO;

namespace CMS.Editor
{
    public readonly struct FileExtension : IEquatable<FileExtension>
    {
        public readonly string Value;
        
        public FileExtension(string value)
        {
            Value = value?.TrimStart('.').ToLowerInvariant();
        }

        public static FileExtension FromPath(string path)
        {
            return new FileExtension(Path.GetExtension(path));
        }
        public bool Equals(FileExtension other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is FileExtension other && Equals(other);
        }

        public override int GetHashCode()
        {
            return (Value != null ? Value.GetHashCode() : 0);
        }

        public override string ToString()
        {
            return Value;
        }
    }
}
