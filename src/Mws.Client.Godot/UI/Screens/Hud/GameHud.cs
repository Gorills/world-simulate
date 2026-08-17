using Godot;
using Mws.Client.Godot.Input;
using Mws.Client.Godot.Localization;
using Mws.Client.Godot.Session;
using Mws.Client.Godot.UI.Feedback;
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
    private OptionButton _language = null!;
    private InputDeviceFamily _inputDevice = InputDeviceFamily.KeyboardMouse;

    public override void _Ready()
    {
        DesignSystem.ApplyBackground(GetNode<ColorRect>("Background"));
        _settlementView = GetNode<SettlementView>("Margin/Root/Columns/SettlementView");
        _residentPanel = GetNode<ResidentPanelView>("Margin/Root/Columns/Sidebar/ResidentPanel");
        _interactionMenu = GetNode<InteractionMenuView>("Margin/Root/Columns/Sidebar/InteractionMenu");
        _inputHint = GetNode<Label>("Margin/Root/InputHint");
        _feedback = GetNode<Label>("Margin/Root/Feedback");
        _language = GetNode<OptionButton>("Margin/Root/LanguageRow/Language");
        DesignSystem.ApplyLabel(_inputHint, muted: true);
        DesignSystem.ApplyLabel(_feedback);
        DesignSystem.ApplyLabel(GetNode<Label>("Margin/Root/LanguageRow/Label"));
        DesignSystem.ApplyButton(_language);

        var advance = GetNode<Button>("Margin/Root/AdvanceTime");
        DesignSystem.ApplyButton(advance);
        advance.Pressed += () => _session?.AdvanceHours(1);

        ConfigureLanguagePicker();
        GameLocalization.Changed += HandleLocaleChanged;
        _settlementView.ResidentSelected += residentId => _session?.SelectResident(residentId);
        _interactionMenu.ChoiceRequested += choice =>
        {
            if (_session is null)
            {
                return;
            }

            var result = _session.InteractSelected(choice);
            _feedback.Text = SettlementFeedbackText.Format(result, _session.SelectedResident);
        };
    }

    public override void _ExitTree()
    {
        GameLocalization.Changed -= HandleLocaleChanged;
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
        _inputDevice = device;
        _inputHint.Text = GameLocalization.Tr(
            device == InputDeviceFamily.Gamepad ? "UI_HINT_GAMEPAD" : "UI_HINT_KEYBOARD");
    }

    internal void FocusInteraction() => _interactionMenu.FocusFirst();

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

    private void ConfigureLanguagePicker()
    {
        _language.AddItem(GameLocalization.LanguageSelfName(GameLocalization.English));
        _language.AddItem(GameLocalization.LanguageSelfName(GameLocalization.Russian));
        _language.ItemSelected += index => GameLocalization.SetLocale(
            index == 1 ? GameLocalization.Russian : GameLocalization.English);
        SyncLanguagePicker();
    }

    private void HandleLocaleChanged()
    {
        SyncLanguagePicker();
        SetInputDevice(_inputDevice);
        Refresh();
    }

    private void SyncLanguagePicker() =>
        _language.Select(GameLocalization.CurrentLocale == GameLocalization.Russian ? 1 : 0);

    private void Refresh()
    {
        if (_session is null)
        {
            return;
        }

        var projection = _session.Projection;
        var resident = _session.SelectedResident;
        _settlementView.Render(projection, _session.SelectedResidentId);
        _residentPanel.Render(resident, projection);
        _interactionMenu.SetResident(resident);
        _feedback.Text = GameLocalization.Format(
            "UI_WORLD_STATE_TIME",
            projection.Day,
            projection.Hour.ToString("00", System.Globalization.CultureInfo.InvariantCulture));
    }
}
