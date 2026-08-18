using Godot;
using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Client.Godot.World.Village;

internal sealed partial class VillageResidentView : Node3D
{
    private const double TravelInterpolationSeconds = 0.10;
    private const float PositionEpsilonMeters = 0.001f;

    private MeshInstance3D? _selectionMarker;
    private Vector3 _authoritativeTargetPosition;
    private Vector3 _interpolationStartPosition;
    private double _interpolationElapsedSeconds;
    private bool _hasPresentationPosition;
    private bool _isInterpolating;
    private SettlementActorLocationKind? _lastLocationKind;

    internal EntityId ResidentId { get; private set; }

    internal Vector3 AuthoritativeTargetPosition => _authoritativeTargetPosition;

    internal void Initialize(ResidentProjection resident)
    {
        ArgumentNullException.ThrowIfNull(resident);
        ResidentId = resident.Id;
        Name = $"Resident-{resident.Id.Value}";

        var variant = (int)(Math.Abs(resident.Id.Value) % 3);
        var totalHeight = 1.68f + (variant * 0.09f);
        var torsoHeight = 0.78f + (variant * 0.04f);
        var legHeight = totalHeight - torsoHeight - 0.42f;
        var outfit = OutfitColor(resident.Profession, variant);
        var skin = SkinColor(variant);

        AddBox(
            "LeftLeg",
            new Vector3(-0.16f, legHeight * 0.5f, 0.0f),
            new Vector3(0.22f, legHeight, 0.28f),
            new Color(0.19f, 0.18f, 0.17f));
        AddBox(
            "RightLeg",
            new Vector3(0.16f, legHeight * 0.5f, 0.0f),
            new Vector3(0.22f, legHeight, 0.28f),
            new Color(0.19f, 0.18f, 0.17f));
        AddBox(
            "Torso",
            new Vector3(0.0f, legHeight + (torsoHeight * 0.5f), 0.0f),
            new Vector3(0.62f + (variant * 0.04f), torsoHeight, 0.38f),
            outfit);
        AddBox(
            "Head",
            new Vector3(0.0f, legHeight + torsoHeight + 0.22f, 0.0f),
            new Vector3(0.42f, 0.42f, 0.40f),
            skin);

        var roleMarker = new MeshInstance3D
        {
            Name = "RoleMarker",
            Position = new Vector3(0.0f, legHeight + torsoHeight + 0.48f, 0.0f),
            Mesh = new BoxMesh { Size = new Vector3(0.44f, 0.10f, 0.44f) },
            MaterialOverride = Material(RoleMarkerColor(resident.Profession)),
        };
        AddChild(roleMarker);

        _selectionMarker = new MeshInstance3D
        {
            Name = "SelectionMarker",
            Position = new Vector3(0.0f, 0.025f, 0.0f),
            Mesh = new BoxMesh { Size = new Vector3(0.95f, 0.05f, 0.95f) },
            MaterialOverride = Material(new Color(0.92f, 0.73f, 0.22f)),
            Visible = false,
        };
        AddChild(_selectionMarker);

        var interaction = new VillageInteractionArea();
        interaction.Initialize(
            VillageInteractionTarget.ForResident(resident.Id, resident.Name),
            new BoxShape3D { Size = new Vector3(0.95f, totalHeight, 0.95f) },
            new Vector3(0.0f, totalHeight * 0.5f, 0.0f));
        AddChild(interaction);
    }

    public override void _Process(double delta)
    {
        if (!_isInterpolating)
        {
            return;
        }

        _interpolationElapsedSeconds += Math.Max(0.0, delta);
        var progress = (float)Math.Clamp(
            _interpolationElapsedSeconds / TravelInterpolationSeconds,
            0.0,
            1.0);
        Position = _interpolationStartPosition.Lerp(_authoritativeTargetPosition, progress);
        if (progress >= 1.0f)
        {
            Position = _authoritativeTargetPosition;
            _isInterpolating = false;
        }
    }

