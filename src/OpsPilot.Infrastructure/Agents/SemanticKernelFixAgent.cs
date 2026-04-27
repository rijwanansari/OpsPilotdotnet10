using Microsoft.Extensions.Logging;
using OpsPilot.Application.Interfaces;
using OpsPilot.Domain.Entities;

namespace OpsPilot.Infrastructure.Agents;

public sealed class SemanticKernelFixAgent(
    ILogger<SemanticKernelFixAgent> logger) : IFixAgent
{
    public async Task<List<RemediationStep>> ProposeFixAsync(Incident incident, AgentResult rootCauseResult, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Fix agent generating remediation plan based on root cause from {Agent}", rootCauseResult.AgentName);
        await Task.Delay(600, cancellationToken);

        return DetectScenario(incident.Title) switch
        {
            Scenario.Auth => BuildAuthFix(),
            Scenario.Cdn  => BuildCdnFix(),
            _             => BuildPaymentFix()
        };
    }

    private static List<RemediationStep> BuildPaymentFix() =>
    [
        new()
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
        new()
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
        new()
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
        new()
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
        new()
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

    private static List<RemediationStep> BuildAuthFix() =>
    [
        new()
        {
            Order = 1,
            Title = "Immediate: Renew LDAP TLS certificates manually",
            Description = """
                Re-issue wildcard certificate for *.corp-ldap.internal via the CA:

                1. SSH into the PKI server:
                   ssh pki-admin@pki.internal

                2. Generate new certificate (valid 365 days):
                   vault write pki/issue/internal-tls \
                     common_name="corp-ldap.internal" \
                     alt_names="corp-ldap-01,corp-ldap-02" \
                     ttl="8760h"

                3. Apply to both LDAP servers and restart the LDAP service:
                   ansible-playbook renew-ldap-cert.yml -l corp-ldap-01,corp-ldap-02

                Expected recovery: LDAP TLS handshakes resume within 60-90 seconds.
                """,
            Priority = Priority.Critical,
            EstimatedTimeMinutes = 10
        },
        new()
        {
            Order = 2,
            Title = "Immediate: Restart auth service and reset circuit breakers",
            Description = """
                After LDAP certificates are renewed, restart the auth service:

                kubectl rollout restart deployment/auth-service -n identity
                kubectl rollout status deployment/auth-service -n identity --timeout=120s

                Verify LDAP connectivity is restored:
                kubectl exec -n identity deploy/auth-service -- \
                  ldapsearch -H ldaps://corp-ldap-01 -x -b "dc=corp,dc=internal" "(uid=healthcheck)"

                Warm the JWT token cache:
                curl -X POST https://auth.internal/admin/cache/warm
                """,
            Priority = Priority.Critical,
            EstimatedTimeMinutes = 8
        },
        new()
        {
            Order = 3,
            Title = "Short-term: Fix Vault token renewal for automation",
            Description = """
                The Vault token used by the cert-renewal automation expired. Rotate and re-anchor it:

                1. Authenticate to Vault:
                   vault login -method=approle role_id=$ROLE_ID secret_id=$SECRET_ID

                2. Create a periodic token (auto-renewable, never expires):
                   vault token create -policy=pki-renew -period=720h -renewable=true

                3. Update the Kubernetes secret:
                   kubectl create secret generic vault-pki-token \
                     --from-literal=token=<new-token> \
                     --dry-run=client -o yaml | kubectl apply -f -

                4. Test the renewal pipeline:
                   kubectl create job --from=cronjob/cert-renewal cert-renewal-manual -n cert-manager
                """,
            Priority = Priority.High,
            EstimatedTimeMinutes = 20
        },
        new()
        {
            Order = 4,
            Title = "Preventive: Add certificate expiry monitoring and alerts",
            Description = """
                Deploy Prometheus cert-expiry exporter to alert at 30/14/7/1 days before expiry:

                helm upgrade --install cert-exporter enix/x509-certificate-exporter \
                  --namespace monitoring \
                  --set secretsExporter.enabled=true \
                  --set prometheusRules.enabled=true

                Add PagerDuty alert rule:
                  - alert: CertExpiryWarning
                    expr: x509_cert_not_after - time() < 30 * 86400
                    severity: warning

                Also split primary/secondary LDAP onto different certificates (not wildcard shared).
                """,
            Priority = Priority.Medium,
            EstimatedTimeMinutes = 45
        }
    ];

    private static List<RemediationStep> BuildCdnFix() =>
    [
        new()
        {
            Order = 1,
            Title = "Immediate: Restore BGP routes via emergency peer session",
            Description = """
                Contact upstream provider AS64512 NOC immediately (24/7 NOC: +1-800-XXX-XXXX):

                1. Request emergency BGP session restore:
                   "Route table on EU-West and AP-South withdrawn during maintenance.
                    Default route 0.0.0.0/0 missing. Need immediate re-announcement."

                2. While waiting, enable backup transit via AS64513 (hot standby):
                   birdc "configure soft"  # on EU-West edge router
                   # Activate backup peer session:
                   birdc "enable BGP_AS64513_BACKUP"

                Expected route restoration: 3-8 minutes after activation.
                """,
            Priority = Priority.Critical,
            EstimatedTimeMinutes = 15
        },
        new()
        {
            Order = 2,
            Title = "Immediate: Relax DDoS mitigation — remove /24 block",
            Description = """
                The /24 IP block is causing collateral damage to 340 legitimate IPs.
                Replace with specific /32 blocks for the 14 attack source IPs:

                # Remove broad block:
                ufw delete deny from 203.0.113.0/24

                # Add specific blocks:
                for ip in 203.0.113.5 203.0.113.14 ...; do
                  ufw deny from $ip to any
                done

                # Alternatively via AWS WAF:
                aws wafv2 update-ip-set --id <ATTACK-SET-ID> \
                  --addresses "203.0.113.5/32" "203.0.113.14/32" ...

                Verify legitimate traffic recovering within 2 minutes.
                """,
            Priority = Priority.Critical,
            EstimatedTimeMinutes = 10
        },
        new()
        {
            Order = 3,
            Title = "Short-term: Purge CDN cache and verify edge health",
            Description = """
                After BGP routes are restored, force cache revalidation:

                # Purge stale cache from all edge nodes (via CDN API):
                curl -X POST "https://api.cdn.internal/cache/purge" \
                  -H "Authorization: Bearer $CDN_TOKEN" \
                  -d '{"type":"all","regions":["eu-west","us-east","ap-south"]}'

                Verify edge health:
                for region in eu-west us-east ap-south; do
                  curl -I "https://cdn.$region.internal/health"
                done

                Monitor cache hit ratio returning to >90% within 10 minutes.
                """,
            Priority = Priority.High,
            EstimatedTimeMinutes = 15
        },
        new()
        {
            Order = 4,
            Title = "Preventive: BGP graceful maintenance procedure + backup origin routing",
            Description = """
                1. Update BGP maintenance runbook: require gradual route withdrawal over 15 minutes
                   using GRACEFUL_SHUTDOWN community (RFC 8326):
                   bgp community add 65535:0 (GRACEFUL_SHUTDOWN)
                   # Wait 15 minutes before full withdrawal

                2. Move backup origin (10.0.2.50) to a different routed subnet (/25 split)
                   to ensure backup is reachable when primary subnet is blackholed.

                3. Add BGP route-count monitoring:
                   - Alert if received prefixes drop >10% in 60 seconds
                   - Alert if default route 0.0.0.0/0 is withdrawn

                4. Add DDoS mitigation guardrail: max block prefix size /28
                   (never block larger than a /28 without manual approval).
                """,
            Priority = Priority.Medium,
            EstimatedTimeMinutes = 90
        }
    ];

    private static Scenario DetectScenario(string title) =>
        title.Contains("Auth", StringComparison.OrdinalIgnoreCase)  ? Scenario.Auth  :
        title.Contains("CDN",  StringComparison.OrdinalIgnoreCase)  ? Scenario.Cdn   :
        title.Contains("Edge", StringComparison.OrdinalIgnoreCase)  ? Scenario.Cdn   :
        Scenario.Payment;

    private enum Scenario { Payment, Auth, Cdn }
}

