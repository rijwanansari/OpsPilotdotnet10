using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpsPilot.Application.Interfaces;
using OpsPilot.Application.SampleData;
using OpsPilot.Domain.Entities;
using OpsPilot.Infrastructure.DependencyInjection;
using DomainLogLevel = OpsPilot.Domain.Entities.LogLevel;

// ─── Local Functions ──────────────────────────────────────────────────────────
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
    Console.WriteLine($@"  ║  {C.Gray}Built with .NET 10 · Clean Architecture · Simulation Mode{C.Cyan}                  ║");
    Console.WriteLine(@"  ╚═══════════════════════════════════════════════════════════════════════╝");
    Console.WriteLine($"{C.Reset}");
}

static void PrintDivider(string? label = null)
{
    if (label is null)
    {
        Console.WriteLine($"{C.Gray}  {'─'.ToString().PadRight(71, '─')}{C.Reset}");
    }
    else
    {
        var pad = Math.Max(0, 69 - label.Length);
        Console.WriteLine($"{C.Gray}  ─── {C.White}{C.Bold}{label}{C.Reset}{C.Gray} {'─'.ToString().PadRight(pad, '─')}{C.Reset}");
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
    {
        Console.WriteLine($"  {color}│{C.Reset}  {line}");
    }
    Console.WriteLine($"  {color}└{'─'.ToString().PadRight(60, '─')}{C.Reset}");
}

static void PrintRemediationSteps(List<RemediationStep> steps)
{
    Console.WriteLine($"\n  {C.Yellow}{C.Bold}┌─ Remediation Plan ({steps.Count} steps) {C.Reset}");

    foreach (var step in steps)
    {
        var (priorityColor, priorityIcon) = step.Priority switch
        {
            Priority.Critical => (C.Red, "🔴"),
            Priority.High     => (C.Orange, "🟠"),
            Priority.Medium   => (C.Yellow, "🟡"),
            Priority.Low      => (C.Green, "🟢"),
            _                 => (C.Gray, "⚪")
        };

        Console.WriteLine($"  {C.Yellow}│{C.Reset}");
        Console.WriteLine($"  {C.Yellow}│  {priorityIcon} {C.Bold}{priorityColor}[{step.Priority}]{C.Reset} {C.White}{C.Bold}Step {step.Order}: {step.Title}{C.Reset}");
        Console.WriteLine($"  {C.Yellow}│  {C.Gray}     Est. time: {step.EstimatedTimeMinutes} min{C.Reset}");

        var descLines = step.Description.Trim().Split('\n');
        foreach (var dLine in descLines)
        {
            Console.WriteLine($"  {C.Yellow}│{C.Reset}  {C.Gray}     {dLine.TrimStart()}{C.Reset}");
        }
    }

    Console.WriteLine($"  {C.Yellow}└{'─'.ToString().PadRight(60, '─')}{C.Reset}");
    var totalTime = steps.Sum(s => s.EstimatedTimeMinutes);
    Console.WriteLine($"\n  {C.Gray}  Total estimated recovery time: {C.White}{C.Bold}{totalTime} minutes{C.Reset}");
}

static void PrintLogSummary(IReadOnlyList<LogEntry> logs)
{
    var byLevel = logs.GroupBy(l => l.Level).ToDictionary(g => g.Key, g => g.Count());
    var byService = logs.GroupBy(l => l.Source).ToDictionary(g => g.Key, g => g.Count());

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
        var bar = new string('█', Math.Min(count * 2, 30));
        Console.WriteLine($"  {C.Blue}│{C.Reset}    {levelColor}{level,-10}{C.Reset} {bar} {C.Bold}{count}{C.Reset}");
    }
    Console.WriteLine($"  {C.Blue}│{C.Reset}");
    Console.WriteLine($"  {C.Blue}│{C.Reset}  {C.White}By service:{C.Reset}");
    foreach (var (svc, count) in byService)
    {
        Console.WriteLine($"  {C.Blue}│{C.Reset}    {C.Cyan}{svc,-38}{C.Reset} {C.Bold}{count,2}{C.Reset} entries");
    }
    Console.WriteLine($"  {C.Blue}└{'─'.ToString().PadRight(60, '─')}{C.Reset}");
}

