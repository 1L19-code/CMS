namespace CMS
{
    public sealed class DefinitionPluginInstaller : RuntimeExtensionInstaller<ReaderId, ICmsPlugin>
    {
        public override void Install(Locator locator, IRegistrator<ReaderId, ICmsPlugin> registrator)
        {
            var plugin = new DefinitionPlugin();
            registrator.Register(plugin.ReaderId, plugin);
        }
    }
}
