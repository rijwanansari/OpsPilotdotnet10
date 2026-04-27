namespace OpsPilot.Domain.Entities;

public class Incident
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Title { get; set; }
    public required string Description { get; set; }
    public IncidentStatus Status { get; set; } = IncidentStatus.Open;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public AgentResult? TriageResult { get; set; }
    public AgentResult? RootCauseResult { get; set; }
    public List<RemediationStep> RemediationPlan { get; set; } = [];
}
