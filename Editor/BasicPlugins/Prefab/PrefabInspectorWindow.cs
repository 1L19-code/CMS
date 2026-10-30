using System.Text;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace CMS.Editor
{
    internal class PrefabInspectorWindow : IInspectorWindow
    {
        private readonly IFileManager _fileManager;

        public PrefabInspectorWindow(IFileManager fileManager)
        {
            _fileManager = fileManager;
        }

        public ReaderId ReaderId => PrefabConstants.ReaderId;

        public VisualElement GetWindow(FileEntry fileEntry)
        {
            var root = new VisualElement();
            var objectField = new ObjectField();
            var guid = _fileManager.LoadUtf8(fileEntry);

            if (!string.IsNullOrEmpty(guid) && CmsPrefabPluginUtility.TryGetAsset(guid, out Object foundObj))
            {
                objectField.value = foundObj;
            }

            var saveButton = new Button(() =>
            {
                if (objectField.value is not Object obj)
                {
                    Debug.LogWarning("Prefab inspector window object is null.");

                    return;
                }

                if (!CmsPrefabPluginUtility.TruGetGuid(obj, out guid))
                {
                    if (obj is not GameObject go)
                    {
                        Debug.LogWarning("Object from scene is not GameObject.");

                        return;
                    }
                    
                    guid = CmsPrefabPluginUtility.CreateAsset(go);
                }

                _fileManager.Save(Encoding.UTF8.GetBytes(guid), fileEntry);
            })
            {
                text = "Save",
            };

            root.Add(objectField);
            root.Add(saveButton);

            return root;
        }
    }
}