#if UNITY_EDITOR
using System.IO;
using System.Text;

namespace CMS.Editor
{
    internal static class SegmentUtility
    {
        public static byte[] Encode(Segment segment)
        {
            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream, Encoding.UTF8, leaveOpen: false);
            
            writer.Write(segment.ContentId.Id.ToByteArray());
            writer.Write(segment.ReaderId.Value);
            writer.Write(segment.DataLength);
            writer.Write(segment.Data);
            
            return stream.ToArray();
        }
    }
}
#endif
