using System;
using System.Linq;

namespace CMS
{
    public static class ExtensionsBootstrapUtility
    {
        public static TInstaller[] GetInstallers<TInstaller>() where TInstaller : class
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type =>
                    typeof(TInstaller).IsAssignableFrom(type) &&
                    !type.IsAbstract &&
                    !type.IsInterface &&
                    !type.ContainsGenericParameters);

            return types.Select(type => (TInstaller)Activator.CreateInstance(type)).ToArray();
        }
    }
}
