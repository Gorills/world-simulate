using Godot;

namespace Mws.Client.Godot.World.Player;

internal sealed class PlayerMotor
{
    private readonly PlayerControlProfile _profile;
    private float _coyoteRemaining;
    private float _jumpBufferRemaining;
    private bool _wasGrounded = true;

    internal PlayerMotor(PlayerControlProfile profile)
    {
        _profile = profile ?? throw new ArgumentNullException(nameof(profile));
    }

    internal PlayerMotionState State { get; private set; } = PlayerMotionState.Idle;

    internal void Configure(CharacterBody3D body)
    {
        ArgumentNullException.ThrowIfNull(body);
        body.FloorSnapLength = _profile.FloorSnapLength;
        body.FloorMaxAngle = Mathf.DegToRad(_profile.FloorMaxAngleDegrees);
        body.FloorConstantSpeed = true;
        body.FloorStopOnSlope = true;
    }

    internal void Tick(
        CharacterBody3D body,
        Node3D visual,
        Vector3 desiredDirection,
        float inputAmount,
        bool sprintHeld,
        bool jumpPressed,
        float delta)
    {
        ArgumentNullException.ThrowIfNull(body);
        ArgumentNullException.ThrowIfNull(visual);

        var groundedBeforeMove = body.IsOnFloor();
        UpdateJumpWindows(groundedBeforeMove, jumpPressed, delta);

        var horizontal = new Vector3(body.Velocity.X, 0.0f, body.Velocity.Z);
        var normalizedInput = Mathf.Clamp(inputAmount, 0.0f, 1.0f);
        var requestedSpeed = (sprintHeld ? _profile.RunSpeed : _profile.WalkSpeed) * normalizedInput;
        var targetHorizontal = desiredDirection * requestedSpeed;
        horizontal = MoveTowards(
            horizontal,
            targetHorizontal,
            ResolveAcceleration(horizontal, targetHorizontal, groundedBeforeMove, sprintHeld),
            delta);

        var vertical = body.Velocity.Y;
        var justJumped = false;
        if (_jumpBufferRemaining > 0.0f && _coyoteRemaining > 0.0f)
        {
            vertical = _profile.JumpSpeed;
            _jumpBufferRemaining = 0.0f;
            _coyoteRemaining = 0.0f;
            justJumped = true;
        }
        else if (groundedBeforeMove && vertical < 0.0f)
        {
            vertical = -0.5f;
        }
        else
        {
            vertical -= _profile.Gravity * delta;
        }

        body.Velocity = new Vector3(horizontal.X, vertical, horizontal.Z);
        body.MoveAndSlide();

        var grounded = body.IsOnFloor();
        var justLanded = !_wasGrounded && grounded && !justJumped;
        _wasGrounded = grounded;

        horizontal = new Vector3(body.Velocity.X, 0.0f, body.Velocity.Z);
        RotateVisual(visual, horizontal, delta);
        State = BuildState(horizontal.Length(), requestedSpeed, grounded, sprintHeld, justJumped, justLanded);
    }

    internal void Stop(CharacterBody3D body)
    {
        ArgumentNullException.ThrowIfNull(body);
        body.Velocity = Vector3.Zero;
        _coyoteRemaining = 0.0f;
        _jumpBufferRemaining = 0.0f;
        _wasGrounded = body.IsOnFloor();
        State = PlayerMotionState.Idle with { Grounded = _wasGrounded };
    }

    private void UpdateJumpWindows(bool grounded, bool jumpPressed, float delta)
    {
        _coyoteRemaining = grounded
            ? _profile.CoyoteTimeSeconds
            : Math.Max(0.0f, _coyoteRemaining - delta);
        _jumpBufferRemaining = jumpPressed
            ? _profile.JumpBufferSeconds
            : Math.Max(0.0f, _jumpBufferRemaining - delta);
    }

    private float ResolveAcceleration(
        Vector3 horizontal,
        Vector3 target,
        bool grounded,
        bool sprintHeld)
    {
        if (!grounded)
        {
            return _profile.AirAcceleration;
        }

        if (target.LengthSquared() <= 0.0001f)
        {
            return _profile.GroundBraking;
        }

        if (horizontal.LengthSquared() > 0.01f
            && Vector3.Dot(horizontal.Normalized(), target.Normalized()) < 0.55f)
        {
            return _profile.GroundTurnAcceleration;
        }

        if (sprintHeld && horizontal.Length() >= _profile.WalkSpeed * 0.9f)
        {
            return _profile.SprintAcceleration;
        }

        return _profile.GroundAcceleration;
    }

    private void RotateVisual(Node3D visual, Vector3 horizontal, float delta)
    {
        if (horizontal.LengthSquared() <= 0.01f)
        {
            return;
        }

        var targetYaw = Mathf.Atan2(-horizontal.X, -horizontal.Z);
        var rotation = visual.Rotation;
        var response = 1.0f - MathF.Exp(-_profile.VisualTurnResponsiveness * delta);
        rotation.Y = Mathf.LerpAngle(rotation.Y, targetYaw, response);
        visual.Rotation = rotation;
    }

    private PlayerMotionState BuildState(
        float horizontalSpeed,
        float desiredSpeed,
        bool grounded,
        bool sprintHeld,
        bool justJumped,
        bool justLanded)
    {
        var gait = !grounded
            ? PlayerGait.Airborne
            : horizontalSpeed < 0.15f
                ? PlayerGait.Idle
                : sprintHeld && horizontalSpeed > _profile.WalkSpeed + 0.25f
                    ? PlayerGait.Running
                    : PlayerGait.Walking;
        return new PlayerMotionState(
            gait,
            horizontalSpeed,
            desiredSpeed,
            grounded,
            sprintHeld,
            justJumped,
            justLanded);
    }

    private static Vector3 MoveTowards(Vector3 current, Vector3 target, float acceleration, float delta)
    {
        var difference = target - current;
        var distance = difference.Length();
        var maximumDelta = Math.Max(0.0f, acceleration * delta);
        if (distance <= maximumDelta || distance <= 0.0001f)
        {
            return target;
        }

        return current + (difference / distance * maximumDelta);
    }
}
