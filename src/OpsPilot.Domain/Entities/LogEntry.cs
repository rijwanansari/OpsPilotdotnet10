namespace OpsPilot.Domain.Entities;

public enum LogLevel
{
    Info,
    Warn,
    Error,
    Critical
}

public class LogEntry
{
    public DateTime Timestamp { get; init; }
    public LogLevel Level { get; init; }
    public required string Source { get; init; }
    public required string Message { get; init; }
    public string? CorrelationId { get; init; }
}
