using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class WorldRuntime
{
    public void AdvanceHours(long hours)
    {
        if (hours < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(hours), hours, "Hours cannot be negative.");
        }

        AdvanceTo(Time.AddMilliseconds(checked(hours * SettlementSimulation.HourMilliseconds)));
    }

    public void AdvanceTo(SimulationTime target)
    {
        ValidateAdvanceTarget(target);
        if (target == Time)
        {
            return;
        }

        EnsureInputJournalCapacity(1);
        var recordedAt = Time;
        AdvanceToCore(target);
        RecordInput(CreateInput(
            recordedAt,
            WorldInputKind.AdvanceTo,
            advanceTo: new WorldAdvanceToInput(target)));
    }

    private void AdvanceToCore(SimulationTime target)
    {
        ValidateAdvanceTarget(target);
        if (target == Time)
        {
            return;
        }

        var stagedPlayer = StagePlayerAdvance(target);
        var rollbackStates = new List<(
            WorldPartitionRuntime Partition,
            SettlementAdvanceRollbackState State)>(_partitions.Count);
        try
        {
            foreach (var partition in _partitions.Values)
            {
                if (EffectiveRevision(partition) == long.MaxValue)
                {
                    throw new InvalidOperationException("World partition revision space is exhausted.");
                }

                if (!partition.IsLoaded)
                {
                    continue;
                }

                var simulation = partition.Simulation;
                rollbackStates.Add((partition, simulation.CaptureAdvanceRollbackState()));
                simulation.AdvanceTo(target);
            }
        }
        catch
        {
            for (var index = rollbackStates.Count - 1; index >= 0; index--)
            {
                var entry = rollbackStates[index];
                entry.Partition.Simulation.RestoreAdvanceRollbackState(entry.State);
            }

            throw;
        }

        foreach (var entry in rollbackStates)
        {
            entry.Partition.Revision = checked(entry.Partition.Revision + 1);
        }

        foreach (var partition in _partitions.Values.Where(entry => !entry.IsLoaded))
        {
            partition.DeferredAdvanceCount = checked(partition.DeferredAdvanceCount + 1);
        }

        _player = stagedPlayer;
        Time = target;
    }

    private void ValidateAdvanceTarget(SimulationTime target)
    {
        if (target.Milliseconds < Time.Milliseconds)
        {
            throw new InvalidOperationException("World simulation time is monotonic.");
        }
    }
}
