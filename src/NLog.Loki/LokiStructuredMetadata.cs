using System.Collections.Generic;
using System.Text.Json;
using NLog.Loki.Model;

namespace NLog.Loki;

internal static class LokiStructuredMetadata
{
    /// <summary>
    /// Writes the optional third element of a Loki push entry: [ts, line, {metadata}].
    /// Omitted entirely when there is no metadata.
    /// </summary>
    /// <remarks>See https://grafana.com/docs/loki/latest/reference/loki-http-api/#ingest-logs</remarks>
    public static void Write(Utf8JsonWriter writer, HashSet<LokiMetadata> metadata)
    {
        if(metadata == null || metadata.Count == 0)
            return;

        writer.WriteStartObject();
        foreach (var keyValue in metadata)
        {
            writer.WritePropertyName(keyValue.Name);
            writer.WriteStringValue(keyValue.Value);
        }
        writer.WriteEndObject();
    }
}
