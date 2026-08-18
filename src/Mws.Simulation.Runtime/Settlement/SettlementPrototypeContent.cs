using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

internal static class SettlementPrototypeContent
{
    internal const long EntityIdSpan = 1_024;

    private const long P3TravelResidentLocalId = 12;
    private const long P3TravelHomeLocalId = 206;
    private const long P3TravelWorkplaceLocalId = 103;
    private const long P3TravelConnectionId = 1;
    private const long P3TravelDistanceMeters = 300;
    private const string P3TravelCapabilityProvenance =
        "prototype-content:p3-karo-on-foot-capability-v1";
    private const string P3TravelLoadProvenance =
        "prototype-content:p3-karo-starting-load-v1";
    private const string P3TravelRouteProvenance =
        "prototype-content:p3-karo-grove-route-v1";

    internal static ResidentState[] CreateResidents(long entityIdOffset = 0)
    {
        var farmId = Entity(entityIdOffset, 101);
        var kitchenId = Entity(entityIdOffset, 102);
        var herbGroveId = Entity(entityIdOffset, 103);
        var northHouseholdId = Entity(entityIdOffset, 301);
        var eastHouseholdId = Entity(entityIdOffset, 302);
        var millerHouseholdId = Entity(entityIdOffset, 303);
        var cookHouseholdId = Entity(entityIdOffset, 304);
        var riverHouseholdId = Entity(entityIdOffset, 305);
        var groveHouseholdId = Entity(entityIdOffset, 306);

        return
        [
            Resident(entityIdOffset, 1, "Mira", 20, 100, ResidentProfession.Farmer, farmId, northHouseholdId),
            Resident(entityIdOffset, 2, "Tor", 25, 90, ResidentProfession.Cook, kitchenId, northHouseholdId),
            Resident(entityIdOffset, 3, "Ena", 30, 80, ResidentProfession.Forager, herbGroveId, eastHouseholdId),
            Resident(entityIdOffset, 4, "Ivo", 18, 92, ResidentProfession.Farmer, farmId, eastHouseholdId),
            Resident(entityIdOffset, 5, "Lysa", 36, 88, ResidentProfession.Cook, kitchenId, millerHouseholdId),
            Resident(entityIdOffset, 6, "Bran", 28, 84, ResidentProfession.Forager, herbGroveId, millerHouseholdId),
            Resident(entityIdOffset, 7, "Nera", 42, 95, ResidentProfession.Farmer, farmId, cookHouseholdId),
            Resident(entityIdOffset, 8, "Oren", 22, 78, ResidentProfession.Cook, kitchenId, cookHouseholdId),
            Resident(entityIdOffset, 9, "Sela", 34, 86, ResidentProfession.Forager, herbGroveId, riverHouseholdId),
            Resident(entityIdOffset, 10, "Dain", 26, 91, ResidentProfession.Farmer, farmId, riverHouseholdId),
            Resident(entityIdOffset, 11, "Veya", 38, 82, ResidentProfession.Cook, kitchenId, groveHouseholdId),
            Resident(
                entityIdOffset,
                P3TravelResidentLocalId,
                "Karo",
                24,
                89,
                ResidentProfession.Forager,
                herbGroveId,
                groveHouseholdId,
                SettlementOnFootActorCapabilityClass.BaselineCompatible,
                P3TravelCapabilityProvenance,
                SettlementOnFootCarriedLoadClass.NoMaterialLoad,
                P3TravelLoadProvenance),
        ];
    }

    internal static ItemStackState[] CreateItemStacks(long entityIdOffset = 0) =>
    [
        new ItemStackState(1, SettlementItems.Ration, GetSettlementOwnerId(entityIdOffset), 24),
        new ItemStackState(2, SettlementItems.Grain, GetSettlementOwnerId(entityIdOffset), 16),
    ];

