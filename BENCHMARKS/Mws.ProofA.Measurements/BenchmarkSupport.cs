using System.Runtime.InteropServices;
using System.Text.Json;
using Mws.Simulation.Api;

internal static class BenchmarkSupport
{
    public static JsonSerializerOptions JsonOptions { get; } = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        WriteIndented = true,
    };

    public static BenchmarkResult Create(
        string workloadId,
        int iterations,
        int repetitions,
        Dictionary<string, object> metrics,
        string interpretation)
    {
        var subjectVersion = Environment.GetEnvironmentVariable("MWS_SUBJECT_VERSION")
            ?? Environment.GetEnvironmentVariable("GITHUB_SHA")
            ?? "local-unresolved";
        return new BenchmarkResult(
            $"BR_P06_{workloadId.Replace('-', '_')}_V1",
            workloadId,
            "1",
            "P0.6",
            subjectVersion,
            new Dictionary<string, object>
            {
                ["build"] = subjectVersion,
                ["schema"] = ProofAVersions.CurrentSchemaVersion,
                ["model"] = ProofAVersions.CurrentModelVersion,
                ["configuration"] = ProofAVersions.CurrentConfigurationVersion,
                ["benchmark_workload"] = "1",
            },
            "release",
            new Dictionary<string, object>
            {
                ["hardware_class"] = "github-hosted-ubuntu-x64-reference-ci",
                ["os"] = RuntimeInformation.OSDescription,
                ["runtime"] = RuntimeInformation.FrameworkDescription,
                ["processor_count"] = Environment.ProcessorCount,
                ["architecture"] = RuntimeInformation.ProcessArchitecture.ToString(),
            },
            new object[] { 42, 43, 44, 45 },
            new Dictionary<string, object> { ["iterations"] = iterations, ["fixture"] = "proof-a-kernel-v1" },
            new Dictionary<string, object> { ["iterations"] = iterations },
            "Stopwatch timing plus runtime allocation/process memory counters; paired paths are used where overhead is compared.",
            repetitions,
            metrics,
            "UNMEASURED_BLOCKER",
            "MEASURED",
            interpretation,
            DateTimeOffset.UtcNow,
            "github-hosted-ubuntu-x64-reference-ci",
            "This runner class is a reproducible CI reference baseline only; no product-scale capacity is inferred.",
            "Investigate a same-workload mean regression above 25%; do not auto-fail before an accepted baseline history exists.");
    }

    public static double Percentile(IReadOnlyList<double> samples, double percentile)
    {
        var ordered = samples.OrderBy(value => value).ToArray();
        var index = (int)Math.Ceiling(percentile * ordered.Length) - 1;
        return ordered[Math.Clamp(index, 0, ordered.Length - 1)];
    }
}

internal sealed record BenchmarkResult(
    string Id,
    string WorkloadId,
    string WorkloadVersion,
    string SubjectId,
    string SubjectVersion,
    Dictionary<string, object> VersionBundle,
    string ExecutionMode,
    Dictionary<string, object> Environment,
    IReadOnlyList<object> SeedIds,
    Dictionary<string, object> WorkloadParameters,
    Dictionary<string, object> Duration,
    string InstrumentationOverhead,
    int Repetitions,
    Dictionary<string, object> Metrics,
    string BudgetStatusBefore,
    string BudgetStatusAfter,
    string Interpretation,
    DateTimeOffset Timestamp,
    string TargetHardwareClass,
    string SafetyMarginRationale,
    string RegressionThreshold);
