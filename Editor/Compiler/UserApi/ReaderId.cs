namespace Cms.Build
{
    public readonly struct ReaderId : System.IEquatable<ReaderId>
    {
        public readonly int Value;

        public ReaderId(int value)
        {
            Value = value;
        }

        public static ReaderId FromString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new System.ArgumentException("Plugin name must not be null or empty", nameof(value));

            unchecked
            {
                uint hash = 2166136261;

                foreach (var character in System.Text.Encoding.UTF8.GetBytes(value))
                    hash = (hash ^ character) * 16777619;

                return new ReaderId((int)hash);
            }
        }

        public bool Equals(ReaderId other)
        {
            return Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            return obj is ReaderId other && Equals(other);
        }

        public override int GetHashCode()
        {
            return Value.GetHashCode();
        }
    }
}
