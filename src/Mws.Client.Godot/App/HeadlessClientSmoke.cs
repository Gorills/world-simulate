using Mws.Client.Godot.Localization;
using Mws.Client.Godot.Session;
using Mws.Client.Godot.World.Village;
using Mws.Simulation.Api;

namespace Mws.Client.Godot.App;

internal static class HeadlessClientSmoke
{
    internal static string Run(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        GameLocalization.ValidateCatalogs();

        var initial = session.Projection;
        if (initial.Hour != PlaytestTimeProfile.StartHour
            || initial.Residents.Count != VillageLayout.PlaytestResidentCount
            || initial.Residents.Any(resident => resident.Activity != ResidentActivity.Working))
        {
            throw new InvalidOperationException(
                "Playtest session did not bootstrap into authoritative morning work activity.");
        }

        session.AdvanceHours(24);
        var interaction = session.InteractSelected(ResidentInteractionChoice.Encourage);
        var projection = session.Projection;
        VillageWorld.ValidateLifeProjection(projection);
        var stockpileStack = projection.Stockpile[0];

        if (!interaction.Success
            || projection.Day != 1
            || projection.Hour != PlaytestTimeProfile.StartHour
            || projection.Residents.Count != VillageLayout.PlaytestResidentCount
            || projection.Homes?.Count != 10
            || projection.Households?.Count != 6
            || session.FindStockpileStack(stockpileStack.StackId) is null)
        {
            throw new InvalidOperationException("Client foundation smoke produced an invalid state.");
        }

        var resident = session.SelectedResident;
        return
            $"MWS_GODOT_SMOKE_OK client=village-v0.11 day={projection.Day} hour={projection.Hour} " +
            $"resident={resident.Name} population={projection.Residents.Count} affinity={resident.Affinity} " +
            "clock=continuous-hourly-playtest input=third-person-keyboard-gamepad-validated " +
            "locale=en-ru-validated spatial=village-layout-validated " +
            "interaction=session-targeting-validated life=authoritative-residence-routing-validated";
    }
}
