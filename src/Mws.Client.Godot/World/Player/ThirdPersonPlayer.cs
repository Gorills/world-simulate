using Godot;
using Mws.Client.Godot.Input;

namespace Mws.Client.Godot.World.Player;

public partial class ThirdPersonPlayer : CharacterBody3D
{
    private const float WalkSpeed = 3.8f;
    private const float RunSpeed = 6.7f;
    private const float Acceleration = 17.0f;
    private const float Deceleration = 22.0f;
    private const float Gravity = 24.0f;
    private const float TurnResponsiveness = 10.0f;
    private const float MouseLookRadiansPerPixel = 0.0026f;
    private const float GamepadLookRadiansPerSecond = 2.4f;
    private static readonly float MinimumPitch = Mathf.DegToRad(-48.0f);
    private static readonly float MaximumPitch = Mathf.DegToRad(32.0f);

    private Node3D _visual = null!;
    private Node3D _yawPivot = null!;
    private Node3D _pitchPivot = null!;
    private bool _inputEnabled = true;

    public override void _Ready()
    {
        _visual = GetNode<Node3D>("Visual");
        _yawPivot = GetNode<Node3D>("CameraYaw");
        _pitchPivot = GetNode<Node3D>("CameraYaw/CameraPitch");
        FloorSnapLength = 0.25f;

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
        var step = (float)delta;
        if (!_inputEnabled)
        {
            Velocity = Vector3.Zero;
            return;
        }

        ApplyGamepadLook(step);
        var move = GameInput.ReadMovement();
        var forward = -_yawPivot.GlobalTransform.Basis.Z;
        var right = _yawPivot.GlobalTransform.Basis.X;
        forward.Y = 0.0f;
        right.Y = 0.0f;
        forward = forward.Normalized();
        right = right.Normalized();

        var direction = (right * move.X) + (forward * move.Y);
        if (direction.LengthSquared() > 1.0f)
        {
            direction = direction.Normalized();
        }

        var speed = global::Godot.Input.IsActionPressed(GameInput.Sprint) ? RunSpeed : WalkSpeed;
        var targetHorizontal = direction * speed;
        var horizontal = new Vector3(Velocity.X, 0.0f, Velocity.Z);
        horizontal = MoveTowards(
            horizontal,
            targetHorizontal,
            (direction.LengthSquared() > 0.0001f ? Acceleration : Deceleration) * step);

        var vertical = Velocity.Y;
        if (IsOnFloor() && vertical < 0.0f)
        {
            vertical = -0.5f;
        }
        else
        {
            vertical -= Gravity * step;
        }

        Velocity = new Vector3(horizontal.X, vertical, horizontal.Z);
        MoveAndSlide();

        if (direction.LengthSquared() > 0.0001f)
        {
            var targetYaw = Mathf.Atan2(-direction.X, -direction.Z);
            var rotation = _visual.Rotation;
            rotation.Y = Mathf.LerpAngle(
                rotation.Y,
                targetYaw,
                Mathf.Clamp(TurnResponsiveness * step, 0.0f, 1.0f));
            _visual.Rotation = rotation;
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!_inputEnabled || !GameInput.TryReadPointerLook(@event, out var pointerDelta))
        {
            return;
        }

        ApplyLook(new Vector2(
            -pointerDelta.X * MouseLookRadiansPerPixel,
            -pointerDelta.Y * MouseLookRadiansPerPixel));
        GetViewport().SetInputAsHandled();
    }

    internal void SetInputEnabled(bool enabled)
    {
        _inputEnabled = enabled;
        if (!enabled)
        {
            Velocity = Vector3.Zero;
        }

        if (!string.Equals(DisplayServer.GetName(), "headless", StringComparison.OrdinalIgnoreCase))
        {
            global::Godot.Input.MouseMode = enabled
                ? global::Godot.Input.MouseModeEnum.Captured
                : global::Godot.Input.MouseModeEnum.Visible;
        }
    }

    private void ApplyGamepadLook(float delta)
    {
        var look = GameInput.ReadCameraLook();
        if (look.LengthSquared() <= 0.0001f)
        {
            return;
        }

        ApplyLook(new Vector2(
            -look.X * GamepadLookRadiansPerSecond * delta,
            -look.Y * GamepadLookRadiansPerSecond * delta));
    }

    private void ApplyLook(Vector2 radians)
    {
        var yaw = _yawPivot.Rotation;
        yaw.Y += radians.X;
        _yawPivot.Rotation = yaw;

        var pitch = _pitchPivot.Rotation;
        pitch.X = Mathf.Clamp(pitch.X + radians.Y, MinimumPitch, MaximumPitch);
        _pitchPivot.Rotation = pitch;
    }

    private static Vector3 MoveTowards(Vector3 current, Vector3 target, float maximumDelta)
    {
        var delta = target - current;
        var distance = delta.Length();
        if (distance <= maximumDelta || distance <= 0.0001f)
        {
            return target;
        }

        return current + (delta / distance * maximumDelta);
    }
}
