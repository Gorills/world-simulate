using Mws.Client.Godot.Localization;
using Mws.Client.Godot.Session;
using Mws.Client.Godot.World.Village;
using Mws.Simulation.Api;

namespace Mws.Client.Godot.App;

internal static class HeadlessClientSmoke
{
    private const long ExpectedTravelDurationMilliseconds = 214_286;
    private const long PartialTravelElapsedMilliseconds = 100_000;

    internal static string Run(GameWorldSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        GameLocalization.ValidateCatalogs();

        var initial = session.Projection;
        var player = session.Player;
        var startingRation = player.Inventory.Single(item => item.ItemId == SettlementItems.Ration);
        var initialResident = session.SelectedResident;
        var initialTask = initialResident.SelectedTask;
        if (initial.Hour != PlaytestTimeProfile.StartHour
            || initial.Residents.Count != VillageLayout.PlaytestResidentCount
            || initial.Residents.Any(resident => !HasAuthoritativeHomePresence(resident))
            || initialResident.Name != "Karo"
            || initialTask?.RequiredPlace is not { Kind: SettlementPlaceKind.Workplace }
            || !session.TravelPlaytestPending
            || player.Id != session.PlayerId
            || player.ScopeId != session.SettlementScopeId
            || startingRation.Quantity != 2)
        {
            throw new InvalidOperationException(
                "Playtest session did not bootstrap the authoritative P3 travel fixture.");
        }

        VillageWorld.ValidateLifeProjection(initial);
        ValidateAuthoritativeResidentPlacement(initial);

        if (!session.TryStartTravelPlaytest())
        {
            throw new InvalidOperationException(
                "P3 playtest task did not depart at the next resident decision boundary.");
        }

        var departedProjection = session.Projection;
        var departedResident = session.SelectedResident;
        var departedLocation = departedResident.Location
            ?? throw new InvalidOperationException("Departed P3 resident has no authoritative location.");
        var departedTravel = departedLocation.Travel
            ?? throw new InvalidOperationException("Departed P3 resident has no travel progress.");
        if (departedProjection.Hour != PlaytestTimeProfile.StartHour + 1
            || departedLocation.Kind != SettlementActorLocationKind.Travelling
            || departedTravel.DurationMilliseconds != ExpectedTravelDurationMilliseconds
            || departedTravel.ElapsedMilliseconds < 0
            || departedTravel.ElapsedMilliseconds >= PartialTravelElapsedMilliseconds)
        {
            throw new InvalidOperationException("P3 playtest departure state is invalid.");
        }

        var toPartial = PartialTravelElapsedMilliseconds - departedTravel.ElapsedMilliseconds;
        if (toPartial > 0 && !session.AdvanceActiveTravelSample(toPartial))
        {
            throw new InvalidOperationException("P3 playtest travel ended before the expected partial progress.");
        }

        var partialProjection = session.Projection;
        var partialResident = session.SelectedResident;
        var partialLocation = partialResident.Location
            ?? throw new InvalidOperationException("Partial P3 resident has no authoritative location.");
        var partialTravel = partialLocation.Travel
            ?? throw new InvalidOperationException("Partial P3 resident has no travel progress.");
        if (partialLocation.Kind != SettlementActorLocationKind.Travelling
            || partialTravel.DurationMilliseconds != ExpectedTravelDurationMilliseconds
            || partialTravel.ElapsedMilliseconds != PartialTravelElapsedMilliseconds)
        {
            throw new InvalidOperationException("P3 playtest partial travel progress is invalid.");
        }

        VillageWorld.ValidateLifeProjection(partialProjection);
        ValidateRuntimeTravelPlacement(initialResident, partialResident, partialProjection);

        var checkpoint = session.CreateCheckpoint();
        var restored = GameWorldSession.Restore(checkpoint);
        restored.SelectResident(initialResident.Id);
        var restoredResident = restored.SelectedResident;
        var restoredLocation = restoredResident.Location
            ?? throw new InvalidOperationException("Restored P3 resident has no authoritative location.");
        var restoredTravel = restoredLocation.Travel
            ?? throw new InvalidOperationException("Restored P3 resident has no travel progress.");
        if (restored.Time != session.Time
            || restoredLocation.Kind != SettlementActorLocationKind.Travelling
            || restoredTravel.DurationMilliseconds != partialTravel.DurationMilliseconds
            || restoredTravel.ElapsedMilliseconds != partialTravel.ElapsedMilliseconds)
        {
            throw new InvalidOperationException("P3 playtest checkpoint did not preserve travel progress.");
        }

        var remaining = restoredTravel.DurationMilliseconds - restoredTravel.ElapsedMilliseconds;
        if (restored.AdvanceActiveTravelSample(remaining))
        {
            throw new InvalidOperationException("P3 playtest still reports active travel after exact arrival.");
        }

        var projection = restored.Projection;
        var resident = restored.SelectedResident;
        var arrival = resident.Location
            ?? throw new InvalidOperationException("Arrived P3 resident has no authoritative location.");
        var interaction = restored.InteractSelected(ResidentInteractionChoice.Encourage);
        VillageWorld.ValidateLifeProjection(projection);
        var stockpileStack = projection.Stockpile[0];
        var restoredPlayer = restored.Player;

        if (arrival.Kind != SettlementActorLocationKind.AtPlace
            || arrival.CurrentPlace.Kind != SettlementPlaceKind.Workplace
            || arrival.CurrentPlace != arrival.DestinationPlace
            || arrival.Travel is not null
            || interaction.Success
            || interaction.Code != SettlementResultCodes.InteractionNotCoLocated
            || projection.Day != 0
            || projection.Hour != PlaytestTimeProfile.StartHour + 1
            || projection.Residents.Count != VillageLayout.PlaytestResidentCount
            || projection.Residents
                .Where(entry => entry.Id != resident.Id)
                .Any(entry => !HasAuthoritativeHomePresence(entry))
            || projection.Homes?.Count != 10
            || projection.Households?.Count != 6
            || restored.FindStockpileStack(stockpileStack.StackId) is null
            || restored.PlayerId != session.PlayerId
            || restoredPlayer.ScopeId != player.ScopeId
            || !restoredPlayer.Inventory.SequenceEqual(player.Inventory))
        {
            throw new InvalidOperationException("Client P3 travel playtest produced an invalid state.");
        }

        return
            $"MWS_GODOT_SMOKE_OK client=village-v0.13 day={projection.Day} hour={projection.Hour} " +
            $"resident={resident.Name} population={projection.Residents.Count} " +
            $"player={restored.PlayerId.Value} player_scope={restoredPlayer.ScopeId.Value} " +
            "clock=hourly-plus-active-travel-sampling input=third-person-keyboard-gamepad-validated " +
            "locale=en-ru-validated spatial=authoritative-location-travel-validated " +
            "interaction=semantic-colocation-rejection-validated checkpoint=travel-progress-roundtrip-validated " +
            "playtest=p3-manual-travel-fixture-validated life=authoritative-placement-validated";
    }

