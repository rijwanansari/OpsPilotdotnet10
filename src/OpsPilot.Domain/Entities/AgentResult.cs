namespace OpsPilot.Domain.Entities;

public class AgentResult
{
    public required string AgentName { get; init; }
    public required string Analysis { get; set; }
    public double Confidence { get; set; }
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;
}
