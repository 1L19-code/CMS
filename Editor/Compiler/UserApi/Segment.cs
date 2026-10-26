#if UNITY_EDITOR
using System;
using CMS;

namespace Cms.Build
{
    public readonly struct Segment : IEquatable<Segment>
    {
        public readonly ContentId ContentId;
        public readonly ReaderId ReaderId;
        public readonly byte[] Data;
        public readonly int DataLength;

        public Segment(ContentId contentId, ReaderId readerId, byte[] data)
        {
            ContentId = contentId;
            ReaderId = readerId;
            Data = data;
            DataLength = data.Length;
        }

        public bool Equals(Segment other)
        {
            return ContentId.Equals(other.ContentId) && ReaderId.Equals(other.ReaderId) && Equals(Data, other.Data) &&
                   DataLength == other.DataLength;
        }

        public override bool Equals(object obj)
        {
            return obj is Segment other && Equals(other);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(ContentId, ReaderId, Data, DataLength);
        }
    }
}
#endif
