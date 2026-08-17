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

    private void RestoreOmittedResidentSemanticLocations()
    {
        var hour = CurrentHour(Time);
        foreach (var resident in _residents)
        {
            if (!resident.LocationWasOmitted)
            {
                continue;
            }

            // New runtime starts at the resident's residence fixture.
            // The time-based branch exists only to hydrate snapshots written by
            // the old schedule-compacted format; it is not a canonical location rule.
            resident.Location = SettlementActorLocationState.At(
                Time.Milliseconds == 0
                    ? ResidentHomePlace(resident)
                    : LegacyScheduledResidentStablePlace(resident, hour));
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

    private static SettlementActorLocationState CaptureResidentSemanticLocation(ResidentRuntimeState resident) =>
        SettlementSemanticLocation.Normalize(resident.Location);

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
