namespace Mws.Simulation.Runtime;

public sealed partial class SettlementSimulation
{
    private void ApplySettlementInventoryDelta(
        IReadOnlyDictionary<string, int> consumed,
        IReadOnlyDictionary<string, int> produced)
    {
        ValidateSettlementInventoryDelta(consumed, produced);

        foreach (var entry in consumed.OrderBy(entry => entry.Key, StringComparer.Ordinal))
        {
            if (entry.Value > 0 && !TryConsumeItem(_settlementOwnerId, entry.Key, entry.Value))
            {
                throw new InvalidOperationException("Validated settlement inventory consumption could not be committed.");
            }
        }

        foreach (var entry in produced.OrderBy(entry => entry.Key, StringComparer.Ordinal))
        {
            if (entry.Value > 0)
            {
                AddItem(_settlementOwnerId, entry.Key, entry.Value);
            }
        }
    }

    private void ValidateSettlementInventoryDelta(
        IReadOnlyDictionary<string, int> consumed,
        IReadOnlyDictionary<string, int> produced)
    {
        var itemIds = consumed.Keys
            .Concat(produced.Keys)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        var newStackCount = 0;

        foreach (var itemId in itemIds)
        {
            consumed.TryGetValue(itemId, out var consumedQuantity);
            produced.TryGetValue(itemId, out var producedQuantity);
            if (consumedQuantity < 0 || producedQuantity < 0)
            {
                throw new InvalidOperationException("Settlement inventory delta cannot contain negative quantities.");
            }

            var current = ItemQuantity(_settlementOwnerId, itemId);
            var final = (long)current - consumedQuantity + producedQuantity;
            if (consumedQuantity > current || final is < 0 or > int.MaxValue)
            {
                throw new InvalidOperationException($"Settlement inventory delta for '{itemId}' is not commit-safe.");
            }

            var key = new InventoryKey(_settlementOwnerId.Value, itemId);
            if (producedQuantity > 0 && !_itemStackIndicesByOwnerAndItem.ContainsKey(key))
            {
                newStackCount++;
            }
        }

        if (newStackCount > 0
            && (_nextStackId <= 0 || _nextStackId > long.MaxValue - newStackCount))
        {
            throw new InvalidOperationException("Settlement item stack ID space cannot commit the hourly plan.");
        }
    }
}
