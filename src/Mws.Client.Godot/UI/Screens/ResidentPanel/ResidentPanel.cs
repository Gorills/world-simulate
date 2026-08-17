using System.Globalization;
using Godot;
using Mws.Client.Godot.Localization;
using Mws.Client.Godot.UI.Theme;
using Mws.Simulation.Api;

namespace Mws.Client.Godot.UI.Screens.ResidentPanel;

public partial class ResidentPanel : PanelContainer
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
        var content = GetNode<VBoxContainer>("Content");
        var details = GetNode<GridContainer>("Content/Details");

        _title = GetNode<Label>("Content/Title");
        _nameLabel = GetNode<Label>("Content/Details/NameLabel");
        _nameValue = GetNode<Label>("Content/Details/NameValue");
        _professionLabel = GetNode<Label>("Content/Details/ProfessionLabel");
        _professionValue = GetNode<Label>("Content/Details/ProfessionValue");
        _homeLabel = GetNode<Label>("Content/Details/HomeLabel");
        _homeValue = GetNode<Label>("Content/Details/HomeValue");
        _needsLabel = GetNode<Label>("Content/Details/NeedsLabel");
        _needsValue = GetNode<Label>("Content/Details/NeedsValue");
        _affinityLabel = GetNode<Label>("Content/Details/AffinityLabel");
        _affinityValue = GetNode<Label>("Content/Details/AffinityValue");
        _inventoryLabel = GetNode<Label>("Content/Details/InventoryLabel");
        _inventoryValue = GetNode<Label>("Content/Details/InventoryValue");

        DesignSystem.ApplySurface(this, UiSurface.Card);
        DesignSystem.ApplyStack(content, UiGap.Medium);
        DesignSystem.ApplyGrid(details, UiGap.Medium, UiGap.Small);
        DesignSystem.ApplyText(_title, UiTextRole.SectionHeading);

        foreach (var label in Labels())
        {
            DesignSystem.ApplyText(label, UiTextRole.Caption);
        }

        foreach (var value in Values())
        {
            DesignSystem.ApplyText(value, UiTextRole.Body);
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
