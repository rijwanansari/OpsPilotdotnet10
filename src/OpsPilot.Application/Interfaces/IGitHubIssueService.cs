using OpsPilot.Domain.Entities;

namespace OpsPilot.Application.Interfaces;

public interface IGitHubIssueService
{
    Task<string> CreateIssueAsync(Incident incident, CancellationToken cancellationToken = default);
}
