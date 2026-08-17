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
    private GameWorldSession? _session;
    private SettlementView _settlementView = null!;
    private ResidentPanelView _residentPanel = null!;
    private InteractionMenuView _interactionMenu = null!;
    private Label _inputHint = null!;
    private Label _feedback = null!;
    private Label _languageLabel = null!;
    private Button _advance = null!;
    private OptionButton _language = null!;
    private InputDeviceFamily _inputDevice = InputDeviceFamily.KeyboardMouse;

    public override void _Ready()
    {
        var margin = GetNode<MarginContainer>("Margin");
        var root = GetNode<VBoxContainer>("Margin/Root");
        var columns = GetNode<HBoxContainer>("Margin/Root/Columns");
        var sidebar = GetNode<VBoxContainer>("Margin/Root/Columns/Sidebar");
        var bottomBar = GetNode<PanelContainer>("Margin/Root/BottomBar");
        var bottomContent = GetNode<HBoxContainer>("Margin/Root/BottomBar/Content");
        var languageRow = GetNode<HBoxContainer>("Margin/Root/BottomBar/Content/LanguageRow");

        _settlementView = GetNode<SettlementView>("Margin/Root/Columns/SettlementView");
        _residentPanel = GetNode<ResidentPanelView>("Margin/Root/Columns/Sidebar/ResidentPanel");
        _interactionMenu = GetNode<InteractionMenuView>("Margin/Root/Columns/Sidebar/InteractionMenu");
        _inputHint = GetNode<Label>("Margin/Root/InputHint");
        _feedback = GetNode<Label>("Margin/Root/BottomBar/Content/Feedback");
        _languageLabel = GetNode<Label>("Margin/Root/BottomBar/Content/LanguageRow/Label");
        _advance = GetNode<Button>("Margin/Root/BottomBar/Content/AdvanceTime");
        _language = GetNode<OptionButton>("Margin/Root/BottomBar/Content/LanguageRow/Language");

        DesignSystem.ApplyBackdrop(GetNode<ColorRect>("Background"));
        DesignSystem.ApplyPageMargin(margin);
        DesignSystem.ApplyStack(root, UiGap.Medium);
        DesignSystem.ApplyStack(columns, UiGap.Large);
        DesignSystem.ApplyStack(sidebar, UiGap.Medium);
        DesignSystem.ApplySurface(bottomBar, UiSurface.Card);
        DesignSystem.ApplyStack(bottomContent, UiGap.Medium);
        DesignSystem.ApplyStack(languageRow, UiGap.Small);
        DesignSystem.ApplyText(_inputHint, UiTextRole.Caption);
        DesignSystem.ApplyText(_feedback, UiTextRole.Body);
        DesignSystem.ApplyText(_languageLabel, UiTextRole.Muted);
        DesignSystem.ApplyButton(_advance, UiButtonRole.Primary);
        DesignSystem.ApplyOptionButton(_language);

        _advance.Pressed += () => _session?.AdvanceHours(1);
        ConfigureLanguagePicker();
        GameLocalization.RegisterUiRefresh(RefreshAllUi);

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

        RefreshAllUi();
    }

    public override void _ExitTree()
    {
        GameLocalization.UnregisterUiRefresh(RefreshAllUi);
        if (_session is not null)
        {
            _session.Changed -= RefreshData;
        }
    }

    internal void Bind(GameWorldSession session)
    {
        ArgumentNullException.ThrowIfNull(session);
        if (_session is not null)
        {
            _session.Changed -= RefreshData;
        }

        _session = session;
        _session.Changed += RefreshData;
        RefreshAllUi();
        _settlementView.FocusSelected();
    }

    internal void SetInputDevice(InputDeviceFamily device)
    {
        _inputDevice = device;
        RefreshInputHint();
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

    internal void RefreshAllUi()
    {
        _advance.Text = GameLocalization.Tr("UI_ADVANCE_ONE_HOUR");
        _languageLabel.Text = GameLocalization.Tr("UI_LANGUAGE");
        SyncLanguagePicker();
        RefreshInputHint();
        _settlementView.RefreshLocalization();
        _residentPanel.RefreshLocalization();
        _interactionMenu.RefreshLocalization();
        RefreshData();
    }

    private void ConfigureLanguagePicker()
    {
        _language.AutoTranslateMode = Node.AutoTranslateModeEnum.Disabled;
        _language.AddItem(GameLocalization.LanguageSelfName(GameLocalization.English));
        _language.AddItem(GameLocalization.LanguageSelfName(GameLocalization.Russian));
        for (var index = 0; index < _language.ItemCount; index++)
        {
            _language.SetItemAutoTranslateMode(index, Node.AutoTranslateModeEnum.Disabled);
        }

        _language.ItemSelected += index => GameLocalization.SetLocale(
            index == 1 ? GameLocalization.Russian : GameLocalization.English);
        SyncLanguagePicker();
    }

    private void SyncLanguagePicker()
    {
        if (_language.ItemCount >= 2)
        {
            _language.SetItemText(0, GameLocalization.LanguageSelfName(GameLocalization.English));
            _language.SetItemText(1, GameLocalization.LanguageSelfName(GameLocalization.Russian));
        }

        _language.Select(GameLocalization.CurrentLocale == GameLocalization.Russian ? 1 : 0);
    }

    private void RefreshInputHint()
    {
        if (_inputHint is null)
        {
            return;
        }

        _inputHint.Text = GameLocalization.Tr(
            _inputDevice == InputDeviceFamily.Gamepad ? "UI_HINT_GAMEPAD" : "UI_HINT_KEYBOARD");
    }

    private void RefreshData()
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
