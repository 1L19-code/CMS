using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CMS.Editor
{
    internal interface IRegistrationResolver
    {
    }
    //Collections
    internal interface IRegistrationResolver<out TInstance> : IRegistrationResolver
    {
        TInstance[] GetInstances();
    }
    public interface IRegistrator<in TInstance>
    {
        void Register(TInstance instance);
    }

    internal class Registrator<TInstance> :　IRegistrator<TInstance>, IRegistrationResolver<TInstance>
    {
        private readonly HashSet<TInstance> _instances = new();
        
        public void Register(TInstance instance)
        {
            _instances.Add(instance);
        }
        
        public TInstance[] GetInstances()
        {
            return _instances.ToArray();
        }
    }

    //Dictionaries
    public interface IRegistrator<in TKey, in TInstance>
    {
        void Register(TKey key, TInstance instance);
    }

    internal interface IRegistrationResolver<in TKey, out TInstance> : IRegistrationResolver
    {
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

        public GenericParameterAttributes GenericParameterAttributes
            => typeof(Registrator<TKey, TInstance>).GenericParameterAttributes;

        public void Register(TKey key, TInstance instance)
        {
            _instances.Add(key, instance);
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
