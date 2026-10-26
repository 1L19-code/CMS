using ilya.cms.Editor;
using UnityEngine.UIElements;
using Cms.Build;

namespace CMS.Editor
{
    public class JsonInspector: IInspectorWindow
    {
        public ReaderId ReaderId { get; } = ReaderId.FromString("Json");

        public VisualElement GetWindow(FileEntry entry)
        {
            var root = new VisualElement();
            root.Add(new Label("Json Inspector"));
            
            return root;
        }
    }
}
