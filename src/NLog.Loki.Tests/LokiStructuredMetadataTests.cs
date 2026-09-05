using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using NLog.Loki.Model;
using NUnit.Framework;

namespace NLog.Loki.Tests;

[TestFixture]
public class LokiStructuredMetadataTests
{
    public static IEnumerable<TestCaseData> Values()
    {
        yield return new TestCaseData("hello", "{\"value\":\"hello\"}");
        yield return new TestCaseData('z', "{\"value\":\"z\"}");

        yield return new TestCaseData(42, "{\"value\":42}");
        yield return new TestCaseData(42u, "{\"value\":42}");
        yield return new TestCaseData(42L, "{\"value\":42}");
        yield return new TestCaseData(42UL, "{\"value\":42}");

        yield return new TestCaseData(42.5f, "{\"value\":42.5}");
        yield return new TestCaseData(42.5d, "{\"value\":42.5}");
        yield return new TestCaseData(42.5m, "{\"value\":42.5}");
#if NET
        yield return new TestCaseData(42f, "{\"value\":42.0}");
        yield return new TestCaseData(42d, "{\"value\":42.0}");
        yield return new TestCaseData(42m, "{\"value\":42.0}");
#endif
        yield return new TestCaseData(double.NaN, "{\"value\":\"NaN\"}");
        yield return new TestCaseData(double.PositiveInfinity, "{\"value\":\"Infinity\"}");
        yield return new TestCaseData(double.NegativeInfinity, "{\"value\":\"-Infinity\"}");


        yield return new TestCaseData(true, "{\"value\":true}");
        yield return new TestCaseData(false, "{\"value\":false}");

        yield return new TestCaseData(
            Guid.Parse("12345678-1234-1234-1234-123456789abc"),
            "{\"value\":\"12345678-1234-1234-1234-123456789abc\"}");

        yield return new TestCaseData(
            new DateTime(2026, 9, 5, 12, 30, 45, DateTimeKind.Utc),
            "{\"value\":\"2026-09-05T12:30:45Z\"}");

        yield return new TestCaseData(
            new DateTimeOffset(2026, 9, 5, 12, 30, 45, TimeSpan.Zero),
            "{\"value\":\"2026-09-05T12:30:45+00:00\"}");

        yield return new TestCaseData(DateTimeKind.Utc, "{\"value\":\"Utc\"}");

        yield return new TestCaseData((Sum: 6, Count: 3), "{\"value\":\"(6, 3)\"}");
    }

    [TestCaseSource(nameof(Values))]
    public void Write_SupportedValues_ReturnsExpectedJson(object value, string expected)
    {
        Assert.That(WriteMetadata(new HashSet<LokiMetadata> { new LokiMetadata("value", value)}), Is.EqualTo(expected));
    }

    private static string WriteMetadata(HashSet<LokiMetadata> metadata)
    {
        using var stream = new MemoryStream();

        using(var writer = new Utf8JsonWriter(stream))
        {
            LokiStructuredMetadata.Write(writer, metadata);
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }
}
