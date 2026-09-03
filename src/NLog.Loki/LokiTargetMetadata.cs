using NLog.Config;
using NLog.Layouts;

namespace NLog.Loki;

[NLogConfigurationItem]
public class LokiTargetMetadata
{
    [RequiredParameter]
    public string Name { get; set; }

    [RequiredParameter]
    public Layout Layout { get; set; }
}
