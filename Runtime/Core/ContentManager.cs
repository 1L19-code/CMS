using System;
using System.Threading.Tasks;
using CMS.Editor;

namespace CMS
{
    internal class ContentManager : IContent
    {
        private ContentIndex _index;
        private IRegistrationResolver<ReaderId, ICmsPlugin> _plugins;

        public async ValueTask IndexAsync()
        {
            var locator = new Locator();
            var plugins = new Registrator<ReaderId, ICmsPlugin>(null);
            locator.Register(plugins);

            var installers = ExtensionsBootstrapUtility.GetInstallers<RuntimeExtensionInstaller>();

            foreach (var installer in installers)
            {
                installer.TryInstall(locator, plugins);
            }

            var index = await ContentIndex.IndexAsync(new ResourcesContainerFileReader());
            _plugins = plugins;
            _index = index;
        }

        public T Get<T>(ContentId contentId)
        {
            if (_index == null)
            {
                throw new InvalidOperationException("Call IndexAsync first.");
            }

            var reader = _index.GetReader(contentId);

            if (!_plugins.HasInstance(reader))
            {
                throw new InvalidOperationException("Runtime plugin instance not found.");
            }
            
            var segment = _index.GetSegment(contentId);

            return _plugins.GetInstanceOrDefault(reader).Get<T>(segment);
        }
    }
}