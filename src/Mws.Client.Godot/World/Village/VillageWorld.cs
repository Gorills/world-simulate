using Godot;
using Mws.Domain;
using Mws.Simulation.Api;
using Mws.Client.Godot.World.Player;

namespace Mws.Client.Godot.World.Village;

public partial class VillageWorld : Node3D
{
    private const float InteractionDistanceMeters = 3.6f;

    private readonly Dictionary<long, VillageResidentView> _residentViews = new();
    private Node3D _residentsRoot = null!;
    private Node3D _itemsRoot = null!;
    private Node3D _entrancesRoot = null!;
    private ThirdPersonPlayer _player = null!;
    private VillageInteractionTarget? _currentInteractionTarget;

    internal event Action<VillageInteractionTarget?>? InteractionTargetChanged;
    internal event Action<VillageInteractionTarget>? InteractionRequested;

    public override void _Ready()
    {
        _residentsRoot = GetNode<Node3D>("Residents");
        _itemsRoot = GetNode<Node3D>("Items");
        _entrancesRoot = GetNode<Node3D>("Entrances");
        _player = GetNode<ThirdPersonPlayer>("Player");
        ValidateSpatialContract();

        if (string.Equals(DisplayServer.GetName(), "headless", StringComparison.OrdinalIgnoreCase))
        {
            SetPhysicsProcess(false);
            return;
        }

        var geometry = GetNode<Node3D>("Geometry");
        VillageGeometryBuilder.Build(geometry);
        VillageLifeGeometryBuilder.Build(geometry);
        BuildEntranceTargets();
        _player.Position = VillageLayout.PlayerSpawn;
    }

    public override void _PhysicsProcess(double delta)
    {
        _ = delta;
        UpdateInteractionTarget();
    }

    internal void Render(SettlementProjection projection, EntityId selectedResidentId)
    {
        ArgumentNullException.ThrowIfNull(projection);
        if (string.Equals(DisplayServer.GetName(), "headless", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        RenderResidents(projection, selectedResidentId);
        RenderItems(projection.Stockpile);
    }

    internal bool TryRequestInteraction()
    {
        if (_currentInteractionTarget is null)
        {
            return false;
        }

        InteractionRequested?.Invoke(_currentInteractionTarget);
        return true;
    }

    internal void SetPlayerInputEnabled(bool enabled)
    {
        if (string.Equals(DisplayServer.GetName(), "headless", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _player.SetInputEnabled(enabled);
        if (!enabled)
        {
            SetInteractionTarget(null);
        }
    }

    internal static void ValidateSpatialContract() => VillageLayout.Validate();

    internal static void ValidateLifeProjection(SettlementProjection projection) =>
        VillageResidentPlacement.ValidateProjection(projection);

    private void UpdateInteractionTarget()
    {
        VillageInteractionTarget? next = null;
        if (_player.GetInteractionCollider() is VillageInteractionArea area
            && area.GlobalPosition.DistanceTo(_player.GlobalPosition) <= InteractionDistanceMeters)
        {
            next = area.Target;
        }

        SetInteractionTarget(next);
    }

    private void SetInteractionTarget(VillageInteractionTarget? target)
    {
        if (Equals(_currentInteractionTarget, target))
        {
            return;
        }

        _currentInteractionTarget = target;
        InteractionTargetChanged?.Invoke(target);
    }

    private void BuildEntranceTargets()
    {
        foreach (var placement in VillageLayout.Buildings)
        {
            var anchor = new Node3D
            {
                Name = $"Entrance-{placement.Name}",
                Position = placement.Position,
                RotationDegrees = new Vector3(0.0f, placement.YawDegrees, 0.0f),
            };
            _entrancesRoot.AddChild(anchor);

            var area = new VillageInteractionArea();
            area.Initialize(
                VillageInteractionTarget.ForEntrance(placement.Name),
                new BoxShape3D
                {
                    Size = new Vector3(placement.DoorWidth, 2.2f, 1.2f),
                },
                new Vector3(0.0f, 1.1f, (placement.Footprint.Y * 0.5f) + 0.65f));
            anchor.AddChild(area);
        }
    }

    private void RenderResidents(SettlementProjection projection, EntityId selectedResidentId)
    {
        var residents = projection.Residents;
        var activeIds = residents.Select(resident => resident.Id.Value).ToHashSet();
        foreach (var staleId in _residentViews.Keys.Where(id => !activeIds.Contains(id)).ToArray())
        {
            _residentViews[staleId].QueueFree();
            _residentViews.Remove(staleId);
        }

        foreach (var resident in residents)
        {
            if (!_residentViews.TryGetValue(resident.Id.Value, out var view))
            {
                view = new VillageResidentView();
                _residentsRoot.AddChild(view);
                view.Initialize(resident);
                _residentViews.Add(resident.Id.Value, view);
            }

            view.Position = VillageResidentPlacement.Resolve(resident, projection);
            view.Render(resident, resident.Id == selectedResidentId);
        }
    }

    private void RenderItems(IReadOnlyList<ItemStackProjection> stockpile)
    {
        foreach (var child in _itemsRoot.GetChildren())
        {
            child.QueueFree();
        }

        for (var index = 0; index < stockpile.Count; index++)
        {
            var stack = stockpile[index];
            var view = new VillageItemView();
            _itemsRoot.AddChild(view);
            view.Initialize(stack);
            var column = index % 4;
            var row = index / 4;
            view.Position = VillageLayout.StockpileOrigin + new Vector3(column * 1.35f, 0.0f, row * 1.35f);
        }
    }
}