// ─── Main ─────────────────────────────────────────────────────────────────────

Console.OutputEncoding = System.Text.Encoding.UTF8;

PrintBanner();

// Setup DI
var configuration = new ConfigurationBuilder()
    .SetBasePath(AppContext.BaseDirectory)
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

var services = new ServiceCollection();
services.AddSingleton<IConfiguration>(configuration);
services.AddLogging(lb => lb.AddConsole().SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Warning));
services.AddOpsPilotInfrastructure(configuration);

await using var serviceProvider = services.BuildServiceProvider();

// ─── Incident Declaration ──────────────────────────────────────────────────────
PrintDivider("INCIDENT TRIGGERED");
Console.WriteLine();
Console.WriteLine($"  {C.BgRed}{C.Bold} 🚨 P0 INCIDENT DETECTED {C.Reset}");
Console.WriteLine();

var incident = new Incident
{
    Title = "Payment API Service Outage — Critical",
    Description = "Complete payment processing halt affecting all regions. " +
                  "Multiple services in cascade failure. Revenue impact: ~$47K/min. " +
                  "Kubernetes pods in CrashLoopBackOff. Circuit breakers open.",
};

Console.WriteLine($"  {C.White}{C.Bold}Incident ID:{C.Reset}    {C.Cyan}{incident.Id}{C.Reset}");
Console.WriteLine($"  {C.White}{C.Bold}Title:{C.Reset}          {C.Red}{C.Bold}{incident.Title}{C.Reset}");
Console.WriteLine($"  {C.White}{C.Bold}Status:{C.Reset}         {C.Yellow}{incident.Status}{C.Reset}");
Console.WriteLine($"  {C.White}{C.Bold}Created:{C.Reset}        {incident.CreatedAt:yyyy-MM-dd HH:mm:ss} UTC");
Console.WriteLine($"  {C.White}{C.Bold}Description:{C.Reset}");
Console.WriteLine($"  {C.Gray}  {incident.Description}{C.Reset}");

// ─── Load Sample Logs ──────────────────────────────────────────────────────────
PrintDivider("LOADING TELEMETRY");
var logs = SampleLogProvider.GetPaymentApiOutageLogs();
PrintLogSummary(logs);

// ─── Multi-Agent Pipeline ─────────────────────────────────────────────────────
PrintDivider("MULTI-AGENT PIPELINE");

var orchestrator = serviceProvider.GetRequiredService<IIncidentOrchestrator>();

Console.WriteLine();
Console.WriteLine($"  {C.Bold}{C.Magenta}Initiating OpsPilot Agent Pipeline...{C.Reset}");
Console.WriteLine($"  {C.Gray}Agents: TriageAgent → RootCauseAgent → FixAgent → GitHubService{C.Reset}");
Console.WriteLine();
PrintDivider();

// Run with progress indicators per-agent
var triageAgent = serviceProvider.GetRequiredService<ITriageAgent>();
var rootCauseAgent = serviceProvider.GetRequiredService<IRootCauseAgent>();
var fixAgent = serviceProvider.GetRequiredService<IFixAgent>();
var gitHubService = serviceProvider.GetRequiredService<IGitHubIssueService>();

AgentResult? triageResult = null;
AgentResult? rootCauseResult = null;
List<RemediationStep>? remediationPlan = null;
string? issueUrl = null;

// Step 1: Triage
await PrintAgentSpinner("Triage Agent", "🔍", C.Cyan, async () =>
{
    incident.Status = IncidentStatus.Triaging;
    triageResult = await triageAgent.AnalyzeAsync(incident, logs);
    incident.TriageResult = triageResult;
    await Task.Delay(200);
});

PrintSection("Triage Analysis", triageResult!.Analysis, C.Cyan);
Console.WriteLine($"\n  {C.Cyan}  Confidence: {C.Bold}{triageResult.Confidence * 100:F1}%{C.Reset}  ·  Agent: {C.Gray}{triageResult.AgentName}{C.Reset}");

