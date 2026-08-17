using Godot;
using Mws.Domain;
using Mws.Simulation.Api;
using Mws.Client.Godot.Input;
using Mws.Client.Godot.Session;
using Mws.Client.Godot.UI.Screens.Hud;
using Mws.Client.Godot.World.Village;

namespace Mws.Client.Godot.App;

public partial class Main : Node
{
    private readonly InputDeviceTracker _inputDevice = new();
    private GameSession? _session;
    private GameHud? _hud;
    private VillageWorld? _village;
    private bool _hudOpen;

    public override void _Ready()
    {
        try
        {
            GameInput.ConfigureDefaults();
            GameInput.ValidateDefaults();
            VillageWorld.ValidateSpatialContract();
            _session = new GameSession(new WorldSeed(42));

            if (string.Equals(DisplayServer.GetName(), "headless", StringComparison.OrdinalIgnoreCase))
            {
                RunHeadlessSmoke();
                return;
            }

            _village = GetNode<VillageWorld>("VillageWorld");
            _hud = GetNode<GameHud>("GameHud");
            _hud.Bind(_session);
            _hud.SetInputDevice(_inputDevice.Current);
            _session.Changed += RefreshVillage;
            RefreshVillage();
            SetHudOpen(open: false);
        }
        catch (Exception exception)
        {
            GD.PushError($"MWS_CLIENT_STARTUP_FAIL {exception}");
            GetTree().Quit(1);
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (_inputDevice.Observe(@event))
        {
            _hud?.SetInputDevice(_inputDevice.Current);
        }

        if (@event.IsActionPressed(GameInput.Menu) && _hud is not null && _village is not null)
        {
            SetHudOpen(!_hudOpen);
            GetViewport().SetInputAsHandled();
            return;
        }

        if (_hudOpen && _hud?.HandleInput(@event) == true)
        {
            GetViewport().SetInputAsHandled();
        }
    }

    private void SetHudOpen(bool open)
    {
        _hudOpen = open;
        if (_hud is not null)
        {
            _hud.Visible = open;
        }

        _village?.SetPlayerInputEnabled(!open);
    }

    private void RefreshVillage()
    {
        if (_session is null || _village is null)
        {
            return;
        }

        _village.Render(_session.Projection, _session.SelectedResidentId);
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
            $"MWS_GODOT_SMOKE_OK client=village-v0.1 day={projection.Day} resident={resident.Name} " +
            $"affinity={resident.Affinity} input=third-person-keyboard-gamepad-validated spatial=village-layout-validated");
        GetTree().Quit(0);
    }
}
