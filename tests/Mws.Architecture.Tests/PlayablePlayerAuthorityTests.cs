using Xunit;

namespace Mws.Architecture.Tests;

public sealed class PlayablePlayerAuthorityTests
{
    [Fact]
    public void PlayerAuthorityLivesInWorldRuntimeNotGodotPlayerPresentation()
    {
        var root = FindRepositoryRoot();
        var client = Path.Combine(root, "src", "Mws.Client.Godot");
        var session = File.ReadAllText(Path.Combine(client, "Session", "GameWorldSession.cs"));

        Assert.Contains("_world.AddPlayerActor(_settlementScopeId)", session, StringComparison.Ordinal);
        Assert.Contains("public EntityId PlayerId => _playerId;", session, StringComparison.Ordinal);
        Assert.Contains("public WorldPlayerProjection Player => _world.ProjectPlayer();", session, StringComparison.Ordinal);
        Assert.Contains("WorldRuntime.Restore(checkpoint)", session, StringComparison.Ordinal);
        Assert.DoesNotContain("WorldPlayerActorState", session, StringComparison.Ordinal);

        var playerPresentation = Path.Combine(client, "World", "Player");
        foreach (var file in Directory.EnumerateFiles(playerPresentation, "*.cs", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(file);
            Assert.DoesNotContain("WorldPlayer", text, StringComparison.Ordinal);
            Assert.DoesNotContain("SettlementItems", text, StringComparison.Ordinal);
        }
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

        throw new InvalidOperationException(
            "Repository root containing WorldSimulate.sln was not found.");
    }
}
