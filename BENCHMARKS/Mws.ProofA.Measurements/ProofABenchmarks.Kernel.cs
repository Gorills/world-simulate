using System.Diagnostics;
using Mws.Domain;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;

internal static partial class ProofABenchmarks
{
    public static BenchmarkResult RunKernelEvents(int iterations, int repetitions)
    {
        var wall = new List<double>(repetitions);
        var allocated = new List<long>(repetitions);
        long peakMemory = 0;
        double cpuUtilization = 0;
        var orderingMismatches = 0;
        var transitionFailures = 0;

        for (var repetition = 0; repetition < repetitions; repetition++)
        {
            var kernel = new ProofAKernel(new WorldSeed(42), traceEnabled: false);
            var owner = kernel.CreateEntity();
            var subject = kernel.CreateEntity(owner, initialResource: iterations + 100L);
            var beforeAllocated = GC.GetTotalAllocatedBytes(precise: true);
            using var process = Process.GetCurrentProcess();
            var cpuBefore = process.TotalProcessorTime;
            var watch = Stopwatch.StartNew();
            for (var index = 0; index < iterations; index++)
            {
                if (!kernel.AdjustResource(new CommandId(index + 1), owner, subject, -1).Success)
                {
                    transitionFailures++;
                }
            }
            watch.Stop();
            process.Refresh();
            wall.Add(watch.Elapsed.TotalMilliseconds);
            allocated.Add(GC.GetTotalAllocatedBytes(precise: true) - beforeAllocated);
            peakMemory = Math.Max(peakMemory, process.PeakWorkingSet64);
            var cpuMs = (process.TotalProcessorTime - cpuBefore).TotalMilliseconds;
            if (watch.Elapsed.TotalMilliseconds > 0)
            {
                cpuUtilization += Math.Min(100d, cpuMs / watch.Elapsed.TotalMilliseconds / Math.Max(Environment.ProcessorCount, 1) * 100d);
            }
            if (!SameTimeOrderIsStable())
            {
                orderingMismatches++;
            }
        }

        return BenchmarkSupport.Create(
            "RW-A_KERNEL_EVENTS",
            iterations,
            repetitions,
            new Dictionary<string, object>
            {
                ["wall_time"] = wall.Average(),
                ["wall_time_ms_p95"] = BenchmarkSupport.Percentile(wall, 0.95),
                ["cpu_utilization"] = cpuUtilization / repetitions,
                ["peak_memory"] = peakMemory,
                ["transition_count"] = iterations,
                ["transition_failure_count"] = transitionFailures,
                ["ordering_mismatch_count"] = orderingMismatches,
                ["allocated_bytes_mean"] = allocated.Average(),
            },
            "Owner-mediated transition loop plus deterministic same-time conflict arbitration; this is a CI reference baseline, not product scale.");
    }

    private static bool SameTimeOrderIsStable()
    {
        static string Run(bool reverse)
        {
            var kernel = new ProofAKernel(new WorldSeed(142), traceEnabled: false);
            var owner = kernel.CreateEntity();
            var source = kernel.CreateEntity(owner, initialResource: 10);
            var left = kernel.CreateEntity(owner);
            var right = kernel.CreateEntity(owner);
            var first = new ProofATransferIntent(new CommandId(100), new SimulationTime(1_000), 0, owner, source, left, 8);
            var second = new ProofATransferIntent(new CommandId(101), new SimulationTime(1_000), 0, owner, source, right, 8);
            var intents = reverse ? new[] { second, first } : new[] { first, second };
            return string.Join(';', kernel.ResolveSameTimeTransfers(intents).Select(r => $"{r.CommandId.Value}:{r.Success}:{r.Code}"));
        }
        return string.Equals(Run(false), Run(true), StringComparison.Ordinal);
    }
}
