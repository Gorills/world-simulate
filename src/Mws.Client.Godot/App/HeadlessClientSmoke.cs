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
            || initial.Residents.Any(resident => resident.Activity != ResidentActivity.Working)
            || player.Id != session.PlayerId
            || player.ScopeId != session.SettlementScopeId
            || startingRation.Quantity != 2)
        {
            throw new InvalidOperationException(
                "Playtest session did not bootstrap authoritative world/player morning state.");
        }

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

        if (!interaction.Success
            || projection.Day != 1
            || projection.Hour != PlaytestTimeProfile.StartHour
            || projection.Residents.Count != VillageLayout.PlaytestResidentCount
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
            $"MWS_GODOT_SMOKE_OK client=village-v0.11 day={projection.Day} hour={projection.Hour} " +
            $"resident={resident.Name} population={projection.Residents.Count} affinity={resident.Affinity} " +
            $"player={session.PlayerId.Value} player_scope={player.ScopeId.Value} " +
            "clock=continuous-hourly-playtest input=third-person-keyboard-gamepad-validated " +
            "locale=en-ru-validated spatial=village-layout-validated " +
            "interaction=world-runtime-targeting-validated checkpoint=world-runtime-roundtrip-validated " +
            "player_actor=authoritative-persisted-replayable life=authoritative-residence-routing-validated";
    }
}
