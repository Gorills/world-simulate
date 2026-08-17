using Godot;
using Mws.Client.Godot.Input;

namespace Mws.Client.Godot.World.Player;

public partial class ThirdPersonPlayer : CharacterBody3D
{
    private static readonly PlayerControlProfile ControlProfile = PlayerControlProfiles.Exploration;

    private Node3D _visual = null!;
    private Node3D _yawPivot = null!;
    private RayCast3D _interactionRay = null!;
    private PlayerMotor _motor = null!;
    private ThirdPersonCameraController _cameraController = null!;
    private bool _inputEnabled = true;

    internal PlayerMotionState MotionState => _motor?.State ?? PlayerMotionState.Idle;

    public override void _Ready()
    {
        _visual = GetNode<Node3D>("Visual");
        _yawPivot = GetNode<Node3D>("CameraYaw");
        var pitchPivot = GetNode<Node3D>("CameraYaw/CameraPitch");
        var springArm = GetNode<SpringArm3D>("CameraYaw/CameraPitch/SpringArm");
        var camera = GetNode<Camera3D>("CameraYaw/CameraPitch/SpringArm/Camera");
        _interactionRay = GetNode<RayCast3D>("CameraYaw/CameraPitch/SpringArm/Camera/InteractionRay");

        _motor = new PlayerMotor(ControlProfile);
        _motor.Configure(this);
        _cameraController = new ThirdPersonCameraController(
            ControlProfile,
            _yawPivot,
            pitchPivot,
            springArm,
            camera);
        _cameraController.Configure();

        if (string.Equals(DisplayServer.GetName(), "headless", StringComparison.OrdinalIgnoreCase))
        {
            _inputEnabled = false;
            SetPhysicsProcess(false);
            return;
        }

        global::Godot.Input.MouseMode = global::Godot.Input.MouseModeEnum.Captured;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_inputEnabled)
        {
            return;
        }

        var step = (float)delta;
        var input = GameInput.ReadPlayerMotion();
        _cameraController.Tick(input.CameraLook, _motor.State, step);

        var direction = ResolveWorldDirection(input.Movement);
        _motor.Tick(
            this,
            _visual,
            direction,
            input.Movement.Length(),
            input.SprintHeld,
            input.JumpPressed,
            step);
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!_inputEnabled || !GameInput.TryReadPointerLook(@event, out var pointerDelta))
        {
            return;
        }

        _cameraController.ApplyPointerDelta(pointerDelta);
        GetViewport().SetInputAsHandled();
    }

    internal GodotObject? GetInteractionCollider()
    {
        if (!_inputEnabled)
        {
            return null;
        }

        _interactionRay.ForceRaycastUpdate();
        return _interactionRay.IsColliding() ? _interactionRay.GetCollider() : null;
    }

    internal void SetInputEnabled(bool enabled)
    {
        _inputEnabled = enabled;
        if (!enabled && _motor is not null)
        {
            _motor.Stop(this);
        }

        if (!string.Equals(DisplayServer.GetName(), "headless", StringComparison.OrdinalIgnoreCase))
        {
            global::Godot.Input.MouseMode = enabled
                ? global::Godot.Input.MouseModeEnum.Captured
                : global::Godot.Input.MouseModeEnum.Visible;
        }
    }

    private Vector3 ResolveWorldDirection(Vector2 movement)
    {
        var forward = -_yawPivot.GlobalTransform.Basis.Z;
        var right = _yawPivot.GlobalTransform.Basis.X;
        forward.Y = 0.0f;
        right.Y = 0.0f;
        forward = forward.Normalized();
        right = right.Normalized();

        var direction = (right * movement.X) + (forward * movement.Y);
        return direction.LengthSquared() > 1.0f ? direction.Normalized() : direction;
    }
}
