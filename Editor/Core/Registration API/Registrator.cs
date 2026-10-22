using System;
using System.Collections.Generic;
using System.Linq;

namespace CMS.Editor
{
    public interface IRegistrator
    {
    }

    public interface IRegistrator<in TKey, in TInstance> : IRegistrator
    {
        void Register(TKey key, TInstance instance);
    }

    internal interface IRegistrationResolver
    {
        Type KeyType { get; }
        Type InstanceType { get; }
    }

    internal interface IRegistrationResolver<in TKey, out TInstance> :　IRegistrationResolver
    {
        TInstance[] GetInstances();
        TInstance GetInstanceOrDefault(TKey key);
        bool HasInstance(TKey key);
    }

    internal class Registrator<TKey, TInstance> : IRegistrator<TKey, TInstance>, IRegistrationResolver<TKey, TInstance>
    {
        private readonly Dictionary<TKey, TInstance> _instances = new();
        private readonly TInstance _defaultInstance;

        public Registrator(TInstance defaultInstance)
        {
            _defaultInstance = defaultInstance;
        }

        public Type KeyType => typeof(TKey);
        public Type InstanceType => typeof(TInstance);

        public void Register(TKey key, TInstance instance)
        {
            _instances.Add(key, instance);
        }

        public TInstance[] GetInstances()
        {
            return _instances.Values.ToArray();
        }

        public TInstance GetInstanceOrDefault(TKey key)
        {
            return _instances.GetValueOrDefault(key, _defaultInstance);
        }

        public bool HasInstance(TKey key)
        {
            return _instances.ContainsKey(key);
        }
    }
}
