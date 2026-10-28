using UnityEngine.UIElements;

namespace CMS.Editor
{
    public interface IInspectorWindow
    {
        ReaderId ReaderId { get; }

        VisualElement GetWindow(FileEntry fileEntry);
    }
}
