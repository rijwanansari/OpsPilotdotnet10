using OpsPilot.Domain.Entities;

namespace OpsPilot.Tests.Domain;

public class IncidentTests
{
    [Fact]
    public void Incident_CreatedWithDefaults_HasOpenStatusAndNewGuid()
    {
        var incident = new Incident
        {
            Title = "Test Incident",
            Description = "Test Description"
        };

        Assert.NotEqual(Guid.Empty, incident.Id);
        Assert.Equal(IncidentStatus.Open, incident.Status);
        Assert.Equal("Test Incident", incident.Title);
        Assert.Empty(incident.RemediationPlan);
        Assert.Null(incident.TriageResult);
        Assert.Null(incident.RootCauseResult);
    }

    [Fact]
    public void Incident_TwoInstances_HaveDifferentIds()
    {
        var a = new Incident { Title = "A", Description = "A" };
        var b = new Incident { Title = "B", Description = "B" };

        Assert.NotEqual(a.Id, b.Id);
    }

    [Theory]
    [InlineData(IncidentStatus.Open)]
    [InlineData(IncidentStatus.Triaging)]
    [InlineData(IncidentStatus.RootCauseAnalysis)]
    [InlineData(IncidentStatus.RemediationProposed)]
    [InlineData(IncidentStatus.Resolved)]
    public void Incident_Status_CanBeSetToAllValues(IncidentStatus status)
    {
        var incident = new Incident { Title = "Test", Description = "Test" };
        incident.Status = status;
        Assert.Equal(status, incident.Status);
    }

    [Fact]
    public void LogEntry_CreatesWithRequiredProperties()
    {
        var ts = DateTime.UtcNow;
        var entry = new LogEntry
        {
            Timestamp = ts,
            Level = OpsPilot.Domain.Entities.LogLevel.Error,
            Source = "PaymentGateway.API",
            Message = "Connection timeout"
        };

        Assert.Equal(ts, entry.Timestamp);
        Assert.Equal(OpsPilot.Domain.Entities.LogLevel.Error, entry.Level);
        Assert.Equal("PaymentGateway.API", entry.Source);
        Assert.Equal("Connection timeout", entry.Message);
        Assert.Null(entry.CorrelationId);
    }

    [Fact]
    public void LogEntry_WithCorrelationId_IsSet()
    {
        var entry = new LogEntry
        {
            Timestamp = DateTime.UtcNow,
            Level = OpsPilot.Domain.Entities.LogLevel.Critical,
            Source = "PaymentGateway.DB",
            Message = "Pool exhausted",
            CorrelationId = "corr-1234"
        };

        Assert.Equal("corr-1234", entry.CorrelationId);
    }

    [Fact]
    public void RemediationStep_PriorityAndTimeProperties_AreSet()
    {
        var step = new RemediationStep
        {
            Order = 1,
            Title = "Kill long-running queries",
            Description = "Execute pg_terminate_backend",
            Priority = Priority.Critical,
            EstimatedTimeMinutes = 5
        };

        Assert.Equal(1, step.Order);
        Assert.Equal("Kill long-running queries", step.Title);
        Assert.Equal(Priority.Critical, step.Priority);
        Assert.Equal(5, step.EstimatedTimeMinutes);
    }

    [Fact]
    public void AgentResult_DefaultTimestamp_IsSetToUtcNow()
    {
        var before = DateTime.UtcNow;
        var result = new AgentResult
        {
            AgentName = "TestAgent",
            Analysis = "Test analysis",
            Confidence = 0.95
        };
        var after = DateTime.UtcNow;

        Assert.InRange(result.Timestamp, before, after);
        Assert.Equal("TestAgent", result.AgentName);
        Assert.Equal("Test analysis", result.Analysis);
        Assert.Equal(0.95, result.Confidence);
    }
}
