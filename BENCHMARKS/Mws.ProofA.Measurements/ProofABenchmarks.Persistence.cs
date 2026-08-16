using System.Diagnostics;
using System.Text;
using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Runtime;

internal static partial class ProofABenchmarks
{
    public static BenchmarkResult RunSaveReplay(int iterations, int repetitions)
    {
        var saves = new List<double>(repetitions);
        var loads = new List<double>(repetitions);
        var sizes = new List<long>(repetitions);
        var replayMismatches = 0;
        long peakMemory = 0;

        for (var repetition = 0; repetition < repetitions; repetition++)
        {
            var kernel = new ProofAKernel(new WorldSeed(43));
            var owner = kernel.CreateEntity();
            var subject = kernel.CreateEntity(owner, initialResource: iterations + 100L);
            _ = kernel.StartLongProcess(new CommandId(1), owner, subject, 10_000, 5);
            for (var index = 0; index < iterations; index++)
            {
                _ = kernel.AdjustResource(new CommandId(index + 2), owner, subject, -1);
            }

            var saveWatch = Stopwatch.StartNew();
            var json = ProofAKernelJson.Serialize(kernel.CaptureState());
            saveWatch.Stop();
            var loadWatch = Stopwatch.StartNew();
            var loaded = ProofAKernelJson.Deserialize(json);
            var left = ProofAKernel.Restore(loaded.State);
            var right = ProofAKernel.Restore(loaded.State);
            loadWatch.Stop();
            left.AdvanceTo(new SimulationTime(10_000));
            right.AdvanceTo(new SimulationTime(10_000));
            if (!string.Equals(
                    ProofAKernelJson.Serialize(left.CaptureState()),
                    ProofAKernelJson.Serialize(right.CaptureState()),
                    StringComparison.Ordinal))
            {
                replayMismatches++;
            }
            saves.Add(saveWatch.Elapsed.TotalMilliseconds);
            loads.Add(loadWatch.Elapsed.TotalMilliseconds);
            sizes.Add(Encoding.UTF8.GetByteCount(json));
            using var process = Process.GetCurrentProcess();
            process.Refresh();
            peakMemory = Math.Max(peakMemory, process.PeakWorkingSet64);
        }

        return BenchmarkSupport.Create(
            "RW-B_SAVE_REPLAY",
            iterations,
            repetitions,
            new Dictionary<string, object>
            {
                ["save_bytes"] = sizes.Average(),
                ["save_latency"] = saves.Average(),
                ["load_latency"] = loads.Average(),
                ["replay_mismatch_count"] = replayMismatches,
                ["peak_memory"] = peakMemory,
            },
            "Snapshot includes pending process and command/RNG state; identical post-load antecedents must replay to identical serialized state.");
    }
}
