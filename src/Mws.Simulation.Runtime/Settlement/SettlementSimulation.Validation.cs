namespace Mws.Simulation.Runtime;

public sealed partial class SettlementSimulation
{
    private void ValidateState()
    {
        EnsureUnique(_residents.Select(resident => resident.Id.Value), "resident");
        EnsureUnique(_itemStacks.Select(stack => stack.StackId), "item stack");
        EnsureUnique(_workplaces.Select(workplace => workplace.Id.Value), "workplace");
        EnsureUnique(_events.Select(entry => entry.Id), "event");
        EnsureUnique(_commandReceipts.Select(entry => entry.CommandId.Value), "command receipt");

        if (_scopeId.Value == 0 || Time.Milliseconds < 0)
        {
            throw new InvalidOperationException(
                "Settlement scope must be non-zero and time must be non-negative.");
        }

        if (_settlementOwnerId.Value <= 0
            || _residents.Any(resident => resident.Id.Value <= 0)
            || _workplaces.Any(workplace => workplace.Id.Value <= 0)
            || _itemStacks.Any(stack => stack.StackId <= 0 || stack.OwnerId.Value <= 0)
            || _events.Any(entry => entry.Id <= 0)
            || _commandReceipts.Any(entry => entry.CommandId.Value <= 0))
        {
            throw new InvalidOperationException("Settlement persisted identifiers must be positive.");
        }

        if (_residents.Any(resident =>
            string.IsNullOrWhiteSpace(resident.Name)
            || resident.Hunger is < 0 or > 100
            || resident.Energy is < 0 or > 100))
        {
            throw new InvalidOperationException("Settlement state contains an invalid resident.");
        }

        if (_itemStacks.Any(stack => string.IsNullOrWhiteSpace(stack.ItemId) || stack.Quantity < 0))
        {
            throw new InvalidOperationException("Settlement state contains an invalid item stack.");
        }

        foreach (var workplace in _workplaces)
        {
            if (string.IsNullOrWhiteSpace(workplace.Name)
                || string.IsNullOrWhiteSpace(workplace.OutputItemId)
                || workplace.OutputQuantity <= 0
                || (workplace.InputItemId is null && workplace.InputQuantity != 0)
                || (workplace.InputItemId is not null
                    && (string.IsNullOrWhiteSpace(workplace.InputItemId) || workplace.InputQuantity <= 0)))
            {
                throw new InvalidOperationException(
                    $"Settlement workplace {workplace.Id.Value} is invalid.");
            }
        }

        foreach (var resident in _residents)
        {
            if (resident.WorkplaceId.Value == 0)
            {
                continue;
            }

            if (_workplaces.All(workplace =>
                workplace.Id != resident.WorkplaceId || workplace.Profession != resident.Profession))
            {
                throw new InvalidOperationException(
                    $"Resident {resident.Id.Value} references a missing or incompatible workplace.");
            }
        }

        if (_nextEventId <= _events.Select(entry => entry.Id).DefaultIfEmpty(0).Max()
            || _nextStackId <= _itemStacks.Select(stack => stack.StackId).DefaultIfEmpty(0).Max()
            || _nextCommandId <= _commandReceipts.Select(entry => entry.CommandId.Value).DefaultIfEmpty(0).Max())
        {
            throw new InvalidOperationException(
                "Settlement next-ID markers must be greater than persisted IDs.");
        }
    }

    private static void EnsureVersion(string actual, string expected, string kind)
    {
        if (!string.Equals(actual, expected, StringComparison.Ordinal))
        {
            throw new NotSupportedException(
                $"Settlement {kind} version '{actual}' is unsupported; expected '{expected}'.");
        }
    }

    private static void EnsureUnique(IEnumerable<long> values, string kind)
    {
        var ids = values.ToArray();
        if (ids.Distinct().Count() != ids.Length)
        {
            throw new InvalidOperationException(
                $"Settlement state contains duplicate {kind} IDs.");
        }
    }
}
