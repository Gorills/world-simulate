using Godot;
using Mws.Client.Godot.Input;
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
    private string? _feedback;
    private double _feedbackRemaining;
    private bool _worldEnabled = true;

    public override void _Ready()
    {
        _label = GetNode<Label>("Anchor/Panel/Label");
        DesignSystem.ApplyLabel(_label);
        Render();
    }

    public override void _Process(double delta)
    {
        if (_feedbackRemaining <= 0.0)
        {
            return;
        }

        _feedbackRemaining = Math.Max(0.0, _feedbackRemaining - delta);
        if (_feedbackRemaining <= 0.0)
        {
            _feedback = null;
            Render();
        }
    }

    internal void SetTarget(VillageInteractionTarget? target)
    {
        _target = target;
        _feedback = null;
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
        ShowFeedback($"{FriendlyItemName(stack.ItemId)} × {stack.Quantity} · village stockpile");
    }

    internal void ShowEntrance(string buildingName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(buildingName);
        ShowFeedback($"{buildingName} · doorway is open — walk inside");
    }

    private void ShowFeedback(string text)
    {
        _feedback = text;
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

        if (_feedback is not null)
        {
            Visible = true;
            _label.Text = _feedback;
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
            VillageInteractionKind.Resident => $"{key} Talk · {_target.DisplayName}",
            VillageInteractionKind.ItemStack =>
                $"{key} Inspect · {FriendlyItemName(_target.ItemId ?? _target.DisplayName)} × {_target.Quantity}",
            VillageInteractionKind.BuildingEntrance => $"{key} Inspect · {_target.DisplayName}",
            _ => string.Empty,
        };
    }

    private static string FriendlyItemName(string itemId) => itemId switch
    {
        SettlementItems.Grain => "Grain",
        SettlementItems.Ration => "Rations",
        SettlementItems.Herb => "Herbs",
        _ => itemId,
    };
}
