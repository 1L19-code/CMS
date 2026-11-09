using System;
using System.IO;

namespace CMS.Editor
{
    /// <summary>Excludes content from CmsData without excluding it from metadata or compilation.</summary>
    public sealed class StaticSourceIgnore
    {
        private readonly Func<FileEntry, ContentId, bool> _matches;

        private StaticSourceIgnore(Func<FileEntry, ContentId, bool> matches)
        {
            _matches = matches;
        }

        public static StaticSourceIgnore ForId(ContentId id)
        {
            return new StaticSourceIgnore((entry, candidate) => candidate.Equals(id));
        }

        public static StaticSourceIgnore ForPredicate(Func<FileEntry, ContentId, bool> matches)
        {
            return new StaticSourceIgnore(matches ?? throw new ArgumentNullException(nameof(matches)));
        }

        /// <param name="path">Absolute path, or path relative to CmsConstants.PreCompilePath.</param>
        public static StaticSourceIgnore ForPath(string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                throw new ArgumentException("A content path is required.", nameof(path));

            var fullPath = Path.GetFullPath(Path.IsPathRooted(path)
                ? path : Path.Combine(CmsConstants.PreCompilePath, path));
            return new StaticSourceIgnore((entry, id) =>
                string.Equals(Path.GetFullPath(entry.Path), fullPath, StringComparison.OrdinalIgnoreCase));
        }

        public static StaticSourceIgnore ForExtension(string extension)
        {
            if (string.IsNullOrWhiteSpace(extension) || string.IsNullOrEmpty(extension.TrimStart('.')))
                throw new ArgumentException("A file extension is required.", nameof(extension));

            var normalized = new FileExtension(extension);
            return new StaticSourceIgnore((entry, id) => FileExtension.FromPath(entry.Path).Equals(normalized));
        }

        internal bool Matches(FileEntry entry, ContentId id) => _matches(entry, id);
    }
}
