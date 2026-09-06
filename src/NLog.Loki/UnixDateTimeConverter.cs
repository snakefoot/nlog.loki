using System;
using System.Globalization;
using System.Text.Json;

namespace NLog.Loki;

internal static class UnixDateTimeConverter
{
    private static readonly DateTime UnixEpoch = new(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public static long ToUnixTimeNs(DateTime dateTime) => (dateTime.ToUniversalTime() - UnixEpoch).Ticks * 100;

    public static void WriteAsUnixTimeNs(Utf8JsonWriter writer, DateTime datetime)
    {
        var timestamp = ToUnixTimeNs(datetime);
#if NET || NETSTANDARD2_1_OR_GREATER
        Span<char> buffer = stackalloc char[32];
        if (timestamp.TryFormat(buffer, out var charsWritten, "g", CultureInfo.InvariantCulture))
            writer.WriteStringValue(buffer[..charsWritten]);
        else
            writer.WriteStringValue(timestamp.ToString("g", CultureInfo.InvariantCulture));
#else
        writer.WriteStringValue(timestamp.ToString("g", CultureInfo.InvariantCulture));
#endif
    }
}
