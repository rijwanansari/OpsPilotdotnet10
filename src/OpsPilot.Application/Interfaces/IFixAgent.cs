using OpsPilot.Domain.Entities;

namespace OpsPilot.Application.Interfaces;

public interface IFixAgent
{
    Task<List<RemediationStep>> ProposeFixAsync(Incident incident, AgentResult rootCauseResult, CancellationToken cancellationToken = default);
}
