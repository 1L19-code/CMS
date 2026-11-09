namespace CMS.Editor
{
    internal static class ExtensionsBootstrap
    {
        public static void Build(Locator locator)
        {
            var registrators = locator.GetSubclassOf<IRegistrationResolver>();
            var installers = ExtensionsBootstrapUtility.GetInstallers<ExtensionInstaller>();

            foreach (var registrator in registrators)
            foreach (var installer in installers)
            {
                installer.TryInstall(locator, registrator);
            }
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
            locator.Register(new Registrator<FileTreeIgnore>());
            locator.Register(customCompiler);
            locator.Register(metadataGenerators);
            locator.Register(new Registrator<StaticSourceIgnore>());
        }
    }
}