    internal static WorkplaceState[] CreateWorkplaces(long entityIdOffset = 0) =>
    [
        new WorkplaceState(
            Entity(entityIdOffset, 101),
            "North Field",
            ResidentProfession.Farmer,
            null,
            0,
            SettlementItems.Grain,
            2),
        new WorkplaceState(
            Entity(entityIdOffset, 102),
            "Common Kitchen",
            ResidentProfession.Cook,
            SettlementItems.Grain,
            2,
            SettlementItems.Ration,
            1),
        new WorkplaceState(
            Entity(entityIdOffset, 103),
            "Herb Grove",
            ResidentProfession.Forager,
            null,
            0,
            SettlementItems.Herb,
            1),
    ];

    internal static HomeState[] CreateHomes(long entityIdOffset = 0) =>
    [
        Home(entityIdOffset, 201, "North Cottage", SettlementHomeSpatialKeys.NorthWest, 4),
        Home(entityIdOffset, 202, "East Cottage", SettlementHomeSpatialKeys.NorthEast, 4),
        Home(entityIdOffset, 203, "Miller Cottage", SettlementHomeSpatialKeys.Miller, 5),
        Home(entityIdOffset, 204, "Cook Cottage", SettlementHomeSpatialKeys.Cook, 4),
        Home(entityIdOffset, 205, "River Cottage", SettlementHomeSpatialKeys.River, 4),
        Home(entityIdOffset, P3TravelHomeLocalId, "Grove Cottage", SettlementHomeSpatialKeys.Grove, 4),
        Home(entityIdOffset, 207, "Southwest Cottage", SettlementHomeSpatialKeys.SouthWest, 4),
        Home(entityIdOffset, 208, "Southeast Cottage", SettlementHomeSpatialKeys.SouthEast, 4),
        Home(entityIdOffset, 209, "Far Southwest Cottage", SettlementHomeSpatialKeys.FarSouthWest, 4),
        Home(entityIdOffset, 210, "Far Southeast Cottage", SettlementHomeSpatialKeys.FarSouthEast, 4),
    ];

    internal static HouseholdState[] CreateHouseholds(long entityIdOffset = 0) =>
    [
        Household(entityIdOffset, 301, "North Household", 201),
        Household(entityIdOffset, 302, "East Household", 202),
        Household(entityIdOffset, 303, "Miller Household", 203),
        Household(entityIdOffset, 304, "Cook Household", 204),
        Household(entityIdOffset, 305, "River Household", 205),
        Household(entityIdOffset, 306, "Grove Household", P3TravelHomeLocalId),
    ];

    internal static SettlementRouteConnectionState[] CreateRouteConnections(long entityIdOffset = 0) =>
    [
        // Deliberately one route: enough to exercise the accepted P3 lifecycle without
        // pretending that the prototype already owns a complete village road network.
        new SettlementRouteConnectionState(
            P3TravelConnectionId,
            new SettlementPlaceRef(
                SettlementPlaceKind.Home,
                Entity(entityIdOffset, P3TravelHomeLocalId)),
            new SettlementPlaceRef(
                SettlementPlaceKind.Workplace,
                Entity(entityIdOffset, P3TravelWorkplaceLocalId)),
            P3TravelDistanceMeters,
            SettlementRoutePhysicalState.Passable,
            SettlementRoutePassageStatus.Open,
            P3TravelRouteProvenance,
            IsFixture: false,
            SupportedModes: [SettlementTravelMode.OnFoot],
            OnFootTimingClass: SettlementOnFootRouteTimingClass.BaselineLevelUnobstructed),
    ];

    internal static SettlementResidentRouteKnowledgeState[] CreateResidentRouteKnowledge(
        long entityIdOffset = 0) =>
    [
        new SettlementResidentRouteKnowledgeState(
            Entity(entityIdOffset, P3TravelResidentLocalId),
            [P3TravelConnectionId]),
    ];

    internal static EntityId GetSettlementOwnerId(long entityIdOffset = 0) =>
        Entity(entityIdOffset, 1_000);

