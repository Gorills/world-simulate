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

    [Fact]
    public void Authoritative_core_has_no_Godot_dependency()
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
                Assert.DoesNotContain("Godot", text, StringComparison.OrdinalIgnoreCase);
            }
        }
    }

    [Fact]
    public void Core_projects_do_not_reference_client_project()
    {
        var root = FindRepositoryRoot();

        foreach (var project in AuthoritativeCoreProjects)
        {
            var projectFile = Path.Combine(root, "src", project, $"{project}.csproj");
            var text = File.ReadAllText(projectFile);
            Assert.DoesNotContain("Mws.Client.Godot", text, StringComparison.OrdinalIgnoreCase);
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

        throw new InvalidOperationException("Repository root containing WorldSimulate.sln was not found.");
    }
}
