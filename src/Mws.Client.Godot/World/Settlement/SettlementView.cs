using Godot;
using Mws.Client.Godot.Localization;
using Mws.Client.Godot.UI.Theme;
using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Client.Godot.World.Settlement;

public partial class SettlementView : PanelContainer
{
    private Label _heading = null!;
    private Label _timeLabel = null!;
    private Label _stockpileLabel = null!;
    private VBoxContainer _residentList = null!;
    private Button? _selectedButton;

    public event Action<EntityId>? ResidentSelected;

    public override void _Ready()
    {
        var content = GetNode<VBoxContainer>("Content");
        _heading = GetNode<Label>("Content/Heading");
        _timeLabel = GetNode<Label>("Content/Time");
        _stockpileLabel = GetNode<Label>("Content/Stockpile");
        _residentList = GetNode<VBoxContainer>("Content/Residents");

        DesignSystem.ApplySurface(this, UiSurface.Window);
        DesignSystem.ApplyStack(content, UiGap.Medium);
        DesignSystem.ApplyStack(_residentList, UiGap.Small);
        DesignSystem.ApplyText(_heading, UiTextRole.Display);
        DesignSystem.ApplyText(_timeLabel, UiTextRole.Metric);
        DesignSystem.ApplyText(_stockpileLabel, UiTextRole.Muted);
        RefreshLocalization();
    }

    public void RefreshLocalization()
    {
        if (_heading is not null)
        {
            _heading.Text = GameLocalization.Tr("UI_SETTLEMENT");
        }
    }

    public void Render(SettlementProjection projection, EntityId selectedResidentId)
    {
        ArgumentNullException.ThrowIfNull(projection);

        _timeLabel.Text = GameLocalization.Format(
            "UI_DAY_TIME",
            projection.Day,
            projection.Hour.ToString("00", System.Globalization.CultureInfo.InvariantCulture));
        _stockpileLabel.Text = GameLocalization.Format(
            "UI_STOCKPILE",
            string.Join(
                " · ",
                projection.Stockpile.Select(stack => $"{LocalizedContent.Item(stack.ItemId)} {stack.Quantity}")));

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
                Text = GameLocalization.Format(
                    "UI_RESIDENT_ROW",
                    selected ? "▶ " : string.Empty,
                    resident.Name,
                    LocalizedContent.Profession(resident.Profession),
                    LocalizedContent.Activity(resident.Activity)),
                Alignment = HorizontalAlignment.Left,
                AutoTranslateMode = Node.AutoTranslateModeEnum.Disabled,
            };
            DesignSystem.ApplyButton(
                button,
                selected ? UiButtonRole.SelectedRow : UiButtonRole.Row);
            if (selected)
            {
                _selectedButton = button;
            }

            button.Pressed += () => ResidentSelected?.Invoke(residentId);
            _residentList.AddChild(button);
        }
    }

    public void FocusSelected() => _selectedButton?.GrabFocus();
}
