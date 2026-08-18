using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

internal sealed record SettlementAdvanceRollbackState(
    SimulationTime Time,
    long NextEventId,
    long NextStackId,
    ResidentAdvanceRollbackState[] Residents,
    int[] ItemQuantities,
    SettlementEvent[] Events);

internal readonly record struct ResidentAdvanceRollbackState(
    int Hunger,
    int Energy,
    ResidentActivity Activity,
    SettlementActorLocationState Location);

public sealed partial class SettlementSimulation
{
    internal SettlementAdvanceRollbackState CaptureAdvanceRollbackState()
    {
        var residents = new ResidentAdvanceRollbackState[_residents.Length];
        for (var index = 0; index < _residents.Length; index++)
        {
            var resident = _residents[index];
            residents[index] = new ResidentAdvanceRollbackState(
                resident.Hunger,
                resident.Energy,
                resident.Activity,
                resident.Location);
        }

        var itemQuantities = new int[_itemStacks.Count];
        for (var index = 0; index < _itemStacks.Count; index++)
        {
            itemQuantities[index] = _itemStacks[index].Quantity;
        }

        return new SettlementAdvanceRollbackState(
            Time,
            _nextEventId,
            _nextStackId,
            residents,
            itemQuantities,
            _events.ToArray());
    }

    internal void RestoreAdvanceRollbackState(SettlementAdvanceRollbackState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (state.Residents.Length != _residents.Length
            || state.ItemQuantities.Length > _itemStacks.Count)
        {
            throw new InvalidOperationException("Settlement advance rollback state no longer matches runtime structure.");
        }

        Time = state.Time;
        _nextEventId = state.NextEventId;
        _nextStackId = state.NextStackId;

        for (var index = 0; index < _residents.Length; index++)
        {
            var source = state.Residents[index];
            var resident = _residents[index];
            resident.Hunger = source.Hunger;
            resident.Energy = source.Energy;
            resident.Activity = source.Activity;
            resident.Location = source.Location;
        }

        if (_itemStacks.Count > state.ItemQuantities.Length)
        {
            _itemStacks.RemoveRange(
                state.ItemQuantities.Length,
                _itemStacks.Count - state.ItemQuantities.Length);
        }

        for (var index = 0; index < state.ItemQuantities.Length; index++)
        {
            var stack = _itemStacks[index];
            _itemStacks[index] = stack with { Quantity = state.ItemQuantities[index] };
        }

        _events.Clear();
        _events.AddRange(state.Events);
        RebuildInventoryIndexes();
    }
}
