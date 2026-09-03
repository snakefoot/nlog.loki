using System;

namespace NLog.Loki.Model;

internal readonly struct LokiMetadata
{
    public string Name { get; }

    public string Value { get; }

    public LokiMetadata(string name, string value)
    {
        Name = name ?? throw new ArgumentNullException(nameof(name));
        Value = value ?? throw new ArgumentNullException(nameof(value));
    }
}
