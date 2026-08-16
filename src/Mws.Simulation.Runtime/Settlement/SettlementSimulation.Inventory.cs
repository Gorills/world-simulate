using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class SettlementSimulation
{
    private ItemStackProjection[] ProjectInventory(EntityId ownerId) =>
        _itemStacks
            .Where(stack => stack.OwnerId == ownerId && stack.Quantity > 0)
            .OrderBy(stack => stack.StackId)
            .Select(stack => new ItemStackProjection(stack.StackId, stack.ItemId, stack.Quantity))
            .ToArray();

    private int ItemQuantity(EntityId ownerId, string itemId) =>
        _itemStacks
            .Where(stack =>
                stack.OwnerId == ownerId
                && string.Equals(stack.ItemId, itemId, StringComparison.Ordinal))
            .Sum(stack => stack.Quantity);

    private void AddItem(EntityId ownerId, string itemId, int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Quantity must be positive.");
        }

        var index = _itemStacks.FindIndex(stack =>
            stack.OwnerId == ownerId && string.Equals(stack.ItemId, itemId, StringComparison.Ordinal));
        if (index >= 0)
        {
            var stack = _itemStacks[index];
            _itemStacks[index] = stack with { Quantity = checked(stack.Quantity + quantity) };
            return;
        }

        _itemStacks.Add(new ItemStackState(_nextStackId, itemId, ownerId, quantity));
        _nextStackId = checked(_nextStackId + 1);
    }

    private bool TryTransferItem(
        EntityId sourceOwnerId,
        EntityId destinationOwnerId,
        string itemId,
        int quantity)
    {
        if (!TryConsumeItem(sourceOwnerId, itemId, quantity))
        {
            return false;
        }

        AddItem(destinationOwnerId, itemId, quantity);
        return true;
    }

    private bool TryConsumeItem(EntityId ownerId, string itemId, int quantity)
    {
        if (quantity <= 0 || ItemQuantity(ownerId, itemId) < quantity)
        {
            return false;
        }

        var remaining = quantity;
        for (var index = 0; index < _itemStacks.Count && remaining > 0; index++)
        {
            var stack = _itemStacks[index];
            if (stack.OwnerId != ownerId
                || !string.Equals(stack.ItemId, itemId, StringComparison.Ordinal)
                || stack.Quantity == 0)
            {
                continue;
            }

            var consumed = Math.Min(stack.Quantity, remaining);
            _itemStacks[index] = stack with { Quantity = stack.Quantity - consumed };
            remaining -= consumed;
        }

        return remaining == 0;
    }
}
