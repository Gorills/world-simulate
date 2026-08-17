using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class SettlementSimulation
{
    private const int MorningCommuteHour = 7;
    private const int WorkStartHour = 8;
    private const int WorkEndHour = 17;

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

            resident.Location = SettlementActorLocationState.At(
                ScheduledResidentStablePlace(resident, hour));
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
            var target = ScheduledResidentDestination(resident, hour);
            resident.Location = StepResidentLocation(resident.Location, target);
        }
    }

    private SettlementActorLocationState? CaptureResidentSemanticLocation(ResidentRuntimeState resident)
    {
        var location = SettlementSemanticLocation.Normalize(resident.Location);
        var canonical = ScheduledResidentStablePlace(resident, CurrentHour(Time));
        return location.Kind == SettlementActorLocationKind.AtPlace
            && location.CurrentPlace == canonical
            ? null
            : location;
    }

    private bool IsResidentAtHome(ResidentRuntimeState resident) =>
        IsAtPlace(resident.Location, ResidentHomePlace(resident));

    private bool IsResidentAtWorkplace(ResidentRuntimeState resident) =>
        IsAtPlace(resident.Location, ResidentWorkplacePlace(resident));

    private SettlementPlaceRef ScheduledResidentDestination(ResidentRuntimeState resident, int hour) =>
        hour >= MorningCommuteHour && hour < WorkEndHour
            ? ResidentWorkplacePlace(resident)
            : ResidentHomePlace(resident);

    private SettlementPlaceRef ScheduledResidentStablePlace(ResidentRuntimeState resident, int hour) =>
        hour >= WorkStartHour && hour < WorkEndHour
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

    private static SettlementActorLocationState StepResidentLocation(
        SettlementActorLocationState location,
        SettlementPlaceRef target)
    {
        location = SettlementSemanticLocation.Normalize(location);
        ArgumentNullException.ThrowIfNull(target);

        if (location.Kind == SettlementActorLocationKind.Travelling)
        {
            if (location.DestinationPlace == target)
            {
                return SettlementActorLocationState.At(target);
            }

            if (location.CurrentPlace == target)
            {
                return SettlementActorLocationState.At(target);
            }

            return new SettlementActorLocationState(
                SettlementActorLocationKind.Travelling,
                location.CurrentPlace,
                target);
        }

        return location.CurrentPlace == target
            ? location
            : new SettlementActorLocationState(
                SettlementActorLocationKind.Travelling,
                location.CurrentPlace,
                target);
    }

    private static bool IsAtPlace(
        SettlementActorLocationState location,
        SettlementPlaceRef place) =>
        location.Kind == SettlementActorLocationKind.AtPlace
        && location.CurrentPlace == place;

    private static int CurrentHour(SimulationTime time) =>
        checked((int)((time.Milliseconds / HourMilliseconds) % 24));
}
