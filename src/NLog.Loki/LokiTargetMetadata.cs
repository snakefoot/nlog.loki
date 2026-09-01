using NLog.Config;
using NLog.Layouts;

namespace NLog.Loki;

/// <summary>
/// Configuration item behind a &lt;metadata&gt; element on the Loki target.
/// </summary>
[NLogConfigurationItem]
public class LokiTargetMetadata
{
    [RequiredParameter]
    public string Name { get; set; }

    [RequiredParameter]
    public Layout Layout { get; set; }
}
