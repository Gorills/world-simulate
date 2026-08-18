using Godot;
using Mws.Client.Godot.Input;
using Mws.Client.Godot.Localization;
using Mws.Client.Godot.UI.Screens.Hud;
using Mws.Client.Godot.UI.Theme;
using Mws.Client.Godot.World.Village;
using Mws.Simulation.Api;

namespace Mws.Client.Godot.Debug.VillageMonitor;

public partial class VillageDebugOverlay : Control
{
    private const double RefreshIntervalSeconds = 0.15;

    private VillageWorld? _world;
    private GameHud? _hud;
    private VillageDebugMap _map = null!;
    private Label _title = null!;
    private Label _badge = null!;
    private Label _summary = null!;
    private Label _time = null!;
    private Label _details = null!;
    private Label _legend = null!;
    private double _refreshRemaining;

    public override void _Ready()
    {
        DebugInput.ConfigureDefaults();

        var panel = GetNode<PanelContainer>("Anchor/Panel");
        var root = GetNode<VBoxContainer>("Anchor/Panel/Root");
        var titleRow = GetNode<HBoxContainer>("Anchor/Panel/Root/TitleRow");
        var divider = GetNode<HSeparator>("Anchor/Panel/Root/Divider");
        var body = GetNode<HBoxContainer>("Anchor/Panel/Root/Body");
        var mapPanel = GetNode<PanelContainer>("Anchor/Panel/Root/Body/MapPanel");
        var detailsPanel = GetNode<PanelContainer>("Anchor/Panel/Root/Body/DetailsPanel");
        var detailsScroll = GetNode<ScrollContainer>(
            "Anchor/Panel/Root/Body/DetailsPanel/DetailsScroll");

        _map = GetNode<VillageDebugMap>("Anchor/Panel/Root/Body/MapPanel/Map");
        _title = GetNode<Label>("Anchor/Panel/Root/TitleRow/Title");
        _badge = GetNode<Label>("Anchor/Panel/Root/TitleRow/Badge");
        _summary = GetNode<Label>("Anchor/Panel/Root/Summary");
        _time = GetNode<Label>("Anchor/Panel/Root/Time");
        _details = GetNode<Label>("Anchor/Panel/Root/Body/DetailsPanel/DetailsScroll/Details");
        _legend = GetNode<Label>("Anchor/Panel/Root/Legend");

        DesignSystem.ApplySurface(panel, UiSurface.Window);
        DesignSystem.ApplyStack(root, UiGap.Small);
        DesignSystem.ApplyStack(titleRow, UiGap.Small);
        DesignSystem.ApplyStack(body, UiGap.Small);
        DesignSystem.ApplySurface(mapPanel, UiSurface.Inset);
        DesignSystem.ApplySurface(detailsPanel, UiSurface.Inset);
        DesignSystem.ApplyScroll(detailsScroll);
        DesignSystem.ApplyDivider(divider);
        DesignSystem.ApplyText(_title, UiTextRole.Heading);
        DesignSystem.ApplyBadge(_badge, UiTone.Info);
        DesignSystem.ApplyText(_summary, UiTextRole.Body);
        DesignSystem.ApplyText(_time, UiTextRole.Caption);
        DesignSystem.ApplyText(_details, UiTextRole.Caption);
        DesignSystem.ApplyText(_legend, UiTextRole.Caption);

        var parent = GetParent();
        _world = parent?.GetNodeOrNull<VillageWorld>("VillageWorld");
        _hud = parent?.GetNodeOrNull<GameHud>("GameHud");
        if (_world is null || _hud is null)
        {
            GD.PushWarning("MWS_DEBUG_VILLAGE_OBSERVER_DISABLED missing sibling VillageWorld or GameHud.");
            SetProcessInput(false);
        }

        GameLocalization.RegisterUiRefresh(RefreshAllUi);
        Visible = false;
        SetProcess(false);
        RefreshStaticText();
    }

    public override void _ExitTree() => GameLocalization.UnregisterUiRefresh(RefreshAllUi);

