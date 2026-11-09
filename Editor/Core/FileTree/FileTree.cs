using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using TreeView = UnityEngine.UIElements.TreeView;

namespace CMS.Editor
{
    internal readonly struct FileMoveRequest
    {
        public readonly FileEntry File;
        public readonly string NewPath;

        public FileMoveRequest(FileEntry file, string newPath)
        {
            File = file;
            NewPath = newPath;
        }
    }

    internal class FileTree : VisualElement, IDisposable
    {
        private class Node
        {
            public FileEntry Entry;
            public List<Node> Children { get; } = new();
        }

        private readonly IRegistrationResolver<FileExtension, Icon> _iconRegistrator;
        private readonly IRegistrationResolver<ContextMenuAction> _contextMenuRegistrator;
        private readonly IRegistrationResolver<FileExtension> _customMetadataResolver;
        private readonly IRootDirectoryChangeObserver _changeObserver;
        private readonly IRegistrationResolver<FileEntry> _hiddenFiles;
        private readonly IRegistrationResolver<FileTreeIgnore> _visibilityRules;
        private readonly HashSet<int> _directoryIndexes = new();

        private TreeView _treeView;

        public FileTree(IRegistrationResolver<FileExtension, Icon> iconRegistrator,
            IRegistrationResolver<ContextMenuAction> contextMenuRegistrator,
            IRegistrationResolver<FileExtension> customMetadataResolver,
            IRootDirectoryChangeObserver changeObserver,
            IRegistrationResolver<FileEntry> hiddenFiles,
            IRegistrationResolver<FileTreeIgnore> visibilityRules)
        {
            _iconRegistrator = iconRegistrator;
            _contextMenuRegistrator = contextMenuRegistrator;
            _changeObserver = changeObserver;
            _customMetadataResolver = customMetadataResolver;
            _hiddenFiles = hiddenFiles;
            _visibilityRules = visibilityRules;

            _changeObserver.Changed += Refresh;

            Show();
        }

        public event Action<FileEntry> FileOpenRequested;
        public event Action<FileMoveRequest> FileMoveRequested;
        public event Action FileSelectionCanceled;

        public void Dispose()
        {
            _changeObserver.Changed -= Refresh;
        }

        public void Refresh()
        {
            var expandedIds = GetExpandedIds(_treeView, _directoryIndexes.ToArray());
            _directoryIndexes.Clear();
            _treeView.SetRootItems(GenerateData(RawFileManager.GetAllEntries(),
                _customMetadataResolver.GetInstances()));
            _treeView.Rebuild();

            foreach (var expandedId in expandedIds)
            {
                _treeView.ExpandItem(expandedId);
            }
        }

        private void Show()
        {
            _directoryIndexes.Clear();
            _treeView = new TreeView
            {
                selectionType = SelectionType.Multiple,
                reorderable = true,
            };
            _treeView.SetRootItems(GenerateData(RawFileManager.GetAllEntries(),
                _customMetadataResolver.GetInstances()));

            _treeView.makeItem = () =>
            {
                var root = new VisualElement();
                root.AddToClassList("TreeNode");

                var image = new Image();
                image.AddToClassList("Icon");

                var label = new Label();
                label.AddToClassList("Label");

                root.Add(image);
                root.Add(label);

                return root;
            };

            _treeView.bindItem = (element, index) =>
            {
                var id = _treeView.GetIdForIndex(index);
                var file = _treeView.GetItemDataForId<FileEntry>(id);

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

                if (file.IsDirectory)
                {
                    _directoryIndexes.Add(index);
                }
            };

            _treeView.itemsChosen += items =>
            {
                foreach (var obj in items)
                {
                    var entry = (FileEntry)obj;
                    FileOpenRequested?.Invoke(entry);
                }
            };

            _treeView.itemIndexChanged += (itemId, destinationId) =>
            {
                var destinationEntry = _treeView.GetItemDataForId<FileEntry>(destinationId);

                var directoryEntry = destinationEntry.IsDirectory
                    ? destinationEntry
                    : _treeView.GetItemDataForId<FileEntry>(_treeView.GetParentIdForIndex(destinationId));

                var fileEntry = _treeView.GetItemDataForId<FileEntry>(itemId);
                FileMoveRequested?.Invoke(new FileMoveRequest(fileEntry, directoryEntry.Path));
            };

            _treeView.RegisterCallback<PointerDownEvent>(evt =>
            {
                var target = evt.target as VisualElement;

                while (target != null && target is not TreeView)
                {
                    if (target.ClassListContains("TreeNode"))
                        return;

                    target = target.parent;
                }

                _treeView.ClearSelection();
                FileSelectionCanceled?.Invoke();
            });

            CreateContextMenu(this, _treeView, _contextMenuRegistrator, Refresh);

            _treeView.AddToClassList("fileView");
            Add(_treeView);
        }

        private IList<TreeViewItemData<FileEntry>> GenerateData(FileEntry[] entries,
            FileExtension[] customMetadataExtensions)
        {
            var customMetadataExtensionsHash = customMetadataExtensions.ToHashSet();
            var hiddenPaths = _hiddenFiles.GetInstances()
                .Select(entry => Path.GetFullPath(entry.Path))
                .ToHashSet(StringComparer.OrdinalIgnoreCase);
            var visibilityRules = _visibilityRules.GetInstances();

            var paths = entries
                .Where(entry => !customMetadataExtensionsHash.Contains(FileExtension.FromPath(entry.Path)))
                .Where(entry => !hiddenPaths.Contains(Path.GetFullPath(entry.Path)) &&
                    !visibilityRules.Any(rule => rule.Matches(entry)))
                .ToDictionary
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

        private static void CreateContextMenu(VisualElement target, TreeView tree,
            IRegistrationResolver<ContextMenuAction> resolver, Action refreshAction)
        {
            var rootPath = Path.GetFullPath(CmsConstants.PreCompilePath);

            target.AddManipulator(new ContextualMenuManipulator(evt =>
            {
                foreach (var contextMenuAction in resolver.GetInstances())
                {
                    evt.menu.AppendAction
                    (
                        contextMenuAction.Path,
                        _ =>
                        {
                            var file = tree.selectedItem is not null
                                ? (FileEntry)tree.selectedItem
                                : new FileEntry(rootPath, isDirectory: true);

                            contextMenuAction.Action.Invoke(file);
                        },
                        DropdownMenuAction.AlwaysEnabled
                    );
                }

                evt.menu.AppendAction("Refresh", _ => refreshAction?.Invoke());
            }));
        }

        private static int[] GetExpandedIds(TreeView view, IReadOnlyList<int> indexes)
        {
            return indexes.Select(view.GetIdForIndex).Where(view.IsExpanded).ToArray();
        }
    }
}
