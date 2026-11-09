using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CMS
{
    public enum EntitySlot
    {
        Named,
        Prefab,
        Model,
        UI
    }

    public readonly struct EntityFileSegment
    {
        public readonly ContentId ContentId;
        public readonly EntitySlot Slot;
        public readonly string Name;

        public EntityFileSegment(ContentId contentId, EntitySlot slot, string name = null)
        {
            ContentId = contentId;
            Slot = slot;
            Name = name;
        }
    }

    public class EntityFileIndex
    {
        public IReadOnlyList<EntityFileSegment> Segments { get; }

        internal EntityFileIndex(List<EntityFileSegment> segments)
        {
            Segments = segments.AsReadOnly();
        }

        public IEnumerable<EntityFileSegment> GetSegments(EntitySlot slot)
        {
            return Segments.Where(segment => segment.Slot == slot);
        }
    }

    public class EntityFileCodec
    {
        private readonly List<EntityFileSegment> _segments = new();

        public EntityFileCodec(string filePath)
        {
        }

        public void AppendToFile(EntityFileSegment segment)
        {
            if (!Enum.IsDefined(typeof(EntitySlot), segment.Slot))
                throw new ArgumentOutOfRangeException(nameof(segment), "Unknown entity slot.");

            if (segment.Slot == EntitySlot.Named && string.IsNullOrWhiteSpace(segment.Name))
                throw new ArgumentException("A named attachment must specify a key.", nameof(segment));

            if (segment.Slot != EntitySlot.Named && segment.Name != null)
                throw new ArgumentException("Only named attachments can specify a key.", nameof(segment));

            if (_segments.Any(existing => existing.Slot == segment.Slot &&
                (segment.Slot != EntitySlot.Named || string.Equals(existing.Name, segment.Name, StringComparison.Ordinal))))
                throw new ArgumentException($"Duplicate entity attachment '{segment.Name ?? segment.Slot.ToString()}'.", nameof(segment));

            _segments.Add(segment);
        }

        public byte[] GetByte()
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);

            writer.Write(EntityConstants.FileMagic);

            foreach (var segment in _segments)
            {
                writer.Write(segment.Slot.ToString());

                if (segment.Slot == EntitySlot.Named)
                    writer.Write(segment.Name);

                writer.Write(segment.ContentId.ToString());
            }

            return stream.ToArray();
        }

        public static EntityFileIndex Index(byte[] bytes)
        {
            if (bytes == null)
                throw new ArgumentNullException(nameof(bytes));

            // Newly created entity files are empty until first saved in the inspector.
            if (bytes.Length == 0)
                return new EntityFileIndex(new List<EntityFileSegment>());

            using var stream = new MemoryStream(bytes, false);
            using var reader = new BinaryReader(stream);

            foreach (var expected in EntityConstants.FileMagic)
            {
                if (stream.Position == stream.Length || reader.ReadByte() != expected)
                    throw new InvalidDataException("Invalid entity file signature.");
            }

            var codec = new EntityFileCodec(null);

            try
            {
                while (stream.Position < stream.Length)
                {
                    var slotName = reader.ReadString();
                    // Preserve old singleton entries as named attachments without resolving their types.
                    var slot = slotName == "Singleton" ? EntitySlot.Named : ParseSlot(slotName);
                    var name = slot == EntitySlot.Named ? reader.ReadString() : null;
                    var contentId = ContentId.FromGuid(reader.ReadString());

                    codec.AppendToFile(new EntityFileSegment(contentId, slot, name));
                }
            }
            catch (EndOfStreamException exception)
            {
                throw new InvalidDataException("The entity file contains an incomplete segment.", exception);
            }
            catch (FormatException exception)
            {
                throw new InvalidDataException("The entity file contains an invalid segment.", exception);
            }
            catch (ArgumentException exception)
            {
                throw new InvalidDataException("The entity file contains an invalid or duplicate attachment.", exception);
            }

            return new EntityFileIndex(codec._segments);
        }

        private static EntitySlot ParseSlot(string name)
        {
            if (!Enum.TryParse<EntitySlot>(name, out var slot) || !Enum.IsDefined(typeof(EntitySlot), slot))
                throw new InvalidDataException($"Unknown entity slot '{name}'.");

            return slot;
        }
    }
}
