using Godot;
using Mws.Domain;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;

namespace Mws.Client.Godot;

public partial class Main : Node
{
    public override void _Ready()
    {
        try
        {
            var simulation = SettlementSimulation.CreateDefault(new WorldSeed(42));
            simulation.AdvanceHours(24);

            var firstResidentId = simulation.Project().Residents[0].Id;
            var interaction = simulation.InteractWithResident(firstResidentId, ResidentInteractionChoice.Encourage);
            var itemTransfer = simulation.GiveItemToResident(firstResidentId, SettlementItems.Herb, 1);
            var projection = simulation.Project();

            if (projection.Day != 1
                || projection.Residents.Count != 3
                || projection.Workplaces.Count != 3
                || !interaction.Success
                || !itemTransfer.Success)
            {
                throw new InvalidOperationException(
                    $"Unexpected settlement projection day={projection.Day} residents={projection.Residents.Count} workplaces={projection.Workplaces.Count}.");
            }

            var first = projection.Residents[0];
            var herb = first.Inventory.FirstOrDefault(stack => string.Equals(stack.ItemId, SettlementItems.Herb, StringComparison.Ordinal));
            if (herb is null || herb.Quantity != 1 || first.Affinity != 1)
            {
                throw new InvalidOperationException(
                    $"Expected persisted gameplay projection for {first.Name}: herb=1 affinity=1.");
            }

            GD.Print(
                $"MWS_GODOT_SMOKE_OK day={projection.Day} hour={projection.Hour} pantry={projection.PantryRations} " +
                $"resident={first.Name} job={first.Profession} workplace={first.WorkplaceName} affinity={first.Affinity} herb={herb.Quantity}");
            GetTree().Quit(0);
        }
        catch (Exception exception)
        {
            GD.PushError($"MWS_GODOT_SMOKE_FAIL {exception}");
            GetTree().Quit(1);
        }
    }
}
