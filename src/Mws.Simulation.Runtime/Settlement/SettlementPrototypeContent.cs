using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

internal static class SettlementPrototypeContent
{
    internal static readonly EntityId SettlementOwnerId = new(1_000);

    internal static ResidentState[] CreateResidents()
    {
        var farmId = new EntityId(101);
        var kitchenId = new EntityId(102);
        var groveId = new EntityId(103);

        return
        [
            new ResidentState(new EntityId(1), "Mira", 20, 100, ResidentActivity.Idle, ResidentProfession.Farmer, farmId, 0),
            new ResidentState(new EntityId(2), "Tor", 25, 90, ResidentActivity.Idle, ResidentProfession.Cook, kitchenId, 0),
            new ResidentState(new EntityId(3), "Ena", 30, 80, ResidentActivity.Idle, ResidentProfession.Forager, groveId, 0),
        ];
    }

    internal static ItemStackState[] CreateItemStacks() =>
    [
        new ItemStackState(1, SettlementItems.Ration, SettlementOwnerId, 6),
        new ItemStackState(2, SettlementItems.Grain, SettlementOwnerId, 4),
    ];

    internal static WorkplaceState[] CreateWorkplaces() =>
    [
        new WorkplaceState(new EntityId(101), "North Field", ResidentProfession.Farmer, null, 0, SettlementItems.Grain, 2),
        new WorkplaceState(new EntityId(102), "Common Kitchen", ResidentProfession.Cook, SettlementItems.Grain, 2, SettlementItems.Ration, 1),
        new WorkplaceState(new EntityId(103), "Herb Grove", ResidentProfession.Forager, null, 0, SettlementItems.Herb, 1),
    ];

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

    private static void EnsureUnique(IEnumerable<long> values, string kind)
    {
        var ids = values.ToArray();
        if (ids.Distinct().Count() != ids.Length)
        {
            throw new InvalidOperationException($"Prototype content contains duplicate {kind} IDs.");
        }
    }
}