    public override void _Input(InputEvent @event)
    {
        if (!DebugInput.IsToggle(@event))
        {
            return;
        }

        Visible = !Visible;
        SetProcess(Visible);
        _refreshRemaining = 0.0;
        if (Visible)
        {
            RefreshAllUi();
        }

        GetViewport().SetInputAsHandled();
    }

    public override void _Process(double delta)
    {
        _refreshRemaining -= delta;
        if (_refreshRemaining > 0.0)
        {
            return;
        }

        _refreshRemaining = RefreshIntervalSeconds;
        RefreshAllUi();
    }

    private void RefreshAllUi()
    {
        RefreshStaticText();
        if (!Visible || _world is null || _hud is null)
        {
            return;
        }

        var projection = _hud.CaptureDebugProjection();
        if (projection is null)
        {
            _summary.Text = GameLocalization.Tr("UI_DEBUG_NO_DATA");
            _time.Text = string.Empty;
            _details.Text = string.Empty;
            _map.SetSnapshot(null);
            return;
        }

        var snapshot = _world.CaptureDebugSnapshot(projection);
        _map.SetSnapshot(snapshot);
        _summary.Text = GameLocalization.Format(
            "UI_DEBUG_SUMMARY",
            projection.Residents.Count,
            snapshot.Residents.Count,
            projection.Residents.Count(entry => entry.Activity == ResidentActivity.Working),
            projection.Residents.Count(entry => entry.Activity == ResidentActivity.Eating),
            projection.Residents.Count(entry => entry.Activity == ResidentActivity.Resting),
            projection.Residents.Count(entry => entry.Activity == ResidentActivity.Idle));
        _time.Text = GameLocalization.Format(
            "UI_DEBUG_TIME",
            snapshot.Day,
            snapshot.Hour,
            snapshot.PlayerPosition.X,
            snapshot.PlayerPosition.Z);
        _details.Text = snapshot.Residents.Count == 0
            ? GameLocalization.Tr("UI_DEBUG_NO_DATA")
            : string.Join("\n", snapshot.Residents.Select(entry => FormatResident(projection, entry)));
    }

    private static string FormatResident(
        SettlementProjection projection,
        VillageDebugResidentSnapshot resident)
    {
        var movement = resident.LocationKind == SettlementActorLocationKind.Travelling ? "→" : "✓";
        var destination = FormatPlace(projection, resident.DestinationPlace);
        var progress = resident.LocationKind == SettlementActorLocationKind.Travelling
            ? $"{resident.TravelElapsedMilliseconds}/{resident.TravelDurationMilliseconds}ms"
            : "AtPlace";
        var row = GameLocalization.Format(
            "UI_DEBUG_RESIDENT_ROW",
            resident.Name,
            resident.Id.Value,
            LocalizedContent.Activity(resident.Activity),
            resident.Hunger,
            resident.Energy,
            destination,
            movement,
            resident.DistanceToDestination,
            progress);
        return resident.PlacementMatchesAuthority
            ? row
            : $"{row}\n  ! {GameLocalization.Tr("UI_DEBUG_PLACEMENT_MISMATCH")}";
    }

    private static string FormatPlace(
        SettlementProjection projection,
        SettlementPlaceRef place) =>
        place.Kind switch
        {
            SettlementPlaceKind.Home => LocalizedContent.Home(projection, place.EntityId),
            SettlementPlaceKind.Workplace => LocalizedContent.Workplace(
                projection.Workplaces.Single(entry => entry.Id == place.EntityId).Name),
            SettlementPlaceKind.Settlement => GameLocalization.Tr("UI_SETTLEMENT"),
            _ => $"{place.Kind} #{place.EntityId.Value}",
        };

    private void RefreshStaticText()
    {
        if (_title is null)
        {
            return;
        }

        _title.Text = GameLocalization.Tr("UI_DEBUG_TITLE");
        _badge.Text = "DEV";
        _legend.Text = GameLocalization.Tr("UI_DEBUG_LEGEND");
    }
}
