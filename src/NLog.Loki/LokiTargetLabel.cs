using NLog.Config;
using NLog.Layouts;

namespace NLog.Loki;

[NLogConfigurationItem]
public class LokiTargetLabel
{
    public string Name { get; set; }
    public Layout Layout { get; set; }
}