    private static void ValidateAuthoritativeResidentPlacement(SettlementProjection projection)
    {
        var resident = projection.Residents.Single(entry => entry.Name == "Karo");
        var location = resident.Location
            ?? throw new InvalidOperationException("P3 client smoke resident has no authoritative location.");
        if (location.Kind != SettlementActorLocationKind.AtPlace
            || location.CurrentPlace.Kind != SettlementPlaceKind.Home
            || resident.WorkplaceId == default)
        {
            throw new InvalidOperationException("P3 client smoke resident does not expose the expected places.");
        }

        var homePosition = VillageResidentPlacement.Resolve(resident, projection);
        var activityOnly = resident with { Activity = ResidentActivity.Working };
        var activityOnlyPosition = VillageResidentPlacement.Resolve(activityOnly, projection);
        if (homePosition.DistanceTo(activityOnlyPosition) > 0.001f)
        {
            throw new InvalidOperationException(
                "Resident presentation moved because Activity changed without semantic location travel.");
        }
    }

    private static void ValidateRuntimeTravelPlacement(
        ResidentProjection initialResident,
        ResidentProjection travellingResident,
        SettlementProjection projection)
    {
        var travel = travellingResident.Location?.Travel
            ?? throw new InvalidOperationException("Runtime travel placement fixture has no progress.");
        var homePosition = VillageResidentPlacement.Resolve(initialResident, projection);
        var workplace = new SettlementPlaceRef(
            SettlementPlaceKind.Workplace,
            travellingResident.WorkplaceId);
        var arrivedResident = travellingResident with
        {
            Location = new SettlementActorLocationProjection(
                SettlementActorLocationKind.AtPlace,
                workplace,
                workplace),
        };
        var workplacePosition = VillageResidentPlacement.Resolve(arrivedResident, projection);
        var actual = VillageResidentPlacement.Resolve(travellingResident, projection);
        var progress = (float)travel.ElapsedMilliseconds / travel.DurationMilliseconds;
        var expected = homePosition.Lerp(workplacePosition, progress);
        if (actual.DistanceTo(expected) > 0.001f)
        {
            throw new InvalidOperationException(
                "Runtime resident placement does not follow authoritative travel progress.");
        }
    }

    private static bool HasAuthoritativeHomePresence(ResidentProjection resident)
    {
        var location = resident.Location;
        return resident.Activity != ResidentActivity.Working
            && location is not null
            && location.Kind == SettlementActorLocationKind.AtPlace
            && location.CurrentPlace.Kind == SettlementPlaceKind.Home
            && location.CurrentPlace.EntityId == resident.HomeId
            && location.Travel is null;
    }
}
