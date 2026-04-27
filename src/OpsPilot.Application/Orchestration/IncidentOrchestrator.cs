using Microsoft.Extensions.Logging;
using OpsPilot.Application.Interfaces;
using OpsPilot.Domain.Entities;

namespace OpsPilot.Application.Orchestration;

public sealed class IncidentOrchestrator(
    ITriageAgent triageAgent,
    IRootCauseAgent rootCauseAgent,
    IFixAgent fixAgent,
    IGitHubIssueService gitHubIssueService,
    ILogger<IncidentOrchestrator> logger) : IIncidentOrchestrator
{
    public async Task<Incident> RunAsync(Incident incident, IReadOnlyList<LogEntry> logs, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting incident response pipeline for: {Title}", incident.Title);

        incident.Status = IncidentStatus.Triaging;
        incident.UpdatedAt = DateTime.UtcNow;
        incident.TriageResult = await triageAgent.AnalyzeAsync(incident, logs, cancellationToken);

        incident.Status = IncidentStatus.RootCauseAnalysis;
        incident.UpdatedAt = DateTime.UtcNow;
        incident.RootCauseResult = await rootCauseAgent.AnalyzeAsync(incident, incident.TriageResult, logs, cancellationToken);

        incident.Status = IncidentStatus.RemediationProposed;
        incident.UpdatedAt = DateTime.UtcNow;
        incident.RemediationPlan = await fixAgent.ProposeFixAsync(incident, incident.RootCauseResult, cancellationToken);

        var issueUrl = await gitHubIssueService.CreateIssueAsync(incident, cancellationToken);
        logger.LogInformation("GitHub issue created: {Url}", issueUrl);

        incident.Status = IncidentStatus.Resolved;
        incident.UpdatedAt = DateTime.UtcNow;

        return incident;
    }
}
