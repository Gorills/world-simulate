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
            var simulation = new DeterministicWorldSimulation(new WorldSeed(42));
            var snapshot = simulation.Advance();

            if (snapshot.Tick.Value != 1)
            {
                throw new InvalidOperationException($"Expected tick 1, got {snapshot.Tick.Value}.");
            }

            GD.Print($"MWS_GODOT_SMOKE_OK tick={snapshot.Tick.Value} state={snapshot.DeterministicState}");
            GetTree().Quit(0);
        }
        catch (Exception exception)
        {
            GD.PushError($"MWS_GODOT_SMOKE_FAIL {exception}");
            GetTree().Quit(1);
        }
    }
}
