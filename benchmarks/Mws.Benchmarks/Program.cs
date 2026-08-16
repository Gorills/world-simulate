using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;

var scale = 20_000;
string? outputPath = null;

for (var index = 0; index < args.Length; index++)
{
    if (string.Equals(args[index], "--scale", StringComparison.Ordinal))
    {
        if (index + 1 >= args.Length || !int.TryParse(args[++index], out scale))
        {
            Console.Error.WriteLine("--scale requires an integer value.");
            return 2;
        }
    }
    else if (string.Equals(args[index], "--output", StringComparison.Ordinal))
    {
        if (index + 1 >= args.Length)
        {
            Console.Error.WriteLine("--output requires a file path.");
            return 2;
        }

        outputPath = args[++index];
    }
    else if (index == 0 && int.TryParse(args[index], out var legacyScale))
    {
        scale = legacyScale;
    }
    else
    {
        Console.Error.WriteLine($"Unknown argument: {args[index]}");
        return 2;
    }
}

if (scale < 100 || scale > 200_000)
{
    Console.Error.WriteLine("Scale must be between 100 and 200000.");
    return 2;
}

var workloads = new[]
{
    RunKernelEvents(scale),
    RunSaveReplay(scale),
    RunCausalTrace(scale),
    RunLodRoundTrip(scale),
};
var workloadsPassed = workloads.All(workload => workload.Passed);
var report = new ProofAWorkloadReport(
    "P0.6 Proof A",
    "partial",
    false,
    DateTimeOffset.UtcNow,
    scale,
    Environment.Version.ToString(),
    workloads);
var json = JsonSerializer.Serialize(report);

if (outputPath is not null)
{
    var directory = Path.GetDirectoryName(outputPath);
    if (!string.IsNullOrEmpty(directory))
    {
        Directory.CreateDirectory(directory);
    }

    File.WriteAllText(outputPath, json, Encoding.UTF8);
}

Console.WriteLine($"MWS_PROOF_A_WORKLOADS {json}");
return workloadsPassed ? 0 : 1;

static WorkloadResult RunKernelEvents(int scale)
{
    var kernel = new ProofAKernel(new WorldSeed(101), traceEnabled: false);
    var owner = kernel.CreateEntity();
    var subject = kernel.CreateEntity(owner, initialResource: 1_000);
    var measured = Measure(() =>
    {
        var passed = true;
        for (var index = 0; index < scale; index++)
        {
            var delta = (index & 1) == 0 ? 1L : -1L;
            passed &= kernel.AdjustResource(kernel.AllocateCommandId(), owner, subject, delta).Success;
        }

        return passed;
    });

    var deterministicOrdering = SameTimeFingerprint(false) == SameTimeFingerprint(true);
    var passed = measured.Value && deterministicOrdering;
    return new WorkloadResult(
        "RW-A",
        "kernel-events",
        passed,
        scale,
        measured.ElapsedMilliseconds,
        measured.AllocatedBytes,
        new Dictionary<string, double>(StringComparer.Ordinal)
        {
            ["events_per_second"] = Rate(scale, measured.ElapsedMilliseconds),
            ["allocated_bytes_per_event"] = measured.AllocatedBytes / Math.Max(1d, scale),
        },
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["mutations"] = measured.Value ? "pass" : "fail",
            ["same_time_ordering"] = deterministicOrdering ? "pass" : "fail",
        });
}

static string SameTimeFingerprint(bool reverse)
{
    var kernel = new ProofAKernel(new WorldSeed(102), traceEnabled: false);
    var owner = kernel.CreateEntity();
    var source = kernel.CreateEntity(owner, initialResource: 10);
    var left = kernel.CreateEntity(owner);
    var right = kernel.CreateEntity(owner);
    var first = new ProofATransferIntent(new CommandId(100), new SimulationTime(1_000), 0, owner, source, left, 8);
    var second = new ProofATransferIntent(new CommandId(101), new SimulationTime(1_000), 0, owner, source, right, 8);
    var intents = reverse ? new[] { second, first } : new[] { first, second };
    var results = kernel.ResolveSameTimeTransfers(intents);
    return string.Join(';', results.Select(result => $"{result.CommandId.Value}:{result.Success}:{result.Code}"))
        + "|"
        + ProofAKernelJson.Serialize(kernel.CaptureState());
}

