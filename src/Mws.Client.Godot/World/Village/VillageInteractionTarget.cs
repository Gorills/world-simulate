using Mws.Domain;

namespace Mws.Client.Godot.World.Village;

internal enum VillageInteractionKind
{
    Resident,
    ItemStack,
    BuildingEntrance,
}

internal sealed record VillageInteractionTarget(
    VillageInteractionKind Kind,
    string DisplayName,
    EntityId? ResidentId = null,
    long? StackId = null,
    string? ItemId = null,
    int Quantity = 0)
{
    internal static VillageInteractionTarget ForResident(EntityId residentId, string displayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        return new VillageInteractionTarget(
            VillageInteractionKind.Resident,
            displayName,
            ResidentId: residentId);
    }

    internal static VillageInteractionTarget ForItem(long stackId, string itemId, int quantity)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(itemId);
        if (stackId <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(stackId), stackId, "Item stack ID must be positive.");
        }

        if (quantity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Item quantity cannot be negative.");
        }

        return new VillageInteractionTarget(
            VillageInteractionKind.ItemStack,
            itemId,
            StackId: stackId,
            ItemId: itemId,
            Quantity: quantity);
    }

    internal static VillageInteractionTarget ForEntrance(string buildingName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(buildingName);
        return new VillageInteractionTarget(
            VillageInteractionKind.BuildingEntrance,
            buildingName);
    }
}
