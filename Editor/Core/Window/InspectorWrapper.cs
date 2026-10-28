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
            _fileTree.FileOpenRequested += OnFileOpenRequested;
            _fileTree.FileSelectionCanceled += OnFileSelectionCanceled;
            
            style.height = new StyleLength(Length.Percent(100));
            style.width = new StyleLength(Length.Percent(100));
        }

        public void Dispose()
        {
            _fileTree.FileOpenRequested -= OnFileOpenRequested;
            _fileTree.FileSelectionCanceled -= OnFileSelectionCanceled;
        }

        private void OnFileOpenRequested(FileEntry entry)
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