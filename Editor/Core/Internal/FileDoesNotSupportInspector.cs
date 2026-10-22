using ilya.cms.Editor;
using UnityEngine.UIElements;

namespace CMS.Editor
{
    internal class FileDoesNotSupportInspector : IInspectorWindow
    {
        public VisualElement GetWindow(FileEntry entry)
        {
            var root = new VisualElement();
            root.Add(new Label("File does not support"));
            
            return root;
        }
    }
}
