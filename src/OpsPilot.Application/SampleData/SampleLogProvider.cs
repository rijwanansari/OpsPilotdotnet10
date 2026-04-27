using OpsPilot.Domain.Entities;
using DomainLogLevel = OpsPilot.Domain.Entities.LogLevel;

namespace OpsPilot.Application.SampleData;

public static class SampleLogProvider
{
    public static IReadOnlyList<LogEntry> GetPaymentApiOutageLogs()
    {
        var baseTime = DateTime.UtcNow.AddMinutes(-32);
        var corrId1 = "corr-4f2a9b1c";
        var corrId2 = "corr-7e8d3a5f";
        var corrId3 = "corr-1b6c9d2e";

        return
        [
            new() { Timestamp = baseTime.AddSeconds(0),   Level = DomainLogLevel.Info,     Source = "PaymentGateway.API",          Message = "Service started. Listening on port 8443.",                                                      CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(12),  Level = DomainLogLevel.Info,     Source = "PaymentGateway.LoadBalancer", Message = "Health check passed. Upstream: 3/3 instances healthy.",                                        CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(45),  Level = DomainLogLevel.Info,     Source = "PaymentGateway.Cache",        Message = "Redis connection pool initialized. Pool size: 20.",                                             CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(60),  Level = DomainLogLevel.Info,     Source = "PaymentGateway.DB",           Message = "Database connection pool initialized. Min: 5, Max: 50.",                                       CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(120), Level = DomainLogLevel.Info,     Source = "PaymentGateway.API",          Message = "POST /api/v2/payments processed in 142ms. StatusCode=200.",                                    CorrelationId = corrId1 },
            new() { Timestamp = baseTime.AddSeconds(135), Level = DomainLogLevel.Info,     Source = "PaymentGateway.API",          Message = "POST /api/v2/payments processed in 138ms. StatusCode=200.",                                    CorrelationId = corrId2 },
            new() { Timestamp = baseTime.AddSeconds(180), Level = DomainLogLevel.Warn,     Source = "PaymentGateway.DB",           Message = "Slow query detected: SELECT * FROM transactions WHERE account_id=? took 1823ms. Threshold=1000ms.", CorrelationId = corrId1 },
            new() { Timestamp = baseTime.AddSeconds(195), Level = DomainLogLevel.Warn,     Source = "PaymentGateway.DB",           Message = "Slow query detected: UPDATE transactions SET status=? WHERE id=? took 2100ms.",                 CorrelationId = corrId2 },
            new() { Timestamp = baseTime.AddSeconds(210), Level = DomainLogLevel.Warn,     Source = "PaymentGateway.DB",           Message = "Connection pool utilization at 78%. Available: 11/50.",                                        CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(240), Level = DomainLogLevel.Warn,     Source = "PaymentGateway.Cache",        Message = "Cache miss rate elevated: 34% (threshold: 20%). Key pattern: payment_session_*",               CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(270), Level = DomainLogLevel.Error,    Source = "PaymentGateway.DB",           Message = "Connection timeout after 5000ms. Retrying (1/3)...",                                           CorrelationId = corrId3 },
            new() { Timestamp = baseTime.AddSeconds(272), Level = DomainLogLevel.Error,    Source = "PaymentGateway.DB",           Message = "Connection timeout after 5000ms. Retrying (2/3)...",                                           CorrelationId = corrId3 },
            new() { Timestamp = baseTime.AddSeconds(275), Level = DomainLogLevel.Error,    Source = "PaymentGateway.DB",           Message = "Connection timeout after 5000ms. Retrying (3/3)...",                                           CorrelationId = corrId3 },
            new() { Timestamp = baseTime.AddSeconds(276), Level = DomainLogLevel.Critical, Source = "PaymentGateway.DB",           Message = "All retry attempts exhausted. Unable to acquire database connection. Pool exhausted.",          CorrelationId = corrId3 },
            new() { Timestamp = baseTime.AddSeconds(277), Level = DomainLogLevel.Error,    Source = "PaymentGateway.API",          Message = "POST /api/v2/payments failed: Database unavailable. StatusCode=500.",                           CorrelationId = corrId3 },
            new() { Timestamp = baseTime.AddSeconds(280), Level = DomainLogLevel.Error,    Source = "PaymentGateway.DB",           Message = "Connection pool exhausted. Active connections: 50/50. Waiters: 12.",                           CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(281), Level = DomainLogLevel.Error,    Source = "PaymentGateway.DB",           Message = "Long-running query detected (>30s): SELECT * FROM transactions JOIN audit_log ON ...",          CorrelationId = "corr-9a2c4e7f" },
            new() { Timestamp = baseTime.AddSeconds(282), Level = DomainLogLevel.Error,    Source = "PaymentGateway.API",          Message = "POST /api/v2/payments failed: Database unavailable. StatusCode=500.",                           CorrelationId = "corr-3d5e8f1a" },
            new() { Timestamp = baseTime.AddSeconds(283), Level = DomainLogLevel.Error,    Source = "PaymentGateway.API",          Message = "GET /api/v2/payments/status failed: Database unavailable. StatusCode=500.",                     CorrelationId = "corr-2f7b4c9d" },
            new() { Timestamp = baseTime.AddSeconds(284), Level = DomainLogLevel.Error,    Source = "PaymentGateway.Cache",        Message = "Redis SETEX failed: NOAUTH Authentication required. Reconnecting...",                           CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(285), Level = DomainLogLevel.Critical, Source = "PaymentGateway.Cache",        Message = "Redis connection lost. Falling back to in-memory cache. Performance degradation expected.",     CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(286), Level = DomainLogLevel.Error,    Source = "PaymentGateway.API",          Message = "POST /api/v2/refunds failed: Downstream service timeout. StatusCode=503.",                      CorrelationId = "corr-6c1d8e3b" },
            new() { Timestamp = baseTime.AddSeconds(287), Level = DomainLogLevel.Error,    Source = "PaymentGateway.API",          Message = "POST /api/v2/payments failed: Database unavailable. StatusCode=500.",                           CorrelationId = "corr-5a9f2b7c" },
            new() { Timestamp = baseTime.AddSeconds(288), Level = DomainLogLevel.Critical, Source = "PaymentGateway.API",          Message = "Circuit breaker OPENED for PaymentGateway.DB after 5 consecutive failures.",                    CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(289), Level = DomainLogLevel.Error,    Source = "PaymentGateway.LoadBalancer", Message = "Upstream PaymentGateway.API instance 10.0.1.12 health check failed. Removing from pool.",      CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(290), Level = DomainLogLevel.Error,    Source = "PaymentGateway.API",          Message = "POST /api/v2/payments failed: Circuit breaker open. StatusCode=503.",                           CorrelationId = "corr-4b8d1c5e" },
            new() { Timestamp = baseTime.AddSeconds(291), Level = DomainLogLevel.Error,    Source = "PaymentGateway.API",          Message = "POST /api/v2/payments failed: Circuit breaker open. StatusCode=503.",                           CorrelationId = "corr-7e2a9f3c" },
            new() { Timestamp = baseTime.AddSeconds(292), Level = DomainLogLevel.Critical, Source = "PaymentGateway.LoadBalancer", Message = "2/3 upstream instances unhealthy. Traffic failover to region us-west-2 initiated.",             CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(293), Level = DomainLogLevel.Error,    Source = "PaymentGateway.DB",           Message = "Replication lag: 47 seconds. Primary overloaded.",                                             CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(294), Level = DomainLogLevel.Error,    Source = "PaymentGateway.API",          Message = "POST /api/v2/payments failed: Circuit breaker open. StatusCode=503.",                           CorrelationId = "corr-1c6e4a8d" },
            new() { Timestamp = baseTime.AddSeconds(295), Level = DomainLogLevel.Error,    Source = "PaymentGateway.API",          Message = "POST /api/v2/payments failed: Circuit breaker open. StatusCode=503.",                           CorrelationId = "corr-8f3b7e2c" },
            new() { Timestamp = baseTime.AddSeconds(296), Level = DomainLogLevel.Critical, Source = "PaymentGateway.API",          Message = "Error rate: 94%. SLA breach imminent. Alert escalated to PagerDuty.",                          CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(297), Level = DomainLogLevel.Error,    Source = "PaymentGateway.API",          Message = "POST /api/v2/payments failed: Circuit breaker open. StatusCode=503.",                           CorrelationId = "corr-2d5c9b1e" },
            new() { Timestamp = baseTime.AddSeconds(298), Level = DomainLogLevel.Error,    Source = "PaymentGateway.Cache",        Message = "In-memory cache eviction rate: 89%. Memory pressure critical.",                                CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(299), Level = DomainLogLevel.Error,    Source = "PaymentGateway.API",          Message = "GET /api/v2/payments/history failed: Service unavailable. StatusCode=503.",                     CorrelationId = "corr-9e4f2a6b" },
            new() { Timestamp = baseTime.AddSeconds(300), Level = DomainLogLevel.Error,    Source = "PaymentGateway.API",          Message = "POST /api/v2/payments failed: Circuit breaker open. StatusCode=503.",                           CorrelationId = "corr-3a7c5d1f" },
            new() { Timestamp = baseTime.AddSeconds(301), Level = DomainLogLevel.Critical, Source = "PaymentGateway.DB",           Message = "FATAL: Out of shared memory. Cannot allocate new connection slot.",                            CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(302), Level = DomainLogLevel.Error,    Source = "PaymentGateway.API",          Message = "POST /api/v2/payments failed: Circuit breaker open. StatusCode=503.",                           CorrelationId = "corr-6b1e8f4c" },
            new() { Timestamp = baseTime.AddSeconds(303), Level = DomainLogLevel.Error,    Source = "PaymentGateway.API",          Message = "POST /api/v2/payments failed: Circuit breaker open. StatusCode=503.",                           CorrelationId = "corr-5d9a3c7e" },
            new() { Timestamp = baseTime.AddSeconds(304), Level = DomainLogLevel.Critical, Source = "PaymentGateway.LoadBalancer", Message = "ALL upstream instances unhealthy. Service completely unavailable.",                             CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(305), Level = DomainLogLevel.Error,    Source = "PaymentGateway.API",          Message = "POST /api/v2/payments failed: No healthy upstream. StatusCode=502.",                            CorrelationId = "corr-7f2b6e9a" },
            new() { Timestamp = baseTime.AddSeconds(306), Level = DomainLogLevel.Error,    Source = "PaymentGateway.API",          Message = "GET /api/v2/health failed: StatusCode=503. Kubernetes liveness probe failing.",                 CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(307), Level = DomainLogLevel.Critical, Source = "PaymentGateway.API",          Message = "Kubernetes: Pod payment-api-7d9f6b-xk2pq restarting (CrashLoopBackOff). Restart count: 4.",    CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(308), Level = DomainLogLevel.Critical, Source = "PaymentGateway.API",          Message = "Kubernetes: Pod payment-api-7d9f6b-m3nw5 restarting (CrashLoopBackOff). Restart count: 3.",    CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(309), Level = DomainLogLevel.Critical, Source = "PaymentGateway.API",          Message = "INCIDENT SEVERITY P0: Payment processing completely halted. Revenue impact: ~$47K/min.",        CorrelationId = null },
        ];
    }
}
