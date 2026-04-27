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

    /// <summary>38 realistic log entries for an authentication service certificate expiry scenario.</summary>
    public static IReadOnlyList<LogEntry> GetAuthServiceDegradationLogs()
    {
        var baseTime = DateTime.UtcNow.AddMinutes(-18);
        var c1 = "corr-auth-a1b2";
        var c2 = "corr-auth-c3d4";
        var c3 = "corr-auth-e5f6";

        return
        [
            new() { Timestamp = baseTime.AddSeconds(0),   Level = DomainLogLevel.Info,     Source = "AuthService.API",      Message = "AuthService started. JWT issuer: https://auth.internal/. LDAP: ldap://corp-ldap:389.", CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(15),  Level = DomainLogLevel.Info,     Source = "AuthService.LDAP",     Message = "LDAP connection pool ready. Connections: 5/20.", CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(30),  Level = DomainLogLevel.Info,     Source = "AuthService.Cache",    Message = "Token cache warmed. Entries: 14,322 active sessions.", CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(60),  Level = DomainLogLevel.Info,     Source = "AuthService.API",      Message = "POST /auth/token completed in 48ms. StatusCode=200.", CorrelationId = c1 },
            new() { Timestamp = baseTime.AddSeconds(75),  Level = DomainLogLevel.Info,     Source = "AuthService.API",      Message = "POST /auth/token completed in 51ms. StatusCode=200.", CorrelationId = c2 },
            new() { Timestamp = baseTime.AddSeconds(120), Level = DomainLogLevel.Warn,     Source = "AuthService.LDAP",     Message = "LDAP TLS handshake took 340ms (threshold: 200ms). Server: corp-ldap-01.", CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(135), Level = DomainLogLevel.Warn,     Source = "AuthService.LDAP",     Message = "Certificate expiry warning: corp-ldap-01 TLS cert expires in 2 days.", CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(150), Level = DomainLogLevel.Warn,     Source = "AuthService.API",      Message = "POST /auth/token latency elevated: 890ms (SLA: 500ms).", CorrelationId = c3 },
            new() { Timestamp = baseTime.AddSeconds(165), Level = DomainLogLevel.Warn,     Source = "AuthService.LDAP",     Message = "Connection pool utilization at 72%. Available: 6/20.", CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(180), Level = DomainLogLevel.Error,    Source = "AuthService.LDAP",     Message = "TLS certificate validation FAILED for corp-ldap-01: Certificate has expired (NotAfter: 2026-04-27T03:00:00Z).", CorrelationId = c3 },
            new() { Timestamp = baseTime.AddSeconds(181), Level = DomainLogLevel.Error,    Source = "AuthService.LDAP",     Message = "LDAP bind failed: SSL/TLS connection error. Retrying (1/3).", CorrelationId = c3 },
            new() { Timestamp = baseTime.AddSeconds(183), Level = DomainLogLevel.Error,    Source = "AuthService.LDAP",     Message = "LDAP bind failed: SSL/TLS connection error. Retrying (2/3).", CorrelationId = c3 },
            new() { Timestamp = baseTime.AddSeconds(185), Level = DomainLogLevel.Error,    Source = "AuthService.LDAP",     Message = "LDAP bind failed: SSL/TLS connection error. Retrying (3/3).", CorrelationId = c3 },
            new() { Timestamp = baseTime.AddSeconds(186), Level = DomainLogLevel.Critical, Source = "AuthService.LDAP",     Message = "All LDAP connections failed. Certificate expired on primary and secondary: corp-ldap-01, corp-ldap-02.", CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(187), Level = DomainLogLevel.Error,    Source = "AuthService.API",      Message = "POST /auth/token failed: LDAP directory unreachable. StatusCode=503.", CorrelationId = "corr-auth-g7h8" },
            new() { Timestamp = baseTime.AddSeconds(188), Level = DomainLogLevel.Error,    Source = "AuthService.API",      Message = "POST /auth/token failed: LDAP directory unreachable. StatusCode=503.", CorrelationId = "corr-auth-i9j0" },
            new() { Timestamp = baseTime.AddSeconds(189), Level = DomainLogLevel.Error,    Source = "AuthService.JWT",      Message = "JWT validation failed: Unable to fetch JWKS from LDAP-backed issuer. 5 tokens rejected.", CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(190), Level = DomainLogLevel.Error,    Source = "AuthService.API",      Message = "GET /auth/userinfo failed: Token validation error. StatusCode=401.", CorrelationId = "corr-auth-k1l2" },
            new() { Timestamp = baseTime.AddSeconds(191), Level = DomainLogLevel.Critical, Source = "AuthService.API",      Message = "Authentication failure rate: 98%. All new login attempts failing.", CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(192), Level = DomainLogLevel.Error,    Source = "AuthService.Cache",    Message = "Token cache lookup miss rate: 100%. Fallback to LDAP disabled due to cert failure.", CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(193), Level = DomainLogLevel.Error,    Source = "AuthService.API",      Message = "POST /auth/token failed: LDAP directory unreachable. StatusCode=503.", CorrelationId = "corr-auth-m3n4" },
            new() { Timestamp = baseTime.AddSeconds(194), Level = DomainLogLevel.Error,    Source = "AuthService.API",      Message = "POST /auth/refresh failed: Cannot validate refresh token without LDAP. StatusCode=503.", CorrelationId = "corr-auth-o5p6" },
            new() { Timestamp = baseTime.AddSeconds(195), Level = DomainLogLevel.Critical, Source = "AuthService.LDAP",     Message = "FATAL: All LDAP servers unreachable. corp-ldap-01 (cert expired), corp-ldap-02 (cert expired).", CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(196), Level = DomainLogLevel.Error,    Source = "AuthService.API",      Message = "POST /auth/token failed: LDAP directory unreachable. StatusCode=503.", CorrelationId = "corr-auth-q7r8" },
            new() { Timestamp = baseTime.AddSeconds(197), Level = DomainLogLevel.Error,    Source = "AuthService.JWT",      Message = "JWT validation failed: 47 tokens rejected in last 30s. Downstream services affected.", CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(198), Level = DomainLogLevel.Error,    Source = "AuthService.API",      Message = "POST /auth/token failed: LDAP directory unreachable. StatusCode=503.", CorrelationId = "corr-auth-s9t0" },
            new() { Timestamp = baseTime.AddSeconds(199), Level = DomainLogLevel.Critical, Source = "AuthService.API",      Message = "Circuit breaker OPENED: LDAP service after 10 consecutive failures.", CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(200), Level = DomainLogLevel.Error,    Source = "AuthService.API",      Message = "POST /auth/token failed: Circuit breaker open. StatusCode=503.", CorrelationId = "corr-auth-u1v2" },
            new() { Timestamp = baseTime.AddSeconds(201), Level = DomainLogLevel.Error,    Source = "AuthService.API",      Message = "POST /auth/token failed: Circuit breaker open. StatusCode=503.", CorrelationId = "corr-auth-w3x4" },
            new() { Timestamp = baseTime.AddSeconds(202), Level = DomainLogLevel.Error,    Source = "AuthService.Cache",    Message = "Session cache TTL refresh failing. 2,847 sessions will expire in 5 minutes.", CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(203), Level = DomainLogLevel.Error,    Source = "AuthService.API",      Message = "GET /auth/userinfo failed: Circuit breaker open. StatusCode=503.", CorrelationId = "corr-auth-y5z6" },
            new() { Timestamp = baseTime.AddSeconds(204), Level = DomainLogLevel.Critical, Source = "AuthService.API",      Message = "Downstream impact: OrderService, InventoryService, ReportingService all returning 401.", CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(205), Level = DomainLogLevel.Error,    Source = "AuthService.API",      Message = "POST /auth/token failed: Circuit breaker open. StatusCode=503.", CorrelationId = "corr-auth-a7b8" },
            new() { Timestamp = baseTime.AddSeconds(206), Level = DomainLogLevel.Error,    Source = "AuthService.API",      Message = "POST /auth/token failed: Circuit breaker open. StatusCode=503.", CorrelationId = "corr-auth-c9d0" },
            new() { Timestamp = baseTime.AddSeconds(207), Level = DomainLogLevel.Critical, Source = "AuthService.LDAP",     Message = "Certificate renewal automation script FAILED: Vault token expired. Manual intervention required.", CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(208), Level = DomainLogLevel.Error,    Source = "AuthService.API",      Message = "POST /auth/token failed: Circuit breaker open. StatusCode=503.", CorrelationId = "corr-auth-e1f2" },
            new() { Timestamp = baseTime.AddSeconds(209), Level = DomainLogLevel.Error,    Source = "AuthService.API",      Message = "POST /auth/token failed: Circuit breaker open. StatusCode=503.", CorrelationId = "corr-auth-g3h4" },
            new() { Timestamp = baseTime.AddSeconds(210), Level = DomainLogLevel.Critical, Source = "AuthService.API",      Message = "INCIDENT SEVERITY P1: Authentication completely unavailable. All user logins blocked.", CorrelationId = null },
        ];
    }

    /// <summary>32 realistic log entries for a CDN BGP routing misconfiguration scenario.</summary>
    public static IReadOnlyList<LogEntry> GetCdnOutageLogs()
    {
        var baseTime = DateTime.UtcNow.AddMinutes(-25);
        var c1 = "corr-cdn-1a2b";
        var c2 = "corr-cdn-3c4d";

        return
        [
            new() { Timestamp = baseTime.AddSeconds(0),   Level = DomainLogLevel.Info,     Source = "CDN.EdgeNode.EU-West",   Message = "Edge node healthy. Cached objects: 1.2M. Cache hit ratio: 94.2%.", CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(10),  Level = DomainLogLevel.Info,     Source = "CDN.EdgeNode.US-East",   Message = "Edge node healthy. Cached objects: 1.4M. Cache hit ratio: 96.1%.", CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(20),  Level = DomainLogLevel.Info,     Source = "CDN.Origin.Primary",     Message = "Origin server healthy. Response time: 42ms. Connections: 120/500.", CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(60),  Level = DomainLogLevel.Info,     Source = "CDN.EdgeNode.EU-West",   Message = "BGP maintenance window started by upstream provider AS64512.", CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(90),  Level = DomainLogLevel.Warn,     Source = "CDN.EdgeNode.EU-West",   Message = "BGP route count decreased from 847k to 12 prefixes. Possible route leak.", CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(92),  Level = DomainLogLevel.Warn,     Source = "CDN.EdgeNode.EU-West",   Message = "Traffic shifted to fallback path. Latency increase: 18ms → 320ms.", CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(95),  Level = DomainLogLevel.Warn,     Source = "CDN.Origin.Primary",     Message = "Inbound connection rate spiking: 120 → 1,847/s. Possible traffic reroute from edge.", CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(100), Level = DomainLogLevel.Warn,     Source = "CDN.EdgeNode.AP-South",  Message = "BGP peer AS64512 withdrawn all prefixes. Rerouting in progress.", CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(110), Level = DomainLogLevel.Error,    Source = "CDN.EdgeNode.EU-West",   Message = "GET /assets/app.js failed: Origin unreachable via primary route. StatusCode=502.", CorrelationId = c1 },
            new() { Timestamp = baseTime.AddSeconds(111), Level = DomainLogLevel.Error,    Source = "CDN.EdgeNode.EU-West",   Message = "GET /assets/styles.css failed: Origin unreachable. StatusCode=502.", CorrelationId = c1 },
            new() { Timestamp = baseTime.AddSeconds(112), Level = DomainLogLevel.Error,    Source = "CDN.Origin.Primary",     Message = "Connection queue full: 500/500. New connections being dropped.", CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(113), Level = DomainLogLevel.Error,    Source = "CDN.EdgeNode.US-East",   Message = "Origin TCP connection timeout after 30s. Route to 203.0.113.0/24 blackholed.", CorrelationId = c2 },
            new() { Timestamp = baseTime.AddSeconds(114), Level = DomainLogLevel.Error,    Source = "CDN.EdgeNode.EU-West",   Message = "Cache miss on 4,200 objects — origin fetch failing. Stale cache serving where possible.", CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(115), Level = DomainLogLevel.Critical, Source = "CDN.EdgeNode.EU-West",   Message = "BGP route table corrupted: Default route 0.0.0.0/0 missing. Traffic blackholed.", CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(116), Level = DomainLogLevel.Error,    Source = "CDN.EdgeNode.EU-West",   Message = "GET /assets/app.js failed: 502 Bad Gateway.", CorrelationId = "corr-cdn-5e6f" },
            new() { Timestamp = baseTime.AddSeconds(117), Level = DomainLogLevel.Error,    Source = "CDN.EdgeNode.AP-South",  Message = "GET /api/manifest.json failed: Origin timeout. StatusCode=504.", CorrelationId = "corr-cdn-7g8h" },
            new() { Timestamp = baseTime.AddSeconds(118), Level = DomainLogLevel.Critical, Source = "CDN.Origin.Primary",     Message = "CPU usage: 98%. TCP SYN flood detected from 14 source IPs. Rate: 42k/s.", CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(119), Level = DomainLogLevel.Error,    Source = "CDN.EdgeNode.US-East",   Message = "GET /assets/logo.svg failed: Origin connection refused. StatusCode=502.", CorrelationId = "corr-cdn-9i0j" },
            new() { Timestamp = baseTime.AddSeconds(120), Level = DomainLogLevel.Error,    Source = "CDN.EdgeNode.EU-West",   Message = "GET /assets/bundle.js failed: 502 Bad Gateway.", CorrelationId = "corr-cdn-1k2l" },
            new() { Timestamp = baseTime.AddSeconds(121), Level = DomainLogLevel.Critical, Source = "CDN.EdgeNode.AP-South",  Message = "All routes to origin cluster withdrawn by BGP peer. Island node — no origin connectivity.", CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(122), Level = DomainLogLevel.Error,    Source = "CDN.EdgeNode.US-East",   Message = "GET /assets/app.js failed: 502 Bad Gateway.", CorrelationId = "corr-cdn-3m4n" },
            new() { Timestamp = baseTime.AddSeconds(123), Level = DomainLogLevel.Error,    Source = "CDN.Origin.Primary",     Message = "DDoS mitigation triggered. Blocking 14 source IPs. Collateral damage: 340 legitimate IPs in same /24.", CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(124), Level = DomainLogLevel.Error,    Source = "CDN.EdgeNode.EU-West",   Message = "Serving stale content (age: 24h) for /assets/app.js. Origin unreachable.", CorrelationId = "corr-cdn-5o6p" },
            new() { Timestamp = baseTime.AddSeconds(125), Level = DomainLogLevel.Error,    Source = "CDN.EdgeNode.US-East",   Message = "GET /assets/styles.css failed: 502 Bad Gateway.", CorrelationId = "corr-cdn-7q8r" },
            new() { Timestamp = baseTime.AddSeconds(126), Level = DomainLogLevel.Critical, Source = "CDN.EdgeNode.EU-West",   Message = "Edge node failover to backup origin 10.0.2.50 FAILED: backup also unreachable via corrupted route.", CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(127), Level = DomainLogLevel.Error,    Source = "CDN.EdgeNode.AP-South",  Message = "GET /assets/app.js failed: 503 Service Unavailable.", CorrelationId = "corr-cdn-9s0t" },
            new() { Timestamp = baseTime.AddSeconds(128), Level = DomainLogLevel.Error,    Source = "CDN.EdgeNode.US-East",   Message = "GET /assets/bundle.js failed: 502 Bad Gateway.", CorrelationId = "corr-cdn-1u2v" },
            new() { Timestamp = baseTime.AddSeconds(129), Level = DomainLogLevel.Critical, Source = "CDN.Origin.Primary",     Message = "Origin load balancer reporting 0 healthy backends. All instances failing health checks.", CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(130), Level = DomainLogLevel.Error,    Source = "CDN.EdgeNode.EU-West",   Message = "Cache hit ratio degraded: 94.2% → 0%. All requests require origin fetch, all failing.", CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(131), Level = DomainLogLevel.Error,    Source = "CDN.EdgeNode.AP-South",  Message = "GET /assets/app.js failed: 503 Service Unavailable.", CorrelationId = "corr-cdn-3w4x" },
            new() { Timestamp = baseTime.AddSeconds(132), Level = DomainLogLevel.Critical, Source = "CDN.EdgeNode.US-East",   Message = "ALERT: All 3 CDN regions reporting origin unreachable. Global static asset serving down.", CorrelationId = null },
            new() { Timestamp = baseTime.AddSeconds(133), Level = DomainLogLevel.Critical, Source = "CDN.Origin.Primary",     Message = "INCIDENT SEVERITY P1: CDN completely unable to serve assets globally. Web application broken.", CorrelationId = null },
        ];
    }
}
