using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class SettlementSimulation
{
    private const int LegacyWorkStartHour = 8;
    private const int LegacyWorkEndHour = 17;

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
            ValidateSettlementPlaceReference(resident.Location.CurrentPlace);
            ValidateSettlementPlaceReference(resident.Location.DestinationPlace);
            if (resident.SelectedTask is not null)
            {
                ValidateResidentSelectedTask(resident.SelectedTask);
            }

            var travel = resident.Location.Travel;
            if (travel?.Plan is not null)
            {
                ValidateResidentTravelPlan(resident, travel);
            }
        }
    }

    private void ValidateResidentTravelPlan(
        ResidentRuntimeState resident,
        SettlementTravelProgressState travel)
    {
        var plan = travel.Plan
            ?? throw new InvalidOperationException("Travel plan validation requires a plan.");
        var task = resident.SelectedTask
            ?? throw new InvalidOperationException("Travel plan requires its source selected task.");
        if (plan.TaskId != task.TaskId)
        {
            throw new InvalidOperationException("Travel plan task does not match the resident selected task.");
        }

        if (task.RequiredPlace is null || task.RequiredPlace != resident.Location.DestinationPlace)
        {
            throw new InvalidOperationException("Travel plan destination does not match its selected task.");
        }

        if (plan.DepartedAt.Milliseconds < task.SelectedAt.Milliseconds
            || plan.DepartedAt.Milliseconds > Time.Milliseconds)
        {
            throw new InvalidOperationException("Travel plan departure time is outside its causal time range.");
        }

        var elapsedSinceDeparture = Time.Milliseconds - plan.DepartedAt.Milliseconds;
        if (travel.ElapsedMilliseconds > elapsedSinceDeparture)
        {
            throw new InvalidOperationException("Travel progress exceeds elapsed simulation time since departure.");
        }

        var currentPlace = resident.Location.CurrentPlace;
        foreach (var connectionId in plan.ConnectionIds)
        {
            var connection = _routeConnections.FirstOrDefault(
                entry => entry.ConnectionId == connectionId)
                ?? throw new InvalidOperationException("Travel plan references a missing route connection.");
            if (connection.SupportedModes?.Contains(plan.TravelMode) != true)
            {
                throw new InvalidOperationException("Travel plan uses a mode unsupported by its route connection.");
            }

            if (connection.FirstPlace == currentPlace)
            {
                currentPlace = connection.SecondPlace;
                continue;
            }

            if (connection.SecondPlace == currentPlace)
            {
                currentPlace = connection.FirstPlace;
                continue;
            }

            throw new InvalidOperationException("Travel plan route connections do not form an ordered path.");
        }

        if (currentPlace != resident.Location.DestinationPlace)
        {
            throw new InvalidOperationException("Travel plan route does not end at its destination.");
        }
    }

    private void ValidateSettlementPlaceReference(SettlementPlaceRef place)
    {
        if (place.Kind == SettlementPlaceKind.Home && !_homesById.ContainsKey(place.EntityId))
        {
            throw new InvalidOperationException("Settlement semantic place references a missing home.");
        }

        if (place.Kind == SettlementPlaceKind.Workplace && !_workplacesById.ContainsKey(place.EntityId))
        {
            throw new InvalidOperationException("Settlement semantic place references a missing workplace.");
        }
    }

    private void AdvanceCompatibilityResidentTravel()
    {
        foreach (var resident in _residents)
        {
            if (resident.Location.Kind != SettlementActorLocationKind.Travelling
                || resident.Location.Travel?.Plan is not null)
            {
                continue;
            }

            // Older snapshots may contain planless one-hour travel. Let that persisted
            // state finish, but never create a new trip from clock hour or profession.
            resident.Location = SettlementSemanticLocation.AdvanceTravel(
                resident.Location,
                HourMilliseconds);
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

    private SettlementPlaceRef LegacyScheduledResidentStablePlace(ResidentRuntimeState resident, int hour) =>
        hour >= LegacyWorkStartHour && hour < LegacyWorkEndHour
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
