using System;
using System.Linq;
using ilya.cms.Editor;
using UnityEngine;

namespace CMS.Editor
{
    public abstract class ExtensionInstaller
    {
        /// <summary>
        /// Use it to cast object registrator to generic IRegistrator
        /// </summary>
        /// <param name="registrator"></param>
        internal abstract void Install(IRegistrationResolver registrator);
    }

    public abstract class ExtensionInstaller<TKey, TInstance> : ExtensionInstaller
    {
        internal override void Install(IRegistrationResolver registrator)
        {
            if (registrator is not IRegistrator<TKey, TInstance> casted)
                throw new ArgumentException("IRegistrationResolver registrator is not of type IRegistrator<TKey, TInstance>");

            Install(casted);
        }

        public abstract void Install(IRegistrator<TKey, TInstance> registrator);
    }

    internal static class ExtensionsBootstrap
    {
        public static void Build(Locator locator)
        {
            ExtensionsBootstrapUtility.BindRegistratorsInLocator(locator);
            var registrators = locator.GetSubclassOf<IRegistrationResolver>();

            var method = typeof(ExtensionsBootstrapUtility)
                .GetMethod(nameof(ExtensionsBootstrapUtility.GetInstallers));

            if (method is null)
                throw new Exception("Could not find extension installer method");

            foreach (var registrator in registrators)
            {
                var generic = method.MakeGenericMethod(registrator.KeyType, registrator.InstanceType);
                var installers = (ExtensionInstaller[])generic.Invoke(null, new object[] { });
                
                foreach (var installer in installers)
                {
                    installer.Install(registrator);
                }
            }
        }
    }

    internal static class ExtensionsBootstrapUtility
    {
        public static ExtensionInstaller<TKey, TInstance>[] GetInstallers<TKey, TInstance>()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type =>
                    typeof(ExtensionInstaller<TKey, TInstance>).IsAssignableFrom(type) &&
                    !type.IsAbstract &&
                    !type.IsInterface);

            return types.Select(type => Activator.CreateInstance(type) as ExtensionInstaller<TKey, TInstance>).ToArray();
        }

        public static void BindRegistratorsInLocator(Locator locator)
        {
            var icon = new Registrator<FileExtension, Icon>(new Icon("d_DefaultAsset Icon"));
            var inspector = new Registrator<FileExtension, IInspectorWindow>(new FileDoesNotSupportInspector());

            locator.Register(icon);
            locator.Register(inspector);
        }
    }
}
