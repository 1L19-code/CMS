using ilya.cms.Editor;
using UnityEngine.UIElements;
using Cms.Build;

namespace CMS.Editor
{
    internal class FileDoesNotSupportInspector : IInspectorWindow
    {
        public ReaderId ReaderId { get; } = ReaderId.FromString("FileDoesNotSupportInspector");

        public VisualElement GetWindow(FileEntry entry)
        {
            var root = new VisualElement();
            root.Add(new Label("File does not support"));
            
            return root;
        }
    }
}
