using Godot;
using Mws.Simulation.Api;

namespace Mws.Client.Godot.World.Village;

internal sealed partial class VillageItemView : Node3D
{
    internal void Initialize(ItemStackProjection stack)
    {
        ArgumentNullException.ThrowIfNull(stack);
        Name = $"Item-{stack.ItemId}-{stack.StackId}";

        switch (stack.ItemId)
        {
            case SettlementItems.Grain:
                BuildGrain(stack.Quantity);
                break;
            case SettlementItems.Ration:
                BuildRations(stack.Quantity);
                break;
            case SettlementItems.Herb:
                BuildHerbs(stack.Quantity);
                break;
            default:
                BuildUnknown(stack.Quantity);
                break;
        }
    }

    private void BuildGrain(int quantity)
    {
        var color = new Color(0.72f, 0.58f, 0.26f);
        var count = Math.Clamp(quantity, 1, 3);
        for (var index = 0; index < count; index++)
        {
            AddBox(
                $"GrainSack{index}",
                new Vector3((index - 1) * 0.38f, 0.34f, (index % 2) * 0.16f),
                new Vector3(0.34f, 0.68f, 0.30f),
                color);
        }
    }

    private void BuildRations(int quantity)
    {
        var crate = new Color(0.48f, 0.25f, 0.16f);
        var band = new Color(0.74f, 0.61f, 0.36f);
        var width = 0.72f + Math.Min(quantity, 8) * 0.035f;
        AddBox("RationCrate", new Vector3(0.0f, 0.28f, 0.0f), new Vector3(width, 0.56f, 0.68f), crate);
        AddBox("RationBand", new Vector3(0.0f, 0.57f, 0.0f), new Vector3(0.14f, 0.05f, 0.72f), band);
    }

    private void BuildHerbs(int quantity)
    {
        var herb = new Color(0.22f, 0.52f, 0.24f);
        var twine = new Color(0.63f, 0.49f, 0.27f);
        var count = Math.Clamp(quantity, 1, 4);
        for (var index = 0; index < count; index++)
        {
            AddBox(
                $"HerbStem{index}",
                new Vector3((index - 1.5f) * 0.13f, 0.33f, (index % 2) * 0.06f),
                new Vector3(0.09f, 0.66f, 0.09f),
                herb);
        }

        AddBox("HerbTwine", new Vector3(0.0f, 0.25f, 0.02f), new Vector3(0.62f, 0.10f, 0.16f), twine);
    }

    private void BuildUnknown(int quantity)
    {
        var extent = 0.45f + Math.Min(quantity, 10) * 0.02f;
        AddBox(
            "UnknownItem",
            new Vector3(0.0f, extent * 0.5f, 0.0f),
            new Vector3(extent, extent, extent),
            new Color(0.50f, 0.50f, 0.52f));
    }

    private void AddBox(string name, Vector3 position, Vector3 size, Color color)
    {
        AddChild(new MeshInstance3D
        {
            Name = name,
            Position = position,
            Mesh = new BoxMesh { Size = size },
            MaterialOverride = new StandardMaterial3D
            {
                AlbedoColor = color,
                Roughness = 0.9f,
            },
        });
    }
}
