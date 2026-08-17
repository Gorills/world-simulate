using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class WorldRuntime
{
    public bool IsSettlementLoaded(SimulationScopeId scopeId) =>
        GetPartition(scopeId).IsLoaded;

    public void UnloadSettlement(SimulationScopeId scopeId)
    {
        EnsureInputJournalCapacity(1);
        var recordedAt = Time;
        UnloadSettlementCore(scopeId);
        RecordInput(CreateInput(
            recordedAt,
            WorldInputKind.UnloadSettlement,
            unloadSettlement: new WorldPartitionResidencyInput(scopeId)));
    }

    public void LoadSettlement(SimulationScopeId scopeId)
    {
        EnsureInputJournalCapacity(1);
        var recordedAt = Time;
        LoadSettlementCore(scopeId);
        RecordInput(CreateInput(
            recordedAt,
            WorldInputKind.LoadSettlement,
            loadSettlement: new WorldPartitionResidencyInput(scopeId)));
    }

    private void UnloadSettlementCore(SimulationScopeId scopeId)
    {
        var partition = GetPartition(scopeId);
        if (!partition.IsLoaded)
        {
            throw new InvalidOperationException($"Settlement scope {scopeId.Value} is already unloaded.");
        }

        partition.Unload(partition.Simulation.CaptureState());
    }

    private void LoadSettlementCore(SimulationScopeId scopeId)
    {
        var partition = GetPartition(scopeId);
        if (partition.IsLoaded)
        {
            throw new InvalidOperationException($"Settlement scope {scopeId.Value} is already loaded.");
        }

        var dormant = partition.DormantState
            ?? throw new InvalidOperationException("Unloaded world partition has no dormant settlement state.");
        var simulation = SettlementSimulation.Restore(dormant);
        if (simulation.Time != Time)
        {
            simulation.AdvanceTo(Time);
        }

        partition.Load(simulation, EffectiveRevision(partition));
    }

    private SettlementState CapturePartitionStateAtCurrentTime(WorldPartitionRuntime partition)
    {
        if (partition.IsLoaded)
        {
            return partition.Simulation.CaptureState();
        }

        var dormant = partition.DormantState
            ?? throw new InvalidOperationException("Unloaded world partition has no dormant settlement state.");
        if (dormant.Time == Time)
        {
            return dormant;
        }

        var simulation = SettlementSimulation.Restore(dormant);
        simulation.AdvanceTo(Time);
        return simulation.CaptureState();
    }

    private static long EffectiveRevision(WorldPartitionRuntime partition)
    {
        if (partition.DeferredAdvanceCount < 0
            || partition.Revision > long.MaxValue - partition.DeferredAdvanceCount)
        {
            throw new InvalidOperationException("World partition revision space is exhausted.");
        }

        return partition.Revision + partition.DeferredAdvanceCount;
    }
}
