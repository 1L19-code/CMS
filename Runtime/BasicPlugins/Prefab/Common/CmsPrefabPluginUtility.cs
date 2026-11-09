#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CMS
{
    public static class CmsPrefabPluginUtility
    {
        public static bool TruGetGuid(Object obj, out string guid)
        {
            return AssetDatabase.TryGetGUIDAndLocalFileIdentifier(obj, out guid, out _);
        }

        public static string CreateAsset(GameObject obj)
        {
            var fullDirectoryPath = Path.GetFullPath(PrefabConstants.PrefabPath);

            if (!Directory.Exists(fullDirectoryPath))
            {
                Directory.CreateDirectory(fullDirectoryPath);
            }
            
            var prefabPath = Path.Combine(PrefabConstants.PrefabPath, obj.name + ".prefab");
            prefabPath = AssetDatabase.GenerateUniqueAssetPath(prefabPath);

            if (PrefabUtility.IsPartOfAnyPrefab(obj))
            {
                var prefab = PrefabUtility.GetCorrespondingObjectFromSource(obj);

                if (prefab is not null)
                {
                    var assetPath =  AssetDatabase.GetAssetPath(prefab);
                    
                    return AssetDatabase.AssetPathToGUID(assetPath);
                }
            }
            
            PrefabUtility.SaveAsPrefabAssetAndConnect(obj, prefabPath, InteractionMode.UserAction);

            return AssetDatabase.AssetPathToGUID(prefabPath);
        }

        public static bool TryGetAsset(string guid, out Object obj)
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            obj = string.IsNullOrEmpty(path) ? null : AssetDatabase.LoadAssetAtPath<Object>(path);

            return obj != null;
        }
    }
}
#endif
