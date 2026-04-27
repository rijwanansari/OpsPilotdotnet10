using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpsPilot.Application.Interfaces;
using OpsPilot.Application.SampleData;
using OpsPilot.Domain.Entities;
using OpsPilot.Infrastructure.DependencyInjection;
using DomainLogLevel = OpsPilot.Domain.Entities.LogLevel;

// ─── Print Helpers ────────────────────────────────────────────────────────────
static void PrintBanner()
{
    Console.WriteLine();
    Console.WriteLine($"{C.Cyan}{C.Bold}");
    Console.WriteLine(@"  ╔═══════════════════════════════════════════════════════════════════════╗");
    Console.WriteLine(@"  ║                                                                       ║");
    Console.WriteLine(@"  ║    ██████╗ ██████╗ ███████╗██████╗ ██╗██╗      ██████╗ ████████╗    ║");
    Console.WriteLine(@"  ║   ██╔═══██╗██╔══██╗██╔════╝██╔══██╗██║██║     ██╔═══██╗╚══██╔══╝   ║");
    Console.WriteLine(@"  ║   ██║   ██║██████╔╝███████╗██████╔╝██║██║     ██║   ██║   ██║      ║");
    Console.WriteLine(@"  ║   ██║   ██║██╔═══╝ ╚════██║██╔═══╝ ██║██║     ██║   ██║   ██║      ║");
    Console.WriteLine(@"  ║   ╚██████╔╝██║     ███████║██║     ██║███████╗╚██████╔╝   ██║      ║");
    Console.WriteLine(@"  ║    ╚═════╝ ╚═╝     ╚══════╝╚═╝     ╚═╝╚══════╝ ╚═════╝   ╚═╝      ║");
    Console.WriteLine(@"  ║                                                                       ║");
    Console.WriteLine($@"  ║  {C.Yellow}Multi-Agent Incident Response System{C.Cyan}  ·  {C.Green}Azure AI Foundry + Semantic Kernel{C.Cyan}  ║");
    Console.WriteLine($@"  ║  {C.Gray}Built with .NET 10 · Clean Architecture · Interactive Mode{C.Cyan}                  ║");
    Console.WriteLine(@"  ╚═══════════════════════════════════════════════════════════════════════╝");
    Console.WriteLine($"{C.Reset}");
}

static void PrintDivider(string? label = null)
{
    if (label is null)
        Console.WriteLine($"{C.Gray}  {new string('-', 71)}{C.Reset}");
    else
    {
        var pad = Math.Max(0, 69 - label.Length);
        Console.WriteLine($"{C.Gray}  --- {C.White}{C.Bold}{label}{C.Reset}{C.Gray} {new string('-', pad)}{C.Reset}");
    }
}

static async Task PrintAgentSpinner(string agentName, string emoji, string color, Func<Task> work)
{
    var frames = new[] { "⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏" };
    var frameIndex = 0;
    var done = false;
    Console.Write($"\n  {emoji}  {color}{C.Bold}{agentName}{C.Reset} {C.Gray}");
    var spinnerTask = Task.Run(async () =>
    {
        while (!done)
        {
            Console.Write($"\b{frames[frameIndex++ % frames.Length]}");
            await Task.Delay(80);
        }
    });
    await work();
    done = true;
    await spinnerTask;
    Console.Write($"\b{C.Green}✓{C.Reset}");
    Console.WriteLine($" {C.Green}Complete{C.Reset}");
}

static void PrintSection(string title, string content, string color)
{
    Console.WriteLine($"\n  {color}{C.Bold}┌─ {title} {C.Reset}");
    foreach (var line in content.Split('\n'))
        Console.WriteLine($"  {color}│{C.Reset}  {line}");
    Console.WriteLine($"  {color}└{new string('─', 60)}{C.Reset}");
}

