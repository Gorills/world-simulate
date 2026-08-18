using Xunit;

namespace Mws.Architecture.Tests;

public sealed class PlayerControlFoundationTests
{
    [Fact]
    public void ThirdPersonPlayerComposesMotorCameraAndCentralProfile()
    {
        var root = FindRepositoryRoot();
        var playerRoot = Path.Combine(root, "src", "Mws.Client.Godot", "World", "Player");
        var player = File.ReadAllText(Path.Combine(playerRoot, "ThirdPersonPlayer.cs"));
        var profile = File.ReadAllText(Path.Combine(playerRoot, "PlayerControlProfile.cs"));

        Assert.Contains("PlayerControlProfiles.Exploration", player, StringComparison.Ordinal);
        Assert.Contains("new PlayerMotor", player, StringComparison.Ordinal);
        Assert.Contains("new ThirdPersonCameraController", player, StringComparison.Ordinal);
        Assert.DoesNotContain("private const float WalkSpeed", player, StringComparison.Ordinal);
        Assert.DoesNotContain("private const float RunSpeed", player, StringComparison.Ordinal);
        Assert.DoesNotContain("private const float Gravity", player, StringComparison.Ordinal);

        Assert.Contains("WalkSpeed:", profile, StringComparison.Ordinal);
        Assert.Contains("RunSpeed:", profile, StringComparison.Ordinal);
        Assert.Contains("SprintAcceleration:", profile, StringComparison.Ordinal);
        Assert.Contains("JumpBufferSeconds:", profile, StringComparison.Ordinal);
        Assert.Contains("CoyoteTimeSeconds:", profile, StringComparison.Ordinal);
        Assert.Contains("CameraSpringLength:", profile, StringComparison.Ordinal);
        Assert.Contains("CameraRunFov:", profile, StringComparison.Ordinal);
    }

    [Fact]
    public void ExplorationInputReservesSpaceForJumpAndMovesPrototypeTimeAdvance()
    {
        var root = FindRepositoryRoot();
        var input = File.ReadAllText(
            Path.Combine(root, "src", "Mws.Client.Godot", "Input", "GameInput.cs"));

        Assert.Contains("BindKey(Jump, Key.Space)", input, StringComparison.Ordinal);
        Assert.Contains("BindButton(Jump, JoyButton.B)", input, StringComparison.Ordinal);
        Assert.Contains("BindKey(AdvanceTime, Key.T)", input, StringComparison.Ordinal);
        Assert.Contains("BindButton(AdvanceTime, JoyButton.Y)", input, StringComparison.Ordinal);
        Assert.DoesNotContain("BindKey(AdvanceTime, Key.Space)", input, StringComparison.Ordinal);
    }

    [Fact]
    public void MotionStateIsAnimationFacingAndCameraUsesShapeCollision()
    {
        var root = FindRepositoryRoot();
        var playerRoot = Path.Combine(root, "src", "Mws.Client.Godot", "World", "Player");
        var state = File.ReadAllText(Path.Combine(playerRoot, "PlayerMotionState.cs"));
        var scene = File.ReadAllText(Path.Combine(playerRoot, "ThirdPersonPlayer.tscn"));
        var contract = File.ReadAllText(Path.Combine(root, "DESIGN", "PLAYER_CONTROL_SYSTEM.md"));

        Assert.Contains("Walking", state, StringComparison.Ordinal);
        Assert.Contains("Running", state, StringComparison.Ordinal);
        Assert.Contains("Airborne", state, StringComparison.Ordinal);
        Assert.Contains("JustJumped", state, StringComparison.Ordinal);
        Assert.Contains("JustLanded", state, StringComparison.Ordinal);
        Assert.Contains("SphereShape3D", scene, StringComparison.Ordinal);
        Assert.Contains("shape = SubResource(\"SphereShape_camera\")", scene, StringComparison.Ordinal);
        Assert.Contains("PlayerControlProfile", contract, StringComparison.Ordinal);
        Assert.Contains("PlayerMotionState", contract, StringComparison.Ordinal);
    }

    private static string FindRepositoryRoot()
    {
        DirectoryInfo? directory = new(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "WorldSimulate.sln")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Repository root containing WorldSimulate.sln was not found.");
    }
}
