using System;
using System.Linq;
using Cms.Build;
using ilya.cms.Editor;

namespace CMS.Editor
{
    internal static class ExtensionsBootstrap
    {
        public static void Build(Locator locator)
        {
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
            var customMetadataRegistrator = new Registrator<FileExtension>();
            var visibilityIgnoreFile = new Registrator<FileEntry>();
            var customCompiler = new Registrator<ICustomContentCompiler>();
            var metadataGenerators = new Registrator<IMetadataGenerator>();
            
            locator.Register(icon);
            locator.Register(inspector);
            locator.Register(contextMenu);
            locator.Register(customMetadataRegistrator);
            locator.Register(visibilityIgnoreFile);
            locator.Register(customCompiler);
            locator.Register(metadataGenerators);
        }
    }
}
