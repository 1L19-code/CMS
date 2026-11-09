using System;

namespace CMS
{
    public sealed class EntityPlugin : ICmsPlugin
    {
        private readonly IContent _content;

        public EntityPlugin(IContent content)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));
        }

        public T Get<T>(ReadOnlyMemory<byte> data)
        {
            if (!typeof(T).IsAssignableFrom(typeof(Entity)))
                throw new ArgumentException($"Cannot resolve entity as '{typeof(T).FullName}'.");

            var entity = new Entity(_content, EntityFileCodec.Index(data.ToArray()));
            return (T)(object)entity;
        }
    }
}
