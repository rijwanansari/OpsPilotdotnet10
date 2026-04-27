# OpsPilot — Multi-Agent Incident Response System

> A production-inspired multi-agent incident response system built with **.NET 10**, **Azure AI Foundry**, and **Semantic Kernel**, following **Clean Architecture** principles.

[![.NET 10](https://img.shields.io/badge/.NET-10.0-purple)](https://dotnet.microsoft.com/)
[![Semantic Kernel](https://img.shields.io/badge/Semantic%20Kernel-1.74.0-blue)](https://github.com/microsoft/semantic-kernel)
[![Clean Architecture](https://img.shields.io/badge/Architecture-Clean-green)]()

---

## 🚀 Overview

OpsPilot demonstrates how enterprise AI agents move beyond chatbots into **real operational automation**. When a payment API outage is detected, a pipeline of specialized AI agents automatically:

1. **🔍 Triage Agent** — Analyzes 45 realistic log entries using Semantic Kernel tool-calling (`LogAnalysisPlugin`)
2. **🧠 Root Cause Agent** — Determines the likely failure cause (e.g., DB connection pool exhaustion)
3. **🔧 Fix Agent** — Proposes prioritized remediation steps with exact SQL/kubectl commands
4. **📋 GitHub Service** — Simulates creating a GitHub issue with full incident details

## 📁 Solution Structure

```
OpsPilot.slnx
├── src/
│   ├── OpsPilot.Domain/           # Domain entities (Incident, LogEntry, AgentResult, RemediationStep)
│   ├── OpsPilot.Application/      # Interfaces, IncidentOrchestrator, SampleLogProvider
│   ├── OpsPilot.Infrastructure/   # Semantic Kernel agents, LogAnalysisPlugin, GitHub simulator, DI
│   └── OpsPilot.Console/          # Entry point — colorful CLI demo with ANSI output
└── tests/
    └── OpsPilot.Tests/            # xUnit tests (32 tests covering domain, application, infrastructure)
```

## ⚡ Quick Start

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)

### Run (Simulation Mode — no API keys needed)

```bash
git clone https://github.com/rijwanansari/OpsPilotdotnet10.git
cd OpsPilotdotnet10
dotnet run --project src/OpsPilot.Console
```

### Run Tests

```bash
dotnet test OpsPilot.slnx
```

### Build

```bash
dotnet build OpsPilot.slnx
```

## 🤖 Agentic Concepts Demonstrated

| Concept | Implementation |
|---|---|
| **Tool Calling** | `LogAnalysisPlugin` with 3 `[KernelFunction]` methods invoked by the Triage Agent |
| **Multi-Agent Orchestration** | `IncidentOrchestrator` chains Triage → RootCause → Fix → GitHub |
| **Reasoning Workflows** | Each agent receives prior agents' results for contextual reasoning |
| **Autonomous Actions** | GitHub issue creation happens automatically at pipeline end |

## 🔧 Production Configuration (Azure AI Foundry)

Edit `src/OpsPilot.Console/appsettings.json`:

```json
{
  "AzureAI": {
    "Endpoint": "https://your-foundry.openai.azure.com/",
    "ApiKey": "your-api-key",
    "DeploymentName": "gpt-4o",
    "UseSimulation": false
  },
  "GitHub": {
    "Token": "your-github-pat",
    "Owner": "yourorg",
    "Repo": "payment-api",
    "SimulateOnly": false
  }
}
```

Set `UseSimulation: false` to use real Azure AI Foundry + Semantic Kernel for live inference.

## 📊 Sample Incident Scenario

**Incident**: Payment API Service Outage — P0 Critical  
**Revenue impact**: ~$47K/min  
**Log corpus**: 45 entries across 4 services over 5.2 minutes

**Root cause found**: Database connection pool exhaustion from an unoptimized `SELECT *` query (missing index from migration #247) combined with a simultaneous Redis auth token rotation.

**Remediation steps** (100 min total):
1. 🔴 Kill long-running DB queries (`pg_terminate_backend`)
2. 🔴 Reset circuit breakers + rolling pod restart
3. 🟠 Restore Redis auth token via Kubernetes secret
4. 🟠 Recreate missing DB index (`CREATE INDEX CONCURRENTLY`)
5. 🟡 Fix liveness probe + increase connection pool size

## 🏗 Architecture

```
User Trigger
    │
    ▼
IncidentOrchestrator (Application layer)
    │
    ├──► TriageAgent ──────► LogAnalysisPlugin (KernelFunctions: tool calling)
    │         │
    │         └── AgentResult (triage analysis)
    │
    ├──► RootCauseAgent ───► AgentResult (root cause + confidence)
    │
    ├──► FixAgent ──────────► List<RemediationStep> (prioritized)
    │
    └──► GitHubIssueService ► Issue URL (simulated or real)
```

## 🧪 Tests

32 tests across 3 categories:

- **Domain tests**: Entity creation, status transitions, property validation
- **Application tests**: `SampleLogProvider` corpus validation, `IncidentOrchestrator` pipeline with stubs
- **Infrastructure tests**: `LogAnalysisPlugin` KernelFunction logic (error counting, rate calculation, timeline)

