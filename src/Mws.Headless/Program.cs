using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Runtime;

var simulation = new DeterministicWorldSimulation(new WorldSeed(42));

for (var i = 0; i < 100; i++)
{
    simulation.Advance();
}

Console.WriteLine(WorldSnapshotJson.Serialize(simulation.Snapshot));
