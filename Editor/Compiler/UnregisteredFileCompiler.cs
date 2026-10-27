#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CMS.Editor
{
    internal class UnregisteredFileCompiler
    {
        private readonly string _rootPath;

        private readonly IMetadataResolver<ContentId> _contentService;
        private readonly IReaderIdService _readerIdService;
        private readonly IRegistrationResolver<FileExtension> _customMetadataResolver;

        public UnregisteredFileCompiler(string rootPath, IMetadataResolver<ContentId> contentService,
            IReaderIdService readerIdService, IRegistrationResolver<FileExtension> customMetadataResolver)
        {
            _rootPath = rootPath;
            _contentService = contentService;
            _readerIdService = readerIdService;
            _customMetadataResolver = customMetadataResolver;
        }

        public void Build(IEnumerable<Segment> customSegments, HashSet<string> ignoredExtensions)
        {
            var path = Path.GetFullPath(_rootPath);
            Directory.CreateDirectory(path);
            path = Path.Combine(path, CmsConstants.ContentFileName);

            if (File.Exists(path))
                File.Delete(path);

            using var stream = new FileStream(path, FileMode.CreateNew);
            using var writer = new BinaryWriter(stream);

            writer.Seek(0, SeekOrigin.Begin);
            writer.Write(ContainerFileConstants.Magic);
            writer.Write(ContainerFileConstants.Version);

            var sizePosition = stream.Position;
            writer.Write(0);
            var bodyStart = stream.Position;

            var segments = new List<Segment>(GetSegments(ignoredExtensions));
            segments.AddRange(customSegments);

            foreach (var segment in segments)
                writer.Write(SegmentUtility.Encode(segment));

            var bodyEnd = stream.Position;
            var size = checked((int)(bodyEnd - bodyStart));
            
            stream.Seek(sizePosition, SeekOrigin.Begin);
            writer.Write(size);
            stream.Seek(bodyEnd, SeekOrigin.Begin);
        }

        private List<Segment> GetSegments(HashSet<string> ignoredExtensions)
        {
            var entries = RawFileManager.GetAllEntries();
            var customMetadataExtensions = _customMetadataResolver.GetInstances().ToHashSet();
            var segments = new List<Segment>();

            foreach (var entry in entries)
            {
                if (entry.IsDirectory)
                    continue;

                if (customMetadataExtensions.Contains(FileExtension.FromPath(entry.Path)))
                    continue;

                var extension = Path.GetExtension(entry.Path);

                if (ignoredExtensions.Contains(extension))
                    continue;

                if (!_readerIdService.TryGetId(entry.Path, out var readerId))
                    continue;

                var id = _contentService.Resolve(entry.Path);
                var data = RawFileManager.Load(entry.Path);
                segments.Add(new Segment(id, readerId, data));
            }

            return segments;
        }
    }
}
#endif
