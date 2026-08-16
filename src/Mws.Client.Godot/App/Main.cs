using Godot;
using Mws.Domain;
using Mws.Simulation.Api;
using Mws.Client.Godot.Input;
using Mws.Client.Godot.Session;
using Mws.Client.Godot.UI.Screens.Hud;

namespace Mws.Client.Godot.App;

public partial class Main : Node
{
    private readonly InputDeviceTracker _inputDevice = new();
    private GameSession? _session;
    private GameHud? _hud;

    public override void _Ready()
    {
        try
        {
            GameInput.ConfigureDefaults();
            GameInput.ValidateDefaults();
            _session = new GameSession(new WorldSeed(42));

            if (string.Equals(DisplayServer.GetName(), "headless", StringComparison.OrdinalIgnoreCase))
            {
                RunHeadlessSmoke();
                return;
            }

            _hud = GetNode<GameHud>("GameHud");
            _hud.Bind(_session);
            _hud.SetInputDevice(_inputDevice.Current);
        }
        catch (Exception exception)
        {
            GD.PushError($"MWS_CLIENT_STARTUP_FAIL {exception}");
            GetTree().Quit(1);
        }
    }

    public override void _Input(InputEvent inputEvent)
    {
        if (_inputDevice.Observe(inputEvent))
        {
            _hud?.SetInputDevice(_inputDevice.Current);
        }

        if (_hud?.HandleInput(inputEvent) == true)
        {
            GetViewport().SetInputAsHandled();
        }
    }

    private void RunHeadlessSmoke()
    {
        if (_session is null)
        {
            throw new InvalidOperationException("Game session was not created.");
        }

        _session.AdvanceHours(24);
        var interaction = _session.InteractSelected(ResidentInteractionChoice.Encourage);
        var projection = _session.Projection;

        if (!interaction.Success
            || projection.Day != 1
            || projection.Residents.Count != 3)
        {
            throw new InvalidOperationException("Client foundation smoke produced an invalid state.");
        }

        var resident = _session.SelectedResident;
        GD.Print(
            $"MWS_GODOT_SMOKE_OK client=v0.1 day={projection.Day} resident={resident.Name} " +
            $"affinity={resident.Affinity} input=keyboard-gamepad-validated");
        GetTree().Quit(0);
    }
}
