using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using CMS.Editor;
#if UNITY_WEBGL
using System.Diagnostics;
#endif

namespace CMS
{
    internal class ContentIndex
    {
        private readonly Dictionary<ContentId, ReaderId> _readers = new();
        private readonly Dictionary<ContentId, ReadOnlyMemory<byte>> _segments = new();

        private ContentIndex()
        {
        }

        // Call on Unity's main thread: file readers may access Unity resources.
#if UNITY_WEBGL
        private const double FrameBudgetMilliseconds = 2;

        public static async ValueTask<ContentIndex> IndexAsync(IContentFileReader contentFileReader)
        {
            var body = contentFileReader.GetBody();
            var index = new ContentIndex();

            using var stream = new MemoryStream(body);
            using var reader = new BinaryReader(stream);
            var stopwatch = Stopwatch.StartNew();

            while (stream.Position < stream.Length)
            {
                index.IndexEntry(reader);

                if (stopwatch.Elapsed.TotalMilliseconds >= FrameBudgetMilliseconds && stream.Position < stream.Length)
                {
                    // UnitySynchronizationContext resumes this on the next Update.
                    await Task.Yield();
                    stopwatch.Restart();
                }
            }

            return index;
        }
#else
        public static async ValueTask<ContentIndex> IndexAsync(IContentFileReader contentFileReader)
        {
            var body = contentFileReader.GetBody();

            return await Task.Run(() =>
            {
                var index = new ContentIndex();
                using var stream = new MemoryStream(body);
                using var reader = new BinaryReader(stream);

                while (stream.Position < stream.Length)
                {
                    index.IndexEntry(reader);
                }

                return index;
            });
        }
#endif

        public ReaderId GetReader(ContentId contentId)
        {
            return _readers.TryGetValue(contentId, out var readerId)
                ? readerId
                : throw new Exception($"No reader found for content id {contentId}");
        }

        public ReadOnlyMemory<byte> GetSegment(ContentId contentId)
        {
            return _segments.TryGetValue(contentId, out var readerId)
                ? readerId
                : throw new Exception($"No segment found for content id {contentId}");
        }

        private void IndexEntry(BinaryReader reader)
        {
            var guid = new Guid(reader.ReadBytes(ContentFileConstants.GuidSize));
            var readerId = reader.ReadInt32();
            var dataLength = reader.ReadInt32();
            var data = reader.ReadBytes(dataLength);

            var contentId = ContentId.FromGuid(guid);
            _readers[contentId] = new ReaderId(readerId);
            _segments.Add(contentId, data);
        }
    }
}
