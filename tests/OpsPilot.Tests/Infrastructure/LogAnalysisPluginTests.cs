using System.Text.Json;
using Microsoft.SemanticKernel;
using OpsPilot.Infrastructure.Plugins;

namespace OpsPilot.Tests.Infrastructure;

public class LogAnalysisPluginTests
{
    private readonly LogAnalysisPlugin _plugin = new();

    private static string BuildLogsJson(IEnumerable<(string Level, string Source, string Message, DateTime Timestamp)> entries)
    {
        return JsonSerializer.Serialize(entries.Select(e => new
        {
            Level = e.Level,
            Source = e.Source,
            Message = e.Message,
            Timestamp = e.Timestamp
        }));
    }

    [Fact]
    public void CountErrorsByService_ReturnsCorrectCounts()
    {
        var now = DateTime.UtcNow;
        var logsJson = BuildLogsJson([
            ("Error", "ServiceA", "Msg1", now),
            ("Error", "ServiceA", "Msg2", now),
            ("Critical", "ServiceB", "Msg3", now),
            ("Info", "ServiceA", "Msg4", now),
            ("Warn", "ServiceB", "Msg5", now),
        ]);

        var result = _plugin.CountErrorsByService(logsJson);
        var counts = JsonSerializer.Deserialize<Dictionary<string, int>>(result)!;

        Assert.Equal(2, counts["ServiceA"]);
        Assert.Equal(1, counts["ServiceB"]);
        Assert.DoesNotContain("ServiceC", counts.Keys);
    }

    [Fact]
    public void CountErrorsByService_NoErrors_ReturnsEmptyJson()
    {
        var logsJson = BuildLogsJson([
            ("Info", "ServiceA", "All good", DateTime.UtcNow),
            ("Warn", "ServiceA", "Minor warning", DateTime.UtcNow),
        ]);

        var result = _plugin.CountErrorsByService(logsJson);
        var counts = JsonSerializer.Deserialize<Dictionary<string, int>>(result)!;

        Assert.Empty(counts);
    }

    [Fact]
    public void FindTimeOfFirstError_ReturnsEarliestErrorTimestamp()
    {
        var now = DateTime.UtcNow;
        var earlier = now.AddMinutes(-10);
        var logsJson = BuildLogsJson([
            ("Info", "Svc", "Normal", now),
            ("Error", "Svc", "Later error", now),
            ("Error", "Svc", "Earlier error", earlier),
        ]);

        var result = _plugin.FindTimeOfFirstError(logsJson);

        Assert.Contains(earlier.ToString("O"), result);
        Assert.Contains("Earlier error", result);
    }

    [Fact]
    public void FindTimeOfFirstError_NoErrors_ReturnsNoErrorsFound()
    {
        var logsJson = BuildLogsJson([
            ("Info", "Svc", "All OK", DateTime.UtcNow),
        ]);

        var result = _plugin.FindTimeOfFirstError(logsJson);

        Assert.Equal("No errors found", result);
    }

    [Fact]
    public void GetHighestErrorRate_ReturnsServiceWithMostErrors()
    {
        var now = DateTime.UtcNow;
        var logsJson = BuildLogsJson([
            ("Error", "ServiceA", "E1", now),
            ("Error", "ServiceA", "E2", now),
            ("Info",  "ServiceA", "I1", now),
            ("Info",  "ServiceA", "I2", now),   // ServiceA: 2/4 = 50%
            ("Error", "ServiceB", "E3", now),
            ("Info",  "ServiceB", "I3", now),   // ServiceB: 1/2 = 50%
            ("Error", "ServiceC", "E4", now),   // ServiceC: 1/1 = 100%
        ]);

        var result = _plugin.GetHighestErrorRate(logsJson);

        Assert.Contains("ServiceC", result);
        Assert.Contains("100.0%", result);
    }

    [Fact]
    public void GetHighestErrorRate_EmptyLogs_ReturnsNoLogsProvided()
    {
        var result = _plugin.GetHighestErrorRate("[]");
        Assert.Equal("No logs provided", result);
    }

    [Fact]
    public void GetHighestErrorRate_NoErrors_ReturnsNoErrorsFound()
    {
        var logsJson = BuildLogsJson([
            ("Info", "Svc", "Fine", DateTime.UtcNow),
        ]);

        var result = _plugin.GetHighestErrorRate(logsJson);
        Assert.Equal("No errors found", result);
    }

    [Fact]
    public void CountErrorsByService_CriticalCountedAsError()
    {
        var logsJson = BuildLogsJson([
            ("Critical", "PaymentGateway.DB", "Pool exhausted", DateTime.UtcNow),
            ("Critical", "PaymentGateway.DB", "Out of memory", DateTime.UtcNow),
        ]);

        var result = _plugin.CountErrorsByService(logsJson);
        var counts = JsonSerializer.Deserialize<Dictionary<string, int>>(result)!;

        Assert.Equal(2, counts["PaymentGateway.DB"]);
    }
}
