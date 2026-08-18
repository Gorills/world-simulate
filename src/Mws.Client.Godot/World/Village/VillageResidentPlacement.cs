using Godot;
using Mws.Simulation.Api;

namespace Mws.Client.Godot.World.Village;

internal static class VillageResidentPlacement
{
    private const long P3GroveRouteConnectionId = 1;
    private const float P3MainRoadX = 0.0f;
    private const float P3GroveTrackNorthZ = 98.0f;

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
            SettlementActorLocationKind.AtPlace => ResolveAtPlace(resident, location, settlement),
            SettlementActorLocationKind.Travelling => ResolveTravelling(resident, location, settlement),
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
        ResidentProjection resident,
        SettlementActorLocationProjection location,
        SettlementProjection settlement)
    {
        if (location.Travel is not null || location.CurrentPlace != location.DestinationPlace)
        {
            throw new InvalidOperationException(
                "At-place resident presentation requires one authoritative place and no travel progress.");
        }

        return ResolveResidentPlace(resident, location.CurrentPlace, settlement);
    }

    private static Vector3 ResolveTravelling(
        ResidentProjection resident,
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

        var origin = ResolveResidentPlace(resident, location.CurrentPlace, settlement);
        var destination = ResolveResidentPlace(resident, location.DestinationPlace, settlement);
        var progress = (float)((double)travel.ElapsedMilliseconds / travel.DurationMilliseconds);
        if (IsP3GroveRoute(location, travel, settlement))
        {
            return ResolveP3GroveRoute(location, origin, destination, progress);
        }

        return origin.Lerp(destination, progress);
    }

    private static Vector3 ResolveResidentPlace(
        ResidentProjection resident,
        SettlementPlaceRef place,
        SettlementProjection settlement)
    {
        var anchor = ResolvePlace(place, settlement);
        if (place.Kind != SettlementPlaceKind.Home || resident.HomeId != place.EntityId)
        {
            return anchor;
        }

        var home = (settlement.Homes ?? []).Single(entry => entry.Id == place.EntityId);
        var building = VillageLayout.GetHomeBuilding(home.SpatialKey);
        var radians = Mathf.DegToRad(building.YawDegrees);
        var tangent = new Vector3(Mathf.Cos(radians), 0.0f, -Mathf.Sin(radians));
        var slot = (int)(Math.Abs(resident.Id.Value) % 4);
        var offsetMeters = (slot - 1.5f) * 0.55f;
        return anchor + (tangent * offsetMeters);
    }

    private static bool IsP3GroveRoute(
        SettlementActorLocationProjection location,
        SettlementTravelProgressState travel,
        SettlementProjection settlement)
    {
        var plan = travel.Plan;
        if (plan is null
            || plan.ConnectionIds.Count != 1
            || plan.ConnectionIds[0] != P3GroveRouteConnectionId)
        {
            return false;
        }

        return (IsGroveHome(location.CurrentPlace, settlement)
                && IsHerbGroveWorkplace(location.DestinationPlace, settlement))
            || (IsHerbGroveWorkplace(location.CurrentPlace, settlement)
                && IsGroveHome(location.DestinationPlace, settlement));
    }

    private static bool IsGroveHome(
        SettlementPlaceRef place,
        SettlementProjection settlement)
    {
        if (place.Kind != SettlementPlaceKind.Home)
        {
            return false;
        }

        var home = (settlement.Homes ?? []).SingleOrDefault(entry => entry.Id == place.EntityId);
        return home is not null
            && string.Equals(home.SpatialKey, SettlementHomeSpatialKeys.Grove, StringComparison.Ordinal);
    }

    private static bool IsHerbGroveWorkplace(
        SettlementPlaceRef place,
        SettlementProjection settlement)
    {
        if (place.Kind != SettlementPlaceKind.Workplace)
        {
            return false;
        }

        var workplace = settlement.Workplaces.SingleOrDefault(entry => entry.Id == place.EntityId);
        return workplace is not null && workplace.Profession == ResidentProfession.Forager;
    }

    private static Vector3 ResolveP3GroveRoute(
        SettlementActorLocationProjection location,
        Vector3 origin,
        Vector3 destination,
        float progress)
    {
        Vector3[] points;
        if (location.CurrentPlace.Kind == SettlementPlaceKind.Home)
        {
            points =
            [
                origin,
                new Vector3(P3MainRoadX, origin.Y, origin.Z),
                new Vector3(P3MainRoadX, origin.Y, P3GroveTrackNorthZ),
                new Vector3(VillageLayout.HerbGroveWorkAnchor.X, origin.Y, P3GroveTrackNorthZ),
                destination,
            ];
        }
        else
        {
            points =
            [
                origin,
                new Vector3(VillageLayout.HerbGroveWorkAnchor.X, origin.Y, P3GroveTrackNorthZ),
                new Vector3(P3MainRoadX, origin.Y, P3GroveTrackNorthZ),
                new Vector3(P3MainRoadX, origin.Y, destination.Z),
                destination,
            ];
        }

        return LerpPolyline(points, progress);
    }

    private static Vector3 LerpPolyline(IReadOnlyList<Vector3> points, float progress)
    {
        if (points.Count < 2)
        {
            throw new InvalidOperationException("Resident travel presentation path needs at least two points.");
        }

        var totalLength = 0.0f;
        for (var index = 1; index < points.Count; index++)
        {
            totalLength += points[index - 1].DistanceTo(points[index]);
        }

        if (totalLength <= 0.001f)
        {
            return points[^1];
        }

        var remaining = totalLength * Math.Clamp(progress, 0.0f, 1.0f);
        for (var index = 1; index < points.Count; index++)
        {
            var start = points[index - 1];
            var end = points[index];
            var segmentLength = start.DistanceTo(end);
            if (remaining <= segmentLength || index == points.Count - 1)
            {
                var segmentProgress = segmentLength <= 0.001f
                    ? 1.0f
                    : Math.Clamp(remaining / segmentLength, 0.0f, 1.0f);
                return start.Lerp(end, segmentProgress);
            }

            remaining -= segmentLength;
        }

        return points[^1];
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
