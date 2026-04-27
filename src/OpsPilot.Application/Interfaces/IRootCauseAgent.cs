using OpsPilot.Domain.Entities;

namespace OpsPilot.Application.Interfaces;

public interface IRootCauseAgent
{
    Task<AgentResult> AnalyzeAsync(Incident incident, AgentResult triageResult, IReadOnlyList<LogEntry> logs, CancellationToken cancellationToken = default);
}
