using Xunit;

namespace Mws.Architecture.Tests;

public sealed class GodotClientQualityTests
{
    [Fact]
    public void RawInputBindingsStayInsideInputModule()
    {
        var client = FindClientRoot();
        string[] forbiddenTokens =
        [
            "Key.",
            "JoyButton.",
            "JoyAxis.",
            "InputMap.",
            "InputEventJoypadButton",
            "InputEventJoypadMotion",
        ];

        foreach (var file in ClientSourceFiles(client))
        {
            var relative = Relative(client, file);
            if (relative.StartsWith("Input/", StringComparison.Ordinal))
            {
                continue;
            }

            var text = File.ReadAllText(file);
            foreach (var token in forbiddenTokens)
            {
                Assert.False(
                    text.Contains(token, StringComparison.Ordinal),
                    $"Raw input binding token '{token}' leaked outside Input/: {relative}");
            }
        }
    }

    [Fact]
    public void PresentationModulesDoNotReferenceSimulationRuntime()
    {
        var client = FindClientRoot();

        foreach (var file in ClientSourceFiles(client))
        {
            var relative = Relative(client, file);
            if (relative.StartsWith("Session/", StringComparison.Ordinal))
            {
                continue;
            }

            var text = File.ReadAllText(file);
            Assert.False(
                text.Contains("Mws.Simulation.Runtime", StringComparison.Ordinal),
                $"Only Session/ may orchestrate the authoritative runtime: {relative}");
        }
    }

    [Fact]
    public void ThemeImplementationStaysInsideThemeModule()
    {
        var client = FindClientRoot();
        string[] forbiddenTokens =
        [
            "AddThemeColorOverride",
            "AddThemeFontSizeOverride",
            "DesignTokens.",
        ];

        foreach (var file in ClientSourceFiles(client))
        {
            var relative = Relative(client, file);
            if (relative.StartsWith("UI/Theme/", StringComparison.Ordinal))
            {
                continue;
            }

            var text = File.ReadAllText(file);
            foreach (var token in forbiddenTokens)
            {
                Assert.False(
                    text.Contains(token, StringComparison.Ordinal),
                    $"Theme implementation token '{token}' leaked outside UI/Theme/: {relative}");
            }
        }
    }

    [Fact]
    public void SceneScriptsStayNextToTheirOwningScenes()
    {
        var client = FindClientRoot();

        foreach (var scene in Directory.EnumerateFiles(client, "*.tscn", SearchOption.AllDirectories))
        {
            var sceneRelative = Relative(client, scene);
            var sceneDirectory = Normalize(Path.GetDirectoryName(sceneRelative) ?? string.Empty);

            foreach (var line in File.ReadLines(scene))
            {
                if (!line.Contains("type=\"Script\"", StringComparison.Ordinal))
                {
                    continue;
                }

                const string marker = "path=\"res://";
                var start = line.IndexOf(marker, StringComparison.Ordinal);
                Assert.True(start >= 0, $"Script resource has no res:// path: {sceneRelative}");
                start += marker.Length;
                var end = line.IndexOf('"', start);
                Assert.True(end > start, $"Script resource path is malformed: {sceneRelative}");

                var scriptPath = line[start..end];
                var scriptDirectory = Normalize(Path.GetDirectoryName(scriptPath) ?? string.Empty);
                Assert.Equal(sceneDirectory, scriptDirectory);
            }
        }
    }

    [Fact]
    public void GodotProjectUsesStableCompositionRootAndStretchMode()
    {
        var client = FindClientRoot();
        var project = File.ReadAllText(Path.Combine(client, "project.godot"));

        Assert.Contains("run/main_scene=\"res://App/Main.tscn\"", project, StringComparison.Ordinal);
        Assert.Contains("window/stretch/mode=\"canvas_items\"", project, StringComparison.Ordinal);
    }

    private static IEnumerable<string> ClientSourceFiles(string client) =>
        Directory.EnumerateFiles(client, "*.cs", SearchOption.AllDirectories)
            .Where(file =>
            {
                var relative = Relative(client, file);
                return !relative.StartsWith("bin/", StringComparison.Ordinal)
                    && !relative.StartsWith("obj/", StringComparison.Ordinal);
            });

    private static string FindClientRoot() =>
        Path.Combine(FindRepositoryRoot(), "src", "Mws.Client.Godot");

    private static string Relative(string root, string path) =>
        Normalize(Path.GetRelativePath(root, path));

    private static string Normalize(string path) => path.Replace('\\', '/');

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
