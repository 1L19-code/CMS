using System;
using System.Linq;
using ilya.cms.Editor;

namespace CMS.Editor
{
    internal static class ExtensionsBootstrap
    {
        public static void Build(Locator locator)
        {
            ExtensionsBootstrapUtility.BindRegistratorsInLocator(locator);
            var registrators = locator.GetSubclassOf<IRegistrationResolver>();
            var installers = ExtensionsBootstrapUtility.GetInstallers();

            foreach (var registrator in registrators)
            foreach (var installer in installers)
            {
                installer.TryInstall(locator, registrator);
            }
        }
    }

    internal static class ExtensionsBootstrapUtility
    {
        public static ExtensionInstaller[] GetInstallers()
        {
            var types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(assembly => assembly.GetTypes())
                .Where(type =>
                    typeof(ExtensionInstaller).IsAssignableFrom(type) &&
                    !type.IsAbstract &&
                    !type.IsInterface);

            return types.Select(type => Activator.CreateInstance(type) as ExtensionInstaller).ToArray();
        }

        public static void BindRegistratorsInLocator(Locator locator)
        {
            var icon = new Registrator<FileExtension, Icon>(new Icon("d_DefaultAsset Icon"));
            var inspector = new Registrator<FileExtension, IInspectorWindow>(new FileDoesNotSupportInspector());
            var contextMenu = new Registrator<ContextMenuAction>();
            
            locator.Register(icon);
            locator.Register(inspector);
            locator.Register(contextMenu);
        }
    }
}
