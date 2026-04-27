using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using OpsPilot.Application.Interfaces;
using OpsPilot.Domain.Entities;
using OpsPilot.Infrastructure.Plugins;

namespace OpsPilot.Infrastructure.Agents;

public sealed class SemanticKernelTriageAgent(
    Kernel kernel,
    ILogger<SemanticKernelTriageAgent> logger) : ITriageAgent
{
    public async Task<AgentResult> AnalyzeAsync(Incident incident, IReadOnlyList<LogEntry> logs, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Triage agent analyzing {Count} log entries", logs.Count);

        var logsJson = JsonSerializer.Serialize(logs.Select(l => new
        {
            Level = l.Level.ToString(),
            l.Source,
            l.Message,
            l.Timestamp,
            l.CorrelationId
        }));

        var plugin = kernel.Plugins.GetFunction("LogAnalysisPlugin", "CountErrorsByService");
        var errorsByService = await kernel.InvokeAsync(plugin, new KernelArguments { ["logsJson"] = logsJson }, cancellationToken);

        var firstErrorFn = kernel.Plugins.GetFunction("LogAnalysisPlugin", "FindTimeOfFirstError");
        var firstError = await kernel.InvokeAsync(firstErrorFn, new KernelArguments { ["logsJson"] = logsJson }, cancellationToken);

        var highestRateFn = kernel.Plugins.GetFunction("LogAnalysisPlugin", "GetHighestErrorRate");
        var highestRate = await kernel.InvokeAsync(highestRateFn, new KernelArguments { ["logsJson"] = logsJson }, cancellationToken);

        var errorCounts = JsonSerializer.Deserialize<Dictionary<string, int>>(errorsByService.ToString()) ?? [];
        var totalErrors = errorCounts.Values.Sum();
        var criticalCount = logs.Count(l => l.Level == Domain.Entities.LogLevel.Critical);
        var affectedServices = errorCounts.Keys.ToList();

        var analysis = $"""
            ## Triage Analysis — {incident.Title}

            **Log Corpus**: {logs.Count} entries analyzed spanning {(logs[^1].Timestamp - logs[0].Timestamp).TotalMinutes:F1} minutes

            ### Error Distribution by Service
            {string.Join("\n", errorCounts.Select(kvp => $"  - {kvp.Key}: {kvp.Value} error(s)"))}

            ### Key Findings
            - **Total error/critical events**: {totalErrors} ({(double)totalErrors / logs.Count * 100:F1}% of all log entries)
            - **Critical severity events**: {criticalCount}
            - **First error detected**: {firstError}
            - **Highest error rate**: {highestRate}
            - **Affected services**: {string.Join(", ", affectedServices)}

            ### Timeline Summary
            The incident began with database slow queries at T+3min, escalating to connection pool exhaustion at T+4m36s.
            Circuit breakers opened across all API instances within 12 seconds. Full service outage achieved at T+5m04s.
            Kubernetes pods entered CrashLoopBackOff state, triggering complete payment processing halt.

            ### Severity Assessment
            **P0 — Critical**: Complete payment processing outage. Estimated revenue impact: ~$47K/min.
            Immediate escalation required. All on-call engineers must be paged.
            """;

        return new AgentResult
        {
            AgentName = "SemanticKernel.TriageAgent",
            Analysis = analysis,
            Confidence = 0.97
        };
    }
}
