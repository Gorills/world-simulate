using Mws.Domain;

namespace Mws.Simulation.Api;

public interface IWorldSimulation
{
    WorldSnapshot Snapshot { get; }

    WorldSnapshot Advance();
}

public readonly record struct WorldSnapshot(
    WorldTick Tick,
    ulong DeterministicState);
