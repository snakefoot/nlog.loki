using System;
using NLog.Config;
using NLog.Layouts;
using NUnit.Framework;

namespace NLog.Loki.Tests;

[TestFixture]
public class LokiTargetTests
{
    [TestCase(true)]
    [TestCase(false)]
    public void Write(bool includeDynamicContext)
    {
        using var logFactory = new LogFactory();

        var configuration = new LoggingConfiguration(logFactory);

        using var lokiTarget = new LokiTarget
        {
            Endpoint = "http://grafana.lvh.me:3100",
            IncludeEventProperties = includeDynamicContext,
            Labels = {
                new LokiTargetLabel {
                    Name = "env",
                    Layout = Layout.FromString("${basedir}")
                },
                new LokiTargetLabel {
                    Name = "server",
                    Layout = Layout.FromString("${machinename:lowercase=true}")
                },
            }
        };
        if (includeDynamicContext)
        {
            lokiTarget.Labels.Add(new LokiTargetLabel
            {
                Name = "name",
                Layout = Layout.FromString("${level:lowercase=true}")
            });
            lokiTarget.Metadata.Add(new Targets.TargetPropertyWithContext
            {
                Name = "threadid",
                Layout = Layout.FromString("${threadid}")
            });
        }

        configuration.AddTarget("loki", lokiTarget);

        var rule = new LoggingRule("*", LogLevel.Debug, lokiTarget);
        configuration.LoggingRules.Add(rule);

        logFactory.Configuration = configuration;

        var log = logFactory.GetLogger(typeof(LokiTargetTests).FullName);

        for(var n = 0; n < 100; ++n)
        {
            log.Info("Hello world {WorldCounter}", n);

            try
            {
                throw new InvalidOperationException();
            }
            catch(Exception e)
            {
                log.Error(e);
            }
        }

        logFactory.Shutdown();
    }

    [Test]
    [TestCase("${environment:SCHEME}://${environment:HOST}:3100/", ExpectedResult = typeof(HttpLokiTransport))]
    [TestCase("udp://${environment:HOST}:3100/", ExpectedResult = typeof(NullLokiTransport))]
    [TestCase("", ExpectedResult = typeof(NullLokiTransport))]
    [TestCase(null, ExpectedResult = typeof(NullLokiTransport))]
    public Type GetLokiTransport(string endpointLayout)
    {
        Environment.SetEnvironmentVariable("SCHEME", "https");
        Environment.SetEnvironmentVariable("HOST", "loki.lvh.me");

        var endpoint = Layout.FromString(endpointLayout);
        using var target = new LokiTarget();
        target.Endpoint = endpoint;
        target.ProxyUrl = "https://myproxy.com";
        target.ProxyUser = "proxyDomain\\proxyUserA";
        target.ProxyPassword = "proxyPasswordA";
        using var lokiTargetTransport = target.GetLokiTransport();
        return lokiTargetTransport.GetType();
    }
}

