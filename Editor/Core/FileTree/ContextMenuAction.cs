using System;

namespace CMS.Editor
{
    public readonly struct ContextMenuAction
    {
        public readonly string Path;
        public readonly Action<FileEntry> Action;

        public ContextMenuAction(string path, Action<FileEntry> action)
        {
            Path = path;
            Action = action;
        }
    }
}
