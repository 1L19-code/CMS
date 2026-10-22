using CMS.Editor;
using UnityEngine.UIElements;

namespace ilya.cms.Editor
{
    public interface IInspectorWindow
    {
        VisualElement GetWindow(FileEntry entry);
    }
}
