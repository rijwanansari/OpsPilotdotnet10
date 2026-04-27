namespace OpsPilot.Domain.Entities;

public enum Priority
{
    Critical,
    High,
    Medium,
    Low
}

public class RemediationStep
{
    public int Order { get; init; }
    public required string Title { get; init; }
    public required string Description { get; init; }
    public Priority Priority { get; init; }
    public int EstimatedTimeMinutes { get; init; }
}
