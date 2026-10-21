using System.Collections.Generic;
using System.Linq;

namespace CMS.Editor
{
    public interface IRegistrator<in TInstance, in TKey>
    {
        void Register(TInstance editorWindow, TKey key);
    }

    internal interface IRegistrationResolver<out TInstance, in TKey>
    {
        TInstance[] GetInstances();
        TInstance GetInstance(TKey key);
    }
    
    internal class Registrator<TInstance, TKey> : IRegistrator<TInstance, TKey>, IRegistrationResolver<TInstance, TKey>
    {
        private readonly Dictionary<TKey, TInstance> _instances = new();
        
        public void Register(TInstance editorWindow, TKey key)
        {
            _instances.Add(key, editorWindow);
        }

        public TInstance[] GetInstances()
        {
            return _instances.Values.ToArray();
        }

        public TInstance GetInstance(TKey key)
        {
            return _instances.GetValueOrDefault(key);
        }
    }
}