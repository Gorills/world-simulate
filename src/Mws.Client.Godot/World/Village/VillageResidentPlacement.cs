using Godot;
using Mws.Simulation.Api;

namespace Mws.Client.Godot.World.Village;

internal static class VillageResidentPlacement
{
    internal static Vector3 Resolve(
        ResidentProjection resident,
        SettlementProjection settlement)
    {
        ArgumentNullException.ThrowIfNull(resident);
        ArgumentNullException.ThrowIfNull(settlement);

        var location = resident.Location
            ?? throw new InvalidOperationException(
                $"Resident {resident.Id.Value} has no authoritative semantic location to present.");

        return location.Kind switch
        {
            SettlementActorLocationKind.AtPlace => ResolveAtPlace(location, settlement),
            SettlementActorLocationKind.Travelling => ResolveTravelling(location, settlement),
            _ => throw new ArgumentOutOfRangeException(
                nameof(resident),
                location.Kind,
                "Unknown authoritative resident location kind."),
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

    private static Vector3 ResolveAtPlace(
        SettlementActorLocationProjection location,
        SettlementProjection settlement)
    {
        if (location.Travel is not null || location.CurrentPlace != location.DestinationPlace)
        {
            throw new InvalidOperationException(
                "At-place resident presentation requires one authoritative place and no travel progress.");
        }

        return ResolvePlace(location.CurrentPlace, settlement);
    }

    private static Vector3 ResolveTravelling(
        SettlementActorLocationProjection location,
        SettlementProjection settlement)
    {
        var travel = location.Travel
            ?? throw new InvalidOperationException(
                "Travelling resident presentation requires authoritative travel progress.");
        if (travel.DurationMilliseconds <= 0
            || travel.ElapsedMilliseconds < 0
            || travel.ElapsedMilliseconds >= travel.DurationMilliseconds
            || location.CurrentPlace == location.DestinationPlace)
        {
            throw new InvalidOperationException(
                "Travelling resident presentation has invalid authoritative progress or places.");
        }

        var origin = ResolvePlace(location.CurrentPlace, settlement);
        var destination = ResolvePlace(location.DestinationPlace, settlement);
        var progress = (float)((double)travel.ElapsedMilliseconds / travel.DurationMilliseconds);
        return origin.Lerp(destination, progress);
    }

    internal static Vector3 ResolvePlace(
        SettlementPlaceRef place,
        SettlementProjection settlement) =>
        place.Kind switch
        {
            SettlementPlaceKind.Home => ResolveHome(place, settlement),
            SettlementPlaceKind.Workplace => ResolveWorkplace(place, settlement),
            SettlementPlaceKind.Settlement => throw new InvalidOperationException(
                "Settlement-wide semantic place has no accepted resident presentation anchor."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(place),
                place.Kind,
                "Unknown authoritative settlement place kind."),
        };

    private static Vector3 ResolveHome(
        SettlementPlaceRef place,
        SettlementProjection settlement)
    {
        var home = (settlement.Homes ?? []).SingleOrDefault(entry => entry.Id == place.EntityId)
            ?? throw new InvalidOperationException(
                $"Authoritative home {place.EntityId.Value} has no projected presentation mapping.");
        var building = VillageLayout.GetHomeBuilding(home.SpatialKey);
        return VillageLayout.GetEntranceWorldPosition(building);
    }

    private static Vector3 ResolveWorkplace(
        SettlementPlaceRef place,
        SettlementProjection settlement)
    {
        var workplace = settlement.Workplaces.SingleOrDefault(entry => entry.Id == place.EntityId)
            ?? throw new InvalidOperationException(
                $"Authoritative workplace {place.EntityId.Value} has no projected presentation mapping.");

        return workplace.Profession switch
        {
            ResidentProfession.Farmer => VillageLayout.FarmWorkAnchor,
            ResidentProfession.Cook => VillageLayout.GetEntranceWorldPosition(
                VillageLayout.GetBuilding(VillageLayout.CookWorkBuildingName)),
            ResidentProfession.Forager => VillageLayout.HerbGroveWorkAnchor,
            _ => throw new InvalidOperationException(
                $"Workplace {workplace.Id.Value} has no accepted prototype presentation anchor."),
        };
    }
}
