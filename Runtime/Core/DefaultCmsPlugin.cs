using System;
using CMS.Editor;

namespace CMS
{
    public sealed class DefaultCmsPlugin : ICmsPlugin
    {
        public ReaderId ReaderId { get; } = ReaderId.FromString("Default");

        public T Get<T>(ReadOnlyMemory<byte> data)
        {
            throw new InvalidOperationException(
                $"No runtime CMS plugin is registered to read content as '{typeof(T).FullName}'. " +
                "Register a matching RuntimeExtensionInstaller before calling Get.");
        }
    }
}