static WorkloadResult RunSaveReplay(int scale)
{
    var entityCount = Math.Clamp(scale / 10, 100, 2_000);
    var replayCount = Math.Clamp(scale / 20, 50, 1_000);
    var kernel = new ProofAKernel(new WorldSeed(201));
    var owner = kernel.CreateEntity();
    var subjects = new List<EntityId>(entityCount);

    for (var index = 0; index < entityCount; index++)
    {
        var subject = kernel.CreateEntity(owner, initialResource: 5 + (index % 11), rare: index % 97 == 0);
        subjects.Add(subject);
        if (index % 50 == 0)
        {
            _ = kernel.ResolveBoundRandom("rw-b-fixture", subject, index);
        }

        if (index % 250 == 0)
        {
            _ = kernel.StartLongProcess(kernel.AllocateCommandId(), owner, subject, 10_000, 1);
        }
    }

    var save = Measure(() => ProofAKernelJson.Serialize(kernel.CaptureState()));
    var load = Measure(() => ProofAKernelJson.Deserialize(save.Value));
    var replaySubject = subjects[0];
    var replay = Measure(() =>
    {
        var left = ProofAKernel.Restore(load.Value.State);
        var right = ProofAKernel.Restore(load.Value.State);
        for (var index = 0; index < replayCount; index++)
        {
            var commandId = new CommandId(1_000_000L + index);
            var delta = (index & 1) == 0 ? 1L : -1L;
            _ = left.AdjustResource(commandId, owner, replaySubject, delta);
            _ = right.AdjustResource(commandId, owner, replaySubject, delta);
            _ = left.ResolveBoundRandom("rw-b-replay", replaySubject, index);
            _ = right.ResolveBoundRandom("rw-b-replay", replaySubject, index);
        }

        return string.Equals(
            ProofAKernelJson.Serialize(left.CaptureState()),
            ProofAKernelJson.Serialize(right.CaptureState()),
            StringComparison.Ordinal);
    });

    var passed = load.Value.Compatibility == SnapshotCompatibility.CompatibleDecode && replay.Value;
    return new WorkloadResult(
        "RW-B",
        "save-load-replay",
        passed,
        entityCount + replayCount,
        save.ElapsedMilliseconds + load.ElapsedMilliseconds + replay.ElapsedMilliseconds,
        save.AllocatedBytes + load.AllocatedBytes + replay.AllocatedBytes,
        new Dictionary<string, double>(StringComparer.Ordinal)
        {
            ["snapshot_bytes"] = Encoding.UTF8.GetByteCount(save.Value),
            ["save_ms"] = save.ElapsedMilliseconds,
            ["load_ms"] = load.ElapsedMilliseconds,
            ["replay_ms"] = replay.ElapsedMilliseconds,
        },
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["compatibility"] = load.Value.Compatibility.ToString(),
            ["replay_equivalence"] = replay.Value ? "pass" : "fail",
        });
}

static WorkloadResult RunCausalTrace(int scale)
{
    var processCount = Math.Clamp(scale / 5, 50, 5_000);
    var kernel = new ProofAKernel(new WorldSeed(301));
    var owner = kernel.CreateEntity();
    var subject = kernel.CreateEntity(owner, initialResource: 10);
    var writes = Measure(() =>
    {
        var passed = true;
        for (var index = 0; index < processCount; index++)
        {
            var processId = kernel.StartLongProcess(kernel.AllocateCommandId(), owner, subject, 1, 1);
            passed &= processId > 0;
            kernel.AdvanceTo(kernel.Time.AddMilliseconds(1));
        }

        return passed;
    });

    var state = kernel.CaptureState();
    var completionIds = state.Trace
        .Where(entry => string.Equals(entry.Kind, "long-process-completed", StringComparison.Ordinal))
        .Select(entry => entry.TraceId)
        .TakeLast(Math.Min(100, processCount))
        .ToArray();
    var lookups = Measure(() => completionIds.All(traceId =>
    {
        var chain = kernel.ReconstructConsequence(traceId);
        return chain.Count == 2
            && string.Equals(chain[0].Kind, "long-process-started", StringComparison.Ordinal)
            && string.Equals(chain[1].Kind, "long-process-completed", StringComparison.Ordinal);
    }));

    var passed = writes.Value && lookups.Value && completionIds.Length > 0;
    return new WorkloadResult(
        "RW-C",
        "causal-trace",
        passed,
        processCount,
        writes.ElapsedMilliseconds + lookups.ElapsedMilliseconds,
        writes.AllocatedBytes + lookups.AllocatedBytes,
        new Dictionary<string, double>(StringComparer.Ordinal)
        {
            ["trace_entries"] = state.Trace.Count,
            ["processes_per_second"] = Rate(processCount, writes.ElapsedMilliseconds),
            ["lookup_average_us"] = completionIds.Length == 0 ? 0 : (lookups.ElapsedMilliseconds * 1_000d) / completionIds.Length,
        },
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["write_path"] = writes.Value ? "pass" : "fail",
            ["reconstruction"] = lookups.Value ? "pass" : "fail",
        });
}

