using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CMS.Editor
{
    public static class EditorBootstrap
    {
        public static Locator Initialize()
        {
            var locator = InitializeServices();

            var fileTree = CreateFileTree(locator);
            locator.Register(fileTree);

            return locator;
        }

        internal static Locator InitializeServices()
        {
            ValidateRoot();

            var locator = new Locator();
            ExtensionsBootstrapUtility.BindRegistratorsInLocator(locator);

            var customMetadataRegistrator = locator.Get<IRegistrator<FileExtension>>();
            customMetadataRegistrator.Register(new FileExtension(CmsConstants.ContentIdFileExtension));
            customMetadataRegistrator.Register(new FileExtension(CmsConstants.ReaderIdFileExtension));

            var fileManager = new ContentFileManager();
            locator.Register(fileManager);

            var readerIdService = new ReaderIdService(
                locator.Get<IRegistrationResolver<FileExtension, IInspectorWindow>>(),
                locator.Get<IRegistrationResolver<FileExtension>>());
            locator.Register(readerIdService);

            var contentIdService = new ContentIdService(locator.Get<IRegistrationResolver<FileExtension>>());
            locator.Register(contentIdService);

            var metadataGenerators = locator.Get<IRegistrator<IMetadataGenerator>>();
            metadataGenerators.Register(contentIdService);
            metadataGenerators.Register(readerIdService);

            ExtensionsBootstrap.Build(locator);

            var metadataPipeline = new MetadataGenerationPipeline(
                locator.Get<IRegistrationResolver<IMetadataGenerator>>(),
                locator.Get<IRegistrationResolver<FileExtension>>(),
                customMetadataRegistrator,
                locator.Get<IRootDirectoryChangeObserver>());
            locator.Register(metadataPipeline);

            locator.Register(new PopupsWrapper());

            var popupsWrapper = locator.Get<PopupsWrapper>();

            locator.Get<IRegistrator<ContextMenuAction>>()
                .Register(new ContextMenuAction("Delete", entry =>
                {
                    if (entry.Path == Path.GetFullPath(CmsConstants.PreCompilePath))
                    {
                        Debug.LogWarning($"Cannot delete {CmsConstants.PreCompilePath} because it is the root directory");
                        
                        return;
                    }
                    
                    popupsWrapper.ShowPopup(PopupDefinition
                        .WithTitle("Delete?")
                        .WithButtons(
                            new ButtonDefinition("Delete", _ =>
                            {
                                fileManager.Delete(entry);
                            })));
                }));

            return locator;
        }

        private static void ValidateRoot()
        {
            var directory = Path.GetFullPath(CmsConstants.PreCompilePath);

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private static FileTree CreateFileTree(Locator locator)
        {
            var iconResolver = locator.Get<IRegistrationResolver<FileExtension, Icon>>();
            var contextMenuResolver = locator.Get<IRegistrationResolver<ContextMenuAction>>();
            var customMetadataResolver = locator.Get<IRegistrationResolver<FileExtension>>();
            var observer = locator.Get<IRootDirectoryChangeObserver>();

            var fileTree = new FileTree(iconResolver, contextMenuResolver, customMetadataResolver, observer);
            var style = AssetDatabase.LoadAssetAtPath<StyleSheet>(
                "Packages/com.ilya.cms/Editor/Core/FileTree/file_tree_style.uss");
            fileTree.styleSheets.Add(style);

            return fileTree;
        }
    }
}