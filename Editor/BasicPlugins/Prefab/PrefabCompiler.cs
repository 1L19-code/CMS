using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace CMS.Editor
{
    internal class PrefabCompiler : ICustomContentCompiler
    {
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
            var prefabsContainer = AssetDatabase.LoadAssetAtPath<PrefabsContainer>("Assets/Resources/Prefabs");

            if (prefabsContainer is null)
            {
                _fileManager.TryCreateDirectory("Assets/Resources");
                prefabsContainer = ScriptableObject.CreateInstance<PrefabsContainer>();
                prefabsContainer.Prefabs = new List<ContentIdPrefabPair>();
                AssetDatabase.CreateAsset(prefabsContainer, "Assets/Resources/prefabs.asset");
                AssetDatabase.SaveAssets();
            }

            prefabsContainer.Prefabs.Clear();

            _entries ??= _fileManager.Find($"*{PrefabConstants.PrefabExtension}");

            foreach (var entry in _entries)
            {
                if (entry.IsDirectory)
                {
                    continue;
                }

                var guid = _fileManager.LoadUtf8(entry.Path);
                var prefab = AssetDatabase.LoadAssetByGUID<Object>(new GUID(guid));

                if (prefab is null)
                {
                    continue;
                }

                prefabsContainer.Prefabs.Add(new ContentIdPrefabPair
                {
                    ContentId = _contentIdResolver.Resolve(entry).Id.ToString(), 
                    Prefab = prefab
                });
            }
        }

        public Segment[] GetCustomContentSegments()
        {
            _entries ??= _fileManager.Find($"*{PrefabConstants.PrefabExtension}");

            return _entries.Select(entry => new Segment
            (
                contentId: _contentIdResolver.Resolve(entry),
                readerId: PrefabConstants.ReaderId,
                data: _fileManager.Load(entry)
            )).ToArray();
        }
    }

    [Serializable]
    internal struct ContentIdPrefabPair
    {
        public string ContentId;
        public Object Prefab;
    }

    internal class PrefabsContainer : ScriptableObject
    {
        public List<ContentIdPrefabPair> Prefabs;
    }
}