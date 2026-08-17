using Xunit;

namespace Mws.Architecture.Tests;

public sealed class PlayableWorldRuntimeBoundaryTests
{
    [Fact]
    public void PlayableSessionOwnsWorldRuntimeInsteadOfSettlementSimulation()
    {
        var root = FindRepositoryRoot();
        var client = Path.Combine(root, "src", "Mws.Client.Godot");
        var sessionRoot = Path.Combine(client, "Session");
        var sessionPath = Path.Combine(sessionRoot, "GameWorldSession.cs");

        Assert.True(File.Exists(sessionPath));
        Assert.False(File.Exists(Path.Combine(sessionRoot, "GameSession.cs")));

        var session = File.ReadAllText(sessionPath);
        Assert.Contains("private readonly WorldRuntime _world;", session, StringComparison.Ordinal);
        Assert.Contains("WorldRuntime.Create(seed)", session, StringComparison.Ordinal);
        Assert.Contains("_world.AddDefaultSettlement()", session, StringComparison.Ordinal);
        Assert.Contains("_world.AdvanceHours(", session, StringComparison.Ordinal);
        Assert.Contains("_world.ProjectSettlement(", session, StringComparison.Ordinal);
        Assert.Contains("_world.ExecuteResidentInteraction(", session, StringComparison.Ordinal);
        Assert.Contains("_world.CreateCheckpoint()", session, StringComparison.Ordinal);
        Assert.Contains("WorldRuntime.Restore(checkpoint)", session, StringComparison.Ordinal);

        foreach (var file in Directory.EnumerateFiles(client, "*.cs", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(client, file).Replace('\\', '/');
            if (relative.StartsWith("bin/", StringComparison.Ordinal)
                || relative.StartsWith("obj/", StringComparison.Ordinal))
            {
                continue;
            }

            Assert.DoesNotContain(
                "SettlementSimulation",
                File.ReadAllText(file),
                StringComparison.Ordinal);
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