static void PrintRemediationSteps(List<RemediationStep> steps)
{
    Console.WriteLine($"\n  {C.Yellow}{C.Bold}┌─ Remediation Plan ({steps.Count} steps) {C.Reset}");
    foreach (var step in steps)
    {
        var (priorityColor, priorityIcon) = step.Priority switch
        {
            Priority.Critical => (C.Red,    "🔴"),
            Priority.High     => (C.Orange, "🟠"),
            Priority.Medium   => (C.Yellow, "🟡"),
            Priority.Low      => (C.Green,  "🟢"),
            _                 => (C.Gray,   "⚪")
        };
        Console.WriteLine($"  {C.Yellow}│{C.Reset}");
        Console.WriteLine($"  {C.Yellow}│  {priorityIcon} {C.Bold}{priorityColor}[{step.Priority}]{C.Reset} {C.White}{C.Bold}Step {step.Order}: {step.Title}{C.Reset}");
        Console.WriteLine($"  {C.Yellow}│  {C.Gray}     Est. time: {step.EstimatedTimeMinutes} min{C.Reset}");
        foreach (var dLine in step.Description.Trim().Split('\n'))
            Console.WriteLine($"  {C.Yellow}│{C.Reset}  {C.Gray}     {dLine.TrimStart()}{C.Reset}");
    }
    Console.WriteLine($"  {C.Yellow}└{new string('─', 60)}{C.Reset}");
    Console.WriteLine($"\n  {C.Gray}  Total estimated recovery time: {C.White}{C.Bold}{steps.Sum(s => s.EstimatedTimeMinutes)} minutes{C.Reset}");
}

static void PrintSingleStep(RemediationStep step)
{
    var (priorityColor, priorityIcon) = step.Priority switch
    {
        Priority.Critical => (C.Red,    "🔴"),
        Priority.High     => (C.Orange, "🟠"),
        Priority.Medium   => (C.Yellow, "🟡"),
        Priority.Low      => (C.Green,  "🟢"),
        _                 => (C.Gray,   "⚪")
    };
    Console.WriteLine($"\n  {priorityIcon} {C.Bold}{priorityColor}[{step.Priority}]{C.Reset} {C.White}{C.Bold}Step {step.Order}: {step.Title}{C.Reset}");
    Console.WriteLine($"  {C.Gray}  Est. time: {step.EstimatedTimeMinutes} min{C.Reset}");
    foreach (var dLine in step.Description.Trim().Split('\n'))
        Console.WriteLine($"  {C.Cyan}  {dLine.TrimStart()}{C.Reset}");
}

static void PrintLogSummary(IReadOnlyList<LogEntry> logs)
{
    var byLevel   = logs.GroupBy(l => l.Level)  .ToDictionary(g => g.Key, g => g.Count());
    var byService = logs.GroupBy(l => l.Source) .ToDictionary(g => g.Key, g => g.Count());

    Console.WriteLine($"\n  {C.Blue}{C.Bold}┌─ Log Corpus Summary {C.Reset}");
    Console.WriteLine($"  {C.Blue}│{C.Reset}  {C.White}Total entries:{C.Reset} {C.Bold}{logs.Count}{C.Reset}");
    Console.WriteLine($"  {C.Blue}│{C.Reset}  {C.White}Time span:{C.Reset}    {(logs[^1].Timestamp - logs[0].Timestamp).TotalMinutes:F1} minutes");
    Console.WriteLine($"  {C.Blue}│{C.Reset}");
    Console.WriteLine($"  {C.Blue}│{C.Reset}  {C.White}By severity:{C.Reset}");
    foreach (var (level, count) in byLevel.OrderBy(k => k.Key))
    {
        var levelColor = level switch
        {
            DomainLogLevel.Info     => C.Green,
            DomainLogLevel.Warn     => C.Yellow,
            DomainLogLevel.Error    => C.Orange,
            DomainLogLevel.Critical => C.Red,
            _                       => C.Gray
        };
        Console.WriteLine($"  {C.Blue}│{C.Reset}    {levelColor}{level,-10}{C.Reset} {new string('█', Math.Min(count * 2, 30))} {C.Bold}{count}{C.Reset}");
    }
    Console.WriteLine($"  {C.Blue}│{C.Reset}");
    Console.WriteLine($"  {C.Blue}│{C.Reset}  {C.White}By service:{C.Reset}");
    foreach (var (svc, count) in byService)
        Console.WriteLine($"  {C.Blue}│{C.Reset}    {C.Cyan}{svc,-42}{C.Reset} {C.Bold}{count,2}{C.Reset} entries");
    Console.WriteLine($"  {C.Blue}└{new string('─', 60)}{C.Reset}");
}

