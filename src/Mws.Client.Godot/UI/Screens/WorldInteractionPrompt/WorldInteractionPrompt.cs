using Godot;
using Mws.Client.Godot.Input;
using Mws.Client.Godot.Localization;
using Mws.Client.Godot.UI.Theme;
using Mws.Client.Godot.World.Village;
using Mws.Simulation.Api;

namespace Mws.Client.Godot.UI.Screens.WorldInteractionPrompt;

public partial class WorldInteractionPrompt : Control
{
    private const double FeedbackDurationSeconds = 2.5;

    private Label _label = null!;
    private VillageInteractionTarget? _target;
    private InputDeviceFamily _device = InputDeviceFamily.KeyboardMouse;
    private Func<string>? _feedbackText;
    private double _feedbackRemaining;
    private bool _worldEnabled = true;

    public override void _Ready()
    {
        _label = GetNode<Label>("Anchor/Panel/Label");
        DesignSystem.ApplyLabel(_label);
        GameLocalization.RegisterUiRefresh(RefreshAllUi);
        RefreshAllUi();
    }

    public override void _ExitTree() => GameLocalization.UnregisterUiRefresh(RefreshAllUi);

    public override void _Process(double delta)
    {
        if (_feedbackRemaining <= 0.0)
        {
            return;
        }

        _feedbackRemaining = Math.Max(0.0, _feedbackRemaining - delta);
        if (_feedbackRemaining <= 0.0)
        {
            _feedbackText = null;
            Render();
        }
    }

    internal void SetTarget(VillageInteractionTarget? target)
    {
        _target = target;
        _feedbackText = null;
        _feedbackRemaining = 0.0;
        Render();
    }

    internal void SetInputDevice(InputDeviceFamily device)
    {
        _device = device;
        Render();
    }

    internal void SetWorldEnabled(bool enabled)
    {
        _worldEnabled = enabled;
        Render();
    }

    internal void ShowItem(ItemStackProjection stack)
    {
        ArgumentNullException.ThrowIfNull(stack);
        ShowFeedback(() => GameLocalization.Format(
            "UI_PROMPT_ITEM_STOCKPILE",
            LocalizedContent.Item(stack.ItemId),
            stack.Quantity));
    }

    internal void ShowEntrance(string buildingName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(buildingName);
        ShowFeedback(() => GameLocalization.Format(
            "UI_PROMPT_ENTRANCE_OPEN",
            LocalizedContent.Building(buildingName)));
    }

    internal void RefreshAllUi() => Render();

    private void ShowFeedback(Func<string> text)
    {
        _feedbackText = text;
        _feedbackRemaining = FeedbackDurationSeconds;
        Render();
    }

    private void Render()
    {
        if (_label is null)
        {
            return;
        }

        if (!_worldEnabled)
        {
            Visible = false;
            return;
        }

        if (_feedbackText is not null)
        {
            Visible = true;
            _label.Text = _feedbackText();
            return;
        }

        if (_target is null)
        {
            Visible = false;
            _label.Text = string.Empty;
            return;
        }

        Visible = true;
        var key = _device == InputDeviceFamily.Gamepad ? "A" : "F";
        _label.Text = _target.Kind switch
        {
            VillageInteractionKind.Resident => GameLocalization.Format("UI_PROMPT_TALK", key, _target.DisplayName),
            VillageInteractionKind.ItemStack => GameLocalization.Format(
                "UI_PROMPT_INSPECT_ITEM",
                key,
                LocalizedContent.Item(_target.ItemId ?? _target.DisplayName),
                _target.Quantity),
            VillageInteractionKind.BuildingEntrance => GameLocalization.Format(
                "UI_PROMPT_INSPECT_BUILDING",
                key,
                LocalizedContent.Building(_target.DisplayName)),
            _ => string.Empty,
        };
    }
}
