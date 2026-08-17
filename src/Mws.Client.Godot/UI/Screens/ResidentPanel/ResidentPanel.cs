using System.Globalization;
using Godot;
using Mws.Client.Godot.Localization;
using Mws.Client.Godot.UI.Theme;
using Mws.Simulation.Api;

namespace Mws.Client.Godot.UI.Screens.ResidentPanel;

public partial class ResidentPanel : VBoxContainer
{
    private Label _title = null!;
    private Label _nameLabel = null!;
    private Label _nameValue = null!;
    private Label _professionLabel = null!;
    private Label _professionValue = null!;
    private Label _homeLabel = null!;
    private Label _homeValue = null!;
    private Label _needsLabel = null!;
    private Label _needsValue = null!;
    private Label _affinityLabel = null!;
    private Label _affinityValue = null!;
    private Label _inventoryLabel = null!;
    private Label _inventoryValue = null!;

    public override void _Ready()
    {
        _title = GetNode<Label>("Title");
        _nameLabel = GetNode<Label>("Details/NameLabel");
        _nameValue = GetNode<Label>("Details/NameValue");
        _professionLabel = GetNode<Label>("Details/ProfessionLabel");
        _professionValue = GetNode<Label>("Details/ProfessionValue");
        _homeLabel = GetNode<Label>("Details/HomeLabel");
        _homeValue = GetNode<Label>("Details/HomeValue");
        _needsLabel = GetNode<Label>("Details/NeedsLabel");
        _needsValue = GetNode<Label>("Details/NeedsValue");
        _affinityLabel = GetNode<Label>("Details/AffinityLabel");
        _affinityValue = GetNode<Label>("Details/AffinityValue");
        _inventoryLabel = GetNode<Label>("Details/InventoryLabel");
        _inventoryValue = GetNode<Label>("Details/InventoryValue");

        DesignSystem.ApplyHeading(_title);
        foreach (var label in Labels())
        {
            DesignSystem.ApplyLabel(label, muted: true);
        }

        foreach (var value in Values())
        {
            DesignSystem.ApplyLabel(value);
        }

        RefreshLocalization();
    }

    public void RefreshLocalization()
    {
        if (_title is null)
        {
            return;
        }

        _title.Text = GameLocalization.Tr("UI_RESIDENT");
        _nameLabel.Text = GameLocalization.Tr("UI_NAME");
        _professionLabel.Text = GameLocalization.Tr("UI_PROFESSION");
        _homeLabel.Text = GameLocalization.Tr("UI_HOME");
        _needsLabel.Text = GameLocalization.Tr("UI_NEEDS");
        _affinityLabel.Text = GameLocalization.Tr("UI_AFFINITY");
        _inventoryLabel.Text = GameLocalization.Tr("UI_INVENTORY");
    }

    public void Render(ResidentProjection resident, SettlementProjection settlement)
    {
        ArgumentNullException.ThrowIfNull(resident);
        ArgumentNullException.ThrowIfNull(settlement);

        _nameValue.Text = resident.Name;
        _professionValue.Text =
            $"{LocalizedContent.Profession(resident.Profession)} · {LocalizedContent.Workplace(resident.WorkplaceName)}";
        _homeValue.Text = resident.HomeId == default
            ? GameLocalization.Tr("UI_VALUE_UNASSIGNED")
            : $"{LocalizedContent.Home(settlement, resident.HomeId)} · {LocalizedContent.Household(resident.HouseholdName)}";
        _needsValue.Text = GameLocalization.Format("UI_NEEDS_VALUE", resident.Hunger, resident.Energy);
        _affinityValue.Text = resident.Affinity.ToString(CultureInfo.InvariantCulture);
        _inventoryValue.Text = resident.Inventory.Count == 0
            ? GameLocalization.Tr("UI_VALUE_EMPTY")
            : string.Join(
                " · ",
                resident.Inventory.Select(stack => $"{LocalizedContent.Item(stack.ItemId)} {stack.Quantity}"));
    }

    private IEnumerable<Label> Labels()
    {
        yield return _nameLabel;
        yield return _professionLabel;
        yield return _homeLabel;
        yield return _needsLabel;
        yield return _affinityLabel;
        yield return _inventoryLabel;
    }

    private IEnumerable<Label> Values()
    {
        yield return _nameValue;
        yield return _professionValue;
        yield return _homeValue;
        yield return _needsValue;
        yield return _affinityValue;
        yield return _inventoryValue;
    }
}