static void PrintFilteredLogs(IReadOnlyList<LogEntry> logs, string? serviceFilter)
{
    var filtered = string.IsNullOrWhiteSpace(serviceFilter)
        ? (IEnumerable<LogEntry>)logs
        : logs.Where(l => l.Source.Contains(serviceFilter, StringComparison.OrdinalIgnoreCase));

    var list = filtered.ToList();
    if (list.Count == 0)
    {
        Console.WriteLine($"\n  {C.Yellow}No log entries matched '{serviceFilter}'.{C.Reset}");
        return;
    }

    Console.WriteLine($"\n  {C.Blue}{C.Bold}Log Entries{(string.IsNullOrWhiteSpace(serviceFilter) ? "" : $" — {serviceFilter}")} ({list.Count}){C.Reset}");
    Console.WriteLine($"  {C.Gray}  {"Timestamp",-15} {"Level",-10} {"Source",-30} Message{C.Reset}");
    Console.WriteLine($"  {C.Gray}  {new string('─', 95)}{C.Reset}");
    foreach (var e in list)
    {
        var lvlColor = e.Level switch
        {
            DomainLogLevel.Critical => C.Red,
            DomainLogLevel.Error    => C.Orange,
            DomainLogLevel.Warn     => C.Yellow,
            _                       => C.Gray
        };
        var ts  = e.Timestamp.ToString("HH:mm:ss.fff");
        var msg = e.Message.Length > 65 ? e.Message[..62] + "…" : e.Message;
        Console.WriteLine($"  {C.Gray}  {C.Cyan}{ts,-15}{C.Reset} {lvlColor}{e.Level,-10}{C.Reset} {C.White}{e.Source,-30}{C.Reset} {msg}");
    }
}

static void PrintHelp()
{
    Console.WriteLine($"\n  {C.Cyan}{C.Bold}Available commands:{C.Reset}");
    Console.WriteLine($"  {C.White}  show triage{C.Reset}        {C.Gray}Display full triage analysis{C.Reset}");
    Console.WriteLine($"  {C.White}  show rca{C.Reset}           {C.Gray}Display root cause analysis{C.Reset}");
    Console.WriteLine($"  {C.White}  show fix{C.Reset}           {C.Gray}Display all remediation steps{C.Reset}");
    Console.WriteLine($"  {C.White}  show step <N>{C.Reset}      {C.Gray}Display remediation step N  (e.g. show step 2){C.Reset}");
    Console.WriteLine($"  {C.White}  show logs{C.Reset}          {C.Gray}Display all log entries{C.Reset}");
    Console.WriteLine($"  {C.White}  show logs <filter>{C.Reset} {C.Gray}Filter logs by service  (e.g. show logs DB){C.Reset}");
    Console.WriteLine($"  {C.White}  show issue{C.Reset}         {C.Gray}Show the GitHub issue URL{C.Reset}");
    Console.WriteLine($"  {C.White}  show summary{C.Reset}       {C.Gray}Show incident summary card{C.Reset}");
    Console.WriteLine($"  {C.White}  new{C.Reset}                {C.Gray}Return to the main menu{C.Reset}");
    Console.WriteLine($"  {C.White}  exit / quit{C.Reset}        {C.Gray}Exit OpsPilot{C.Reset}");
}

static void PrintMainHelp()
{
    Console.WriteLine($"\n  {C.Cyan}{C.Bold}OpsPilot — Multi-Agent Incident Response System{C.Reset}");
    Console.WriteLine($"\n  {C.White}How it works:{C.Reset}");
    Console.WriteLine($"  {C.Gray}  1. Select or describe an incident{C.Reset}");
    Console.WriteLine($"  {C.Gray}  2. Telemetry loads automatically (realistic operational log data){C.Reset}");
    Console.WriteLine($"  {C.Gray}  3. AI agent pipeline runs:{C.Reset}");
    Console.WriteLine($"  {C.Gray}       🔍 Triage Agent    — analyzes logs via Semantic Kernel tool calling{C.Reset}");
    Console.WriteLine($"  {C.Gray}       🧠 Root Cause Agent — reasons about likely failure causes{C.Reset}");
    Console.WriteLine($"  {C.Gray}       🔧 Fix Agent        — proposes prioritized remediation steps{C.Reset}");
    Console.WriteLine($"  {C.Gray}       📋 GitHub Service   — opens a tracking issue automatically{C.Reset}");
    Console.WriteLine($"  {C.Gray}  4. Interactive shell lets you query each agent's output{C.Reset}");
    Console.WriteLine($"\n  {C.Yellow}Tip: Use step-by-step mode to pause between agents and inspect the reasoning.{C.Reset}");
}

