using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CMS.Editor
{
    internal class EntityInspector : IInspectorWindow
    {
        private const string StylePath = "Packages/com.ilya.cms/Editor/BasicPlugins/Entity/entity_inspector.uss";

        private readonly IFileManager _files;
        private readonly IMetadataResolver<ContentId> _ids;
        private readonly IRegistrationResolver<FileExtension> _metadata;
        private readonly EntitySlotsSourceService _slotsSource;
        private readonly Action<FileEntry> _openFile;

        public EntityInspector(IFileManager files, IMetadataResolver<ContentId> ids,
            IRegistrationResolver<FileExtension> metadata, Action<FileEntry> openFile)
        {
            _files = files;
            _ids = ids;
            _metadata = metadata;
            _slotsSource = new EntitySlotsSourceService(files);
            _openFile = openFile;
        }

        public ReaderId ReaderId => EntityConstants.ReaderId;

        public VisualElement GetWindow(FileEntry fileEntry)
        {
            var metadata = _metadata.GetInstances().ToHashSet();
            var files = _files.Find("*")
                .Where(entry => !entry.IsDirectory && !metadata.Contains(FileExtension.FromPath(entry.Path))
                    && !string.Equals(Path.GetFullPath(entry.Path), Path.GetFullPath(fileEntry.Path),
                        StringComparison.OrdinalIgnoreCase))
                .OrderBy(entry => entry.Path, StringComparer.Ordinal)
                .ToArray();

            var filesById = files.ToDictionary(entry => _ids.Resolve(entry));
            var segments = new List<EntityFileSegment>();
            var bytes = _files.Load(fileEntry);

            if (bytes.Length != 0)
                segments.AddRange(EntityFileCodec.Index(bytes).Segments);

            var root = new VisualElement();
            root.AddToClassList("entity-inspector");

            var style = AssetDatabase.LoadAssetAtPath<StyleSheet>(StylePath);

            if (style != null)
                root.styleSheets.Add(style);

            var title = new Label("Entity");
            title.AddToClassList("entity-inspector__title");

            var rows = new VisualElement();
            rows.AddToClassList("entity-inspector__attachments");

            var slotField = new EnumField("Slot", EntitySlot.Prefab);
            slotField.AddToClassList("entity-inspector__slot");

            var nameField = new TextField("Key") { tooltip = "Case-sensitive attachment key used by Entity.Get<T>(name)." };
            nameField.AddToClassList("entity-inspector__key");
            nameField.style.display = DisplayStyle.None;

            slotField.RegisterValueChangedCallback(evt =>
                nameField.style.display = (EntitySlot)evt.newValue == EntitySlot.Named
                    ? DisplayStyle.Flex
                    : DisplayStyle.None);

            var actions = new VisualElement();
            actions.AddToClassList("entity-inspector__actions");

            var addNewTitle = new Label("Add New");
            addNewTitle.AddToClassList("entity-inspector__section-title");

            var attachButton = new Button(() =>
            {
                var slot = (EntitySlot)slotField.value;
                var key = slot == EntitySlot.Named ? nameField.value.Trim() : null;

                if (slot == EntitySlot.Named && string.IsNullOrWhiteSpace(key))
                {
                    Debug.LogWarning("Enter a key for the named attachment.");
                    return;
                }

                ShowFileMenu(slot, key);
            }) { text = "Attach" };
            attachButton.AddToClassList("entity-inspector__attach");

            var saveButton = new Button(() =>
            {
                var codec = new EntityFileCodec(fileEntry.Path);

                foreach (var segment in segments)
                    codec.AppendToFile(segment);

                _files.Save(codec.GetByte(), fileEntry);
                _slotsSource.Build();
            }) { text = "Save" };
            saveButton.AddToClassList("entity-inspector__save");

            RefreshRows();

            actions.Add(slotField);
            actions.Add(nameField);
            actions.Add(attachButton);

            root.Add(title);
            root.Add(rows);
            root.Add(addNewTitle);
            root.Add(actions);
            root.Add(saveButton);

            return root;

            void RefreshRows()
            {
                rows.Clear();

                foreach (var segment in segments.ToArray())
                {
                    var currentSegment = segment;
                    var row = new VisualElement();
                    row.AddToClassList("entity-inspector__attachment");

                    var name = filesById.TryGetValue(segment.ContentId, out var attached)
                        ? attached.Path
                        : $"Missing file: {segment.ContentId}";

                    var slotName = segment.Slot == EntitySlot.Named ? segment.Name : segment.Slot.ToString();
                    var label = new Label($"{slotName}: {name}") { tooltip = name };
                    label.AddToClassList("entity-inspector__file");

                    var keyField = new TextField("Key") { value = segment.Name, isDelayed = true };
                    keyField.AddToClassList("entity-inspector__key");
                    keyField.RegisterValueChangedCallback(evt =>
                    {
                        var key = evt.newValue.Trim();

                        if (string.Equals(key, currentSegment.Name, StringComparison.Ordinal))
                            return;

                        if (string.IsNullOrWhiteSpace(key) || HasAttachment(EntitySlot.Named, key))
                        {
                            keyField.SetValueWithoutNotify(currentSegment.Name);
                            Debug.LogWarning("Attachment keys must be non-empty and unique within the entity.");
                            return;
                        }

                        var index = segments.IndexOf(currentSegment);
                        currentSegment = new EntityFileSegment(currentSegment.ContentId, EntitySlot.Named, key);
                        segments[index] = currentSegment;
                        label.text = $"{key}: {name}";
                        keyField.SetValueWithoutNotify(key);
                    });

                    var detachButton = new Button(() =>
                    {
                        segments.Remove(currentSegment);
                        RefreshRows();
                    }) { text = "Detach" };
                    detachButton.AddToClassList("entity-inspector__detach");

                    var replaceButton = new Button(() =>
                        ShowFileMenu(currentSegment.Slot, currentSegment.Name)) { text = "Replace" };
                    replaceButton.AddToClassList("entity-inspector__replace");

                    var openButton = new Button(() => _openFile(attached)) { text = "Open" };
                    openButton.AddToClassList("entity-inspector__open");
                    openButton.SetEnabled(filesById.ContainsKey(segment.ContentId));

                    row.Add(label);

                    if (segment.Slot == EntitySlot.Named)
                        row.Add(keyField);

                    row.Add(openButton);
                    row.Add(replaceButton);
                    row.Add(detachButton);
                    rows.Add(row);
                }
            }

            void ShowFileMenu(EntitySlot slot, string key)
            {
                var menu = new GenericMenu();
                var currentIndex = FindAttachment(slot, key);

                if (files.Length == 0)
                    menu.AddDisabledItem(new GUIContent("No CMS files available"));

                foreach (var file in files)
                {
                    var contentId = _ids.Resolve(file);
                    var isSelected = currentIndex >= 0 && segments[currentIndex].ContentId.Equals(contentId);

                    menu.AddItem(new GUIContent(file.Path), isSelected, () =>
                    {
                        var index = FindAttachment(slot, key);
                        var replacement = new EntityFileSegment(contentId, slot, key);

                        if (index >= 0)
                            segments[index] = replacement;
                        else
                            segments.Add(replacement);

                        RefreshRows();
                    });
                }

                menu.ShowAsContext();
            }

            int FindAttachment(EntitySlot slot, string key)
            {
                return segments.FindIndex(segment => segment.Slot == slot &&
                    (slot != EntitySlot.Named || string.Equals(segment.Name, key, StringComparison.Ordinal)));
            }

            bool HasAttachment(EntitySlot slot, string key)
            {
                return FindAttachment(slot, key) >= 0;
            }
        }
    }
}
