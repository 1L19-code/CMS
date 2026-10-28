using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CMS.Editor
{
    internal class CmsWindow : EditorWindow
    {
        private FileTree _fileTree;
        private IFileManager _fileManager;
        private MetadataGenerationPipeline _metadataGenerationPipeline;
        private InspectorWrapper _inspector;

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
            _inspector = new InspectorWrapper
            (
                locator.Get<IRegistrationResolver<FileExtension, IInspectorWindow>>(),
                _fileTree
            );

            var splitView = new TwoPaneSplitView
            (
                fixedPaneIndex: 0, fixedPaneStartDimension: 300f, orientation: TwoPaneSplitViewOrientation.Horizontal
            );
            
            splitView.Add(_fileTree);
            splitView.Add(_inspector);
            _fileTree.FileMoveRequested += OnFileMoveRequested;
            root.Add(splitView);
            root.Add(locator.Get<PopupsWrapper>());
        }

        private void OnDisable()
        {
            if (_fileTree != null)
            {
                _fileTree.FileMoveRequested -= OnFileMoveRequested;
                _fileTree.Dispose();
            }

            _inspector.Dispose();
            _metadataGenerationPipeline?.Dispose();
        }

        private void OnFileMoveRequested(FileMoveRequest request)
        {
            _fileManager.Move(request.File.Path, request.NewPath);
        }
    }
}