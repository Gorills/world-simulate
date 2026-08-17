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
            "AddThemeStyleboxOverride",
            "AddThemeConstantOverride",
            "new StyleBoxFlat",
            "new StyleBoxLine",
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
    public void SceneFilesDoNotOwnThemeOverrides()
    {
        var client = FindClientRoot();
        foreach (var scene in Directory.EnumerateFiles(client, "*.tscn", SearchOption.AllDirectories))
        {
            foreach (var line in File.ReadLines(scene))
            {
                Assert.False(
                    line.Contains("theme_override_", StringComparison.Ordinal),
                    $"Scene styling must use DesignSystem semantics: {Relative(client, scene)} -> {line.Trim()}");
            }
        }
    }

    [Fact]
    public void DesignSystemDefinesScalableSemanticPrimitives()
    {
        var root = FindRepositoryRoot();
        var client = Path.Combine(root, "src", "Mws.Client.Godot");
        var semantics = File.ReadAllText(
            Path.Combine(client, "UI", "Theme", "UiSemantics.cs"));
        var designSystem = File.ReadAllText(
            Path.Combine(client, "UI", "Theme", "DesignSystem.cs"));
        var contract = File.ReadAllText(
            Path.Combine(root, "DESIGN", "UI_DESIGN_SYSTEM.md"));

        string[] semanticTypes =
        [
            "enum UiSurface",
            "enum UiTextRole",
            "enum UiButtonRole",
            "enum UiTone",
            "enum UiGap",
            "enum UiDataColor",
        ];
        foreach (var semanticType in semanticTypes)
        {
            Assert.Contains(semanticType, semantics, StringComparison.Ordinal);
        }

        Assert.Contains("ApplySurface", designSystem, StringComparison.Ordinal);
        Assert.Contains("ApplyButton", designSystem, StringComparison.Ordinal);
        Assert.Contains("ApplyBadge", designSystem, StringComparison.Ordinal);
        Assert.Contains("DataColor", designSystem, StringComparison.Ordinal);
        Assert.Contains("Extension workflow", contract, StringComparison.Ordinal);
        Assert.Contains("Debug tools", contract, StringComparison.Ordinal);
    }

    [Fact]
    public void LocaleMutationStaysInsideLocalizationModule()
    {
        var client = FindClientRoot();
        string[] forbiddenTokens =
        [
            "TranslationServer.SetLocale",
            "TranslationServer.Translate",
            "OS.GetLocaleLanguage",
            "new ConfigFile(",
        ];

        foreach (var file in ClientSourceFiles(client))
        {
            var relative = Relative(client, file);
            if (relative.StartsWith("Localization/", StringComparison.Ordinal))
            {
                continue;
            }

            var text = File.ReadAllText(file);
            foreach (var token in forbiddenTokens)
            {
                Assert.False(
                    text.Contains(token, StringComparison.Ordinal),
                    $"Localization implementation token '{token}' leaked outside Localization/: {relative}");
            }
        }
    }

    [Fact]
    public void LocalizationCatalogsCoverEnglishAndRussianWithTheSameKeys()
    {
        var client = FindClientRoot();
        var english = PoKeys(Path.Combine(client, "Localization", "en.po"));
        var russian = PoKeys(Path.Combine(client, "Localization", "ru.po"));
        var project = File.ReadAllText(Path.Combine(client, "project.godot"));

        Assert.Equal(english, russian);
        Assert.Contains("UI_SETTLEMENT", english);
        Assert.Contains("UI_NAME", english);
        Assert.Contains("UI_HOME", english);
        Assert.Contains("UI_AFFINITY", english);
        Assert.Contains("UI_DEBUG_TITLE", english);
        Assert.Contains("CONTENT_ITEM_RATION", english);
        Assert.Contains("FEEDBACK_ACTION_FAILED", english);
        Assert.Contains("locale/fallback=\"en\"", project, StringComparison.Ordinal);
        Assert.Contains("res://Localization/en.po", project, StringComparison.Ordinal);
        Assert.Contains("res://Localization/ru.po", project, StringComparison.Ordinal);
        Assert.Contains("rendering/root_node_auto_translate=false", project, StringComparison.Ordinal);
    }

    [Fact]
    public void LocalizationRefreshUsesSingleRegistryAndLanguageItemsDoNotAutoTranslate()
    {
        var client = FindClientRoot();
        var localization = File.ReadAllText(
            Path.Combine(client, "Localization", "GameLocalization.cs"));
        var hud = File.ReadAllText(
            Path.Combine(client, "UI", "Screens", "Hud", "GameHud.cs"));

        Assert.Contains("RefreshAllUi()", localization, StringComparison.Ordinal);
        Assert.Contains("RegisterUiRefresh", localization, StringComparison.Ordinal);
        Assert.DoesNotContain("event Action? Changed", localization, StringComparison.Ordinal);
        Assert.Contains("SetItemAutoTranslateMode", hud, StringComparison.Ordinal);
        Assert.Contains("AutoTranslateModeEnum.Disabled", hud, StringComparison.Ordinal);

        foreach (var file in ClientSourceFiles(client))
        {
            var relative = Relative(client, file);
            if (relative.StartsWith("Localization/", StringComparison.Ordinal))
            {
                continue;
            }

            Assert.DoesNotContain(
                "GameLocalization.Changed",
                File.ReadAllText(file),
                StringComparison.Ordinal);
        }
    }

    [Fact]
    public void PlayerFacingTextIsNotStoredInScenes()
    {
        var client = FindClientRoot();
        foreach (var scene in Directory.EnumerateFiles(client, "*.tscn", SearchOption.AllDirectories))
        {
            foreach (var line in File.ReadLines(scene))
            {
                Assert.False(
                    line.TrimStart().StartsWith("text = \"", StringComparison.Ordinal),
                    $"Player-facing scene text must be assigned through GameLocalization: {Relative(client, scene)} -> {line.Trim()}");
            }
        }
    }

    [Fact]
    public void ResidentPanelSeparatesLocalizedLabelsFromRuntimeValues()
    {
        var client = FindClientRoot();
        var scene = File.ReadAllText(
            Path.Combine(client, "UI", "Screens", "ResidentPanel", "ResidentPanel.tscn"));
        var code = File.ReadAllText(
            Path.Combine(client, "UI", "Screens", "ResidentPanel", "ResidentPanel.cs"));

        string[] requiredNodes =
        [
            "NameLabel", "NameValue",
            "ProfessionLabel", "ProfessionValue",
            "HomeLabel", "HomeValue",
            "NeedsLabel", "NeedsValue",
            "AffinityLabel", "AffinityValue",
            "InventoryLabel", "InventoryValue",
        ];
        foreach (var node in requiredNodes)
        {
            Assert.Contains($"name=\"{node}\"", scene, StringComparison.Ordinal);
        }

        Assert.Contains("_nameLabel.Text = GameLocalization.Tr(\"UI_NAME\")", code, StringComparison.Ordinal);
        Assert.Contains("_nameValue.Text = resident.Name", code, StringComparison.Ordinal);
        Assert.Contains("UiSurface.Card", code, StringComparison.Ordinal);
    }

    [Fact]
    public void VillageDebugObserverIsPresentationOnlyRemovableAndUsesSharedDesignSystem()
    {
        var client = FindClientRoot();
        var debugRoot = Path.Combine(client, "Debug", "VillageMonitor");
        Assert.True(Directory.Exists(debugRoot));

        foreach (var file in Directory.EnumerateFiles(debugRoot, "*.cs", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(file);
            Assert.DoesNotContain("Mws.Simulation.Runtime", text, StringComparison.Ordinal);
            Assert.DoesNotContain("Mws.Client.Godot.Session", text, StringComparison.Ordinal);
            Assert.DoesNotContain("AdvanceHours(", text, StringComparison.Ordinal);
            Assert.DoesNotContain("Execute(", text, StringComparison.Ordinal);
        }

        var main = File.ReadAllText(Path.Combine(client, "App", "Main.cs"));
        var mainScene = File.ReadAllText(Path.Combine(client, "App", "Main.tscn"));
        var debugInput = File.ReadAllText(Path.Combine(client, "Input", "DebugInput.cs"));
        var overlay = File.ReadAllText(
            Path.Combine(debugRoot, "VillageDebugOverlay.cs"));
        var map = File.ReadAllText(
            Path.Combine(debugRoot, "VillageDebugMap.cs"));

        Assert.DoesNotContain("VillageDebug", main, StringComparison.Ordinal);
        Assert.Contains("res://Debug/VillageMonitor/VillageDebugOverlay.tscn", mainScene, StringComparison.Ordinal);
        Assert.Contains("Key.F3", debugInput, StringComparison.Ordinal);
        Assert.Contains("UiSurface.Floating", overlay, StringComparison.Ordinal);
        Assert.Contains("UiSurface.Inset", overlay, StringComparison.Ordinal);
        Assert.Contains("ApplyBadge", overlay, StringComparison.Ordinal);
        Assert.Contains("DesignSystem.DataColor", map, StringComparison.Ordinal);
        Assert.DoesNotContain("new Color(", map, StringComparison.Ordinal);
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

    private static string[] PoKeys(string path) => File.ReadLines(path)
        .Where(line => line.StartsWith("msgid \"", StringComparison.Ordinal) && line.Length > 8)
        .Select(line => line[7..^1])
        .Where(key => !string.IsNullOrEmpty(key))
        .OrderBy(key => key, StringComparer.Ordinal)
        .ToArray();

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

        throw new InvalidOperationException(
            "Repository root containing WorldSimulate.sln was not found.");
    }
}
