using Godot;
using Mws.Client.Godot.Localization;
using Mws.Client.Godot.UI.Theme;
using Mws.Simulation.Api;

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

    public void Render(ResidentProjection resident, SettlementProjection settlement)
    {
        ArgumentNullException.ThrowIfNull(resident);
        ArgumentNullException.ThrowIfNull(settlement);

        var residence = resident.HomeId == default
            ? GameLocalization.Tr("UI_HOME_UNASSIGNED")
            : GameLocalization.Format(
                "UI_HOME_ASSIGNED",
                LocalizedContent.Home(settlement, resident.HomeId),
                LocalizedContent.Household(resident.HouseholdName));
        _name.Text = resident.Name;
        _profession.Text =
            $"{LocalizedContent.Profession(resident.Profession)} · {LocalizedContent.Workplace(resident.WorkplaceName)}\n{residence}";
        _needs.Text = GameLocalization.Format("UI_NEEDS_VALUE", resident.Hunger, resident.Energy);
        _relationship.Text = GameLocalization.Format("UI_AFFINITY_VALUE", resident.Affinity);
        _inventory.Text = resident.Inventory.Count == 0
            ? GameLocalization.Tr("UI_INVENTORY_EMPTY")
            : GameLocalization.Format(
                "UI_INVENTORY_VALUE",
                string.Join(
                    " · ",
                    resident.Inventory.Select(stack => $"{LocalizedContent.Item(stack.ItemId)} {stack.Quantity}")));
    }
}
