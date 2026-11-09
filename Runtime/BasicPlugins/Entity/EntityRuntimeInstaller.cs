namespace CMS
{
    public sealed class EntityRuntimeInstaller : RuntimeExtensionInstaller<ReaderId, ICmsPlugin>
    {
        public override void Install(Locator locator, IRegistrator<ReaderId, ICmsPlugin> registrator)
        {
            registrator.Register(EntityConstants.ReaderId, new EntityPlugin(locator.Get<IContent>()));
        }
    }
}