    internal static void Validate(
        ResidentState[] residents,
        ItemStackState[] itemStacks,
        WorkplaceState[] workplaces,
        HomeState[] homes,
        HouseholdState[] households)
    {
        EnsureUnique(residents.Select(resident => resident.Id.Value), "resident");
        EnsureUnique(itemStacks.Select(stack => stack.StackId), "item stack");
        EnsureUnique(workplaces.Select(workplace => workplace.Id.Value), "workplace");
        EnsureUnique(homes.Select(home => home.Id.Value), "home");
        EnsureUnique(households.Select(household => household.Id.Value), "household");

        var entityIds = residents.Select(resident => resident.Id.Value)
            .Concat(workplaces.Select(workplace => workplace.Id.Value))
            .Concat(homes.Select(home => home.Id.Value))
            .Concat(households.Select(household => household.Id.Value))
            .ToArray();
        EnsureUnique(entityIds, "entity");

        if (itemStacks.Any(stack => stack.Quantity < 0)
            || homes.Any(home => string.IsNullOrWhiteSpace(home.SpatialKey) || home.Capacity <= 0))
        {
            throw new InvalidOperationException("Prototype content contains invalid inventory or housing data.");
        }

        foreach (var resident in residents)
        {
            if (workplaces.All(workplace =>
                workplace.Id != resident.WorkplaceId || workplace.Profession != resident.Profession))
            {
                throw new InvalidOperationException(
                    $"Resident {resident.Id.Value} has no matching prototype workplace.");
            }

            if (resident.HouseholdId.Value != 0
                && households.All(household => household.Id != resident.HouseholdId))
            {
                throw new InvalidOperationException(
                    $"Resident {resident.Id.Value} has no matching prototype household.");
            }
        }

        ValidateHousingCapacity(residents, homes, households);
    }

    private static void ValidateHousingCapacity(
        ResidentState[] residents,
        HomeState[] homes,
        HouseholdState[] households)
    {
        var homeIds = homes.Select(home => home.Id).ToHashSet();
        if (households.Any(household => !homeIds.Contains(household.HomeId))
            || households.GroupBy(household => household.HomeId).Any(group => group.Count() > 1))
        {
            throw new InvalidOperationException("Prototype households have invalid home assignments.");
        }

        foreach (var household in households)
        {
            var home = homes.Single(entry => entry.Id == household.HomeId);
            var residentCount = residents.Count(resident => resident.HouseholdId == household.Id);
            if (residentCount > home.Capacity)
            {
                throw new InvalidOperationException(
                    $"Household {household.Id.Value} exceeds home capacity.");
            }
        }
    }

    private static ResidentState Resident(
        long entityIdOffset,
        long localId,
        string name,
        int hunger,
        int energy,
        ResidentProfession profession,
        EntityId workplaceId,
        EntityId householdId,
        SettlementOnFootActorCapabilityClass onFootCapability =
            SettlementOnFootActorCapabilityClass.Unknown,
        string? onFootCapabilityProvenanceReference = null,
        SettlementOnFootCarriedLoadClass onFootCarriedLoad =
            SettlementOnFootCarriedLoadClass.Unknown,
        string? onFootCarriedLoadProvenanceReference = null) =>
        new(
            Entity(entityIdOffset, localId),
            name,
            hunger,
            energy,
            ResidentActivity.Resting,
            profession,
            workplaceId,
            0,
            householdId,
            Location: null,
            SelectedTask: null,
            OnFootCapability: onFootCapability,
            OnFootCapabilityProvenanceReference: onFootCapabilityProvenanceReference,
            IsOnFootCapabilityFixture: false,
            OnFootCarriedLoad: onFootCarriedLoad,
            OnFootCarriedLoadProvenanceReference: onFootCarriedLoadProvenanceReference,
            IsOnFootCarriedLoadFixture: false);

    private static HomeState Home(
        long entityIdOffset,
        long localId,
        string name,
        string spatialKey,
        int capacity) =>
        new(Entity(entityIdOffset, localId), name, spatialKey, capacity);

    private static HouseholdState Household(
        long entityIdOffset,
        long localId,
        string name,
        long homeLocalId) =>
        new(Entity(entityIdOffset, localId), name, Entity(entityIdOffset, homeLocalId));

    private static EntityId Entity(long entityIdOffset, long localId)
    {
        if (entityIdOffset < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(entityIdOffset),
                entityIdOffset,
                "Entity ID offset cannot be negative.");
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
