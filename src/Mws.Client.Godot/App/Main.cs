using Godot;
using Mws.Client.Godot.Input;
using Mws.Client.Godot.Localization;
using Mws.Client.Godot.Session;
using Mws.Client.Godot.UI.Screens.Hud;
using Mws.Client.Godot.World.Village;
using Mws.Domain;
using PromptView = Mws.Client.Godot.UI.Screens.WorldInteractionPrompt.WorldInteractionPrompt;

namespace Mws.Client.Godot.App;

public partial class Main : Node
{
    private readonly InputDeviceTracker _inputDevice = new();
    private GameSession? _session;
    private GameHud? _hud;
    private VillageWorld? _village;
    private PromptView? _prompt;
    private bool _hudOpen;

    public override void _EnterTree()
    {
        GameLocalization.Initialize();
    }

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
                GD.Print(HeadlessClientSmoke.Run(_session));
                GetTree().Quit(0);
                return;
            }

            var clock = new PlaytestClock();
            AddChild(clock);
            clock.Bind(_session);

            _village = GetNode<VillageWorld>("VillageWorld");
            _prompt = GetNode<PromptView>("WorldInteractionPrompt");
            _hud = GetNode<GameHud>("GameHud");
            _hud.Bind(_session);
            _hud.SetInputDevice(_inputDevice.Current);
            _prompt.SetInputDevice(_inputDevice.Current);
            _village.InteractionTargetChanged += _prompt.SetTarget;
            _village.InteractionRequested += HandleVillageInteraction;
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
            _prompt?.SetInputDevice(_inputDevice.Current);
        }

        if (@event.IsActionPressed(GameInput.Menu) && _hud is not null && _village is not null)
        {
            SetHudOpen(!_hudOpen);
            GetViewport().SetInputAsHandled();
            return;
        }

        if (!_hudOpen
            && @event.IsActionPressed(GameInput.Interact)
            && _village?.TryRequestInteraction() == true)
        {
            GetViewport().SetInputAsHandled();
            return;
        }

        if (!_hudOpen && @event.IsActionPressed(GameInput.AdvanceTime) && _session is not null)
        {
            _session.AdvanceHours(1);
            GetViewport().SetInputAsHandled();
            return;
        }

        if (_hudOpen && _hud?.HandleInput(@event) == true)
        {
            GetViewport().SetInputAsHandled();
        }
    }

    private void HandleVillageInteraction(VillageInteractionTarget target)
    {
        if (_session is null || _hud is null || _prompt is null)
        {
            return;
        }

        switch (target.Kind)
        {
            case VillageInteractionKind.Resident when target.ResidentId.HasValue:
                _session.SelectResident(target.ResidentId.Value);
                SetHudOpen(open: true);
                _hud.FocusInteraction();
                break;
            case VillageInteractionKind.ItemStack when target.StackId.HasValue:
                var stack = _session.FindStockpileStack(target.StackId.Value);
                if (stack is not null)
                {
                    _prompt.ShowItem(stack);
                }

                break;
            case VillageInteractionKind.BuildingEntrance:
                _prompt.ShowEntrance(target.DisplayName);
                break;
        }
    }

    private void SetHudOpen(bool open)
    {
        _hudOpen = open;
        if (_hud is not null)
        {
            _hud.Visible = open;
        }

        _prompt?.SetWorldEnabled(!open);
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
}
