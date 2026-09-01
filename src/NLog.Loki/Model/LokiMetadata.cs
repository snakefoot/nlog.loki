using System;

namespace NLog.Loki.Model;

/// <summary>
/// A single structured metadata entry attached to a log entry.
/// </summary>
/// <remarks>
/// Intentionally separate from <see cref="LokiLabel"/> and without equality members: labels are
/// hashed into a <see cref="System.Collections.Generic.HashSet{T}"/> because they define stream
/// identity, whereas metadata varies per event and is never grouped on.
/// </remarks>
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
