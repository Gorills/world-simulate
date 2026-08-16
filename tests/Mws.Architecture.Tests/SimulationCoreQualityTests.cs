using System.Xml.Linq;
using Xunit;

namespace Mws.Architecture.Tests;

public sealed class SimulationCoreQualityTests
{
    [Fact]
    public void CoreProjectReferencesFollowDependencyDirection()
    {
        AssertProjectReferences("Mws.Domain");
        AssertProjectReferences("Mws.Simulation.Api", "Mws.Domain");
        AssertProjectReferences("Mws.Simulation.Runtime", "Mws.Domain", "Mws.Simulation.Api");
        AssertProjectReferences("Mws.Persistence.Json", "Mws.Domain", "Mws.Simulation.Api");
    }

    [Fact]
    public void AuthoritativeRuntimeRejectsAmbientNondeterminism()
    {
        var runtime = Path.Combine(FindRepositoryRoot(), "src", "Mws.Simulation.Runtime");
        string[] forbiddenTokens =
        [
            "DateTime.Now",
            "DateTime.UtcNow",
            "Guid.NewGuid",
            "Random.Shared",
            "new Random(",
            "Task.Run(",
            "Parallel.",
            "new Thread(",
        ];

        foreach (var file in Directory.EnumerateFiles(runtime, "*.cs", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(file);
            foreach (var token in forbiddenTokens)
            {
                Assert.False(
                    text.Contains(token, StringComparison.Ordinal),
                    $"Ambient nondeterminism token '{token}' is forbidden in authoritative runtime: {file}");
            }
        }
    }

    [Fact]
    public void ProductionSettlementDoesNotDependOnProofOrPersistenceImplementation()
    {
        var settlement = Path.Combine(
            FindRepositoryRoot(),
            "src",
            "Mws.Simulation.Runtime",
            "Settlement");

        foreach (var file in Directory.EnumerateFiles(settlement, "*.cs", SearchOption.AllDirectories))
        {
            var text = File.ReadAllText(file);
            Assert.DoesNotContain("ProofAKernel", text, StringComparison.Ordinal);
            Assert.DoesNotContain("Mws.Persistence", text, StringComparison.Ordinal);
            Assert.DoesNotContain("System.Text.Json", text, StringComparison.Ordinal);
        }
    }

    [Fact]
    public void ProductionSettlementFilesStayWithinResponsibilityBudget()
    {
        var settlement = Path.Combine(
            FindRepositoryRoot(),
            "src",
            "Mws.Simulation.Runtime",
            "Settlement");

        foreach (var file in Directory.EnumerateFiles(settlement, "*.cs", SearchOption.AllDirectories))
        {
            var lines = File.ReadLines(file).Count();
            Assert.True(
                lines <= 260,
                $"Production settlement file exceeds 260-line responsibility budget: {file} ({lines}).");
        }
    }

    [Fact]
    public void ObsoleteToySimulationDoesNotReturn()
    {
        var root = FindRepositoryRoot();

        Assert.False(File.Exists(Path.Combine(
            root,
            "src",
            "Mws.Simulation.Runtime",
            "DeterministicWorldSimulation.cs")));
        Assert.False(File.Exists(Path.Combine(
            root,
            "src",
            "Mws.Simulation.Api",
            "IWorldSimulation.cs")));
        Assert.False(File.Exists(Path.Combine(
            root,
            "src",
            "Mws.Persistence.Json",
            "WorldSnapshotJson.cs")));
    }

    private static void AssertProjectReferences(string projectName, params string[] expected)
    {
        var projectFile = Path.Combine(
            FindRepositoryRoot(),
            "src",
            projectName,
            $"{projectName}.csproj");
        var document = XDocument.Load(projectFile);
        var actual = document
            .Descendants("ProjectReference")
            .Select(reference => (string?)reference.Attribute("Include"))
            .Where(include => !string.IsNullOrWhiteSpace(include))
            .Select(include => Path.GetFileNameWithoutExtension(include!.Replace('\\', '/')))
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        var sortedExpected = expected.OrderBy(name => name, StringComparer.Ordinal).ToArray();

        Assert.Equal(sortedExpected, actual);
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
