using System;
using System.Collections.Generic;

namespace CMS
{
    public sealed class Entity
    {
        private readonly IContent _content;
        private readonly Dictionary<EntitySlot, ContentId> _slots = new();
        private readonly Dictionary<string, ContentId> _named = new(StringComparer.Ordinal);

        public Entity(IContent content, EntityFileIndex index)
        {
            _content = content ?? throw new ArgumentNullException(nameof(content));

            if (index == null)
                throw new ArgumentNullException(nameof(index));

            foreach (var segment in index.Segments)
            {
                if (segment.Slot == EntitySlot.Named)
                    _named.Add(segment.Name, segment.ContentId);
                else
                    _slots.Add(segment.Slot, segment.ContentId);
            }
        }

        public T Get<T>(EntitySlot slot)
        {
            if (!Enum.IsDefined(typeof(EntitySlot), slot) || slot == EntitySlot.Named)
                throw new ArgumentOutOfRangeException(nameof(slot), "Use a string key for named attachments.");

            if (!_slots.TryGetValue(slot, out var contentId))
                throw new KeyNotFoundException($"Entity has no attachment in slot '{slot}'.");

            return _content.Get<T>(contentId);
        }

        public T Get<T>(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("An attachment key is required.", nameof(name));

            if (!_named.TryGetValue(name, out var contentId))
                throw new KeyNotFoundException($"Entity has no attachment named '{name}'.");

            return _content.Get<T>(contentId);
        }
    }
}
