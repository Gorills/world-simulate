using Mws.Client.Godot.Localization;
using Mws.Client.Godot.Session;
using Mws.Client.Godot.World.Village;
using Mws.Simulation.Api;

namespace Mws.Client.Godot.App;

internal static class HeadlessClientSmoke
{
    internal static string Run(GameWorldSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        GameLocalization.ValidateCatalogs();

        var initial = session.Projection;
        var player = session.Player;
        var startingRation = player.Inventory.Single(item => item.ItemId == SettlementItems.Ration);
        if (initial.Hour != PlaytestTimeProfile.StartHour
            || initial.Residents.Count != VillageLayout.PlaytestResidentCount
            || initial.Residents.Any(resident => !HasAuthoritativeHomePresence(resident))
            || player.Id != session.PlayerId
            || player.ScopeId != session.SettlementScopeId
            || startingRation.Quantity != 2)
        {
            throw new InvalidOperationException(
                "Playtest session did not bootstrap authoritative world/player home state.");
        }

        VillageWorld.ValidateLifeProjection(initial);
        ValidateAuthoritativeResidentPlacement(initial);

        session.AdvanceHours(24);
        var interaction = session.InteractSelected(ResidentInteractionChoice.Encourage);
        var projection = session.Projection;
        VillageWorld.ValidateLifeProjection(projection);
        var stockpileStack = projection.Stockpile[0];
        var checkpoint = session.CreateCheckpoint();
        var restored = GameWorldSession.Restore(checkpoint);
        var restoredProjection = restored.Projection;
        var restoredResident = restoredProjection.Residents.Single(
            resident => resident.Id == session.SelectedResidentId);
        var restoredPlayer = restored.Player;

        if (interaction.Success
            || interaction.Code != SettlementResultCodes.InteractionNotCoLocated
            || projection.Day != 1
            || projection.Hour != PlaytestTimeProfile.StartHour
            || projection.Residents.Count != VillageLayout.PlaytestResidentCount
            || projection.Residents.Any(resident => !HasAuthoritativeHomePresence(resident))
            || projection.Homes?.Count != 10
            || projection.Households?.Count != 6
            || session.FindStockpileStack(stockpileStack.StackId) is null
            || restored.Time != session.Time
            || restoredProjection.Day != projection.Day
            || restoredProjection.Hour != projection.Hour
            || restoredResident.Affinity != session.SelectedResident.Affinity
            || restored.PlayerId != session.PlayerId
            || restoredPlayer.ScopeId != player.ScopeId
            || !restoredPlayer.Inventory.SequenceEqual(player.Inventory))
        {
            throw new InvalidOperationException("Client WorldRuntime/player smoke produced an invalid state.");
        }

        var resident = session.SelectedResident;
        return
            $"MWS_GODOT_SMOKE_OK client=village-v0.12 day={projection.Day} hour={projection.Hour} " +
            $"resident={resident.Name} population={projection.Residents.Count} affinity={resident.Affinity} " +
            $"player={session.PlayerId.Value} player_scope={player.ScopeId.Value} " +
            "clock=continuous-hourly-playtest input=third-person-keyboard-gamepad-validated " +
            "locale=en-ru-validated spatial=authoritative-location-travel-validated " +
            "interaction=semantic-colocation-rejection-validated checkpoint=world-runtime-roundtrip-validated " +
            "player_actor=authoritative-persisted-replayable life=authoritative-placement-validated";
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

        var workplace = new SettlementPlaceRef(
            SettlementPlaceKind.Workplace,
            resident.WorkplaceId);
        var arrivedResident = resident with
        {
            Activity = ResidentActivity.Idle,
            Location = new SettlementActorLocationProjection(
                SettlementActorLocationKind.AtPlace,
                workplace,
                workplace),
        };
        var workplacePosition = VillageResidentPlacement.Resolve(arrivedResident, projection);
        var travel = new SettlementTravelProgressState(
            200_000,
            100_000,
            new SettlementTravelPlanState(
                1,
                projection.Time,
                [1],
                SettlementTravelMode.OnFoot));
        var travellingResident = resident with
        {
            Activity = ResidentActivity.Working,
            Location = new SettlementActorLocationProjection(
                SettlementActorLocationKind.Travelling,
                location.CurrentPlace,
                workplace,
                travel),
        };
        var midpoint = VillageResidentPlacement.Resolve(travellingResident, projection);
        var expectedMidpoint = homePosition.Lerp(workplacePosition, 0.5f);
        if (midpoint.DistanceTo(expectedMidpoint) > 0.001f)
        {
            throw new InvalidOperationException(
                "Resident travel presentation does not follow authoritative elapsed/duration progress.");
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
