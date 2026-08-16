using System.Diagnostics;
using Mws.Domain;
using Mws.Simulation.Runtime;

var steps = args.Length > 0 && int.TryParse(args[0], out var parsed) ? parsed : 1_000_000;
var simulation = new DeterministicWorldSimulation(new WorldSeed(42));

for (var i = 0; i < 10_000; i++)
{
    simulation.Step();
}

var beforeAllocated = GC.GetAllocatedBytesForCurrentThread();
var stopwatch = Stopwatch.StartNew();

for (var i = 0; i < steps; i++)
{
    simulation.Step();
}

stopwatch.Stop();
var allocated = GC.GetAllocatedBytesForCurrentThread() - beforeAllocated;

Console.WriteLine($"MWS_PROOF_A_BENCH steps={steps} elapsed_ms={stopwatch.Elapsed.TotalMilliseconds:F3} allocated_bytes={allocated} final_tick={simulation.Snapshot.Tick.Value}");
