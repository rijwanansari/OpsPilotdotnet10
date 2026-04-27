using OpsPilot.Domain.Entities;

namespace OpsPilot.Application.Interfaces;

public interface ITriageAgent
{
    Task<AgentResult> AnalyzeAsync(Incident incident, IReadOnlyList<LogEntry> logs, CancellationToken cancellationToken = default);
}
