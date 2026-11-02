using System;
using System.Collections.Generic;
using System.Text;
using Object = UnityEngine.Object;

namespace CMS
{
    public class PrefabPlugin : ICmsPlugin
    {
        private readonly Dictionary<Guid, UnityEngine.Object> _prefabs;

        public PrefabPlugin(Dictionary<Guid, Object> prefabs)
        {
            _prefabs = prefabs;
        }

        public T Get<T>(ReadOnlyMemory<byte> data)
        {
            var guid = Guid.Parse(Encoding.UTF8.GetString(data.Span));

            if (guid == Guid.Empty)
            {
                throw new Exception("Invalid GUID");
            }

            if (!_prefabs.TryGetValue(guid, out var prefab))
            {
                throw new  Exception("Prefab not found");
            }

            if (prefab is not T casted)
            {
                throw new Exception("Cannot resolve type " + typeof(T).FullName);
            }

            return casted;
        }
    }
}