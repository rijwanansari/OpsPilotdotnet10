# OpsPilot — Presentation & Technical Guide

> A production-inspired **Multi-Agent Incident Response System** built with **.NET 10**, **Microsoft Semantic Kernel**, and **Azure AI Foundry**, following **Clean Architecture** principles.

---

## Table of Contents

1. [Elevator Pitch](#1-elevator-pitch)
2. [The Problem: SRE Toil at Scale](#2-the-problem-sre-toil-at-scale)
3. [What OpsPilot Does](#3-what-opspilot-does)
4. [Architecture Overview](#4-architecture-overview)
5. [Technology Stack](#5-technology-stack)
6. [Clean Architecture Breakdown](#6-clean-architecture-breakdown)
7. [Agent Pipeline — Deep Dive](#7-agent-pipeline--deep-dive)
8. [Semantic Kernel & Tool Calling](#8-semantic-kernel--tool-calling)
9. [Incident Scenarios](#9-incident-scenarios)
10. [Interactive Console Guide](#10-interactive-console-guide)
11. [Running the Project](#11-running-the-project)
12. [Production Mode (Azure AI Foundry)](#12-production-mode-azure-ai-foundry)
13. [Test Strategy](#13-test-strategy)
14. [Extension Points & Roadmap](#14-extension-points--roadmap)
15. [Key Concepts Glossary](#15-key-concepts-glossary)

---

## 1. Elevator Pitch

**OpsPilot turns a P0 alert into a full incident report, root cause analysis, and remediation plan — automatically, in seconds.**

Most incident response still looks like this: an alert fires at 3 AM, an engineer is paged, they manually grep through logs, write up a triage note, escalate to a database team, wait for root cause, then draft remediation steps. The whole process can take 30–90 minutes — all while revenue bleeds.

OpsPilot demonstrates how a **pipeline of specialised AI agents** can perform every step of that workflow autonomously:

- Parse and analyse thousands of log entries
- Identify the highest-error services and the exact timestamp of the first failure
- Determine root cause through causal chain reasoning
- Produce prioritised, copy-pasteable remediation commands (SQL, `kubectl`, `ansible`)
- Open a GitHub issue — fully populated — for team tracking

All without waiting for a human to read a single log line.

---

## 2. The Problem: SRE Toil at Scale

| Pain Point | Current State | With OpsPilot |
|---|---|---|
| **Mean Time to Detect root cause** | 20–60 min manual log analysis | < 5 seconds (automated) |
| **Triage note quality** | Varies by engineer experience | Consistent, structured, confidence-scored |
| **Remediation knowledge** | Tribal / on-call runbooks | Encoded in the Fix Agent |
| **Issue tracking** | Manual copy-paste to GitHub/Jira | Automatic issue creation |
| **Coverage at 3 AM** | Relies on waking someone up | 24/7, zero fatigue |
| **Onboarding new SREs** | Months to learn all failure modes | Guided by AI recommendations |

The goal is not to replace engineers — it is to eliminate the **repetitive, error-prone first 30 minutes** of every incident so human engineers can focus on decisions that actually require judgment.

---

## 3. What OpsPilot Does

When an incident is declared, a **4-agent sequential pipeline** runs automatically:

```
Incident Declared
      │
      ▼
 🔍 Triage Agent
      │  Reads all logs via Semantic Kernel tool calling
      │  → Counts errors per service
      │  → Finds the timestamp of the first failure
      │  → Identifies the highest-error-rate service
      │  Outputs: AgentResult (markdown analysis + confidence %)
      │
      ▼
 🧠 Root Cause Agent
      │  Receives incident + triage result + raw logs
      │  → Builds a timestamped causal chain
      │  → Identifies contributing factors
      │  Outputs: AgentResult (root cause narrative + confidence %)
      │
      ▼
 🔧 Fix Agent
      │  Receives incident + root cause result
      │  → Generates prioritised remediation steps
      │  → Each step has priority (Critical/High/Medium/Low),
      │    exact commands, and estimated time in minutes
      │  Outputs: List<RemediationStep>
      │
      ▼
 📋 GitHub Issue Service
      │  Receives the fully-populated incident object
      │  → Creates a GitHub issue with all agent outputs
      │  Outputs: Issue URL
      │
      ▼
 Incident Status: Resolved ✅
```

---

## 4. Architecture Overview

### Layer Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                      OpsPilot.Console                           │
│   Interactive REPL · ANSI colour output · Spinner animations    │
└──────────────────────────────┬──────────────────────────────────┘
                               │ depends on
┌──────────────────────────────▼──────────────────────────────────┐
│                    OpsPilot.Application                         │
│  ITriageAgent  IRootCauseAgent  IFixAgent  IGitHubIssueService  │
│  IIncidentOrchestrator   IncidentOrchestrator   SampleLogProvider│
└──────────────────────────────┬──────────────────────────────────┘
           interfaces only     │ implemented by
┌──────────────────────────────▼──────────────────────────────────┐
│                   OpsPilot.Infrastructure                       │
│  SemanticKernelTriageAgent      LogAnalysisPlugin (3 tools)     │
│  SemanticKernelRootCauseAgent   GitHubIssueSimulatorService     │
│  SemanticKernelFixAgent         ServiceCollectionExtensions (DI)│
└──────────────────────────────┬──────────────────────────────────┘
           entities only       │ uses
┌──────────────────────────────▼──────────────────────────────────┐
│                      OpsPilot.Domain                            │
│  Incident   LogEntry   AgentResult   RemediationStep            │
│  IncidentStatus (enum)   LogLevel (enum)   Priority (enum)      │
└─────────────────────────────────────────────────────────────────┘
```

### Data Flow

```
SampleLogProvider ──► List<LogEntry> ──► TriageAgent
                                              │ logsJson (serialised)
                                              ▼
                                      Semantic Kernel
                                              │ invokes
                                              ▼
                                      LogAnalysisPlugin
                                       ├── CountErrorsByService
                                       ├── FindTimeOfFirstError
                                       └── GetHighestErrorRate
                                              │ tool results back
                                              ▼
                                        AgentResult
                                              │
                                        RootCauseAgent
                                              │
                                        AgentResult
                                              │
                                          FixAgent
                                              │
                                    List<RemediationStep>
                                              │
                                    GitHubIssueService
                                              │
                                         Issue URL
```

---

## 5. Technology Stack

| Technology | Version | Role |
|---|---|---|
| **.NET 10** | 10.0 | Runtime, async/await, primary language C# 13 |
| **Microsoft Semantic Kernel** | 1.74.0 | AI orchestration framework, plugin/tool-calling host |
| **Azure AI Foundry** | (via SK) | Hosted LLM endpoint (GPT-4o / gpt-4o-mini in production) |
| **Microsoft.Extensions.DependencyInjection** | 10.0 | Constructor injection across all layers |
| **Microsoft.Extensions.Configuration** | 10.0 | appsettings.json binding |
| **Microsoft.Extensions.Logging** | 10.0 | Structured logging |
| **xUnit** | 2.9 | Unit test framework |
| **.NET Slnx** | SDK | Solution file format (.slnx) |

### Why Semantic Kernel?

Semantic Kernel (SK) is Microsoft's open-source SDK for building AI-powered applications. It acts as the glue between:

- **Your code** (C# functions, methods)
- **An LLM** (Azure OpenAI, OpenAI, local models)
- **Plugins** (groups of functions the LLM can call)

In OpsPilot, SK serves two purposes:

1. **Tool calling** — The `LogAnalysisPlugin` registers 3 C# methods as `[KernelFunction]` tools that can be invoked by name, with typed arguments, returning structured results. The TriageAgent calls them directly via `kernel.InvokeAsync(...)`.

2. **Production LLM routing** — When `UseSimulation: false`, SK is configured with `AddAzureOpenAIChatCompletion(...)` and routes prompts to your Azure AI Foundry deployment.

---

## 6. Clean Architecture Breakdown

Clean Architecture enforces a **strict dependency rule**: outer layers can depend on inner layers, but inner layers know nothing about outer layers.

```
Domain  ◄──  Application  ◄──  Infrastructure  ◄──  Console
(core)       (use cases)       (external deps)      (UI/entry)
```

### OpsPilot.Domain — The Core

No external dependencies whatsoever. Contains only entities and value objects:

| Type | Purpose |
|---|---|
| `Incident` | Root aggregate — holds all incident data including agent results |
| `LogEntry` | A single structured log line (timestamp, level, source, message, correlationId) |
| `AgentResult` | Output of any agent — markdown analysis + confidence score |
| `RemediationStep` | A single fix step — title, description, priority, estimated minutes |
| `IncidentStatus` | `Open → Triaging → RootCauseAnalysis → RemediationProposed → Resolved` |
| `LogLevel` | `Info / Warn / Error / Critical` |
| `Priority` | `Critical / High / Medium / Low` |

```csharp
public class Incident
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public required string Title { get; set; }
    public required string Description { get; set; }
    public IncidentStatus Status { get; set; } = IncidentStatus.Open;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
    public AgentResult? TriageResult { get; set; }
    public AgentResult? RootCauseResult { get; set; }
    public List<RemediationStep> RemediationPlan { get; set; } = [];
}
```

### OpsPilot.Application — Use Cases & Contracts

Defines *what* the system does, not *how*. Key parts:

**Interfaces** (the ports):
```csharp
public interface ITriageAgent
{
    Task<AgentResult> AnalyzeAsync(Incident incident,
        IReadOnlyList<LogEntry> logs, CancellationToken ct = default);
}

public interface IRootCauseAgent
{
    Task<AgentResult> AnalyzeAsync(Incident incident,
        AgentResult triageResult, IReadOnlyList<LogEntry> logs, CancellationToken ct = default);
}

public interface IFixAgent
{
    Task<List<RemediationStep>> ProposeFixAsync(Incident incident,
        AgentResult rootCauseResult, CancellationToken ct = default);
}
```

**IncidentOrchestrator** — the pipeline coordinator:
```csharp
public async Task<Incident> RunAsync(Incident incident,
    IReadOnlyList<LogEntry> logs, CancellationToken ct = default)
{
    incident.Status = IncidentStatus.Triaging;
    incident.TriageResult = await triageAgent.AnalyzeAsync(incident, logs, ct);

    incident.Status = IncidentStatus.RootCauseAnalysis;
    incident.RootCauseResult = await rootCauseAgent.AnalyzeAsync(
        incident, incident.TriageResult, logs, ct);

    incident.Status = IncidentStatus.RemediationProposed;
    incident.RemediationPlan = await fixAgent.ProposeFixAsync(
        incident, incident.RootCauseResult, ct);

    await gitHubService.CreateIssueAsync(incident, ct);
    incident.Status = IncidentStatus.Resolved;
    return incident;
}
```

The orchestrator knows nothing about Semantic Kernel, HTTP calls, or databases. That is intentional.

### OpsPilot.Infrastructure — Adapters

The Semantic Kernel agents implement the Application interfaces:

| Interface | Implementation |
|---|---|
| `ITriageAgent` | `SemanticKernelTriageAgent` — uses `Kernel` + `LogAnalysisPlugin` |
| `IRootCauseAgent` | `SemanticKernelRootCauseAgent` — scenario-aware analysis |
| `IFixAgent` | `SemanticKernelFixAgent` — scenario-aware remediation steps |
| `IGitHubIssueService` | `GitHubIssueSimulatorService` — simulates issue creation |

Dependency injection wiring (`ServiceCollectionExtensions`):
```csharp
kernelBuilder.Plugins.AddFromType<LogAnalysisPlugin>("LogAnalysisPlugin");
var kernel = kernelBuilder.Build();
services.AddSingleton(kernel);

services.AddScoped<ITriageAgent, SemanticKernelTriageAgent>();
services.AddScoped<IRootCauseAgent, SemanticKernelRootCauseAgent>();
services.AddScoped<IFixAgent, SemanticKernelFixAgent>();
services.AddScoped<IGitHubIssueService, GitHubIssueSimulatorService>();
services.AddScoped<IIncidentOrchestrator, IncidentOrchestrator>();
```

### OpsPilot.Console — Entry Point & UI

The console project is the only place that:
- Wires up DI (`ServiceCollection`)
- Reads `appsettings.json`
- Drives the interactive REPL loop
- Handles all ANSI colour rendering and spinner animations

---

## 7. Agent Pipeline — Deep Dive

### 🔍 Triage Agent (`SemanticKernelTriageAgent`)

**Job**: Rapidly characterise the incident — what services are affected, how bad is it, when did it start?

**Mechanism — real tool calling via Semantic Kernel**:

```csharp
// 1. Serialise all log entries to JSON
var logsJson = JsonSerializer.Serialize(logs.Select(l => new {
    Level = l.Level.ToString(), l.Source, l.Message, l.Timestamp
}));

// 2. Invoke plugin functions by name
var errorsByService = await kernel.InvokeAsync(
    kernel.Plugins.GetFunction("LogAnalysisPlugin", "CountErrorsByService"),
    new KernelArguments { ["logsJson"] = logsJson });

var firstError = await kernel.InvokeAsync(
    kernel.Plugins.GetFunction("LogAnalysisPlugin", "FindTimeOfFirstError"),
    new KernelArguments { ["logsJson"] = logsJson });

var highestRate = await kernel.InvokeAsync(
    kernel.Plugins.GetFunction("LogAnalysisPlugin", "GetHighestErrorRate"),
    new KernelArguments { ["logsJson"] = logsJson });
```

The results are assembled into a structured markdown `AgentResult` with a confidence score of 97%.

**Output sample**:
```
## Triage Analysis — Payment API Service Outage

Log Corpus: 45 entries analyzed spanning 5.2 minutes

Error Distribution by Service:
  - PaymentGateway.API: 21 errors
  - PaymentGateway.DB: 8 errors
  - PaymentGateway.Cache: 3 errors

Key Findings:
  - Total error/critical events: 35 (77.8% of all log entries)
  - First error: 2026-04-27T08:51:23Z from PaymentGateway.DB: Connection timeout
  - Highest error rate: PaymentGateway.API: 21/24 (87.5%)
```

---

### 🧠 Root Cause Agent (`SemanticKernelRootCauseAgent`)

**Job**: Given the triage results and raw logs, determine *why* the incident happened.

**Mechanism**: Receives the `AgentResult` from Triage as context. Performs scenario detection from the incident title, then builds a timestamped causal chain from the log evidence.

**Scenario routing**:
```csharp
var analysis = DetectScenario(incident.Title) switch
{
    Scenario.Auth    => BuildAuthAnalysis(incident, logs),
    Scenario.Cdn     => BuildCdnAnalysis(incident, logs),
    _                => BuildPaymentAnalysis(incident, logs)
};
```

**Causal chain structure** (Payment scenario):
1. Unoptimised query (T+3:00) — `SELECT *` with missing index → full table scan on 12M rows
2. Connection pool starvation (T+4:36) — all 50 slots consumed
3. Redis cache failure (T+4:44) — simultaneous auth token rotation removed safety net
4. Circuit breaker cascade (T+4:48) — all 5 breakers open within 12 seconds
5. Pod crash loop (T+5:07) — Kubernetes liveness probes fail → CrashLoopBackOff

**Output**: `AgentResult` with confidence 96.8%

---

### 🔧 Fix Agent (`SemanticKernelFixAgent`)

**Job**: Generate a prioritised, executable remediation plan.

**Mechanism**: Same scenario detection pattern as RootCauseAgent. Each `RemediationStep` contains:
- `Order` — sequence number
- `Title` — short description
- `Description` — exact commands to run (SQL, `kubectl`, `vault`, `ansible`)
- `Priority` — Critical / High / Medium / Low
- `EstimatedTimeMinutes` — realistic time estimates

**Payment scenario plan** (5 steps, 100 minutes total):

| # | Priority | Step | Time |
|---|---|---|---|
| 1 | 🔴 Critical | Kill long-running DB queries via `pg_terminate_backend` | 5 min |
| 2 | 🔴 Critical | Reset circuit breakers + `kubectl rollout restart` | 10 min |
| 3 | 🟠 High | Restore Redis auth token via Kubernetes secret | 15 min |
| 4 | 🟠 High | `CREATE INDEX CONCURRENTLY` to restore missing index | 25 min |
| 5 | 🟡 Medium | Fix liveness probe + increase connection pool to 150 | 45 min |

---

### 📋 GitHub Issue Service (`GitHubIssueSimulatorService`)

**Job**: Persist the incident record as a GitHub issue for team visibility and post-incident review.

In simulation mode, it generates a realistic GitHub URL:
```csharp
var issueUrl = $"https://github.com/{owner}/{repo}/issues/{issueNumber}";
```

In production mode (when `SimulateOnly: false`), this would call the GitHub REST API with the full incident body — triage, root cause, and remediation steps — all pre-formatted.

---

## 8. Semantic Kernel & Tool Calling

### What is a Kernel Plugin?

A **Semantic Kernel Plugin** is a C# class where methods decorated with `[KernelFunction]` become callable tools. The `[Description]` attribute describes the function to the LLM so it knows *when* to call it.

```csharp
public sealed class LogAnalysisPlugin
{
    [KernelFunction]
    [Description("Counts the number of errors per service from a JSON-serialized list of log entries")]
    public string CountErrorsByService(
        [Description("JSON array of log entries")] string logsJson)
    {
        var logs = JsonSerializer.Deserialize<List<LogEntryDto>>(logsJson) ?? [];
        var counts = logs
            .Where(l => l.Level is "Error" or "Critical")
            .GroupBy(l => l.Source)
            .ToDictionary(g => g.Key, g => g.Count());
        return JsonSerializer.Serialize(counts);
    }

    [KernelFunction]
    [Description("Finds the timestamp of the first error or critical log entry")]
    public string FindTimeOfFirstError(
        [Description("JSON array of log entries")] string logsJson)
    { ... }

    [KernelFunction]
    [Description("Returns the service with the highest error rate from the logs")]
    public string GetHighestErrorRate(
        [Description("JSON array of log entries")] string logsJson)
    { ... }
}
```

### Plugin Registration

```csharp
kernelBuilder.Plugins.AddFromType<LogAnalysisPlugin>("LogAnalysisPlugin");
```

This scans the class for `[KernelFunction]` methods and registers them under the name `"LogAnalysisPlugin"` in the kernel's plugin collection.

### Invocation Pattern

```csharp
// Direct invocation (simulation mode — no LLM roundtrip needed)
var fn = kernel.Plugins.GetFunction("LogAnalysisPlugin", "CountErrorsByService");
var result = await kernel.InvokeAsync(fn,
    new KernelArguments { ["logsJson"] = logsJson });
```

In production with an LLM, Semantic Kernel would include the plugin descriptions in the system prompt so the model can decide *which* functions to call and *when* — a true agentic tool-use loop.

### Simulation vs Production

```
SIMULATION MODE (UseSimulation: true)
─────────────────────────────────────
No LLM calls are made.
kernel.InvokeAsync() calls the C# function directly.
All analysis text is deterministically generated from log data.
No API keys, no costs, no network required.

PRODUCTION MODE (UseSimulation: false)
───────────────────────────────────────
kernelBuilder.AddAzureOpenAIChatCompletion(deployment, endpoint, apiKey)
The LLM receives the plugins as tool definitions.
Agents can use GPT-4o to reason, call tools, and synthesize analysis.
Full autonomous reasoning, not deterministic templates.
```

---

## 9. Incident Scenarios

OpsPilot ships with three complete, realistic incident scenarios — each with its own log corpus and fully tailored agent outputs.

### Scenario 1 — Payment API Service Outage (P0 Critical)

| Attribute | Value |
|---|---|
| **Severity** | P0 — Critical |
| **Revenue impact** | ~$47,000/minute |
| **Log entries** | 45 entries across 4 services |
| **Time span** | 5.2 minutes |
| **Services** | `PaymentGateway.{API, DB, Cache, LoadBalancer}` |

**Failure chain**: Missing DB index → slow query → connection pool exhaustion → Redis cache failure (simultaneous token rotation) → circuit breakers open → Kubernetes CrashLoopBackOff.

**Remediation**: 5 steps, 100 minutes total. Steps include exact `pg_terminate_backend` SQL, `kubectl rollout restart`, `CREATE INDEX CONCURRENTLY`, and Kubernetes secret patching.

---

### Scenario 2 — Authentication Service Degradation (P1 High)

| Attribute | Value |
|---|---|
| **Severity** | P1 — High |
| **Impact** | All user logins blocked; downstream services return HTTP 401 |
| **Log entries** | 38 entries across 4 services |
| **Time span** | ~3.5 minutes |
| **Services** | `AuthService.{API, LDAP, JWT, Cache}` |

**Failure chain**: LDAP TLS certificate expired (both primary and secondary share same wildcard cert) → TLS handshake failures → LDAP connection pool drained → JWT validation fails (JWKS fetch fails) → circuit breaker opens → 2,847 active sessions begin expiring. Certificate auto-renewal Vault token also expired, blocking recovery.

**Remediation**: 4 steps, 83 minutes total. Steps include `vault write pki/issue/...` to re-issue the certificate, `ansible-playbook` to deploy it, Vault token rotation, and Prometheus cert-expiry exporter deployment.

---

### Scenario 3 — CDN Edge Node Failures (P1 High)

| Attribute | Value |
|---|---|
| **Severity** | P1 — High |
| **Impact** | Global static asset serving broken; web application non-functional |
| **Log entries** | 32 entries across 4 CDN nodes |
| **Time span** | ~2.5 minutes |
| **Services** | `CDN.EdgeNode.{EU-West, US-East, AP-South}`, `CDN.Origin.Primary` |

**Failure chain**: Upstream BGP provider (AS64512) began scheduled maintenance but withdrew 847k prefixes simultaneously instead of gracefully. Default route (0.0.0.0/0) removed from EU-West edge node → traffic blackholed. Concurrent DDoS on origin; mitigation blocked an entire /24, causing collateral damage to 340 legitimate IPs. All 3 edge regions became origin-unreachable. Cache hit ratio dropped to 0%.

**Remediation**: 4 steps, 130 minutes total. Steps include BGP session restore via backup peer `AS64513`, DDoS mitigation /24 → /32 refinement, CDN cache purge via API, and BGP maintenance procedure update (RFC 8326 GRACEFUL_SHUTDOWN community).

---

## 10. Interactive Console Guide

The console app is a full interactive REPL. Here is the complete flow:

### Main Menu

```
  --- MAIN MENU ---------------------------------------------------------

  Select an incident to investigate:

    [1] Payment API Service Outage  [P0 — Critical]
        45 log entries · PaymentGateway.{API,DB,Cache,LoadBalancer}

    [2] Authentication Service Degradation  [P1 — High]
        38 log entries · AuthService.{API,LDAP,JWT,Cache}

    [3] CDN Edge Node Failures  [P1 — High]
        32 log entries · CDN.{EdgeNode,Origin}

    [4] Describe a custom incident
        Enter your own title and description, choose log dataset

    [L] View sample logs   [H] Help   [Q] Quit
```

**Navigation**:
- `1–3` — Select a preset incident
- `4` — Enter custom title + description, then pick a log dataset (1/2/3)
- `L` — Preview raw log entries for any scenario before running it
- `H` — Display the help + how-it-works explanation
- `Q` — Exit

### Pipeline Mode Selection

After selecting an incident, you choose how to run the agents:

```
  [s] Step-by-step  — pause between agents (recommended for demos)
  [a] Auto-run      — run all agents without pausing
  [c] Cancel        — return to main menu
```

### Step-by-Step Mode

In step-by-step mode, each agent pauses after displaying its full output:

```
  [Enter] → Root Cause Analysis   [a] auto-run   [q] abort
```

This lets you read each agent's reasoning before advancing — ideal for live demonstrations.

### Post-Pipeline Shell (`opspilot>`)

After the pipeline completes, you drop into an interactive query shell:

```
  opspilot> _
```

Available commands:

| Command | Description |
|---|---|
| `show triage` | Display the full triage analysis |
| `show rca` | Display the root cause analysis |
| `show fix` | Display all remediation steps |
| `show step <N>` | Display a single step in full detail (e.g. `show step 2`) |
| `show logs` | Display all log entries |
| `show logs <filter>` | Filter logs by service name (e.g. `show logs LDAP`) |
| `show issue` | Display the GitHub issue URL |
| `show summary` | Display the incident summary card |
| `new` | Return to the main menu |
| `exit` / `quit` | Exit OpsPilot |

**Example session**:
```
  opspilot> show summary
  Incident:    Authentication Service Degradation — P1 — High
  Status:      Resolved
  Logs:        38 entries
  Triage:      97.0% confidence
  Root Cause:  97.1% confidence
  Fix Steps:   4 steps, 83 min total
  Issue:       https://github.com/yourorg/payment-api/issues/2841

  opspilot> show logs LDAP
  (filtered view of 11 LDAP log entries)

  opspilot> show step 1
  🔴 [Critical] Step 1: Immediate: Renew LDAP TLS certificates manually
    Est. time: 10 min
    Re-issue wildcard certificate for *.corp-ldap.internal via the CA:
    ...

  opspilot> new
  Returning to main menu…
```

---

## 11. Running the Project

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (no other dependencies)

### Clone & Run

```bash
git clone https://github.com/rijwanansari/OpsPilotdotnet10.git
cd OpsPilotdotnet10
dotnet run --project src/OpsPilot.Console
```

No API keys are required. The app runs entirely in simulation mode by default.

### Build

```bash
dotnet build OpsPilot.slnx
```

Expected output:
```
Build succeeded.
    0 Warning(s)
    0 Error(s)
```

### Tests

```bash
dotnet test OpsPilot.slnx
```

Expected output:
```
Passed!  - Failed: 0, Passed: 32, Skipped: 0, Total: 32
```

---

## 12. Production Mode (Azure AI Foundry)

To connect to a real LLM, edit `src/OpsPilot.Console/appsettings.json`:

```json
{
  "AzureAI": {
    "Endpoint": "https://your-foundry.openai.azure.com/",
    "ApiKey": "your-api-key-here",
    "DeploymentName": "gpt-4o",
    "UseSimulation": false
  },
  "GitHub": {
    "Token": "ghp_your-personal-access-token",
    "Owner": "yourorg",
    "Repo": "payment-api",
    "SimulateOnly": false
  }
}
```

When `UseSimulation: false`:

1. `ServiceCollectionExtensions` calls `kernelBuilder.AddAzureOpenAIChatCompletion(deployment, endpoint, apiKey)`
2. The Semantic Kernel `Kernel` instance is configured with the Azure OpenAI backend
3. Agents can now send prompts to GPT-4o, with `LogAnalysisPlugin` available as a tool set
4. The LLM autonomously decides when to call `CountErrorsByService`, `FindTimeOfFirstError`, and `GetHighestErrorRate` based on the prompt context
5. The analysis returned becomes genuinely AI-generated reasoning, not template text

### Azure AI Foundry Setup

1. Create an Azure AI Foundry resource in the Azure Portal
2. Deploy a `gpt-4o` model
3. Copy the endpoint URL and API key into `appsettings.json`
4. Set `UseSimulation: false`

---

## 13. Test Strategy

The project has **32 unit tests** across three test classes, following the **Arrange-Act-Assert** pattern.

### Domain Tests (`/tests/OpsPilot.Tests/Domain/`)

Tests for entity behaviour:
- `Incident` initialises with a new GUID and `Open` status
- `Incident` status transitions are correct
- `LogEntry` properties are correctly set
- `RemediationStep` priority and order are validated
- `AgentResult` confidence is in the valid range

### Application Tests (`/tests/OpsPilot.Tests/Application/`)

**`SampleLogProviderTests`** — validates the log corpus:
- Correct number of entries per scenario
- Log levels are valid enum values
- Sources are non-empty strings
- Timestamps are in ascending order

**`IncidentOrchestratorTests`** — end-to-end pipeline test using stubs:
```csharp
// Real orchestrator, stub agents — tests the wiring, not the AI
var orchestrator = new IncidentOrchestrator(
    new StubTriageAgent(),
    new StubRootCauseAgent(),
    new StubFixAgent(),
    new StubGitHubIssueService(),
    NullLogger<IncidentOrchestrator>.Instance);

var result = await orchestrator.RunAsync(incident, logs);

Assert.Equal(IncidentStatus.Resolved, result.Status);
Assert.NotNull(result.TriageResult);
Assert.NotNull(result.RootCauseResult);
Assert.NotEmpty(result.RemediationPlan);
```

The stubs implement the interfaces and return hardcoded results, isolating the orchestrator's sequencing logic from agent implementations.

### Infrastructure Tests (`/tests/OpsPilot.Tests/Infrastructure/`)

**`LogAnalysisPluginTests`** — tests all three Kernel functions:

| Test | What it verifies |
|---|---|
| `CountErrorsByService_ReturnsCorrectCounts` | Error + Critical both counted; Info/Warn excluded |
| `CountErrorsByService_NoErrors_ReturnsEmptyJson` | Empty dictionary for clean logs |
| `CountErrorsByService_CriticalCountedAsError` | Critical severity treated as error |
| `FindTimeOfFirstError_ReturnsEarliestErrorTimestamp` | Returns the chronologically first error |
| `FindTimeOfFirstError_NoErrors_ReturnsNoErrorsFound` | Correct sentinel for clean logs |
| `GetHighestErrorRate_ReturnsServiceWithMostErrors` | Correct rate calculation (100% → chosen) |
| `GetHighestErrorRate_EmptyLogs_ReturnsNoLogsProvided` | Handles empty input |
| `GetHighestErrorRate_NoErrors_ReturnsNoErrorsFound` | Handles all-info logs |

These tests do **not** require Semantic Kernel or any mocking framework — the plugin functions are pure C# methods tested as regular code.

---

## 14. Extension Points & Roadmap

OpsPilot is designed to be extended. Here are natural next steps:

### Near-Term Enhancements

| Enhancement | How |
|---|---|
| **Real LLM reasoning** | Set `UseSimulation: false`, provide Azure AI Foundry credentials |
| **Real GitHub issue creation** | Implement `GitHubIssueSimulatorService` using Octokit.NET |
| **PagerDuty / OpsGenie integration** | Add `IAlertingService` interface + infrastructure impl |
| **Webhook trigger** | Add ASP.NET Core minimal API that POSTs an incident and triggers the pipeline |
| **Slack/Teams notification** | Add `INotificationService` after GitHub issue creation |
| **YAML runbook output** | Add a `RunbookExporter` agent that generates Ansible/Terraform YAML |

### Architectural Extensions

| Extension | Description |
|---|---|
| **Parallel agents** | Run Triage + Root Cause in parallel where independent (using `Task.WhenAll`) |
| **Agent memory** | Store past incident patterns in a vector database for retrieval-augmented analysis |
| **Feedback loop** | Allow engineers to rate/correct AI output, fine-tuning future confidence scores |
| **Multi-model routing** | Route simple triage to gpt-4o-mini (cheap), complex RCA to gpt-4o (accurate) |
| **Real log ingestion** | Replace `SampleLogProvider` with Azure Monitor / Splunk / ELK Stack connector |
| **Incident history** | Persist `Incident` objects to a database (PostgreSQL + EF Core) |

### More Agent Types

```
ExistingAgents:  TriageAgent → RootCauseAgent → FixAgent → GitHubService
Future agents:   EscalationAgent (notify right team)
                 RollbackAgent (auto-roll back last deployment)
                 CapacityAgent (scale up resources)
                 PostMortemAgent (generate blameless post-mortem draft)
                 PreventionAgent (suggest code/infra changes to prevent recurrence)
```

---

## 15. Key Concepts Glossary

| Term | Definition |
|---|---|
| **Multi-Agent System** | Multiple specialised AI agents, each with a specific role, working in sequence or parallel |
| **Semantic Kernel** | Microsoft's open-source SDK for building AI-powered apps in .NET, Python, and Java |
| **Tool Calling** | An LLM's ability to call C# functions (plugins) mid-reasoning to fetch real data |
| **KernelFunction** | A C# method decorated with `[KernelFunction]` that becomes an LLM-callable tool |
| **Clean Architecture** | Architectural pattern with strict dependency rule: inner layers know nothing about outer layers |
| **IncidentOrchestrator** | The pipeline coordinator — chains agents sequentially, manages incident state |
| **AgentResult** | The typed output of any agent: agent name, markdown analysis, confidence score, timestamp |
| **RemediationStep** | A single actionable fix step: title, description (with exact commands), priority, time estimate |
| **Simulation Mode** | All agents run deterministically with no LLM calls or API keys required |
| **P0 / P1** | Incident severity: P0 = complete outage / critical revenue impact; P1 = significant degradation |
| **CrashLoopBackOff** | Kubernetes state where a pod repeatedly crashes and restarts |
| **Circuit Breaker** | Design pattern that stops calling a failing service to prevent cascade failures |
| **BGP** | Border Gateway Protocol — the routing protocol that governs internet traffic paths |
| **GRACEFUL_SHUTDOWN** | BGP community signal (RFC 8326) to gradually drain traffic before maintenance |
| **Connection Pool Exhaustion** | All database connections are in use; new requests cannot proceed |

---

## Summary

OpsPilot demonstrates that **AI agents can automate the most expensive 30 minutes of any incident response**: from alert to triage, root cause, and remediation plan. By combining:

- **.NET 10** for a modern, high-performance async runtime
- **Semantic Kernel** for structured AI orchestration and tool calling
- **Clean Architecture** to keep the core domain free of AI/cloud dependencies
- **Azure AI Foundry** for production-grade LLM inference

...the system is both a working demonstration today (simulation mode, zero setup) and a production-ready foundation tomorrow (swap in real credentials, real GitHub token, real log ingestion).

The interactive console makes every part of the pipeline observable and queryable — not a black box, but a transparent reasoning system where every agent's output can be inspected, replayed, and questioned.

---

*OpsPilot — Built with .NET 10 · Semantic Kernel 1.74.0 · Azure AI Foundry · Clean Architecture*
