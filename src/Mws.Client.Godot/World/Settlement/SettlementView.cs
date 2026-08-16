using Godot;
using Mws.Domain;
using Mws.Simulation.Api;
using Mws.Client.Godot.UI.Theme;

namespace Mws.Client.Godot.World.Settlement;

public partial class SettlementView : VBoxContainer
{
    private Label _timeLabel = null!;
    private Label _stockpileLabel = null!;
    private VBoxContainer _residentList = null!;
    private Button? _selectedButton;

    public event Action<EntityId>? ResidentSelected;

    public override void _Ready()
    {
        _timeLabel = GetNode<Label>("Time");
        _stockpileLabel = GetNode<Label>("Stockpile");
        _residentList = GetNode<VBoxContainer>("Residents");
        DesignSystem.ApplyHeading(GetNode<Label>("Heading"));
        DesignSystem.ApplyLabel(_timeLabel);
        DesignSystem.ApplyLabel(_stockpileLabel, muted: true);
    }

    public void Render(SettlementProjection projection, EntityId selectedResidentId)
    {
        ArgumentNullException.ThrowIfNull(projection);

        _timeLabel.Text = $"Day {projection.Day}, {projection.Hour:00}:00";
        _stockpileLabel.Text =
            $"Stockpile: {string.Join(" · ", projection.Stockpile.Select(stack => $"{stack.ItemId} {stack.Quantity}"))}";

        foreach (var child in _residentList.GetChildren())
        {
            child.QueueFree();
        }

        _selectedButton = null;
        foreach (var resident in projection.Residents)
        {
            var residentId = resident.Id;
            var selected = residentId == selectedResidentId;
            var button = new Button
            {
                Text = $"{(selected ? "▶ " : string.Empty)}{resident.Name} · {resident.Profession} · {resident.Activity}",
                Alignment = HorizontalAlignment.Left,
            };
            DesignSystem.ApplyButton(button);
            if (selected)
            {
                button.AddThemeColorOverride("font_color", DesignTokens.Accent);
                _selectedButton = button;
            }

            button.Pressed += () => ResidentSelected?.Invoke(residentId);
            _residentList.AddChild(button);
        }
    }

    public void FocusSelected()
    {
        _selectedButton?.GrabFocus();
    }
}
