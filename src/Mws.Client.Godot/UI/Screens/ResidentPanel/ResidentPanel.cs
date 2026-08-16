using Godot;
using Mws.Simulation.Api;
using Mws.Client.Godot.UI.Theme;

namespace Mws.Client.Godot.UI.Screens.ResidentPanel;

public partial class ResidentPanel : VBoxContainer
{
    private Label _name = null!;
    private Label _profession = null!;
    private Label _needs = null!;
    private Label _relationship = null!;
    private Label _inventory = null!;

    public override void _Ready()
    {
        _name = GetNode<Label>("Name");
        _profession = GetNode<Label>("Profession");
        _needs = GetNode<Label>("Needs");
        _relationship = GetNode<Label>("Relationship");
        _inventory = GetNode<Label>("Inventory");

        DesignSystem.ApplyHeading(_name);
        DesignSystem.ApplyLabel(_profession);
        DesignSystem.ApplyLabel(_needs);
        DesignSystem.ApplyLabel(_relationship);
        DesignSystem.ApplyLabel(_inventory, muted: true);
    }

    public void Render(ResidentProjection resident)
    {
        ArgumentNullException.ThrowIfNull(resident);

        _name.Text = resident.Name;
        _profession.Text = $"{resident.Profession} · {resident.WorkplaceName}";
        _needs.Text = $"Hunger {resident.Hunger}/100 · Energy {resident.Energy}/100";
        _relationship.Text = $"Affinity {resident.Affinity}";
        _inventory.Text = resident.Inventory.Count == 0
            ? "Inventory: empty"
            : $"Inventory: {string.Join(" · ", resident.Inventory.Select(stack => $"{stack.ItemId} {stack.Quantity}"))}";
    }
}
