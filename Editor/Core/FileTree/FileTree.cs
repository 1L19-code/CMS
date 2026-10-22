using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;
using TreeView = UnityEngine.UIElements.TreeView;

namespace CMS.Editor
{
    internal class FileTree : VisualElement
    {
        private class Node
        {
            public FileEntry Entry;
            public List<Node> Children { get; } = new();
        }

        private readonly FileEntry[] _entries;
        private readonly IRegistrationResolver<FileExtension, Icon> _iconRegistrator;
        private readonly IRegistrationResolver<ContextMenuAction> _contextMenuRegistrator;

        public FileTree(FileEntry[] entries, IRegistrationResolver<FileExtension, Icon> iconRegistrator, IRegistrationResolver<ContextMenuAction> contextMenuRegistrator)
        {
            _entries = entries;
            _iconRegistrator = iconRegistrator;
            _contextMenuRegistrator = contextMenuRegistrator;
            
            Show();
        }

        public event Action<FileEntry> FileOpenRequested;

        private void Show()
        {
            var treeView = new TreeView
            {
                selectionType = SelectionType.Multiple
            };
            treeView.SetRootItems(GenerateData(_entries));

            treeView.makeItem = () => {
                var root = new VisualElement();
                root.AddToClassList("fileRow");

                var image = new Image();
                image.AddToClassList("icon");

                var label = new Label();
                label.AddToClassList("label");

                root.Add(image);
                root.Add(label);

                return root;
            };

            treeView.bindItem = (element, index) => {
                var id = treeView.GetIdForIndex(index);
                var file = treeView.GetItemDataForId<FileEntry>(id);

                var label = element.Q<Label>();
                label.text = Path.GetFileName(file.Path);
                
                var fileExtension = new FileExtension(Path.GetExtension(file.Path));
                var icon = file.IsDirectory
                    ? EditorGUIUtility.IconContent("Folder Icon").image
                    : EditorGUIUtility.IconContent
                    (
                        _iconRegistrator.GetInstanceOrDefault(fileExtension)
                            .NameInEditorGUI
                    ).image;

                var image = element.Q<Image>();
                image.image = icon;
            };

            treeView.itemsChosen += items => 
            {
                foreach (var obj in items)
                {
                    var entry = (FileEntry)obj;
                    FileOpenRequested?.Invoke(entry);
                }
            };

            treeView.AddToClassList("fileView");
            Add(treeView);
        }

        private static IList<TreeViewItemData<FileEntry>> GenerateData(FileEntry[] entries)
        {
            var paths = entries.ToDictionary
            (
                e => e.Path,
                e => new Node { Entry = e }
            );

            var rootPath = Path.GetFullPath(CmsConstants.PreCompilePath);
            var root = new Node
            {
                Entry = new FileEntry(rootPath, isDirectory: true)
            };
            paths[rootPath] = root;

            foreach (var node in paths.Values)
            {
                if (node == root)
                {
                    continue;
                }

                var directory = Path.GetDirectoryName(node.Entry.Path);

                if (directory == null)
                {
                    throw new ArgumentException("Directory not found");
                }

                if (!paths.TryGetValue(directory, out var parent))
                {
                    continue;
                }

                parent.Children.Add(node);
            }

            return ConvertToTreeViewItems(root);
        }

        private static IList<TreeViewItemData<FileEntry>> ConvertToTreeViewItems(Node root)
        {
            var nextId = 0;
            return new List<TreeViewItemData<FileEntry>> { ConvertNode(root) };

            TreeViewItemData<FileEntry> ConvertNode(Node node)
            {
                var id = nextId++;
                var children = new List<TreeViewItemData<FileEntry>>(node.Children.Count);

                foreach (var child in node.Children)
                {
                    children.Add(ConvertNode(child));
                }

                return new TreeViewItemData<FileEntry>(id, node.Entry, children);
            }
        }
    }
}