// ─── Interactive Pipeline ─────────────────────────────────────────────────────
static async Task<PipelineContext?> RunInteractivePipeline(
    Incident incident,
    IReadOnlyList<LogEntry> logs,
    ITriageAgent triageAgent,
    IRootCauseAgent rootCauseAgent,
    IFixAgent fixAgent,
    IGitHubIssueService gitHubService,
    bool stepByStep)
{
    var ctx     = new PipelineContext { Incident = incident, Logs = logs };
    var autoRun = !stepByStep;

    PrintDivider("MULTI-AGENT PIPELINE");
    Console.WriteLine();
    Console.WriteLine($"  {C.Bold}{C.Magenta}Initiating OpsPilot Agent Pipeline...{C.Reset}");
    Console.WriteLine($"  {C.Gray}Agents: TriageAgent → RootCauseAgent → FixAgent → GitHubService{C.Reset}");
    if (stepByStep)
        Console.WriteLine($"  {C.Gray}Mode: step-by-step  —  press {C.White}[Enter]{C.Gray} after each step, {C.White}[a]{C.Gray} to auto-run, {C.White}[q]{C.Gray} to abort{C.Reset}");
    Console.WriteLine();
    PrintDivider();

    // ── Step 1: Triage ────────────────────────────────────────────────────────
    await PrintAgentSpinner("Triage Agent", "🔍", C.Cyan, async () =>
    {
        incident.Status      = IncidentStatus.Triaging;
        ctx.TriageResult     = await triageAgent.AnalyzeAsync(incident, logs);
        incident.TriageResult = ctx.TriageResult;
        await Task.Delay(200);
    });

    PrintSection("Triage Analysis", ctx.TriageResult!.Analysis, C.Cyan);
    Console.WriteLine($"\n  {C.Cyan}  Confidence: {C.Bold}{ctx.TriageResult.Confidence * 100:F1}%{C.Reset}  ·  Agent: {C.Gray}{ctx.TriageResult.AgentName}{C.Reset}");
    PrintDivider();

    if (!autoRun)
    {
        Console.Write($"\n  {C.Gray}[Enter] → Root Cause Analysis   [a] auto-run   [q] abort › {C.Reset}");
        var k = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();
        if (k == "q") { Console.WriteLine($"\n  {C.Yellow}Pipeline aborted.{C.Reset}"); return null; }
        if (k == "a") autoRun = true;
    }

    // ── Step 2: Root Cause ────────────────────────────────────────────────────
    await PrintAgentSpinner("Root Cause Agent", "🧠", C.Magenta, async () =>
    {
        incident.Status          = IncidentStatus.RootCauseAnalysis;
        ctx.RootCauseResult      = await rootCauseAgent.AnalyzeAsync(incident, ctx.TriageResult!, logs);
        incident.RootCauseResult = ctx.RootCauseResult;
    });

    PrintSection("Root Cause Analysis", ctx.RootCauseResult!.Analysis, C.Magenta);
    Console.WriteLine($"\n  {C.Magenta}  Confidence: {C.Bold}{ctx.RootCauseResult.Confidence * 100:F1}%{C.Reset}  ·  Agent: {C.Gray}{ctx.RootCauseResult.AgentName}{C.Reset}");
    PrintDivider();

    if (!autoRun)
    {
        Console.Write($"\n  {C.Gray}[Enter] → Fix Agent   [a] auto-run   [q] abort › {C.Reset}");
        var k = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();
        if (k == "q") { Console.WriteLine($"\n  {C.Yellow}Pipeline aborted.{C.Reset}"); return null; }
        if (k == "a") autoRun = true;
    }

    // ── Step 3: Fix ───────────────────────────────────────────────────────────
    await PrintAgentSpinner("Fix Agent", "🔧", C.Yellow, async () =>
    {
        incident.Status         = IncidentStatus.RemediationProposed;
        ctx.RemediationPlan     = await fixAgent.ProposeFixAsync(incident, ctx.RootCauseResult!);
        incident.RemediationPlan = ctx.RemediationPlan;
    });

    PrintRemediationSteps(ctx.RemediationPlan!);
    PrintDivider();

    if (!autoRun)
    {
        Console.Write($"\n  {C.Gray}[Enter] → Create GitHub Issue   [s] skip issue   [q] abort › {C.Reset}");
        var k = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();
        if (k == "q") { Console.WriteLine($"\n  {C.Yellow}Pipeline aborted.{C.Reset}"); return null; }
        if (k == "s") { incident.Status = IncidentStatus.Resolved; ctx.PipelineComplete = true; return ctx; }
    }

    // ── Step 4: GitHub Issue ──────────────────────────────────────────────────
    await PrintAgentSpinner("GitHub Issue Service", "📋", C.Green, async () =>
    {
        ctx.IssueUrl     = await gitHubService.CreateIssueAsync(incident);
        incident.Status  = IncidentStatus.Resolved;
    });

    Console.WriteLine();
    Console.WriteLine($"  {C.Green}{C.Bold}  📋 Issue Created (Simulated):{C.Reset}");
    Console.WriteLine($"  {C.Blue}{C.Bold}  🔗 {ctx.IssueUrl}{C.Reset}");

    ctx.PipelineComplete = true;
    return ctx;
}