static WorkloadResult RunLodRoundTrip(int scale)
{
    var entityCount = Math.Clamp(scale / 10, 100, 3_000);
    var kernel = new ProofAKernel(new WorldSeed(401), traceEnabled: false);
    var owner = kernel.CreateEntity();
    var ids = new List<EntityId>(entityCount);
    var rareIds = new List<EntityId>();
    var pendingIds = new List<long>();
    long expectedTotal = 0;

    for (var index = 0; index < entityCount; index++)
    {
        var resource = 5L + (index % 13);
        var rare = index % 97 == 0;
        var subject = kernel.CreateEntity(owner, resource, rare);
        ids.Add(subject);
        expectedTotal = checked(expectedTotal + resource);
        if (rare)
        {
            rareIds.Add(subject);
            var processId = kernel.StartLongProcess(kernel.AllocateCommandId(), owner, subject, 10_000, 1);
            if (processId > 0)
            {
                pendingIds.Add(processId);
                expectedTotal--;
            }
        }
    }

    var roundTrip = Measure(() =>
    {
        var aggregate = kernel.AggregateRegion(ids);
        var materialized = ProofAKernel.MaterializeRegion(aggregate);
        return new LodObservation(aggregate, materialized);
    });
    var observation = roundTrip.Value;
    var identityMatch = ids.SequenceEqual(observation.Materialized.Select(member => member.Id));
    var resourceDelta = observation.Aggregate.TotalResource - expectedTotal;
    var materializedDelta = observation.Materialized.Sum(member => member.Resource) - observation.Aggregate.TotalResource;
    var rareMatch = rareIds.SequenceEqual(observation.Aggregate.RareEntityIds);
    var pendingMatch = pendingIds.SequenceEqual(observation.Aggregate.PendingProcessIds);
    var passed = identityMatch && resourceDelta == 0 && materializedDelta == 0 && rareMatch && pendingMatch;

    return new WorkloadResult(
        "RW-D",
        "lod-roundtrip",
        passed,
        entityCount,
        roundTrip.ElapsedMilliseconds,
        roundTrip.AllocatedBytes,
        new Dictionary<string, double>(StringComparer.Ordinal)
        {
            ["entities"] = entityCount,
            ["resource_divergence"] = Math.Abs(resourceDelta),
            ["materialization_divergence"] = Math.Abs(materializedDelta),
            ["rare_entities"] = rareIds.Count,
            ["pending_processes"] = pendingIds.Count,
        },
        new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["identity"] = identityMatch ? "pass" : "fail",
            ["conservation"] = resourceDelta == 0 && materializedDelta == 0 ? "pass" : "fail",
            ["rare_tail"] = rareMatch ? "pass" : "fail",
            ["pending_processes"] = pendingMatch ? "pass" : "fail",
        });
}

static Measured<T> Measure<T>(Func<T> action)
{
    var beforeAllocated = GC.GetAllocatedBytesForCurrentThread();
    var stopwatch = Stopwatch.StartNew();
    var value = action();
    stopwatch.Stop();
    var allocated = GC.GetAllocatedBytesForCurrentThread() - beforeAllocated;
    return new Measured<T>(value, stopwatch.Elapsed.TotalMilliseconds, allocated);
}

static double Rate(long operations, double elapsedMilliseconds) =>
    elapsedMilliseconds <= 0 ? 0 : operations / (elapsedMilliseconds / 1_000d);

sealed record ProofAWorkloadReport(
    string Gate,
    string GateStatus,
    bool FormalGateClaim,
    DateTimeOffset ObservedUtc,
    int Scale,
    string RuntimeVersion,
    WorkloadResult[] Workloads);

sealed record WorkloadResult(
    string Id,
    string Name,
    bool Passed,
    long Operations,
    double ElapsedMilliseconds,
    long AllocatedBytes,
    Dictionary<string, double> Metrics,
    Dictionary<string, string> Facts);

sealed record Measured<T>(T Value, double ElapsedMilliseconds, long AllocatedBytes);

sealed record LodObservation(ProofARegionAggregate Aggregate, IReadOnlyList<ProofALodMember> Materialized);
