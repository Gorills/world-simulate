using Godot;

namespace Mws.Client.Godot.World.Player;

internal sealed class ThirdPersonCameraController
{
    private readonly PlayerControlProfile _profile;
    private readonly Node3D _yawPivot;
    private readonly Node3D _pitchPivot;
    private readonly SpringArm3D _springArm;
    private readonly Camera3D _camera;
    private float _targetYaw;
    private float _targetPitch;

    internal ThirdPersonCameraController(
        PlayerControlProfile profile,
        Node3D yawPivot,
        Node3D pitchPivot,
        SpringArm3D springArm,
        Camera3D camera)
    {
        _profile = profile ?? throw new ArgumentNullException(nameof(profile));
        _yawPivot = yawPivot ?? throw new ArgumentNullException(nameof(yawPivot));
        _pitchPivot = pitchPivot ?? throw new ArgumentNullException(nameof(pitchPivot));
        _springArm = springArm ?? throw new ArgumentNullException(nameof(springArm));
        _camera = camera ?? throw new ArgumentNullException(nameof(camera));
    }

    internal void Configure()
    {
        _targetYaw = _yawPivot.Rotation.Y;
        _targetPitch = Mathf.DegToRad(-10.0f);
        var pitch = _pitchPivot.Rotation;
        pitch.X = _targetPitch;
        _pitchPivot.Rotation = pitch;

        _springArm.SpringLength = _profile.CameraSpringLength;
        _springArm.Margin = _profile.CameraCollisionMargin;
        var cameraPosition = _camera.Position;
        cameraPosition.X = _profile.CameraShoulderOffset;
        _camera.Position = cameraPosition;
        _camera.Fov = _profile.CameraBaseFov;
    }

    internal void Tick(Vector2 gamepadLook, PlayerMotionState motion, float delta)
    {
        if (gamepadLook.LengthSquared() > 0.0001f)
        {
            AddLook(new Vector2(
                -gamepadLook.X * _profile.GamepadLookRadiansPerSecond * delta,
                -gamepadLook.Y * _profile.GamepadLookRadiansPerSecond * delta));
        }

        var response = 1.0f - MathF.Exp(-_profile.CameraLookResponsiveness * delta);
        var yaw = _yawPivot.Rotation;
        yaw.Y = Mathf.LerpAngle(yaw.Y, _targetYaw, response);
        _yawPivot.Rotation = yaw;

        var pitch = _pitchPivot.Rotation;
        pitch.X = Mathf.LerpAngle(pitch.X, _targetPitch, response);
        _pitchPivot.Rotation = pitch;

        var targetFov = motion.Gait == PlayerGait.Running
            ? _profile.CameraRunFov
            : _profile.CameraBaseFov;
        var fovResponse = 1.0f - MathF.Exp(-_profile.CameraFovResponsiveness * delta);
        _camera.Fov = Mathf.Lerp(_camera.Fov, targetFov, fovResponse);
    }

    internal void ApplyPointerDelta(Vector2 pixels)
    {
        AddLook(new Vector2(
            -pixels.X * _profile.MouseLookRadiansPerPixel,
            -pixels.Y * _profile.MouseLookRadiansPerPixel));
    }

    private void AddLook(Vector2 radians)
    {
        _targetYaw += radians.X;
        _targetPitch = Mathf.Clamp(
            _targetPitch + radians.Y,
            Mathf.DegToRad(_profile.MinimumPitchDegrees),
            Mathf.DegToRad(_profile.MaximumPitchDegrees));
    }
}
