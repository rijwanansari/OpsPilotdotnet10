using Microsoft.Extensions.Logging.Abstractions;
using OpsPilot.Application.Interfaces;
using OpsPilot.Application.Orchestration;
using OpsPilot.Domain.Entities;
using DomainLogLevel = OpsPilot.Domain.Entities.LogLevel;

namespace OpsPilot.Tests.Application;

/// <summary>
/// Tests for the IncidentOrchestrator, using simple stub implementations of the agent interfaces.
/// </summary>
public class IncidentOrchestratorTests
{
    private static Incident CreateTestIncident() => new()
    {
        Title = "Payment API Outage",
        Description = "Payment service is down"
    };

    private static IReadOnlyList<LogEntry> CreateTestLogs() =>
    [
        new() { Timestamp = DateTime.UtcNow, Level = DomainLogLevel.Error, Source = "API", Message = "Failure" }
    ];

    [Fact]
    public async Task RunAsync_AllAgentsSucceed_IncidentEndsAsResolved()
    {
        var orchestrator = new IncidentOrchestrator(
            new StubTriageAgent(),
            new StubRootCauseAgent(),
            new StubFixAgent(),
            new StubGitHubIssueService(),
            NullLogger<IncidentOrchestrator>.Instance);

        var incident = CreateTestIncident();
        var result = await orchestrator.RunAsync(incident, CreateTestLogs());

        Assert.Equal(IncidentStatus.Resolved, result.Status);
    }

    [Fact]
    public async Task RunAsync_TriageResultIsPopulated()
    {
        var orchestrator = new IncidentOrchestrator(
            new StubTriageAgent(),
            new StubRootCauseAgent(),
            new StubFixAgent(),
            new StubGitHubIssueService(),
            NullLogger<IncidentOrchestrator>.Instance);

        var incident = CreateTestIncident();
        var result = await orchestrator.RunAsync(incident, CreateTestLogs());

        Assert.NotNull(result.TriageResult);
        Assert.Equal("StubTriageAgent", result.TriageResult.AgentName);
    }

    [Fact]
    public async Task RunAsync_RootCauseResultIsPopulated()
    {
        var orchestrator = new IncidentOrchestrator(
            new StubTriageAgent(),
            new StubRootCauseAgent(),
            new StubFixAgent(),
            new StubGitHubIssueService(),
            NullLogger<IncidentOrchestrator>.Instance);

        var incident = CreateTestIncident();
        var result = await orchestrator.RunAsync(incident, CreateTestLogs());

        Assert.NotNull(result.RootCauseResult);
        Assert.Equal("StubRootCauseAgent", result.RootCauseResult.AgentName);
    }

    [Fact]
    public async Task RunAsync_RemediationPlanIsPopulated()
    {
        var orchestrator = new IncidentOrchestrator(
            new StubTriageAgent(),
            new StubRootCauseAgent(),
            new StubFixAgent(),
            new StubGitHubIssueService(),
            NullLogger<IncidentOrchestrator>.Instance);

        var incident = CreateTestIncident();
        var result = await orchestrator.RunAsync(incident, CreateTestLogs());

        Assert.NotEmpty(result.RemediationPlan);
        Assert.Equal("Fix DB", result.RemediationPlan[0].Title);
    }

    [Fact]
    public async Task RunAsync_UpdatedAtIsModifiedDuringPipeline()
    {
        var orchestrator = new IncidentOrchestrator(
            new StubTriageAgent(),
            new StubRootCauseAgent(),
            new StubFixAgent(),
            new StubGitHubIssueService(),
            NullLogger<IncidentOrchestrator>.Instance);

        var incident = CreateTestIncident();
        var createdAt = incident.CreatedAt;

        await orchestrator.RunAsync(incident, CreateTestLogs());

        Assert.True(incident.UpdatedAt >= createdAt);
    }

    // ── Stubs ─────────────────────────────────────────────────────────────────

    private sealed class StubTriageAgent : ITriageAgent
    {
        public Task<AgentResult> AnalyzeAsync(Incident incident, IReadOnlyList<LogEntry> logs, CancellationToken cancellationToken = default)
            => Task.FromResult(new AgentResult { AgentName = "StubTriageAgent", Analysis = "Triage complete", Confidence = 1.0 });
    }

    private sealed class StubRootCauseAgent : IRootCauseAgent
    {
        public Task<AgentResult> AnalyzeAsync(Incident incident, AgentResult triageResult, IReadOnlyList<LogEntry> logs, CancellationToken cancellationToken = default)
            => Task.FromResult(new AgentResult { AgentName = "StubRootCauseAgent", Analysis = "DB connection pool", Confidence = 0.95 });
    }

    private sealed class StubFixAgent : IFixAgent
    {
        public Task<List<RemediationStep>> ProposeFixAsync(Incident incident, AgentResult rootCauseResult, CancellationToken cancellationToken = default)
            => Task.FromResult(new List<RemediationStep>
            {
                new() { Order = 1, Title = "Fix DB", Description = "Kill long-running queries", Priority = Priority.Critical, EstimatedTimeMinutes = 5 }
            });
    }

    private sealed class StubGitHubIssueService : IGitHubIssueService
    {
        public Task<string> CreateIssueAsync(Incident incident, CancellationToken cancellationToken = default)
            => Task.FromResult("https://github.com/org/repo/issues/1");
    }
}
