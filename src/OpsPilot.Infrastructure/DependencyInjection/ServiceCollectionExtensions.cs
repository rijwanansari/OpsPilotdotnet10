using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using OpsPilot.Application.Interfaces;
using OpsPilot.Application.Orchestration;
using OpsPilot.Infrastructure.Agents;
using OpsPilot.Infrastructure.Plugins;
using OpsPilot.Infrastructure.Services;

namespace OpsPilot.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddOpsPilotInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var kernelBuilder = Kernel.CreateBuilder();
        kernelBuilder.Services.AddLogging(lb => lb.AddConsole().SetMinimumLevel(LogLevel.Warning));

        var useSimulation = configuration.GetValue<bool>("AzureAI:UseSimulation", true);

        if (!useSimulation)
        {
            var endpoint = configuration["AzureAI:Endpoint"]!;
            var apiKey = configuration["AzureAI:ApiKey"]!;
            var deployment = configuration["AzureAI:DeploymentName"] ?? "gpt-4o";
            kernelBuilder.AddAzureOpenAIChatCompletion(deployment, endpoint, apiKey);
        }

        kernelBuilder.Plugins.AddFromType<LogAnalysisPlugin>("LogAnalysisPlugin");

        var kernel = kernelBuilder.Build();
        services.AddSingleton(kernel);

        services.AddScoped<ITriageAgent, SemanticKernelTriageAgent>();
        services.AddScoped<IRootCauseAgent, SemanticKernelRootCauseAgent>();
        services.AddScoped<IFixAgent, SemanticKernelFixAgent>();
        services.AddScoped<IGitHubIssueService, GitHubIssueSimulatorService>();
        services.AddScoped<IIncidentOrchestrator, IncidentOrchestrator>();

        return services;
    }
}