// ─── Post-Pipeline Shell ─────────────────────────────────────────────────────
static void RunPostPipelineShell(PipelineContext ctx)
{
    PrintDivider("PIPELINE COMPLETE");
    Console.WriteLine();
    Console.WriteLine($"  {C.Green}{C.Bold}╔══════════════════════════════════════════════════════════════╗{C.Reset}");
    Console.WriteLine($"  {C.Green}{C.Bold}║  ✅ OpsPilot Incident Response Pipeline — COMPLETE           ║{C.Reset}");
    Console.WriteLine($"  {C.Green}{C.Bold}╠══════════════════════════════════════════════════════════════╣{C.Reset}");
    Console.WriteLine($"  {C.Green}{C.Bold}║{C.Reset}  Incident:      {C.White}{ctx.Incident.Title,-44}{C.Green}{C.Bold}║{C.Reset}");
    Console.WriteLine($"  {C.Green}{C.Bold}║{C.Reset}  Final Status:  {C.Green}{ctx.Incident.Status,-44}{C.Green}{C.Bold}║{C.Reset}");
    Console.WriteLine($"  {C.Green}{C.Bold}║{C.Reset}  Logs Analyzed: {C.White}{ctx.Logs.Count,-44}{C.Green}{C.Bold}║{C.Reset}");
    Console.WriteLine($"  {C.Green}{C.Bold}║{C.Reset}  Agents Used:   {C.White}{"Triage · RootCause · Fix · GitHubService",-44}{C.Green}{C.Bold}║{C.Reset}");
    Console.WriteLine($"  {C.Green}{C.Bold}║{C.Reset}  GitHub Issue:  {C.Blue}{(ctx.IssueUrl ?? "N/A"),-44}{C.Green}{C.Bold}║{C.Reset}");
    Console.WriteLine($"  {C.Green}{C.Bold}╚══════════════════════════════════════════════════════════════╝{C.Reset}");
    Console.WriteLine();
    Console.WriteLine($"  {C.Gray}  Powered by Azure AI Foundry · Semantic Kernel · .NET 10{C.Reset}");
    Console.WriteLine($"  {C.Gray}  Mode: {C.Yellow}Simulation{C.Gray} (set UseSimulation=false + Azure AI credentials for production){C.Reset}");

    PrintDivider("OPSPILOT SHELL");
    Console.WriteLine($"  {C.Gray}  Type {C.White}'help'{C.Gray} for commands  ·  {C.White}'new'{C.Gray} for another incident  ·  {C.White}'exit'{C.Gray} to quit{C.Reset}");

    while (true)
    {
        Console.Write($"\n  {C.Cyan}{C.Bold}opspilot>{C.Reset} ");
        var input = (Console.ReadLine() ?? "").Trim();
        if (string.IsNullOrWhiteSpace(input)) continue;

        var tokens = input.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var cmd    = tokens[0].ToLowerInvariant();
        var sub    = tokens.Length > 1 ? tokens[1].ToLowerInvariant() : "";
        var rest   = tokens.Length > 2 ? string.Join(" ", tokens[2..]) : "";

        switch ((cmd, sub))
        {
            case ("help" or "?", _):
                PrintHelp();
                break;

            case ("show", "triage"):
                if (ctx.TriageResult is null)
                    Console.WriteLine($"\n  {C.Yellow}Triage result not available.{C.Reset}");
                else
                {
                    PrintSection("Triage Analysis", ctx.TriageResult.Analysis, C.Cyan);
                    Console.WriteLine($"\n  {C.Cyan}  Confidence: {C.Bold}{ctx.TriageResult.Confidence * 100:F1}%{C.Reset}");
                }
                break;

            case ("show", "rca" or "rootcause"):
                if (ctx.RootCauseResult is null)
                    Console.WriteLine($"\n  {C.Yellow}Root cause analysis not available.{C.Reset}");
                else
                {
                    PrintSection("Root Cause Analysis", ctx.RootCauseResult.Analysis, C.Magenta);
                    Console.WriteLine($"\n  {C.Magenta}  Confidence: {C.Bold}{ctx.RootCauseResult.Confidence * 100:F1}%{C.Reset}");
                }
                break;

            case ("show", "fix" or "remediation"):
                if (ctx.RemediationPlan is null)
                    Console.WriteLine($"\n  {C.Yellow}Remediation plan not available.{C.Reset}");
                else
                    PrintRemediationSteps(ctx.RemediationPlan);
                break;

            case ("show", "step"):
            {
                var stepStr = string.IsNullOrEmpty(rest) ? "" : rest.Trim();
                if (ctx.RemediationPlan is null)
                    Console.WriteLine($"\n  {C.Yellow}Remediation plan not available.{C.Reset}");
                else if (!int.TryParse(stepStr, out var stepNum))
                    Console.WriteLine($"\n  {C.Yellow}Usage: show step <number>  (e.g. show step 1){C.Reset}");
                else
                {
                    var step = ctx.RemediationPlan.FirstOrDefault(s => s.Order == stepNum);
                    if (step is null)
                        Console.WriteLine($"\n  {C.Yellow}Step {stepNum} not found. Plan has {ctx.RemediationPlan.Count} steps.{C.Reset}");
                    else
                        PrintSingleStep(step);
                }
                break;
            }

            case ("show", "logs"):
                PrintFilteredLogs(ctx.Logs, string.IsNullOrWhiteSpace(rest) ? null : rest);
                break;

            case ("show", "issue"):
                if (ctx.IssueUrl is null)
                    Console.WriteLine($"\n  {C.Yellow}No GitHub issue was created for this pipeline run.{C.Reset}");
                else
                    Console.WriteLine($"\n  {C.Blue}{C.Bold}  🔗 {ctx.IssueUrl}{C.Reset}");
                break;

            case ("show", "summary"):
                Console.WriteLine($"\n  {C.White}{C.Bold}Incident:{C.Reset}     {ctx.Incident.Title}");
                Console.WriteLine($"  {C.White}{C.Bold}Status:{C.Reset}       {ctx.Incident.Status}");
                Console.WriteLine($"  {C.White}{C.Bold}Logs:{C.Reset}         {ctx.Logs.Count} entries");
                Console.WriteLine($"  {C.White}{C.Bold}Triage:{C.Reset}       {(ctx.TriageResult is null ? "Not run" : $"{ctx.TriageResult.Confidence * 100:F1}% confidence")}");
                Console.WriteLine($"  {C.White}{C.Bold}Root Cause:{C.Reset}   {(ctx.RootCauseResult is null ? "Not run" : $"{ctx.RootCauseResult.Confidence * 100:F1}% confidence")}");
                Console.WriteLine($"  {C.White}{C.Bold}Fix Steps:{C.Reset}    {(ctx.RemediationPlan is null ? "Not generated" : $"{ctx.RemediationPlan.Count} steps, {ctx.RemediationPlan.Sum(s => s.EstimatedTimeMinutes)} min total")}");
                Console.WriteLine($"  {C.White}{C.Bold}Issue:{C.Reset}        {ctx.IssueUrl ?? "Not created"}");
                break;

            case ("show", ""):
                Console.WriteLine($"\n  {C.Yellow}Usage: show <triage | rca | fix | step N | logs [filter] | issue | summary>{C.Reset}");
                break;

            case ("new", _):
                Console.WriteLine($"\n  {C.Cyan}Returning to main menu…{C.Reset}");
                return;

            case ("exit" or "quit" or "q", _):
                Console.WriteLine($"\n  {C.Gray}  Goodbye. Stay on-call! 👋{C.Reset}\n");
                Environment.Exit(0);
                break;

            default:
                Console.WriteLine($"\n  {C.Yellow}Unknown command '{input}'. Type {C.White}'help'{C.Yellow} for available commands.{C.Reset}");
                break;
        }
    }
}

