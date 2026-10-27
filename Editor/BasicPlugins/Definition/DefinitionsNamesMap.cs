#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Linq;
using Cms.DefinitionPlugin.Common;

namespace CMS.Editor
{
    internal class DefinitionsNamesMap
    {
        private readonly Dictionary<string, string> _map;

        public DefinitionsNamesMap()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes().Where(type =>
                    typeof(DefinitionBase).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface));

            _map = types.ToDictionary(type => type.Name, type => type.AssemblyQualifiedName);
        }

        public string[] GetFriendlyNames()
        {
            return _map.Keys.ToArray();
        }

        public string ConvertToAssemblyQualifiedName(string typeName)
        {
            return _map[typeName];
        }

        public Type GetType(string typeName)
        {
            return Type.GetType(_map[typeName]);
        }
    }
}
#endif
