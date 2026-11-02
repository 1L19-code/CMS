using System;
using System.Collections.Generic;
using UnityEngine;

namespace CMS
{
    [Serializable]
    public struct GuidPrefabPair
    {
        public string Guid;
        public UnityEngine.Object Prefab;
    }

    public class PrefabsContainer : ScriptableObject
    {
        public List<GuidPrefabPair> Prefabs = new();
    }
}
