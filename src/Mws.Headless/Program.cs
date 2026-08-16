using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Runtime;

var seed = args.Length > 0 && ulong.TryParse(args[0], out var parsedSeed)
    ? parsedSeed
    : 42UL;
var steps = args.Length > 1 && int.TryParse(args[1], out var parsedSteps) && parsedSteps >= 0
    ? parsedSteps
    : 100;

var simulation = new DeterministicWorldSimulation(new WorldSeed(seed));

for (var i = 0; i < steps; i++)
{
    simulation.Advance();
}

var json = WorldSnapshotJson.Serialize(simulation.Snapshot);
Console.WriteLine(json);
Console.WriteLine($"MWS_HEADLESS_OK seed={seed} steps={steps} tick={simulation.Snapshot.Tick.Value} state={simulation.Snapshot.DeterministicState}");
