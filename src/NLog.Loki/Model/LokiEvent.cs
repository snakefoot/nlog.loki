using System;
using System.Collections.Generic;

namespace NLog.Loki.Model;

internal class LokiEvent
{
    public LokiLabels Labels { get; }

    public DateTime Timestamp { get; }

    public string Line { get; }

    public IReadOnlyList<LokiMetadata> Metadata { get; }

    public LokiEvent(LokiLabels labels, DateTime timestamp, string line, IReadOnlyList<LokiMetadata> metadata = null)
    {
        Labels = labels ?? throw new ArgumentNullException(nameof(labels));
        Timestamp = timestamp;
        Line = line ?? throw new ArgumentNullException(nameof(line));
        Metadata = metadata;
    }
}
