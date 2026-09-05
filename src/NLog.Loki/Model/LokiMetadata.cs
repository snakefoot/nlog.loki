using System;

namespace NLog.Loki.Model;

internal readonly struct LokiMetadata : IEquatable<LokiMetadata>
{
    public string Name { get; }

    public object Value { get; }

    public LokiMetadata(string name, object value)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }

    public bool Equals(LokiMetadata other)
    {
        return string.Equals(Name, other.Name, StringComparison.Ordinal);
    }

    public override bool Equals(object obj)
    {
        return obj is LokiMetadata other && Equals(other);
    }

    public override int GetHashCode()
    {
        return Name?.GetHashCode() ?? 0;
    }
}