    internal void Render(
        ResidentProjection resident,
        bool selected,
        Vector3 authoritativePosition)
    {
        ArgumentNullException.ThrowIfNull(resident);
        if (resident.Id != ResidentId)
        {
            throw new InvalidOperationException("Resident view cannot be rebound to another authoritative entity.");
        }

        var locationKind = resident.Location?.Kind
            ?? throw new InvalidOperationException(
                $"Resident {resident.Id.Value} has no authoritative semantic location to present.");
        var targetChanged = !_hasPresentationPosition
            || _authoritativeTargetPosition.DistanceTo(authoritativePosition) > PositionEpsilonMeters;
        if (targetChanged)
        {
            var interpolate = _hasPresentationPosition
                && (_lastLocationKind == SettlementActorLocationKind.Travelling
                    || locationKind == SettlementActorLocationKind.Travelling);
            PresentAuthoritativePosition(authoritativePosition, interpolate);
        }

        _lastLocationKind = locationKind;
        if (_selectionMarker is not null)
        {
            _selectionMarker.Visible = selected;
        }
    }

    internal static void ValidateInterpolationContract()
    {
        var view = new VillageResidentView();
        try
        {
            var origin = Vector3.Zero;
            var target = new Vector3(10.0f, 0.0f, 0.0f);
            view.PresentAuthoritativePosition(origin, interpolate: false);
            view.PresentAuthoritativePosition(target, interpolate: true);
            if (view.Position.DistanceTo(origin) > PositionEpsilonMeters
                || view.AuthoritativeTargetPosition.DistanceTo(target) > PositionEpsilonMeters)
            {
                throw new InvalidOperationException(
                    "Resident presentation must keep visual position separate from the newest authoritative target.");
            }

            view._Process(TravelInterpolationSeconds * 0.5);
            var midpoint = origin.Lerp(target, 0.5f);
            if (view.Position.DistanceTo(midpoint) > 0.01f)
            {
                throw new InvalidOperationException(
                    "Resident presentation interpolation did not produce a smooth midpoint.");
            }

            view._Process(TravelInterpolationSeconds * 0.5);
            if (view.Position.DistanceTo(target) > PositionEpsilonMeters)
            {
                throw new InvalidOperationException(
                    "Resident presentation interpolation did not finish at the authoritative target.");
            }
        }
        finally
        {
            view.Free();
        }
    }

    private void PresentAuthoritativePosition(Vector3 target, bool interpolate)
    {
        if (!_hasPresentationPosition || !interpolate)
        {
            _authoritativeTargetPosition = target;
            Position = target;
            _interpolationElapsedSeconds = 0.0;
            _isInterpolating = false;
            _hasPresentationPosition = true;
            return;
        }

        _interpolationStartPosition = Position;
        _authoritativeTargetPosition = target;
        _interpolationElapsedSeconds = 0.0;
        _isInterpolating = true;
        _hasPresentationPosition = true;
    }

    private void AddBox(string name, Vector3 position, Vector3 size, Color color)
    {
        AddChild(new MeshInstance3D
        {
            Name = name,
            Position = position,
            Mesh = new BoxMesh { Size = size },
            MaterialOverride = Material(color),
        });
    }

    private static StandardMaterial3D Material(Color color) => new()
    {
        AlbedoColor = color,
        Roughness = 0.88f,
    };

    private static Color OutfitColor(ResidentProfession profession, int variant) => profession switch
    {
        ResidentProfession.Farmer => variant switch
        {
            0 => new Color(0.31f, 0.42f, 0.24f),
            1 => new Color(0.40f, 0.46f, 0.26f),
            _ => new Color(0.34f, 0.38f, 0.19f),
        },
        ResidentProfession.Cook => variant switch
        {
            0 => new Color(0.62f, 0.48f, 0.28f),
            1 => new Color(0.68f, 0.55f, 0.34f),
            _ => new Color(0.55f, 0.40f, 0.24f),
        },
        ResidentProfession.Forager => variant switch
        {
            0 => new Color(0.22f, 0.45f, 0.43f),
            1 => new Color(0.26f, 0.51f, 0.46f),
            _ => new Color(0.20f, 0.39f, 0.36f),
        },
        _ => new Color(0.45f, 0.45f, 0.45f),
    };

    private static Color RoleMarkerColor(ResidentProfession profession) => profession switch
    {
        ResidentProfession.Farmer => new Color(0.70f, 0.76f, 0.30f),
        ResidentProfession.Cook => new Color(0.86f, 0.58f, 0.26f),
        ResidentProfession.Forager => new Color(0.25f, 0.70f, 0.62f),
        _ => new Color(0.75f, 0.75f, 0.75f),
    };

    private static Color SkinColor(int variant) => variant switch
    {
        0 => new Color(0.76f, 0.58f, 0.45f),
        1 => new Color(0.63f, 0.44f, 0.32f),
        _ => new Color(0.84f, 0.68f, 0.54f),
    };
}
