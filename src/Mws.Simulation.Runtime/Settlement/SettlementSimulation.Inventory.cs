using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class SettlementSimulation
{
    private readonly Dictionary<long, List<int>> _itemStackIndicesByOwner = new();
    private readonly Dictionary<InventoryKey, List<int>> _itemStackIndicesByOwnerAndItem = new();

    private ItemStackProjection[] ProjectInventory(EntityId ownerId)
    {
        if (!_itemStackIndicesByOwner.TryGetValue(ownerId.Value, out var indices))
        {
            return [];
        }

        return indices
            .Select(index => _itemStacks[index])
            .Where(stack => stack.Quantity > 0)
            .OrderBy(stack => stack.StackId)
            .Select(stack => new ItemStackProjection(stack.StackId, stack.ItemId, stack.Quantity))
            .ToArray();
    }

    private int ItemQuantity(EntityId ownerId, string itemId)
    {
        var key = new InventoryKey(ownerId.Value, itemId);
        if (!_itemStackIndicesByOwnerAndItem.TryGetValue(key, out var indices))
        {
            return 0;
        }

        var total = 0;
        foreach (var index in indices)
        {
            total = checked(total + _itemStacks[index].Quantity);
        }

        return total;
    }

    private bool CanAddItem(EntityId ownerId, string itemId, int quantity)
    {
        if (quantity <= 0)
        {
            return false;
        }

        var current = ItemQuantity(ownerId, itemId);
        if (current > int.MaxValue - quantity)
        {
            return false;
        }

        var key = new InventoryKey(ownerId.Value, itemId);
        return _itemStackIndicesByOwnerAndItem.ContainsKey(key)
            || (_nextStackId > 0 && _nextStackId < long.MaxValue);
    }

    private void AddItem(EntityId ownerId, string itemId, int quantity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId);
        if (!CanAddItem(ownerId, itemId, quantity))
        {
            throw new InvalidOperationException("Item addition exceeds inventory or stack-ID capacity.");
        }

        var key = new InventoryKey(ownerId.Value, itemId);
        if (_itemStackIndicesByOwnerAndItem.TryGetValue(key, out var indices))
        {
            var remaining = quantity;
            foreach (var index in indices)
            {
                var stack = _itemStacks[index];
                var capacity = int.MaxValue - stack.Quantity;
                if (capacity == 0)
                {
                    continue;
                }

                var added = Math.Min(capacity, remaining);
                _itemStacks[index] = stack with { Quantity = stack.Quantity + added };
                remaining -= added;
                if (remaining == 0)
                {
                    return;
                }
            }

            quantity = remaining;
        }

        var newIndex = _itemStacks.Count;
        _itemStacks.Add(new ItemStackState(_nextStackId, itemId, ownerId, quantity));
        _nextStackId = checked(_nextStackId + 1);
        IndexStack(newIndex);
    }

    private ItemTransferStatus TryTransferItem(
        EntityId sourceOwnerId,
        EntityId destinationOwnerId,
        string itemId,
        int quantity)
    {
        if (quantity <= 0 || ItemQuantity(sourceOwnerId, itemId) < quantity)
        {
            return ItemTransferStatus.SourceUnavailable;
        }

        if (!CanAddItem(destinationOwnerId, itemId, quantity))
        {
            return ItemTransferStatus.DestinationCapacityExceeded;
        }

        if (!TryConsumeItem(sourceOwnerId, itemId, quantity))
        {
            throw new InvalidOperationException("Inventory reservation changed during an atomic transfer.");
        }

        AddItem(destinationOwnerId, itemId, quantity);
        return ItemTransferStatus.Success;
    }

    private bool TryConsumeItem(EntityId ownerId, string itemId, int quantity)
    {
        if (quantity <= 0 || ItemQuantity(ownerId, itemId) < quantity)
        {
            return false;
        }

        var key = new InventoryKey(ownerId.Value, itemId);
        var indices = _itemStackIndicesByOwnerAndItem[key];
        var remaining = quantity;
        foreach (var index in indices)
        {
            if (remaining == 0)
            {
                break;
            }

            var stack = _itemStacks[index];
            var consumed = Math.Min(stack.Quantity, remaining);
            _itemStacks[index] = stack with { Quantity = stack.Quantity - consumed };
            remaining -= consumed;
        }

        if (remaining != 0)
        {
            throw new InvalidOperationException("Indexed inventory quantity disagrees with item stacks.");
        }

        return true;
    }

    private void RebuildInventoryIndexes()
    {
        _itemStackIndicesByOwner.Clear();
        _itemStackIndicesByOwnerAndItem.Clear();

        for (var index = 0; index < _itemStacks.Count; index++)
        {
            IndexStack(index);
        }
    }

    private void IndexStack(int index)
    {
        var stack = _itemStacks[index];
        if (!_itemStackIndicesByOwner.TryGetValue(stack.OwnerId.Value, out var ownerIndices))
        {
            ownerIndices = [];
            _itemStackIndicesByOwner.Add(stack.OwnerId.Value, ownerIndices);
        }

        ownerIndices.Add(index);

        var key = new InventoryKey(stack.OwnerId.Value, stack.ItemId);
        if (!_itemStackIndicesByOwnerAndItem.TryGetValue(key, out var itemIndices))
        {
            itemIndices = [];
            _itemStackIndicesByOwnerAndItem.Add(key, itemIndices);
        }

        itemIndices.Add(index);
    }

    private readonly record struct InventoryKey(long OwnerId, string ItemId);

    private enum ItemTransferStatus
    {
        Success,
        SourceUnavailable,
        DestinationCapacityExceeded,
    }
}
