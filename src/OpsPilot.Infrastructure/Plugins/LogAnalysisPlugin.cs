using System.ComponentModel;
using System.Text.Json;
using Microsoft.SemanticKernel;
using OpsPilot.Domain.Entities;

namespace OpsPilot.Infrastructure.Plugins;

public sealed class LogAnalysisPlugin
{
    [KernelFunction, Description("Counts the number of errors per service from a JSON-serialized list of log entries")]
    public string CountErrorsByService([Description("JSON array of log entries")] string logsJson)
    {
        var logs = JsonSerializer.Deserialize<List<LogEntryDto>>(logsJson) ?? [];
        var counts = logs
            .Where(l => l.Level is "Error" or "Critical")
            .GroupBy(l => l.Source)
            .ToDictionary(g => g.Key, g => g.Count());

        return JsonSerializer.Serialize(counts);
    }

    [KernelFunction, Description("Finds the timestamp of the first error or critical log entry")]
    public string FindTimeOfFirstError([Description("JSON array of log entries")] string logsJson)
    {
        var logs = JsonSerializer.Deserialize<List<LogEntryDto>>(logsJson) ?? [];
        var first = logs
            .Where(l => l.Level is "Error" or "Critical")
            .OrderBy(l => l.Timestamp)
            .FirstOrDefault();

        return first is null ? "No errors found" : $"{first.Timestamp:O} from {first.Source}: {first.Message}";
    }

    [KernelFunction, Description("Returns the service with the highest error rate from the logs")]
    public string GetHighestErrorRate([Description("JSON array of log entries")] string logsJson)
    {
        var logs = JsonSerializer.Deserialize<List<LogEntryDto>>(logsJson) ?? [];
        if (logs.Count == 0) return "No logs provided";

        var errorsByService = logs
            .Where(l => l.Level is "Error" or "Critical")
            .GroupBy(l => l.Source)
            .ToDictionary(g => g.Key, g => g.Count());

        var totalByService = logs
            .GroupBy(l => l.Source)
            .ToDictionary(g => g.Key, g => g.Count());

        var highestRate = errorsByService
            .Select(kvp => new
            {
                Service = kvp.Key,
                ErrorCount = kvp.Value,
                Total = totalByService.GetValueOrDefault(kvp.Key, 1),
                Rate = (double)kvp.Value / totalByService.GetValueOrDefault(kvp.Key, 1) * 100
            })
            .OrderByDescending(x => x.Rate)
            .FirstOrDefault();

        return highestRate is null
            ? "No errors found"
            : $"{highestRate.Service}: {highestRate.ErrorCount} errors / {highestRate.Total} total ({highestRate.Rate:F1}% error rate)";
    }

    private sealed record LogEntryDto(string Level, string Source, string Message, DateTime Timestamp);
}
