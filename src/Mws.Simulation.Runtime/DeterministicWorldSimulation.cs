using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed class DeterministicWorldSimulation : IWorldSimulation
{
    private WorldTick _tick;
    private ulong _state;

    public DeterministicWorldSimulation(WorldSeed seed)
    {
        _state = seed.Value;
        Snapshot = new WorldSnapshot(_tick, _state);
    }

    public WorldSnapshot Snapshot { get; private set; }

    public WorldSnapshot Step()
    {
        _tick = _tick.Next();
        _state = unchecked((_state * 6364136223846793005UL) + 1442695040888963407UL + (ulong)_tick.Value);
        Snapshot = new WorldSnapshot(_tick, _state);
        return Snapshot;
    }
}
