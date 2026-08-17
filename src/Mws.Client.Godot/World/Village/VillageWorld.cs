using Godot;
using Mws.Domain;
using Mws.Simulation.Api;
using Mws.Client.Godot.World.Player;

namespace Mws.Client.Godot.World.Village;

public partial class VillageWorld : Node3D
{
    private readonly Dictionary<long, VillageResidentView> _residentViews = new();
    private Node3D _residentsRoot = null!;
    private Node3D _itemsRoot = null!;
    private ThirdPersonPlayer _player = null!;

    public override void _Ready()
    {
        _residentsRoot = GetNode<Node3D>("Residents");
        _itemsRoot = GetNode<Node3D>("Items");
        _player = GetNode<ThirdPersonPlayer>("Player");
        VillageLayout.Validate();

        if (string.Equals(DisplayServer.GetName(), "headless", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        VillageGeometryBuilder.Build(GetNode<Node3D>("Geometry"));
        _player.Position = VillageLayout.PlayerSpawn;
    }

    internal void Render(SettlementProjection projection, EntityId selectedResidentId)
    {
        ArgumentNullException.ThrowIfNull(projection);
        if (string.Equals(DisplayServer.GetName(), "headless", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        RenderResidents(projection.Residents, selectedResidentId);
        RenderItems(projection.Stockpile);
    }

    internal void SetPlayerInputEnabled(bool enabled)
    {
        if (string.Equals(DisplayServer.GetName(), "headless", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        _player.SetInputEnabled(enabled);
    }

    internal static void ValidateSpatialContract() => VillageLayout.Validate();

    private void RenderResidents(IReadOnlyList<ResidentProjection> residents, EntityId selectedResidentId)
    {
        var activeIds = residents.Select(resident => resident.Id.Value).ToHashSet();
        foreach (var staleId in _residentViews.Keys.Where(id => !activeIds.Contains(id)).ToArray())
        {
            _residentViews[staleId].QueueFree();
            _residentViews.Remove(staleId);
        }

        for (var index = 0; index < residents.Count; index++)
        {
            var resident = residents[index];
            if (!_residentViews.TryGetValue(resident.Id.Value, out var view))
            {
                view = new VillageResidentView();
                _residentsRoot.AddChild(view);
                view.Initialize(resident);
                _residentViews.Add(resident.Id.Value, view);
            }

            var spawn = VillageLayout.ResidentSpawns[index % VillageLayout.ResidentSpawns.Length];
            var row = index / VillageLayout.ResidentSpawns.Length;
            view.Position = spawn + new Vector3(row * 1.4f, 0.0f, row * 1.2f);
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
