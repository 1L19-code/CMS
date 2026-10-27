#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Cms.DefinitionPlugin.Common;
using CMS.Editor.Drawers;
using ilya.cms.Editor;
using UnityEngine;
using UnityEngine.UIElements;

namespace CMS.Editor
{
    internal class DefinitionEditor : IInspectorWindow
    {
        private readonly IFileManager _fileManager;
        private readonly DefinitionsNamesMap _definitionsNamesMap;

        public DefinitionEditor(IFileManager fileManager, DefinitionsNamesMap definitionsNamesMap)
        {
            _fileManager = fileManager;
            _definitionsNamesMap = definitionsNamesMap;
        }

        public ReaderId ReaderId => ReaderId.FromString("Definition");

        public VisualElement GetWindow(FileEntry descriptor)
        {
            var root = new VisualElement();
            root.style.flexBasis = 0f;
            root.style.minHeight = 0f;

            if (!File.Exists(descriptor.Path))
                throw new FileNotFoundException("Definition file not found", descriptor.Path);

            var file = _fileManager.Load(descriptor);
            var json = Encoding.UTF8.GetString(file);
            var meta = JsonUtility.FromJson<DefinitionMeta>(json);

            if (meta == null)
                throw new FileNotFoundException("Definition file not found", descriptor.Path);

            var field = new DropdownField
                { name = "ChooseDefinition", choices = new List<string>(_definitionsNamesMap.GetFriendlyNames()), };

            var drawer = new BasicObjectDrawer();

            if (meta.DefinitionType != null)
            {
                var type = Type.GetType(meta.DefinitionType);

                if (type != null)
                {
                    field.value = type.Name;

                    object instance = null;

                    if (!string.IsNullOrEmpty(meta.Data))
                        instance = JsonUtility.FromJson(meta.Data, type);

                    drawer.Draw(type, instance);
                }
            }

            field.RegisterValueChangedCallback(evt =>
            {
                var typeName = _definitionsNamesMap.ConvertToAssemblyQualifiedName(evt.newValue);
                meta.DefinitionType = typeName;
                var type = Type.GetType(typeName);

                drawer.Draw(type);
            });

            var saveButton = new Button(() =>
            {
                if (!drawer.TryGet(out var obj))
                    return;

                var objJson = JsonUtility.ToJson(obj, true);
                meta.Data = objJson;
                json = JsonUtility.ToJson(meta, true);
                _fileManager.Save(Encoding.UTF8.GetBytes(json), descriptor.Path);
            })
            {
                text = "Save",
                style =
                {
                    width = new StyleLength(50f)
                }
            };

            var scrollView = new ScrollView(ScrollViewMode.Vertical)
            {
                horizontalScrollerVisibility = ScrollerVisibility.Hidden,
                verticalScrollerVisibility = ScrollerVisibility.Auto
            };
            scrollView.style.flexGrow = 1f;
            scrollView.style.flexBasis = 0f;
            scrollView.style.minHeight = 0f;
            scrollView.Add(drawer);
            root.Add(field);
            root.Add(scrollView);
            root.Add(saveButton);

            return root;
        }
    }
}
#endif
