#if UNITY_EDITOR
using System.Globalization;
using System.Linq;
using System.Text;
using ilya.cms.Editor;

namespace CMS.Editor
{
    internal sealed class ReaderIdService : IReaderIdService, IMetadataGenerator
    {
        private readonly IRegistrationResolver<FileExtension, IInspectorWindow> _inspectors;
        private readonly IRegistrationResolver<FileExtension> _customMetadataResolver;

        public ReaderIdService(IRegistrationResolver<FileExtension, IInspectorWindow> inspectors,
            IRegistrationResolver<FileExtension> customMetadataResolver)
        {
            _inspectors = inspectors;
            _customMetadataResolver = customMetadataResolver;
        }

        public int Order => 100;
        public FileExtension MetadataExtension => new(CmsConstants.ReaderIdFileExtension);

        public bool TryGetId(string path, out ReaderId readerId)
        {
            readerId = default;

            if (IsCustomMetadata(path))
                return false;

            var data = MetadataFileUtility.LoadOrDefault(path, MetadataExtension);

            if (data == null)
                return false;

            if (!int.TryParse(Encoding.UTF8.GetString(data), NumberStyles.Integer,
                    CultureInfo.InvariantCulture, out var value))
                return false;

            readerId = new ReaderId(value);
            return true;
        }

        public byte[] Generate(FileEntry entry, byte[] existingData)
        {
            var extension = FileExtension.FromPath(entry.Path);
            var readerId = _inspectors.GetInstanceOrDefault(extension).ReaderId;

            return Encoding.UTF8.GetBytes(readerId.Value.ToString(CultureInfo.InvariantCulture));
        }

        private bool IsCustomMetadata(string path)
        {
            return _customMetadataResolver.GetInstances().Contains(FileExtension.FromPath(path));
        }
    }
}
#endif
