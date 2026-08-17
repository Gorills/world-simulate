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
    private Label _summary = null!;
    private Label _time = null!;
    private Label _details = null!;
    private Label _legend = null!;
    private double _refreshRemaining;

    public override void _Ready()
    {
        DebugInput.ConfigureDefaults();
        _map = GetNode<VillageDebugMap>("Anchor/Panel/Root/Body/Map");
        _title = GetNode<Label>("Anchor/Panel/Root/Title");
        _summary = GetNode<Label>("Anchor/Panel/Root/Summary");
        _time = GetNode<Label>("Anchor/Panel/Root/Time");
        _details = GetNode<Label>("Anchor/Panel/Root/Body/DetailsScroll/Details");
        _legend = GetNode<Label>("Anchor/Panel/Root/Legend");

        DesignSystem.ApplyHeading(_title);
        DesignSystem.ApplyLabel(_summary);
        DesignSystem.ApplyLabel(_time, muted: true);
        DesignSystem.ApplyLabel(_details);
        DesignSystem.ApplyLabel(_legend, muted: true);

        var parent = GetParent();
        _world = parent?.GetNodeOrNull<VillageWorld>("VillageWorld");
        _hud = parent?.GetNodeOrNull<GameHud>("GameHud");
        if (_world is null || _hud is null)
        {
            GD.PushWarning("MWS_DEBUG_VILLAGE_OBSERVER_DISABLED missing sibling VillageWorld or GameHud.");
            SetProcessInput(false);
        }

        GameLocalization.Changed += Refresh;
        Visible = false;
        SetProcess(false);
        RefreshStaticText();
    }

    public override void _ExitTree()
    {
        GameLocalization.Changed -= Refresh;
    }

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
            Refresh();
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
        Refresh();
    }

    private void Refresh()
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
        var movement = resident.DistanceToDestination > 0.25f ? "→" : "✓";
        var destination = resident.Activity switch
        {
            ResidentActivity.Working => LocalizedContent.Workplace(resident.WorkplaceName),
            ResidentActivity.Resting when resident.HomeId != default =>
                LocalizedContent.Home(projection, resident.HomeId),
            ResidentActivity.Resting => GameLocalization.Tr("UI_DEBUG_DEST_UNASSIGNED_HOME"),
            ResidentActivity.Eating => LocalizedContent.Building(VillageLayout.FoodBuildingName),
            _ => GameLocalization.Tr("UI_DEBUG_DEST_SOCIAL"),
        };
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
            resident.Route.Count);
        return resident.RouteMatchesActivity
            ? row
            : $"{row}\n  ! {GameLocalization.Tr("UI_DEBUG_ROUTE_MISMATCH")}";
    }

    private void RefreshStaticText()
    {
        if (_title is null)
        {
            return;
        }

        _title.Text = GameLocalization.Tr("UI_DEBUG_TITLE");
        _legend.Text = GameLocalization.Tr("UI_DEBUG_LEGEND");
    }
}
