using System;
using System.Collections.Generic;

namespace CMS
{
    public readonly struct ContentId : IEquatable<ContentId>
    {
        public readonly Guid Id;

        private ContentId(Guid id)
        {
            Id = id;
        }

        public static ContentId FromGuid(Guid guid)
        {
            return new ContentId(guid);
        }

        public static ContentId FromGuid(string guid)
        {
            var id = Guid.Parse(guid);

            return new ContentId(id);
        }

        public static ContentId Generate()
        {
            return FromGuid(Guid.NewGuid());
        }

        public bool Equals(ContentId other)
        {
            return Id.Equals(other.Id);
        }

        public override bool Equals(object obj)
        {
            return obj is ContentId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public static IEqualityComparer<ContentId> IdComparer { get; } = new ContentIdEqualityComparer();
    }

    internal sealed class ContentIdEqualityComparer : IEqualityComparer<ContentId>
    {
        public bool Equals(ContentId x, ContentId y)
        {
            return x.Id.Equals(y.Id);
        }

        public int GetHashCode(ContentId obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}