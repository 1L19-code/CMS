using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CMS.Editor
{
    internal class PrefabCompiler : ICustomContentCompiler
    {
        private const string ContainerAssetPath = "Assets/Resources/prefabs.asset";

        private readonly IFileManager _fileManager;
        private readonly IMetadataResolver<ContentId> _contentIdResolver;

        private FileEntry[] _entries;

        public PrefabCompiler(IFileManager fileManager, IMetadataResolver<ContentId> contentIdResolver)
        {
            _fileManager = fileManager;
            _contentIdResolver = contentIdResolver;
        }

        public string[] FileExtensions => new[] { PrefabConstants.PrefabExtension };

        public void Compile()
        {
            var prefabsContainer = AssetDatabase.LoadAssetAtPath<PrefabsContainer>(ContainerAssetPath);

            if (prefabsContainer == null)
            {
                _fileManager.TryCreateDirectory("Assets/Resources");
                prefabsContainer = ScriptableObject.CreateInstance<PrefabsContainer>();
                AssetDatabase.CreateAsset(prefabsContainer, ContainerAssetPath);
            }

            prefabsContainer.Prefabs.Clear();

            _entries = FindEntries();
            var includedGuids = new HashSet<Guid>();

            foreach (var entry in _entries)
            {
                var guid = _fileManager.LoadUtf8(entry.Path);
                if (!Guid.TryParse(guid, out var assetGuid) || assetGuid == Guid.Empty)
                    throw new InvalidDataException($"Invalid prefab GUID in '{entry.Path}'.");

                var prefab = AssetDatabase.LoadAssetByGUID<Object>(new GUID(assetGuid.ToString("N")));

                if (prefab == null)
                    throw new InvalidDataException($"Prefab '{guid}' referenced by '{entry.Path}' was not found.");

                // Multiple content IDs may reference the same Unity asset.
                if (!includedGuids.Add(assetGuid))
                    continue;

                prefabsContainer.Prefabs.Add(new GuidPrefabPair
                {
                    Guid = assetGuid.ToString("N"),
                    Prefab = prefab
                });
            }

            EditorUtility.SetDirty(prefabsContainer);
            AssetDatabase.SaveAssets();
        }

        public Segment[] GetCustomContentSegments()
        {
            _entries ??= FindEntries();

            return _entries.Select(entry => new Segment
            (
                contentId: _contentIdResolver.Resolve(entry),
                readerId: PrefabConstants.ReaderId,
                // ContentId belongs to the index; the plugin receives only the asset GUID.
                data: _fileManager.Load(entry)
            )).ToArray();
        }

        private FileEntry[] FindEntries()
        {
            return _fileManager.Find($"*{PrefabConstants.PrefabExtension}")
                .Where(entry => !entry.IsDirectory)
                .ToArray();
        }
    }

}
