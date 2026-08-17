using Godot;

namespace Mws.Client.Godot.World.Village;

internal sealed partial class VillageInteractionArea : Area3D
{
    internal const uint InteractionCollisionLayer = 1u << 2;

    internal VillageInteractionTarget Target { get; private set; } = null!;

    internal void Initialize(
        VillageInteractionTarget target,
        Shape3D shape,
        Vector3 localPosition)
    {
        ArgumentNullException.ThrowIfNull(target);
        ArgumentNullException.ThrowIfNull(shape);

        Target = target;
        Name = $"Interaction-{target.Kind}";
        Position = localPosition;
        CollisionLayer = InteractionCollisionLayer;
        CollisionMask = 0;
        AddChild(new CollisionShape3D
        {
            Name = "Collision",
            Shape = shape,
        });
    }
}
