using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

internal static class SettlementPrototypeContent
{
    internal const long EntityIdSpan = 1_024;

    internal static ResidentState[] CreateResidents(long entityIdOffset = 0)
    {
        var farmId = Entity(entityIdOffset, 101);
        var kitchenId = Entity(entityIdOffset, 102);
        var groveId = Entity(entityIdOffset, 103);

        return
        [
            new ResidentState(Entity(entityIdOffset, 1), "Mira", 20, 100, ResidentActivity.Idle, ResidentProfession.Farmer, farmId, 0),
            new ResidentState(Entity(entityIdOffset, 2), "Tor", 25, 90, ResidentActivity.Idle, ResidentProfession.Cook, kitchenId, 0),
            new ResidentState(Entity(entityIdOffset, 3), "Ena", 30, 80, ResidentActivity.Idle, ResidentProfession.Forager, groveId, 0),
        ];
    }

    internal static ItemStackState[] CreateItemStacks(long entityIdOffset = 0) =>
    [
        new ItemStackState(1, SettlementItems.Ration, GetSettlementOwnerId(entityIdOffset), 6),
        new ItemStackState(2, SettlementItems.Grain, GetSettlementOwnerId(entityIdOffset), 4),
    ];

    internal static WorkplaceState[] CreateWorkplaces(long entityIdOffset = 0) =>
    [
        new WorkplaceState(Entity(entityIdOffset, 101), "North Field", ResidentProfession.Farmer, null, 0, SettlementItems.Grain, 2),
        new WorkplaceState(Entity(entityIdOffset, 102), "Common Kitchen", ResidentProfession.Cook, SettlementItems.Grain, 2, SettlementItems.Ration, 1),
        new WorkplaceState(Entity(entityIdOffset, 103), "Herb Grove", ResidentProfession.Forager, null, 0, SettlementItems.Herb, 1),
    ];

    internal static EntityId GetSettlementOwnerId(long entityIdOffset = 0) => Entity(entityIdOffset, 1_000);

    internal static void Validate(
        IReadOnlyCollection<ResidentState> residents,
        IReadOnlyCollection<ItemStackState> itemStacks,
        IReadOnlyCollection<WorkplaceState> workplaces)
    {
        EnsureUnique(residents.Select(resident => resident.Id.Value), "resident");
        EnsureUnique(itemStacks.Select(stack => stack.StackId), "item stack");
        EnsureUnique(workplaces.Select(workplace => workplace.Id.Value), "workplace");

        if (itemStacks.Any(stack => stack.Quantity < 0))
        {
            throw new InvalidOperationException("Prototype content contains a negative item quantity.");
        }

        foreach (var resident in residents)
        {
            if (workplaces.All(workplace =>
                workplace.Id != resident.WorkplaceId || workplace.Profession != resident.Profession))
            {
                throw new InvalidOperationException(
                    $"Resident {resident.Id.Value} has no matching prototype workplace.");
            }
        }
    }

    private static EntityId Entity(long entityIdOffset, long localId)
    {
        if (entityIdOffset < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(entityIdOffset), entityIdOffset, "Entity ID offset cannot be negative.");
        }

        return new EntityId(checked(entityIdOffset + localId));
    }

    private static void EnsureUnique(IEnumerable<long> values, string kind)
    {
        var ids = values.ToArray();
        if (ids.Distinct().Count() != ids.Length)
        {
            throw new InvalidOperationException($"Prototype content contains duplicate {kind} IDs.");
        }
    }
}
