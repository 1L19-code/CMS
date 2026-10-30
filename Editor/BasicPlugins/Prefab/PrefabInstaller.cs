using System;
using System.IO;

namespace CMS.Editor
{
    internal class PrefabInspectorInstaller : EditorExtensionInstaller<FileExtension, IInspectorWindow>
    {
        public override void Install(Locator locator, IRegistrator<FileExtension, IInspectorWindow> registrator)
        {
            registrator.Register
            (
                new FileExtension(PrefabConstants.PrefabExtension),
                new PrefabInspectorWindow(locator.Get<IFileManager>())
            );
        }
    }

    internal class PrefabContextMenuActionInstaller : EditorExtensionInstaller<ContextMenuAction>
    {
        public override void Install(Locator locator, IRegistrator<ContextMenuAction> registrator)
        {
            registrator.Register
            (
                new ContextMenuAction("Create/Prefab", directoryEntry =>
                {
                    if (!directoryEntry.IsDirectory)
                    {
                        directoryEntry = new FileEntry
                        (
                            path: Path.GetDirectoryName(directoryEntry.Path),
                            isDirectory: true
                        );
                    }

                    locator.Get<PopupsWrapper>()
                        .ShowPopup(PopupDefinition.CreateFilePopup
                            (
                                onSave: fileName =>
                                {
                                    var path = Path.Combine(directoryEntry.Path,
                                        fileName + PrefabConstants.PrefabExtension);

                                    locator.Get<IFileManager>()
                                        .Save(Array.Empty<byte>(), path);
                                },
                                fileExtension: PrefabConstants.PrefabExtension
                            )
                        );
                })
            );
        }
    }

    internal class PrefabIconInstaller : EditorExtensionInstaller<FileExtension, Icon>
    {
        public override void Install(Locator locator, IRegistrator<FileExtension, Icon> registrator)
        {
            registrator.Register
            (
                new FileExtension(PrefabConstants.PrefabExtension),
                new Icon("Prefab Icon")
            );
        }
    }

    internal class PrefabCompilerInstaller : EditorExtensionInstaller<ICustomContentCompiler>
    {
        public override void Install(Locator locator, IRegistrator<ICustomContentCompiler> registrator)
        {
            registrator.Register(new PrefabCompiler
            (
                fileManager: locator.Get<IFileManager>(),
                contentIdResolver: locator.Get<IMetadataResolver<ContentId>>()
            ));
        }
    }
}