// ─── Main ─────────────────────────────────────────────────────────────────────
Console.OutputEncoding = System.Text.Encoding.UTF8;
PrintBanner();

var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var services = new ServiceCollection();
services.AddSingleton<IConfiguration>(configuration);
services.AddLogging(lb => lb.AddConsole().SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Warning));
services.AddOpsPilotInfrastructure(configuration);

await using var serviceProvider = services.BuildServiceProvider();

var triageAgent    = serviceProvider.GetRequiredService<ITriageAgent>();
var rootCauseAgent = serviceProvider.GetRequiredService<IRootCauseAgent>();
var fixAgent       = serviceProvider.GetRequiredService<IFixAgent>();
var gitHubService  = serviceProvider.GetRequiredService<IGitHubIssueService>();

// ─── Preset Incident Scenarios ────────────────────────────────────────────────
var presets = new (string Title, string Description, string Severity, string LogLabel, Func<IReadOnlyList<LogEntry>> GetLogs)[]
{
    (
        "Payment API Service Outage",
        "Complete payment processing halt affecting all regions. Multiple services in cascade failure. " +
        "Revenue impact: ~$47K/min. Kubernetes pods in CrashLoopBackOff. Circuit breakers open.",
        "P0 — Critical",
        "45 log entries · PaymentGateway.{API,DB,Cache,LoadBalancer}",
        SampleLogProvider.GetPaymentApiOutageLogs
    ),
    (
        "Authentication Service Degradation",
        "All user logins failing. JWT token validation errors, LDAP directory unreachable. " +
        "Downstream services returning HTTP 401. ~2,847 active sessions expiring.",
        "P1 — High",
        "38 log entries · AuthService.{API,LDAP,JWT,Cache}",
        SampleLogProvider.GetAuthServiceDegradationLogs
    ),
    (
        "CDN Edge Node Failures",
        "Static asset serving broken globally. BGP routing misconfiguration after upstream provider " +
        "maintenance window. All 3 CDN regions reporting origin unreachable.",
        "P1 — High",
        "32 log entries · CDN.{EdgeNode,Origin}",
        SampleLogProvider.GetCdnOutageLogs
    )
};

