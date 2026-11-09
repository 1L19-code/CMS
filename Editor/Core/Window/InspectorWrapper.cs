using System;
using System.IO;
using UnityEngine.UIElements;

namespace CMS.Editor
{
    internal class InspectorWrapper : VisualElement, IDisposable
    {
        private readonly IRegistrationResolver<FileExtension, IInspectorWindow> _windowResolver;
        private readonly FileTree _fileTree;

        public InspectorWrapper(IRegistrationResolver<FileExtension, IInspectorWindow> windowResolver,
            FileTree fileTree)
        {
            _windowResolver = windowResolver;
            _fileTree = fileTree;
            _fileTree.FileOpenRequested += OpenFile;
            _fileTree.FileSelectionCanceled += OnFileSelectionCanceled;
            
            style.height = new StyleLength(Length.Percent(100));
            style.width = new StyleLength(Length.Percent(100));
        }

        public void Dispose()
        {
            _fileTree.FileOpenRequested -= OpenFile;
            _fileTree.FileSelectionCanceled -= OnFileSelectionCanceled;
        }

        /// <summary>
        /// Opens a file's inspector without requiring a file-tree selection, including hidden attachments.
        /// </summary>
        public void OpenFile(FileEntry entry)
        {
            var extension = new FileExtension(Path.GetExtension(entry.Path));
            var inspector = _windowResolver.GetInstanceOrDefault(extension);
            var window = inspector.GetWindow(entry);

            window.style.height = new StyleLength(Length.Percent(100));
            window.style.width = new StyleLength(Length.Percent(100));

            Clear();
            Add(window);
        }
        
        private void OnFileSelectionCanceled()
        {
            Clear();
        }
    }
}
