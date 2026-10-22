using System;
using System.Collections.Generic;
using System.Linq;
using Codice.CM.Common;
using Unity.VisualScripting;

namespace CMS
{
    internal static class LocatorIgnoreTypes
    {
        public static readonly HashSet<Type> Types = new()
        {
            typeof(IDisposable),
        };
    }
    
    public class Locator
    {
        private readonly Dictionary<Type, object> _objects = new();

        public void Register<T>(T instance)
        {
            if(LocatorIgnoreTypes.Types.Contains(typeof(T)))
                return;
            
            _objects[instance.GetType()] = instance;
            var interfaces = typeof(T).GetInterfaces();

            foreach (var i in interfaces)
            {
                if(LocatorIgnoreTypes.Types.Contains(i))
                    continue;
                
                _objects[i] = instance;
            }
        }

        public T Get<T>()
        {
            if (!_objects.TryGetValue(typeof(T), out var instance))
                throw new ArgumentException(typeof(T) +　" does not exist");

            return (T)instance;
        }

        public T[] GetSubclassOf<T>()
        {
            return _objects.Values
                .OfType<T>()
                .Distinct()
                .ToArray();
        }
    }
}