// ─── Main Menu Loop ───────────────────────────────────────────────────────────
while (true)
{
    Console.WriteLine();
    PrintDivider("MAIN MENU");
    Console.WriteLine();
    Console.WriteLine($"  {C.White}{C.Bold}Select an incident to investigate:{C.Reset}");
    Console.WriteLine();

    for (var i = 0; i < presets.Length; i++)
    {
        var p        = presets[i];
        var sevColor = p.Severity.StartsWith("P0") ? C.Red : C.Orange;
        Console.WriteLine($"  {C.Cyan}{C.Bold}  [{i + 1}]{C.Reset} {C.White}{C.Bold}{p.Title}{C.Reset}  {sevColor}[{p.Severity}]{C.Reset}");
        Console.WriteLine($"      {C.Gray}{p.LogLabel}{C.Reset}");
        Console.WriteLine();
    }

    Console.WriteLine($"  {C.Cyan}{C.Bold}  [4]{C.Reset} {C.White}{C.Bold}Describe a custom incident{C.Reset}");
    Console.WriteLine($"      {C.Gray}Enter your own title and description, choose log dataset{C.Reset}");
    Console.WriteLine();
    Console.WriteLine($"  {C.Gray}  [L] View sample logs   [H] Help   [Q] Quit{C.Reset}");

    Console.Write($"\n  {C.Cyan}{C.Bold}>{C.Reset} ");
    var choice = (Console.ReadLine() ?? "").Trim().ToLowerInvariant();

    if (choice is "q" or "quit" or "exit")
    {
        Console.WriteLine($"\n  {C.Gray}  Goodbye. Stay on-call! 👋{C.Reset}\n");
        break;
    }

    if (choice is "h" or "help")
    {
        PrintMainHelp();
        continue;
    }

    if (choice is "l" or "logs")
    {
        Console.Write($"\n  {C.Gray}Which scenario logs?  {C.White}[1]{C.Gray} Payment  {C.White}[2]{C.Gray} Auth  {C.White}[3]{C.Gray} CDN › {C.Reset}");
        var ls = (Console.ReadLine() ?? "").Trim();
        if (int.TryParse(ls, out var ln) && ln >= 1 && ln <= 3)
            PrintFilteredLogs(presets[ln - 1].GetLogs(), null);
        else
            Console.WriteLine($"\n  {C.Yellow}Enter 1, 2, or 3.{C.Reset}");
        continue;
    }

    Incident?              incident = null;
    IReadOnlyList<LogEntry>? logs   = null;

    if (int.TryParse(choice, out var idx) && idx >= 1 && idx <= 3)
    {
        var p = presets[idx - 1];
        logs  = p.GetLogs();
        incident = new Incident
        {
            Title       = p.Title + " — " + p.Severity,
            Description = p.Description
        };
    }
    else if (choice == "4")
    {
        Console.Write($"\n  {C.Cyan}{C.Bold}Incident title:{C.Reset} ");
        var customTitle = (Console.ReadLine() ?? "").Trim();
        if (string.IsNullOrWhiteSpace(customTitle))
        {
            Console.WriteLine($"\n  {C.Yellow}Title cannot be empty.{C.Reset}");
            continue;
        }
        Console.Write($"  {C.Cyan}{C.Bold}Description:{C.Reset} ");
        var customDesc = (Console.ReadLine() ?? "").Trim();

        Console.Write($"\n  {C.Gray}Which log dataset?  {C.White}[1]{C.Gray} Payment  {C.White}[2]{C.Gray} Auth  {C.White}[3]{C.Gray} CDN › {C.Reset}");
        var lc = (Console.ReadLine() ?? "1").Trim();
        logs = lc switch
        {
            "2" => SampleLogProvider.GetAuthServiceDegradationLogs(),
            "3" => SampleLogProvider.GetCdnOutageLogs(),
            _   => SampleLogProvider.GetPaymentApiOutageLogs()
        };
        incident = new Incident
        {
            Title       = customTitle,
            Description = string.IsNullOrWhiteSpace(customDesc) ? customTitle : customDesc
        };
    }
    else
    {
        Console.WriteLine($"\n  {C.Yellow}Invalid choice '{choice}'. Enter 1–4, L, H, or Q.{C.Reset}");
        continue;
    }

    // ── Show incident card ────────────────────────────────────────────────────
    Console.WriteLine();
    PrintDivider("INCIDENT");
    Console.WriteLine();
    Console.WriteLine($"  {C.BgRed}{C.Bold} 🚨 INCIDENT DETECTED {C.Reset}");
    Console.WriteLine();
    Console.WriteLine($"  {C.White}{C.Bold}ID:{C.Reset}          {C.Cyan}{incident.Id}{C.Reset}");
    Console.WriteLine($"  {C.White}{C.Bold}Title:{C.Reset}       {C.Red}{C.Bold}{incident.Title}{C.Reset}");
    Console.WriteLine($"  {C.White}{C.Bold}Status:{C.Reset}      {C.Yellow}{incident.Status}{C.Reset}");
    Console.WriteLine($"  {C.White}{C.Bold}Created:{C.Reset}     {incident.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC");
    Console.WriteLine($"  {C.White}{C.Bold}Description:{C.Reset}");
    Console.WriteLine($"  {C.Gray}  {incident.Description}{C.Reset}");

    PrintDivider("TELEMETRY");
    PrintLogSummary(logs!);

    // ── Run mode selection ────────────────────────────────────────────────────
    Console.WriteLine();
    Console.WriteLine($"  {C.White}How would you like to run the agent pipeline?{C.Reset}");
    Console.WriteLine($"  {C.Cyan}  [s]{C.Reset} Step-by-step {C.Gray}— pause between agents (recommended){C.Reset}");
    Console.WriteLine($"  {C.Cyan}  [a]{C.Reset} Auto-run     {C.Gray}— run all agents without pausing{C.Reset}");
    Console.WriteLine($"  {C.Cyan}  [c]{C.Reset} Cancel       {C.Gray}— return to main menu{C.Reset}");
    Console.Write($"\n  {C.Cyan}{C.Bold}>{C.Reset} ");
    var runChoice = (Console.ReadLine() ?? "s").Trim().ToLowerInvariant();
    if (runChoice is "c" or "cancel") continue;
    var stepByStep = runChoice != "a";

    // ── Run pipeline ──────────────────────────────────────────────────────────
    var ctx = await RunInteractivePipeline(
        incident, logs!, triageAgent, rootCauseAgent, fixAgent, gitHubService, stepByStep);

    if (ctx is not null)
        RunPostPipelineShell(ctx);
}

