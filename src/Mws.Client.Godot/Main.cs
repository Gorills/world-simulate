using Godot;
using Mws.Domain;
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
            var projection = simulation.Project();

            if (projection.Day != 1 || projection.Residents.Count != 3)
            {
                throw new InvalidOperationException(
                    $"Expected settlement day 1 with 3 residents, got day={projection.Day} residents={projection.Residents.Count}.");
            }

            var first = projection.Residents[0];
            GD.Print(
                $"MWS_GODOT_SMOKE_OK day={projection.Day} hour={projection.Hour} pantry={projection.PantryRations} resident={first.Name} hunger={first.Hunger} energy={first.Energy}");
            GetTree().Quit(0);
        }
        catch (Exception exception)
        {
            GD.PushError($"MWS_GODOT_SMOKE_FAIL {exception}");
            GetTree().Quit(1);
        }
    }
}
