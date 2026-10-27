using CMS.Editor;
using UnityEngine.UIElements;

namespace ilya.cms.Editor
{
    public interface IInspectorWindow
    {
        ReaderId ReaderId { get; }

        VisualElement GetWindow(FileEntry entry);
    }
}
