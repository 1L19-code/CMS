#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using System.Text;

namespace CMS.Editor
{
    internal sealed class ContentIdService : IMetadataResolver<ContentId>, IMetadataGenerator
    {
        private readonly IRegistrationResolver<FileExtension> _customMetadataResolver;

        public ContentIdService(IRegistrationResolver<FileExtension> customMetadataResolver)
        {
            _customMetadataResolver = customMetadataResolver;
        }

        public int Order => 0;
        public FileExtension MetadataExtension => new(CmsConstants.ContentIdFileExtension);

        public ContentId Resolve(FileEntry entry)
        {
            return Resolve(entry.Path);
        }

        public ContentId Resolve(string path)
        {
            if (_customMetadataResolver.GetInstances().Contains(FileExtension.FromPath(path)))
                throw new ArgumentException("Metadata files do not have a content ID", nameof(path));

            var data = MetadataFileUtility.LoadOrDefault(path, MetadataExtension);

            if (data == null)
                throw new FileNotFoundException("Content ID metadata was not generated",
                    MetadataFileUtility.GetPath(path, MetadataExtension));

            if (!TryReadId(data, out var id))
                throw new InvalidDataException($"Invalid content ID metadata for '{path}'");

            return id;
        }

        public byte[] Generate(FileEntry entry, byte[] existingData)
        {
            if (TryReadId(existingData, out _))
                return existingData;

            var text = existingData == null ? string.Empty : GetText(existingData);

            if (!string.IsNullOrEmpty(text) && string.Concat(text.Where(character => !char.IsWhiteSpace(character))) != "{}")
                throw new InvalidDataException($"Invalid content ID metadata for '{entry.Path}'");

            return Encoding.UTF8.GetBytes(ContentId.Generate().Id.ToString("D"));
        }

        private static bool TryReadId(byte[] data, out ContentId id)
        {
            id = default;

            if (data == null || !Guid.TryParse(GetText(data), out var guid) || guid == Guid.Empty)
                return false;

            id = ContentId.FromGuid(guid);
            return true;
        }

        private static string GetText(byte[] data)
        {
            return Encoding.UTF8.GetString(data).Trim().TrimStart('\uFEFF').Trim();
        }
    }
}
#endif
