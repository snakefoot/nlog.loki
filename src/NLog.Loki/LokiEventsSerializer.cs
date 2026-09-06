using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using NLog.Loki.Model;

namespace NLog.Loki;

/// <summary>
/// Serializes log events to json for loki before sending them to the push HTTP API.
/// </summary>
/// <remarks>
/// See https://grafana.com/docs/loki/latest/api/#post-lokiapiv1push
/// </remarks>
internal class LokiEventsSerializer : JsonConverter<IEnumerable<LokiEvent>>
{
    private readonly bool _orderWrites;

    public LokiEventsSerializer(bool orderWrites)
    {
        this._orderWrites = orderWrites;
    }

    public override IEnumerable<LokiEvent> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException("This converter only supports serializing to JSON.");
    }

    public override void Write(Utf8JsonWriter writer, IEnumerable<LokiEvent> value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteStartArray("streams");

        LokiLabels firstLabels = null;
        Dictionary<LokiLabels, List<LokiEvent>> streams = null;

        foreach(var logEvent in value)
        {
            // Order logs by timestamp only if the option is opted-in, because it costs
            // approximately 20% more allocation when serializing 100 events.
            if(!_orderWrites)
            {
                if(firstLabels is null)
                {
                    firstLabels = logEvent.Labels;
                    WriteStreamStart(writer, firstLabels);
                    WriteLogEvent(writer, logEvent);
                    continue;
                }

                if(firstLabels.Equals(logEvent.Labels))
                {
                    // We can continue writing directly into the first stream.
                    WriteLogEvent(writer, logEvent);
                    continue;
                }
            }

            streams ??= new Dictionary<LokiLabels, List<LokiEvent>>();
            if(!streams.TryGetValue(logEvent.Labels, out var bucket))
            {
                bucket = new List<LokiEvent>();
                streams.Add(logEvent.Labels, bucket);
            }
            bucket.Add(logEvent);
        }

        if(firstLabels != null)
        {
            WriteStreamEnd(writer);
        }

        if(streams is not null)
        {
            foreach(var stream in streams)
            {
                WriteLabelStream(writer, stream.Key, stream.Value);
            }
        }

        writer.WriteEndArray();
        writer.WriteEndObject();
    }

    private void WriteLabelStream(Utf8JsonWriter writer, LokiLabels labels, List<LokiEvent> events)
    {
        WriteStreamStart(writer, labels);

        if (_orderWrites)
            events.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));

        foreach(var @event in events)
        {
            WriteLogEvent(writer, @event);
        }

        WriteStreamEnd(writer);
    }

    private static void WriteStreamStart(Utf8JsonWriter writer, LokiLabels labels)
    {
        writer.WriteStartObject();

        writer.WriteStartObject("stream");

        foreach(var label in labels.Labels)
        {
            writer.WritePropertyName(label.Label);
            writer.WriteStringValue(label.Value);
        }

        writer.WriteEndObject();

        writer.WriteStartArray("values");
    }

    private static void WriteLogEvent(Utf8JsonWriter writer, LokiEvent logEvent)
    {
        writer.WriteStartArray();

        var timestamp = UnixDateTimeConverter.ToUnixTimeNs(logEvent.Timestamp);
#if NET || NETSTANDARD2_1_OR_GREATER
        Span<char> buffer = stackalloc char[32];
        if (timestamp.TryFormat(buffer, out var charsWritten, "g", CultureInfo.InvariantCulture))
            writer.WriteStringValue(buffer[..charsWritten]);
        else
            writer.WriteStringValue(timestamp.ToString("g", CultureInfo.InvariantCulture));
#else
        writer.WriteStringValue(timestamp.ToString("g", CultureInfo.InvariantCulture));
#endif
        writer.WriteStringValue(logEvent.Line);

        LokiStructuredMetadata.Write(writer, logEvent.Metadata);

        writer.WriteEndArray();
    }

    private static void WriteStreamEnd(Utf8JsonWriter writer)
    {
        writer.WriteEndArray();  // values
        writer.WriteEndObject(); // stream object
    }
}
