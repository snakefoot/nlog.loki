using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using NLog.Loki.Model;

namespace NLog.Loki;

internal class LokiEventSerializer : JsonConverter<LokiEvent>
{
    public override LokiEvent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        throw new NotSupportedException("This converter only supports serializing to JSON.");
    }

    public override void Write(Utf8JsonWriter writer, LokiEvent value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteStartArray("streams");

        writer.WriteStartObject();

        writer.WriteStartObject("stream");
        foreach(var label in value.Labels.Labels)
        {
            writer.WritePropertyName(label.Label);
            writer.WriteStringValue(label.Value);
        }
        writer.WriteEndObject();

        writer.WriteStartArray("values");
        writer.WriteStartArray();

        UnixDateTimeConverter.WriteAsUnixTimeNs(writer, value.Timestamp);
        writer.WriteStringValue(value.Line);
        LokiStructuredMetadata.Write(writer, value.Metadata);

        writer.WriteEndArray();
        writer.WriteEndArray();

        writer.WriteEndObject();

        writer.WriteEndArray();
        writer.WriteEndObject();
    }
}