// ─── Type Declarations (must follow top-level statements) ─────────────────────

sealed class PipelineContext
{
    public required Incident                Incident        { get; init; }
    public required IReadOnlyList<LogEntry> Logs            { get; init; }
    public AgentResult?                     TriageResult    { get; set; }
    public AgentResult?                     RootCauseResult { get; set; }
    public List<RemediationStep>?           RemediationPlan { get; set; }
    public string?                          IssueUrl        { get; set; }
    public bool                             PipelineComplete { get; set; }
}

static class C
{
    public const string Reset   = "\x1b[0m";
    public const string Bold    = "\x1b[1m";
    public const string Dim     = "\x1b[2m";
    public const string Red     = "\x1b[38;5;196m";
    public const string Orange  = "\x1b[38;5;208m";
    public const string Yellow  = "\x1b[38;5;220m";
    public const string Green   = "\x1b[38;5;46m";
    public const string Cyan    = "\x1b[38;5;51m";
    public const string Blue    = "\x1b[38;5;33m";
    public const string Magenta = "\x1b[38;5;201m";
    public const string White   = "\x1b[38;5;255m";
    public const string Gray    = "\x1b[38;5;245m";
    public const string BgRed   = "\x1b[48;5;196m";
    public const string BgGreen = "\x1b[48;5;22m";
    public static string Colorize(string text, string color) => $"{color}{text}{Reset}";
}
