using Mws.Domain;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class WorldRuntimeClientBoundaryTests
{
    [Fact]
    public void ResidentInteractionRunsThroughWorldJournalAndCheckpoint()
    {
        var world = WorldRuntime.Create(new WorldSeed(9101));
        var scope = world.AddDefaultSettlement();
        _ = world.AddPlayerActor(scope);
        world.AdvanceHours(8);
        var before = world.CaptureSettlementState(scope);
        var residentId = before.Residents[0].Id;

        var result = world.ExecuteResidentInteraction(
            scope,
            residentId,
            ResidentInteractionChoice.Encourage);
        var after = world.CaptureSettlementState(scope);
        var checkpoint = world.CreateCheckpoint();
        var journal = checkpoint.Manifest.InputJournal;

        Assert.True(result.Success);
        Assert.Equal(before.NextCommandId + 1, after.NextCommandId);
        Assert.Equal(before.CommandReceipts.Count + 1, after.CommandReceipts.Count);
        Assert.Equal(WorldInputKind.SettlementCommand, journal[journal.Count - 1].Kind);

        var restored = WorldRuntime.Restore(checkpoint);
        var restoredState = restored.CaptureSettlementState(scope);
        Assert.Equal(after.NextCommandId, restoredState.NextCommandId);
        Assert.Equal(after.CommandReceipts.Count, restoredState.CommandReceipts.Count);
        Assert.Equal(world.Time, restored.Time);
        Assert.Equal(
            world.ProjectSettlement(scope).Residents.Single(resident => resident.Id == residentId).Affinity,
            restored.ProjectSettlement(scope).Residents.Single(resident => resident.Id == residentId).Affinity);
    }
}
