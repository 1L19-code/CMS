using System;

namespace CMS.Editor
{
    /// <summary>Hides matching entries from the file tree without excluding them from compilation.</summary>
    public sealed class FileTreeIgnore
    {
        private readonly Func<FileEntry, bool> _matches;

        public FileTreeIgnore(Func<FileEntry, bool> matches)
        {
            _matches = matches ?? throw new ArgumentNullException(nameof(matches));
        }

        internal bool Matches(FileEntry entry) => _matches(entry);
    }
}
