namespace Mws.Client.Godot.World.Player;

internal enum PlayerGait
{
    Idle,
    Walking,
    Running,
    Airborne,
}

internal readonly record struct PlayerMotionState(
    PlayerGait Gait,
    float HorizontalSpeed,
    float DesiredSpeed,
    bool Grounded,
    bool SprintRequested,
    bool JustJumped,
    bool JustLanded)
{
    internal static PlayerMotionState Idle => new(
        PlayerGait.Idle,
        HorizontalSpeed: 0.0f,
        DesiredSpeed: 0.0f,
        Grounded: true,
        SprintRequested: false,
        JustJumped: false,
        JustLanded: false);
}
