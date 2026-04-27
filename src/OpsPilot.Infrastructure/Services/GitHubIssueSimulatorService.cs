using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using OpsPilot.Application.Interfaces;
using OpsPilot.Domain.Entities;

namespace OpsPilot.Infrastructure.Services;

public sealed class GitHubIssueSimulatorService(
    IConfiguration configuration,
    ILogger<GitHubIssueSimulatorService> logger) : IGitHubIssueService
{
    public async Task<string> CreateIssueAsync(Incident incident, CancellationToken cancellationToken = default)
    {
        await Task.Delay(300, cancellationToken);

        var owner = configuration["GitHub:Owner"] ?? "yourorg";
        var repo = configuration["GitHub:Repo"] ?? "payment-api";
        var issueNumber = new Random().Next(2800, 2900);

        logger.LogInformation("[SIMULATED] Creating GitHub issue for incident {IncidentId}", incident.Id);

        var issueUrl = $"https://github.com/{owner}/{repo}/issues/{issueNumber}";
        logger.LogInformation("[SIMULATED] Issue created: {Url}", issueUrl);

        return issueUrl;
    }
}
