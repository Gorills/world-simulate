using System.Diagnostics;
using System.Text;
using System.Text.Json;
using Mws.Domain;
using Mws.Simulation.Runtime;

internal static partial class ProofABenchmarks
{
    public static BenchmarkResult RunTrace(int iterations, int repetitions)
    {
        var overheads = new List<double>(repetitions);
        var lookups = new List<double>(repetitions);
        var sizes = new List<long>(repetitions);
        var failures = 0;

        for (var repetition = 0; repetition < repetitions; repetition++)
        {
            var traced = new ProofAKernel(new WorldSeed(44), traceEnabled: true);
            var baseline = new ProofAKernel(new WorldSeed(44), traceEnabled: false);
            var tracedOwner = traced.CreateEntity();
            var tracedSubject = traced.CreateEntity(tracedOwner, initialResource: iterations + 100L);
            var baseOwner = baseline.CreateEntity();
            var baseSubject = baseline.CreateEntity(baseOwner, initialResource: iterations + 100L);

            var tracedWatch = Stopwatch.StartNew();
            for (var index = 0; index < iterations; index++)
            {
                _ = traced.AdjustResource(new CommandId(index + 1), tracedOwner, tracedSubject, -1);
            }
            tracedWatch.Stop();
            var baseWatch = Stopwatch.StartNew();
            for (var index = 0; index < iterations; index++)
            {
                _ = baseline.AdjustResource(new CommandId(index + 1), baseOwner, baseSubject, -1);
            }
            baseWatch.Stop();

            var state = traced.CaptureState();
            var target = state.Trace[^1];
            var lookupWatch = Stopwatch.StartNew();
            var chain = traced.ReconstructConsequence(target.TraceId);
            lookupWatch.Stop();
            if (chain.Count == 0)
            {
                failures++;
            }
            overheads.Add(tracedWatch.Elapsed.TotalMilliseconds - baseWatch.Elapsed.TotalMilliseconds);
            lookups.Add(lookupWatch.Elapsed.TotalMilliseconds);
            sizes.Add(Encoding.UTF8.GetByteCount(JsonSerializer.Serialize(state.Trace, BenchmarkSupport.JsonOptions)));
        }

        return BenchmarkSupport.Create(
            "RW-C_TRACE",
            iterations,
            repetitions,
            new Dictionary<string, object>
            {
                ["trace_bytes"] = sizes.Average(),
                ["trace_write_overhead"] = overheads.Average(),
                ["trace_lookup_latency"] = lookups.Average(),
                ["reconstruction_failure_count"] = failures,
            },
            "Paired traced and untraced transition loops estimate trace write cost; lookup reconstructs a persisted causal consequence.");
    }
}
