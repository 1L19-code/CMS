using ilya.cms.Editor;
using UnityEngine;

namespace CMS.Editor
{
    public class JsonInspectorInstaller : ExtensionInstaller<FileExtension, IInspectorWindow>
    {
        public override void Install(IRegistrator<FileExtension, IInspectorWindow> registrator)
        {
            registrator.Register(new FileExtension(".json"), new JsonInspector());
        }
    }
}
