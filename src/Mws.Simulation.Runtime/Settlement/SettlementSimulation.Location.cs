using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class SettlementSimulation
{
    private const int PrototypeMorningCommuteHour = 7;
    private const int PrototypeWorkStartHour = 8;
    private const int PrototypeWorkEndHour = 17;
    private const long PrototypeTravelDurationMilliseconds = HourMilliseconds;

    internal SettlementActorLocationState? TryGetResidentSemanticLocation(EntityId residentId)
    {
        var index = FindResidentIndex(residentId);
        return index < 0 ? null : _residents[index].Location;
    }

    private void RestoreOmittedResidentSemanticLocations(int residentLocationEncodingVersion)
    {
        var usesCurrentEncoding = residentLocationEncodingVersion
            == SettlementVersions.CurrentResidentLocationEncodingVersion;
        var legacyHour = usesCurrentEncoding ? 0 : CurrentHour(Time);

        foreach (var resident in _residents)
        {
            if (!resident.LocationWasOmitted)
            {
                continue;
            }

            // Current snapshots use null only as a compact encoding for the resident's
            // persisted residence-default place. Clock-based hydration is retained solely
            // for older schema-5 snapshots that predate the location-encoding marker.
            var place = usesCurrentEncoding
                ? ResidentHomePlace(resident)
                : Time.Milliseconds == 0
                    ? ResidentHomePlace(resident)
                    : LegacyScheduledResidentStablePlace(resident, legacyHour);
            resident.Location = SettlementActorLocationState.At(place);
        }
    }

    private void ValidateResidentSemanticLocationReferences()
    {
        foreach (var resident in _residents)
        {
            ValidateResidentPlaceReference(resident.Location.CurrentPlace);
            ValidateResidentPlaceReference(resident.Location.DestinationPlace);
        }
    }

    private void ValidateResidentPlaceReference(SettlementPlaceRef place)
    {
        if (place.Kind == SettlementPlaceKind.Home && !_homesById.ContainsKey(place.EntityId))
        {
            throw new InvalidOperationException("Resident semantic location references a missing home.");
        }

        if (place.Kind == SettlementPlaceKind.Workplace && !_workplacesById.ContainsKey(place.EntityId))
        {
            throw new InvalidOperationException("Resident semantic location references a missing workplace.");
        }
    }

    private void AdvanceResidentSemanticLocations(int hour)
    {
        foreach (var resident in _residents)
        {
            if (resident.Location.Kind == SettlementActorLocationKind.Travelling)
            {
                resident.Location = SettlementSemanticLocation.AdvanceTravel(
                    resident.Location,
                    HourMilliseconds);
                continue;
            }

            // Compatibility feeder only. The 07/17 fixture still starts travel while
            // authoritative Task/Intention runtime state is not yet implemented. The travel
            // engine below is no longer derived from the clock and persists its own progress.
            var target = PrototypeScheduledResidentDestination(resident, hour);
            if (IsAtPlace(resident.Location, target))
            {
                continue;
            }

            resident.Location = SettlementSemanticLocation.BeginTravel(
                resident.Location,
                target,
                PrototypeTravelDurationMilliseconds);
        }
    }

    private SettlementActorLocationState? CaptureResidentSemanticLocation(ResidentRuntimeState resident)
    {
        var location = SettlementSemanticLocation.Normalize(resident.Location);
        return IsAtPlace(location, ResidentHomePlace(resident))
            ? null
            : location;
    }

    private bool IsResidentAtHome(ResidentRuntimeState resident) =>
        IsAtPlace(resident.Location, ResidentHomePlace(resident));

    private bool IsResidentAtWorkplace(ResidentRuntimeState resident) =>
        IsAtPlace(resident.Location, ResidentWorkplacePlace(resident));

    private SettlementPlaceRef PrototypeScheduledResidentDestination(ResidentRuntimeState resident, int hour) =>
        hour >= PrototypeMorningCommuteHour && hour < PrototypeWorkEndHour
            ? ResidentWorkplacePlace(resident)
            : ResidentHomePlace(resident);

    private SettlementPlaceRef LegacyScheduledResidentStablePlace(ResidentRuntimeState resident, int hour) =>
        hour >= PrototypeWorkStartHour && hour < PrototypeWorkEndHour
            ? ResidentWorkplacePlace(resident)
            : ResidentHomePlace(resident);

    private SettlementPlaceRef ResidentHomePlace(ResidentRuntimeState resident)
    {
        var household = FindHousehold(resident.HouseholdId);
        var home = household is null ? null : FindHome(household.HomeId);
        return home is null
            ? SettlementPlaceRef.Settlement
            : new SettlementPlaceRef(SettlementPlaceKind.Home, home.Id);
    }

    private SettlementPlaceRef ResidentWorkplacePlace(ResidentRuntimeState resident)
    {
        var workplace = FindWorkplace(resident.WorkplaceId);
        return workplace is null
            ? SettlementPlaceRef.Settlement
            : new SettlementPlaceRef(SettlementPlaceKind.Workplace, workplace.Id);
    }

    private static bool IsAtPlace(
        SettlementActorLocationState location,
        SettlementPlaceRef place) =>
        location.Kind == SettlementActorLocationKind.AtPlace
        && location.CurrentPlace == place;

    private static int CurrentHour(SimulationTime time) =>
        checked((int)((time.Milliseconds / HourMilliseconds) % 24));
}
