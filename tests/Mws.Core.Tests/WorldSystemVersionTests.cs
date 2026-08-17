using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class WorldSystemVersionTests
{
    [Fact]
    public void WorldManifestPersistsCanonicalPerSystemVersionBundle()
    {
        var world = WorldRuntime.Create(new WorldSeed(701));
        _ = world.AddDefaultSettlement();
        var checkpoint = world.CreateCheckpoint();
        var expected = WorldSystemVersions.CreateCurrent().ToArray();

        Assert.Equal(expected, checkpoint.Manifest.SystemVersions.ToArray());

        var serialized = WorldManifestJson.Serialize(checkpoint.Manifest);
        var restored = WorldManifestJson.Deserialize(serialized);

        Assert.Equal(expected, restored.SystemVersions.ToArray());
        Assert.Contains(WorldSystemIds.SettlementHourly, serialized, StringComparison.Ordinal);
        Assert.Contains(WorldSystemIds.ResidentMigration, serialized, StringComparison.Ordinal);
        Assert.Contains(WorldSystemIds.Scheduler, serialized, StringComparison.Ordinal);
    }

    [Fact]
    public void WorldManifestRejectsMissingOrDriftedSystemVersionBundle()
    {
        var manifest = WorldRuntime.Create(new WorldSeed(702)).CaptureCheckpoint().Manifest;
        var drifted = manifest.SystemVersions
            .Select(entry => entry.SystemId == WorldSystemIds.Scheduler
                ? entry with { Version = "2.0.0" }
                : entry)
            .ToArray();

        Assert.Throws<NotSupportedException>(() => manifest with { SystemVersions = [] });
        Assert.Throws<NotSupportedException>(() => manifest with { SystemVersions = drifted });
    }
}
