using Godot;
using Mws.Simulation.Api;

namespace Mws.Client.Godot.World.Village;

internal enum VillageResidentDestinationKind
{
    Home,
    Work,
    Food,
    Social,
}

internal readonly record struct VillageResidentDestination(
    VillageResidentDestinationKind Kind,
    Vector3 Position,
    Vector3? AccessPoint);

internal static class VillageResidentSchedule
{
    internal static VillageResidentDestination Resolve(
        ResidentProjection resident,
        SettlementProjection settlement)
    {
        ArgumentNullException.ThrowIfNull(resident);
        ArgumentNullException.ThrowIfNull(settlement);

        return resident.Activity switch
        {
            ResidentActivity.Working => ResolveWork(resident, settlement),
            ResidentActivity.Resting => ResolveHome(resident, settlement),
            ResidentActivity.Eating => ResolveFood(),
            _ => ResolveSocial(resident, settlement.Day, settlement.Hour),
        };
    }

    internal static void ValidateProjection(SettlementProjection settlement)
    {
        ArgumentNullException.ThrowIfNull(settlement);
        var workplaceIds = settlement.Workplaces.Select(workplace => workplace.Id).ToHashSet();
        var homes = settlement.Homes ?? [];
        var households = settlement.Households ?? [];
        var homeIds = homes.Select(home => home.Id).ToHashSet();
        var householdIds = households.Select(household => household.Id).ToHashSet();

        foreach (var resident in settlement.Residents)
        {
            if (resident.WorkplaceId != default && !workplaceIds.Contains(resident.WorkplaceId))
            {
                throw new InvalidOperationException(
                    $"Resident {resident.Id.Value} has an unknown projected workplace assignment.");
            }

            if (resident.HouseholdId != default && !householdIds.Contains(resident.HouseholdId))
            {
                throw new InvalidOperationException(
                    $"Resident {resident.Id.Value} has an unknown projected household assignment.");
            }

            if (resident.HomeId != default && !homeIds.Contains(resident.HomeId))
            {
                throw new InvalidOperationException(
                    $"Resident {resident.Id.Value} has an unknown projected home assignment.");
            }

            if (resident.HouseholdId != default)
            {
                var household = households.Single(entry => entry.Id == resident.HouseholdId);
                if (resident.HomeId == default || household.HomeId != resident.HomeId)
                {
                    throw new InvalidOperationException(
                        $"Resident {resident.Id.Value} household and home projection disagree.");
                }
            }

            _ = Resolve(resident, settlement);
        }
    }

    private static VillageResidentDestination ResolveHome(
        ResidentProjection resident,
        SettlementProjection settlement)
    {
        if (resident.HomeId == default)
        {
            return ResolveSocial(resident, settlement.Day, settlement.Hour);
        }

        var home = (settlement.Homes ?? []).FirstOrDefault(entry => entry.Id == resident.HomeId);
        if (home is null)
        {
            return ResolveSocial(resident, settlement.Day, settlement.Hour);
        }

        var building = VillageLayout.GetHomeBuilding(home.SpatialKey);
        return new VillageResidentDestination(
            VillageResidentDestinationKind.Home,
            building.Position,
            VillageLayout.GetEntranceWorldPosition(building));
    }

    private static VillageResidentDestination ResolveWork(
        ResidentProjection resident,
        SettlementProjection settlement)
    {
        var workplace = settlement.Workplaces.FirstOrDefault(entry => entry.Id == resident.WorkplaceId);
        if (workplace is null)
        {
            return ResolveSocial(resident, settlement.Day, settlement.Hour);
        }

        return workplace.Profession switch
        {
            ResidentProfession.Farmer => new VillageResidentDestination(
                VillageResidentDestinationKind.Work,
                VillageLayout.FarmWorkAnchor,
                null),
            ResidentProfession.Cook => ForBuilding(
                VillageResidentDestinationKind.Work,
                VillageLayout.CookWorkBuildingName),
            ResidentProfession.Forager => new VillageResidentDestination(
                VillageResidentDestinationKind.Work,
                VillageLayout.HerbGroveWorkAnchor,
                null),
            _ => ResolveSocial(resident, settlement.Day, settlement.Hour),
        };
    }

    private static VillageResidentDestination ResolveFood() =>
        ForBuilding(VillageResidentDestinationKind.Food, VillageLayout.FoodBuildingName);

    private static VillageResidentDestination ResolveSocial(
        ResidentProjection resident,
        int day,
        int hour)
    {
        var count = VillageLayout.SocialAnchors.Length;
        var selector = (resident.Id.Value % count) + (day % count) + (hour % count);
        var index = (int)(selector % count);
        if (index < 0)
        {
            index += count;
        }

        return new VillageResidentDestination(
            VillageResidentDestinationKind.Social,
            VillageLayout.SocialAnchors[index],
            null);
    }

    private static VillageResidentDestination ForBuilding(
        VillageResidentDestinationKind kind,
        string buildingName)
    {
        var building = VillageLayout.GetBuilding(buildingName);
        return new VillageResidentDestination(
            kind,
            building.Position,
            VillageLayout.GetEntranceWorldPosition(building));
    }
}
