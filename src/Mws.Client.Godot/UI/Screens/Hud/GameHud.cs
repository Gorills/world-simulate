using Godot;
using Mws.Client.Godot.Input;
using Mws.Client.Godot.Session;
using Mws.Client.Godot.UI.Theme;
using Mws.Client.Godot.World.Settlement;
using InteractionMenuView = Mws.Client.Godot.UI.Screens.InteractionMenu.InteractionMenu;
using ResidentPanelView = Mws.Client.Godot.UI.Screens.ResidentPanel.ResidentPanel;

namespace Mws.Client.Godot.UI.Screens.Hud;

public partial class GameHud : Control
{
    private GameSession? _session;
    private SettlementView _settlementView = null!;
    private ResidentPanelView _residentPanel = null!;
    private InteractionMenuView _interactionMenu = null!;
    private Label _inputHint = null!;
    private Label _feedback = null!;

    public override void _Ready()
    {
        DesignSystem.ApplyBackground(GetNode<ColorRect>("Background"));
        _settlementView = GetNode<SettlementView>("Margin/Root/Columns/SettlementView");
        _residentPanel = GetNode<ResidentPanelView>("Margin/Root/Columns/Sidebar/ResidentPanel");
        _interactionMenu = GetNode<InteractionMenuView>("Margin/Root/Columns/Sidebar/InteractionMenu");
        _inputHint = GetNode<Label>("Margin/Root/InputHint");
        _feedback = GetNode<Label>("Margin/Root/Feedback");
        DesignSystem.ApplyLabel(_inputHint, muted: true);
        DesignSystem.ApplyLabel(_feedback);

        var advance = GetNode<Button>("Margin/Root/AdvanceTime");
        DesignSystem.ApplyButton(advance);
        advance.Pressed += () => _session?.AdvanceHours(1);

        _settlementView.ResidentSelected += residentId => _session?.SelectResident(residentId);
        _interactionMenu.ChoiceRequested += choice =>
        {
            if (_session is null)
            {
                return;
            }

            var result = _session.InteractSelected(choice);
            _feedback.Text = result.Message;
        };
    }

    internal void Bind(GameSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        _session = session;
        _session.Changed += Refresh;
        Refresh();
        _settlementView.FocusSelected();
    }

    internal void SetInputDevice(InputDeviceFamily device)
    {
        _inputHint.Text = device == InputDeviceFamily.Gamepad
            ? "LB/RB select · A interact/confirm · Y advance time · B back"
            : "Mouse or Q/E select · F interact · Space advance · Enter confirm · Esc back";
    }

    public bool HandleInput(InputEvent inputEvent)
    {
        if (_session is null)
        {
            return false;
        }

        if (!_interactionMenu.HasMenuFocus() && inputEvent.IsActionPressed(GameInput.PreviousTarget))
        {
            _session.SelectRelative(-1);
            _settlementView.FocusSelected();
            return true;
        }

        if (!_interactionMenu.HasMenuFocus() && inputEvent.IsActionPressed(GameInput.NextTarget))
        {
            _session.SelectRelative(1);
            _settlementView.FocusSelected();
            return true;
        }

        if (!_interactionMenu.HasMenuFocus() && inputEvent.IsActionPressed(GameInput.Interact))
        {
            _interactionMenu.FocusFirst();
            return true;
        }

        if (!_interactionMenu.HasMenuFocus() && inputEvent.IsActionPressed(GameInput.AdvanceTime))
        {
            _session.AdvanceHours(1);
            return true;
        }

        if (_interactionMenu.HasMenuFocus() && inputEvent.IsActionPressed(GameInput.Cancel))
        {
            _settlementView.FocusSelected();
            return true;
        }

        return false;
    }

    private void Refresh()
    {
        if (_session is null)
        {
            return;
        }

        var projection = _session.Projection;
        var resident = _session.SelectedResident;
        _settlementView.Render(projection, _session.SelectedResidentId);
        _residentPanel.Render(resident);
        _interactionMenu.SetResident(resident);
        _feedback.Text = $"World state: day {projection.Day}, {projection.Hour:00}:00";
    }
}
