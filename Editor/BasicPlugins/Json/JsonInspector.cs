using ilya.cms.Editor;
using UnityEngine.UIElements;

namespace CMS.Editor
{
    public class JsonInspector: IInspectorWindow
    {
        public VisualElement GetWindow(FileEntry entry)
        {
            var root = new VisualElement();
            root.Add(new Label("Json Inspector"));
            
            return root;
        }
    }
}
