using ilya.cms.Editor;
using UnityEngine;

namespace CMS.Editor
{
    public class JsonInspectorInstaller : ExtensionInstaller<FileExtension, IInspectorWindow>
    {
        public override void Install(Locator locator, IRegistrator<FileExtension, IInspectorWindow> registrator)
        {
            registrator.Register(new FileExtension(".json"), new JsonInspector());
        }
    }

    public class JsonContextMenuInstaller : ExtensionInstaller<ContextMenuAction>
    {
        public override void Install(Locator locator, IRegistrator<ContextMenuAction> registrator)
        {
            registrator.Register
            (
                new ContextMenuAction(path: "Create/Json", entry => Debug.Log(entry))
            );
        }
    }
}