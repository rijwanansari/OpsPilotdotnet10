using OpsPilot.Domain.Entities;

namespace OpsPilot.Application.Interfaces;

public interface IIncidentOrchestrator
{
    Task<Incident> RunAsync(Incident incident, IReadOnlyList<LogEntry> logs, CancellationToken cancellationToken = default);
}
