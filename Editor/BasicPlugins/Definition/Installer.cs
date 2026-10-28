#if UNITY_EDITOR
using System.IO;
using System.Text;
using Cms.DefinitionPlugin.Common;
using UnityEngine;

namespace CMS.Editor
{
    internal class DefinitionEditorInstaller : ExtensionInstaller<FileExtension, IInspectorWindow>
    {
        public override void Install(Locator locator, IRegistrator<FileExtension, IInspectorWindow> registrator)
        {
            var editor = new DefinitionEditor(locator.Get<IFileManager>(), new DefinitionsNamesMap());
            registrator.Register(new FileExtension(DefinitionConstants.FileExtension), editor);
        }
    }

    internal class DefinitionContextMenuInstaller : ExtensionInstaller<ContextMenuAction>
    {
        public override void Install(Locator locator, IRegistrator<ContextMenuAction> registrator)
        {
            var fileManager = locator.Get<IFileManager>();
            
            registrator.Register(new ContextMenuAction("Create/Definition",
                descriptor => {
                    if (!descriptor.IsDirectory)
                        return;

                    locator.Get<PopupsWrapper>().ShowPopup(PopupDefinition.WithTitle("Create Definition")
                        .WithTextField(new TextFieldDefinition("Name", DefinitionConstants.FileExtension))
                        .WithButtons(new ButtonDefinition("Create", name => Create(descriptor, fileManager, name))));
                }));
        }
        
        private static void Create(FileEntry targetDirectory, IFileManager fileManager, string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name == "." || name == ".." ||
                name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
                name.Contains(Path.DirectorySeparatorChar.ToString()) ||
                name.Contains(Path.AltDirectorySeparatorChar.ToString()))
                return;

            var path = Path.Combine(targetDirectory.Path, name + DefinitionConstants.FileExtension);

            if (File.Exists(path) || Directory.Exists(path))
            {
                Debug.LogWarning($"A file or directory already exists at '{path}'.");
                return;
            }

            var def = new DefinitionMeta();
            var json = JsonUtility.ToJson(def);
            fileManager.Save(Encoding.UTF8.GetBytes(json), path);
        }
    }
    
    internal class DefinitionIconInstaller : ExtensionInstaller<FileExtension, Icon>
    {
        public override void Install(Locator locator, IRegistrator<FileExtension, Icon> registrator)
        {
            registrator.Register(new FileExtension(DefinitionConstants.FileExtension), new Icon("ScriptableObject Icon"));
        }
    }
}
#endif
