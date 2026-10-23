using System.IO;
using UnityEditor;
using UnityEngine.UIElements;

namespace CMS.Editor
{
    public static class EditorBootstrap
    {
        public static Locator Initialize()
        {
            ValidateRoot();
            
            var locator = new Locator();
            ExtensionsBootstrap.Build(locator);
            
            var fileTree = CreateFileTree(locator);
            var fileManager = new ContentFileManager();
            
            locator.Register(fileTree);
            locator.Register(fileManager);
            
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
            
            var fileTree = new FileTree(RawFileManager.GetAllEntries(), iconResolver, contextMenuResolver);
            var style = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.ilya.cms/Editor/Core/FileTree/file_tree_style.uss");
            fileTree.styleSheets.Add(style);
            
            return fileTree;
        }
    }
}
