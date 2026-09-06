using System;
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
            WriteMetaDataValue(writer, keyValue.Name, keyValue.Value);
        }
        writer.WriteEndObject();
    }

    private static void WriteMetaDataValue(Utf8JsonWriter writer, string key, object value)
    {
        switch(value)
        {
            case null:
                writer.WritePropertyName(key);
                writer.WriteStringValue("null");  // Loki metadata does not like Json null / nullable values
                break;
            case string v:
                writer.WritePropertyName(key);
                writer.WriteStringValue(v);
                break;
            case int v:
                writer.WritePropertyName(key);
                writer.WriteNumberValue(v);
                break;
            case uint v:
                writer.WritePropertyName(key);
                writer.WriteNumberValue(v);
                break;
            case long v:
                writer.WritePropertyName(key);
                writer.WriteNumberValue(v);
                break;
            case ulong v:
                writer.WritePropertyName(key);
                writer.WriteNumberValue(v);
                break;
            case float v:
            {
                writer.WritePropertyName(key);
#if NET
                Span<byte> buffer = stackalloc byte[32];
                writer.WriteRawValue(TryFormatAndEnsureDecimal(v, buffer), skipInputValidation: true);
#else
                writer.WriteNumberValue(v);
#endif
            }
            break;
            case double v:
            {
                writer.WritePropertyName(key);
#if NET
                Span<byte> buffer = stackalloc byte[32];
                writer.WriteRawValue(TryFormatAndEnsureDecimal(v, buffer), skipInputValidation: true);
#else
                writer.WriteNumberValue(v);
#endif
            }
            break;
            case decimal v:
            {
                writer.WritePropertyName(key);
#if NET
                Span<byte> buffer = stackalloc byte[32];
                writer.WriteRawValue(TryFormatAndEnsureDecimal(v, buffer), skipInputValidation: true);
#else
                writer.WriteNumberValue(v);
#endif
            }
            break;
            case bool v:
                writer.WritePropertyName(key);
                writer.WriteBooleanValue(v);
                break;
            case Guid v:
                writer.WritePropertyName(key);
                writer.WriteStringValue(v);
                break;
            case DateTime v:
                writer.WritePropertyName(key);
                writer.WriteStringValue(v);
                break;
            case DateTimeOffset v:
                writer.WritePropertyName(key);
                writer.WriteStringValue(v);
                break;
            case Enum v:
                writer.WritePropertyName(key);
                writer.WriteStringValue(v.ToString());
                break;
            case IFormattable v:
                writer.WritePropertyName(key);
                writer.WriteStringValue(v.ToString(null, System.Globalization.CultureInfo.InvariantCulture));
                break;
            case System.Collections.IList v:
                writer.WritePropertyName(key);
                writer.WriteStringValue($"Count={v.Count}");
                break;
            case System.Collections.IDictionary v:
                writer.WritePropertyName(key);
                writer.WriteStringValue($"Count={v.Count}");
                break;
            default:
            {
                try
                {
                    var propertyValue = value.ToString() ?? string.Empty;
                    writer.WritePropertyName(key);
                    writer.WriteStringValue(propertyValue);
                }
                catch
                {
                    writer.WritePropertyName(key);
                    writer.WriteStringValue(string.Empty);
                }
                break;
            }
        }
    }

#if NET
    private static ReadOnlySpan<byte> TryFormatAndEnsureDecimal<TDouble>(TDouble value, Span<byte> buffer) where TDouble : IUtf8SpanFormattable
    {
        if(!value.TryFormat(buffer, out var written, default, System.Globalization.CultureInfo.InvariantCulture))
            throw new InvalidOperationException("Buffer too small.");

        var span = buffer[..written];
        if(span.IndexOfAny((byte)'.', (byte)'e', (byte)'E') >= 0)
            return span;

        var firstChar = span[0];
        if(firstChar == (byte)'N' || firstChar == (byte)'I' || (firstChar == (byte)'-' && span[1] == (byte)'I'))
        {
            // NaN or Infinity, wrap in quotes to make it valid JSON
            span.CopyTo(buffer[1..]);   // Move 1 forward
            buffer[0] = (byte)'"';
            buffer[written + 1] = (byte)'"';
            written += 2;
        }
        else
        {
            // Apply a decimal point
            buffer[written++] = (byte)'.';
            buffer[written++] = (byte)'0';
        }
        return buffer[..written];
    }

    private static ReadOnlySpan<byte> TryFormatAndEnsureDecimal(decimal value, Span<byte> buffer)
    {
        if(!value.TryFormat(buffer, out var written, default, System.Globalization.CultureInfo.InvariantCulture))
            throw new InvalidOperationException("Buffer too small.");

        var span = buffer[..written];

        if(span.IndexOf((byte)'.') < 0)
        {
            buffer[written++] = (byte)'.';
            buffer[written++] = (byte)'0';
        }

        return buffer[..written];
    }
#endif
}
