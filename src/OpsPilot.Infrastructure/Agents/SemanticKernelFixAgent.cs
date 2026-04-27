using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using OpsPilot.Application.Interfaces;
using OpsPilot.Domain.Entities;

namespace OpsPilot.Infrastructure.Agents;

public sealed class SemanticKernelFixAgent(
    Kernel kernel,
    ILogger<SemanticKernelFixAgent> logger) : IFixAgent
{
    private readonly Kernel _kernel = kernel;
    public async Task<List<RemediationStep>> ProposeFixAsync(Incident incident, AgentResult rootCauseResult, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Fix agent generating remediation plan based on root cause from {Agent}", rootCauseResult.AgentName);
        await Task.Delay(600, cancellationToken);

        return
        [
            new RemediationStep
            {
                Order = 1,
                Title = "Immediate: Kill long-running database queries",
                Description = """
                    Execute the following on the primary DB node to terminate blocking queries:
                    
                    SELECT pg_terminate_backend(pid)
                    FROM pg_stat_activity
                    WHERE state = 'active'
                      AND query_start < NOW() - INTERVAL '30 seconds'
                      AND query NOT ILIKE '%pg_stat_activity%';
                    
                    This will release held connections and allow the pool to recover.
                    Estimated connection recovery time: 30-60 seconds after execution.
                    """,
                Priority = Priority.Critical,
                EstimatedTimeMinutes = 5
            },
            new RemediationStep
            {
                Order = 2,
                Title = "Immediate: Reset circuit breakers and restart pods",
                Description = """
                    Reset circuit breakers via admin endpoint, then perform rolling restart:
                    
                    kubectl rollout restart deployment/payment-api -n payments
                    
                    Monitor rollout:
                    kubectl rollout status deployment/payment-api -n payments --timeout=120s
                    
                    Verify health checks pass before proceeding:
                    kubectl get pods -n payments -w
                    """,
                Priority = Priority.Critical,
                EstimatedTimeMinutes = 10
            },
            new RemediationStep
            {
                Order = 3,
                Title = "Short-term: Restore Redis authentication",
                Description = """
                    Re-apply the Redis auth token that was rotated. Update the Kubernetes secret:
                    
                    kubectl create secret generic redis-auth \
                      --from-literal=password=<new-token> \
                      --dry-run=client -o yaml | kubectl apply -f -
                    
                    Restart the Redis connection pool in payment-api by triggering a config reload.
                    Verify cache hit rate returns above 80% within 5 minutes of recovery.
                    """,
                Priority = Priority.High,
                EstimatedTimeMinutes = 15
            },
            new RemediationStep
            {
                Order = 4,
                Title = "Short-term: Add missing database index",
                Description = """
                    The index on transactions.account_id was dropped in migration #247. Restore it:
                    
                    CREATE INDEX CONCURRENTLY idx_transactions_account_id_date
                    ON transactions(account_id, transaction_date DESC)
                    WHERE status != 'archived';
                    
                    Use CONCURRENTLY to avoid table lock. This will take ~15-20 minutes on production
                    data volume (~12M rows). Monitor with:
                    SELECT phase, blocks_done, blocks_total FROM pg_stat_progress_create_index;
                    
                    Also add query timeout at application level: SET statement_timeout = '10s';
                    """,
                Priority = Priority.High,
                EstimatedTimeMinutes = 25
            },
            new RemediationStep
            {
                Order = 5,
                Title = "Preventive: Fix liveness probe and increase connection pool",
                Description = """
                    1. Update liveness probe to use a shallow /ping endpoint instead of /health:
                    
                    livenessProbe:
                      httpGet:
                        path: /ping
                        port: 8443
                      initialDelaySeconds: 30
                      periodSeconds: 10
                    
                    2. Increase DB connection pool max size from 50 to 150 in appsettings:
                    "Database": { "MaxPoolSize": 150, "CommandTimeout": 10 }
                    
                    3. Coordinate future Redis auth token rotations through the deployment pipeline
                    to prevent simultaneous secret rotation and service unavailability.
                    
                    4. Add alerting for connection pool utilization >60% (current threshold was 90%).
                    """,
                Priority = Priority.Medium,
                EstimatedTimeMinutes = 45
            }
        ];
    }
}