PrintDivider();

// Step 2: Root Cause
await PrintAgentSpinner("Root Cause Agent", "🧠", C.Magenta, async () =>
{
    incident.Status = IncidentStatus.RootCauseAnalysis;
    rootCauseResult = await rootCauseAgent.AnalyzeAsync(incident, triageResult!, logs);
    incident.RootCauseResult = rootCauseResult;
});

PrintSection("Root Cause Analysis", rootCauseResult!.Analysis, C.Magenta);
Console.WriteLine($"\n  {C.Magenta}  Confidence: {C.Bold}{rootCauseResult.Confidence * 100:F1}%{C.Reset}  ·  Agent: {C.Gray}{rootCauseResult.AgentName}{C.Reset}");

PrintDivider();

// Step 3: Fix Agent
await PrintAgentSpinner("Fix Agent", "🔧", C.Yellow, async () =>
{
    incident.Status = IncidentStatus.RemediationProposed;
    remediationPlan = await fixAgent.ProposeFixAsync(incident, rootCauseResult!);
    incident.RemediationPlan = remediationPlan;
});

PrintRemediationSteps(remediationPlan!);

PrintDivider();

// Step 4: GitHub Issue
await PrintAgentSpinner("GitHub Issue Service", "📋", C.Green, async () =>
{
    issueUrl = await gitHubService.CreateIssueAsync(incident);
    incident.Status = IncidentStatus.Resolved;
});

Console.WriteLine();
Console.WriteLine($"  {C.Green}{C.Bold}  📋 Issue Created (Simulated):{C.Reset}");
Console.WriteLine($"  {C.Blue}{C.Bold}  🔗 {issueUrl}{C.Reset}");

// ─── Final Summary ─────────────────────────────────────────────────────────────
PrintDivider("PIPELINE COMPLETE");
Console.WriteLine();
Console.WriteLine($"  {C.Green}{C.Bold}╔══════════════════════════════════════════════════════════════╗{C.Reset}");
Console.WriteLine($"  {C.Green}{C.Bold}║  ✅ OpsPilot Incident Response Pipeline — COMPLETE           ║{C.Reset}");
Console.WriteLine($"  {C.Green}{C.Bold}╠══════════════════════════════════════════════════════════════╣{C.Reset}");
Console.WriteLine($"  {C.Green}{C.Bold}║{C.Reset}  Incident:      {C.White}{incident.Title,-44}{C.Green}{C.Bold}║{C.Reset}");
Console.WriteLine($"  {C.Green}{C.Bold}║{C.Reset}  Final Status:  {C.Green}{incident.Status,-44}{C.Green}{C.Bold}║{C.Reset}");
Console.WriteLine($"  {C.Green}{C.Bold}║{C.Reset}  Logs Analyzed: {C.White}{logs.Count,-44}{C.Green}{C.Bold}║{C.Reset}");
Console.WriteLine($"  {C.Green}{C.Bold}║{C.Reset}  Agents Used:   {C.White}{"Triage · RootCause · Fix · GitHubService",-44}{C.Green}{C.Bold}║{C.Reset}");
Console.WriteLine($"  {C.Green}{C.Bold}║{C.Reset}  GitHub Issue:  {C.Blue}{(issueUrl ?? "N/A"),-44}{C.Green}{C.Bold}║{C.Reset}");
Console.WriteLine($"  {C.Green}{C.Bold}╚══════════════════════════════════════════════════════════════╝{C.Reset}");
Console.WriteLine();
Console.WriteLine($"  {C.Gray}  Powered by Azure AI Foundry · Semantic Kernel · .NET 10{C.Reset}");
Console.WriteLine($"  {C.Gray}  Mode: {C.Yellow}Simulation{C.Gray} (set UseSimulation=false + provide Azure AI credentials for production){C.Reset}");
Console.WriteLine();

// ─── ANSI Color Helpers ───────────────────────────────────────────────────────
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
    public const string BgDark  = "\x1b[48;5;235m";

    public static string Colorize(string text, string color) => $"{color}{text}{Reset}";
}
