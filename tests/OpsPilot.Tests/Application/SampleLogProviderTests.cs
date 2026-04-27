using OpsPilot.Application.SampleData;
using OpsPilot.Domain.Entities;

namespace OpsPilot.Tests.Application;

public class SampleLogProviderTests
{
    [Fact]
    public void GetPaymentApiOutageLogs_Returns45Entries()
    {
        var logs = SampleLogProvider.GetPaymentApiOutageLogs();
        Assert.Equal(45, logs.Count);
    }

    [Fact]
    public void GetPaymentApiOutageLogs_ContainsAllExpectedServices()
    {
        var logs = SampleLogProvider.GetPaymentApiOutageLogs();
        var services = logs.Select(l => l.Source).Distinct().ToHashSet();

        Assert.Contains("PaymentGateway.API", services);
        Assert.Contains("PaymentGateway.DB", services);
        Assert.Contains("PaymentGateway.Cache", services);
        Assert.Contains("PaymentGateway.LoadBalancer", services);
    }

    [Fact]
    public void GetPaymentApiOutageLogs_ContainsCriticalEntries()
    {
        var logs = SampleLogProvider.GetPaymentApiOutageLogs();
        var criticalCount = logs.Count(l => l.Level == OpsPilot.Domain.Entities.LogLevel.Critical);

        Assert.True(criticalCount >= 5, $"Expected at least 5 Critical entries, got {criticalCount}");
    }

    [Fact]
    public void GetPaymentApiOutageLogs_ContainsErrorEntries()
    {
        var logs = SampleLogProvider.GetPaymentApiOutageLogs();
        var errorCount = logs.Count(l => l.Level == OpsPilot.Domain.Entities.LogLevel.Error);

        Assert.True(errorCount >= 10, $"Expected at least 10 Error entries, got {errorCount}");
    }

    [Fact]
    public void GetPaymentApiOutageLogs_AreOrderedByTimestamp()
    {
        var logs = SampleLogProvider.GetPaymentApiOutageLogs();

        for (int i = 1; i < logs.Count; i++)
        {
            Assert.True(logs[i].Timestamp >= logs[i - 1].Timestamp,
                $"Logs are not sorted: index {i - 1} ({logs[i - 1].Timestamp}) > index {i} ({logs[i].Timestamp})");
        }
    }

    [Fact]
    public void GetPaymentApiOutageLogs_ContainsDatabasePoolExhaustionMessage()
    {
        var logs = SampleLogProvider.GetPaymentApiOutageLogs();
        var hasPoolExhaustion = logs.Any(l => l.Message.Contains("pool exhausted", StringComparison.OrdinalIgnoreCase)
                                           || l.Message.Contains("Pool exhausted", StringComparison.OrdinalIgnoreCase));
        Assert.True(hasPoolExhaustion, "Expected a log entry about connection pool exhaustion");
    }

    [Fact]
    public void GetPaymentApiOutageLogs_ContainsCircuitBreakerMessage()
    {
        var logs = SampleLogProvider.GetPaymentApiOutageLogs();
        var hasCircuitBreaker = logs.Any(l => l.Message.Contains("Circuit breaker", StringComparison.OrdinalIgnoreCase));
        Assert.True(hasCircuitBreaker, "Expected a log entry about circuit breaker");
    }

    [Fact]
    public void GetPaymentApiOutageLogs_AllEntriesHaveRequiredFields()
    {
        var logs = SampleLogProvider.GetPaymentApiOutageLogs();

        foreach (var log in logs)
        {
            Assert.False(string.IsNullOrWhiteSpace(log.Source), "Log entry has empty Source");
            Assert.False(string.IsNullOrWhiteSpace(log.Message), "Log entry has empty Message");
        }
    }
}
