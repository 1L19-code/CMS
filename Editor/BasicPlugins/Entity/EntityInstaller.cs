using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;

namespace CMS.Editor
{
    public class EntityContextMenuInstaller : EditorExtensionInstaller<ContextMenuAction>
    {
        public override void Install(Locator locator, IRegistrator<ContextMenuAction> registrator)
        {
            var fileManager = locator.Get<IFileManager>();

            registrator.Register(new ContextMenuAction("Create/Entity", fileEntry =>
            {
                if (!fileEntry.IsDirectory)
                    return;

                locator.Get<PopupsWrapper>().ShowPopup(PopupDefinition.CreateFilePopup
                (
                    onSave: fileName => Create(fileEntry, fileManager, fileName),
                    fileExtension: EntityConstants.FileExtension
                ));
            }));
        }

        private static void Create(FileEntry targetDirectory, IFileManager fileManager, string name)
        {
            if (string.IsNullOrWhiteSpace(name) || name == "." || name == ".." ||
                name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0 ||
                name.Contains(Path.DirectorySeparatorChar.ToString()) ||
                name.Contains(Path.AltDirectorySeparatorChar.ToString()))
                return;

            var path = Path.Combine(targetDirectory.Path, name + EntityConstants.FileExtension);

            if (File.Exists(path) || Directory.Exists(path))
            {
                Debug.LogWarning($"A file or directory already exists at '{path}'.");
                return;
            }

            fileManager.Save(Array.Empty<byte>(), path);
        }
    }
    
    internal class EntityIconInstaller : EditorExtensionInstaller<FileExtension, Icon>
    {
        public override void Install(Locator locator, IRegistrator<FileExtension, Icon> registrator)
        {
            registrator.Register(new FileExtension(EntityConstants.FileExtension), new Icon("GameObject Icon"));
        }
    }

    internal class EntityInspectorInstaller : EditorExtensionInstaller<FileExtension, IInspectorWindow>
    {
        public override void Install(Locator locator, IRegistrator<FileExtension, IInspectorWindow> registrator)
        {
            var fileManager = locator.Get<IFileManager>();
            var attachedIds = new HashSet<ContentId>();
            var contentIds = locator.Get<IMetadataResolver<ContentId>>();

            void RefreshAttachments()
            {
                var ids = new HashSet<ContentId>();
                foreach (var entry in fileManager.Find("*" + EntityConstants.FileExtension))
                {
                    var bytes = fileManager.Load(entry);
                    if (bytes.Length == 0)
                        continue;

                    foreach (var segment in EntityFileCodec.Index(bytes).Segments)
                        ids.Add(segment.ContentId);
                }

                attachedIds.Clear();
                attachedIds.UnionWith(ids);
            }

            RefreshAttachments();
            locator.Get<IRootDirectoryChangeObserver>().Changed += RefreshAttachments;
            locator.Get<IRegistrator<StaticSourceIgnore>>()
                .Register(StaticSourceIgnore.ForPredicate((entry, id) => attachedIds.Contains(id)));
            locator.Get<IRegistrator<FileTreeIgnore>>()
                .Register(new FileTreeIgnore(entry => !entry.IsDirectory && attachedIds.Contains(contentIds.Resolve(entry))));

            registrator.Register(new FileExtension(EntityConstants.FileExtension), new EntityInspector(
                fileManager, locator.Get<IMetadataResolver<ContentId>>(),
                locator.Get<IRegistrationResolver<FileExtension>>(),
                entry => locator.Get<InspectorWrapper>().OpenFile(entry)));
        }
    }
}
