using Xunit;

namespace Mws.Architecture.Tests;

public sealed class ArchitectureBoundaryTests
{
    private static readonly string[] AuthoritativeCoreProjects =
    [
        "Mws.Domain",
        "Mws.Simulation.Api",
        "Mws.Simulation.Runtime",
        "Mws.Persistence.Json",
    ];

    private static readonly string[] GodotClientFolders =
    [
        "App",
        "Session",
        "Input",
        "World",
        "UI",
    ];

    [Fact]
    public void AuthoritativeCoreHasNoGodotDependency()
    {
        var root = FindRepositoryRoot();

        foreach (var project in AuthoritativeCoreProjects)
        {
            var directory = Path.Combine(root, "src", project);
            var files = Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories)
                .Where(path => path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)
                    || path.EndsWith(".csproj", StringComparison.OrdinalIgnoreCase));

            foreach (var file in files)
            {
                var text = File.ReadAllText(file);
                Assert.False(
                    text.Contains("Godot", StringComparison.OrdinalIgnoreCase),
                    $"Authoritative core file unexpectedly references Godot: {file}");
            }
        }
    }

    [Fact]
    public void CoreProjectsDoNotReferenceClientProject()
    {
        var root = FindRepositoryRoot();

        foreach (var project in AuthoritativeCoreProjects)
        {
            var projectFile = Path.Combine(root, "src", project, $"{project}.csproj");
            var text = File.ReadAllText(projectFile);
            Assert.False(
                text.Contains("Mws.Client.Godot", StringComparison.OrdinalIgnoreCase),
                $"Core project unexpectedly references the client project: {projectFile}");
        }
    }

    [Fact]
    public void GodotClientUsesFocusedFeatureFolders()
    {
        var root = FindRepositoryRoot();
        var client = Path.Combine(root, "src", "Mws.Client.Godot");

        foreach (var requiredDirectory in GodotClientFolders)
        {
            Assert.True(
                Directory.Exists(Path.Combine(client, requiredDirectory)),
                $"Godot client is missing required feature folder: {requiredDirectory}");
        }

        Assert.False(
            File.Exists(Path.Combine(client, "Main.cs")),
            "Legacy root Main.cs must not return; use App/Main.cs as the composition root.");
    }

    [Fact]
    public void GodotClientFilesStayWithinAgentFriendlySizeBudgets()
    {
        var root = FindRepositoryRoot();
        var client = Path.Combine(root, "src", "Mws.Client.Godot");
        var files = Directory.EnumerateFiles(client, "*.cs", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            var lines = File.ReadLines(file).Count();
            Assert.True(lines <= 300, $"Godot client file exceeds 300-line responsibility budget: {file} ({lines}).");
        }

        var compositionRoot = Path.Combine(client, "App", "Main.cs");
        var compositionRootLines = File.ReadLines(compositionRoot).Count();
        Assert.True(
            compositionRootLines <= 180,
            $"Godot composition root exceeds 180 lines: {compositionRootLines}.");
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
