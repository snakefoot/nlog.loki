using System;

namespace NLog.Loki.Model
{
    public class LokiLabel : IEquatable<LokiLabel>
    {
        public string Label { get; }

        public string Value { get; }

        public LokiLabel(string label, string value)
        {
            Label = label ?? throw new ArgumentNullException(nameof(label));
            Value = value ?? throw new ArgumentNullException(nameof(value));
        }

        public bool Equals(LokiLabel other)
        {
            if(ReferenceEquals(null, other)) return false;
            if(ReferenceEquals(this, other)) return true;
            return Label == other.Label && Value == other.Value;
        }

        public override bool Equals(object obj)
        {
            if(ReferenceEquals(null, obj)) return false;
            if(ReferenceEquals(this, obj)) return true;
            if(obj.GetType() != this.GetType()) return false;
            return Equals((LokiLabel)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Label != null ? Label.GetHashCode() : 0) * 397) ^ (Value != null ? Value.GetHashCode() : 0);
            }
        }

        public static bool operator ==(LokiLabel left, LokiLabel right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(LokiLabel left, LokiLabel right)
        {
            return !Equals(left, right);
        }
    }
}
