using Godot;

namespace Mws.Client.Godot.World.Player;

internal sealed record PlayerControlProfile(
    float WalkSpeed,
    float RunSpeed,
    float GroundAcceleration,
    float SprintAcceleration,
    float GroundTurnAcceleration,
    float GroundBraking,
    float AirAcceleration,
    float Gravity,
    float JumpSpeed,
    float CoyoteTimeSeconds,
    float JumpBufferSeconds,
    float VisualTurnResponsiveness,
    float FloorSnapLength,
    float FloorMaxAngleDegrees,
    float MouseLookRadiansPerPixel,
    float GamepadLookRadiansPerSecond,
    float CameraLookResponsiveness,
    float MinimumPitchDegrees,
    float MaximumPitchDegrees,
    float CameraSpringLength,
    float CameraShoulderOffset,
    float CameraCollisionMargin,
    float CameraBaseFov,
    float CameraRunFov,
    float CameraFovResponsiveness);

internal static class PlayerControlProfiles
{
    internal static readonly PlayerControlProfile Exploration = new(
        WalkSpeed: 3.3f,
        RunSpeed: 6.2f,
        GroundAcceleration: 13.0f,
        SprintAcceleration: 6.5f,
        GroundTurnAcceleration: 11.0f,
        GroundBraking: 18.0f,
        AirAcceleration: 3.2f,
        Gravity: 19.5f,
        JumpSpeed: 6.2f,
        CoyoteTimeSeconds: 0.12f,
        JumpBufferSeconds: 0.14f,
        VisualTurnResponsiveness: 9.0f,
        FloorSnapLength: 0.32f,
        FloorMaxAngleDegrees: 46.0f,
        MouseLookRadiansPerPixel: 0.00235f,
        GamepadLookRadiansPerSecond: 2.35f,
        CameraLookResponsiveness: 20.0f,
        MinimumPitchDegrees: -48.0f,
        MaximumPitchDegrees: 32.0f,
        CameraSpringLength: 4.8f,
        CameraShoulderOffset: 0.48f,
        CameraCollisionMargin: 0.2f,
        CameraBaseFov: 66.0f,
        CameraRunFov: 71.0f,
        CameraFovResponsiveness: 5.5f);
}
