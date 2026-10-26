using System;
using System.IO;
using ilya.cms.Editor;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CMS.Editor
{
    internal class CmsWindow : EditorWindow
    {

        private VisualElement _inspectorWrapper;
        private FileTree _fileTree;
        private IFileManager _fileManager;
        private MetadataGenerationPipeline _metadataGenerationPipeline;
        private IRegistrationResolver<FileExtension, IInspectorWindow> _inspectorWindowResolver;

        [MenuItem("CMS/Window")]
        public static void ShowWindow()
        {
            var window = GetWindow<CmsWindow>();
            window.titleContent = new GUIContent("CMS");
            window.Show();
        }

        private void CreateGUI()
        {
            var root = rootVisualElement;
            var locator = EditorBootstrap.Initialize();

            _metadataGenerationPipeline = locator.Get<MetadataGenerationPipeline>();
            _fileTree = locator.Get<FileTree>();
            _fileManager = locator.Get<IFileManager>();
            _inspectorWindowResolver = locator.Get<IRegistrationResolver<FileExtension, IInspectorWindow>>();
            _inspectorWrapper = new VisualElement();
            var splitView = new TwoPaneSplitView();
            splitView.Add(_fileTree);
            splitView.Add(_inspectorWrapper);

            _fileTree.FileOpenRequested += OnFileOpenRequested;
            _fileTree.FileMoveRequested += OnFileMoveRequested;
            root.Add(splitView);
        }

        private void OnDisable()
        {
            if (_fileTree != null)
            {
                _fileTree.FileOpenRequested -= OnFileOpenRequested;
                _fileTree.FileMoveRequested -= OnFileMoveRequested;
                _fileTree.Dispose();
            }

            _metadataGenerationPipeline?.Dispose();
        }

        private void OnFileOpenRequested(FileEntry entry)
        {
            var extension = new FileExtension(Path.GetExtension(entry.Path));
            var window = _inspectorWindowResolver.GetInstanceOrDefault(extension);
            _inspectorWrapper.Clear();
            _inspectorWrapper.Add(window.GetWindow(entry));
        }
        
        private void OnFileMoveRequested(FileMoveRequest request)
        {
            _fileManager.Move(request.File.Path, request.NewPath);
        }
    }
}
