using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class P3InteractionLocationGateTests
{
    [Fact]
    public void ResidentInteractionRequiresAuthoritativePlayer()
    {
        var world = WorldRuntime.Create(new WorldSeed(9401));
        var scope = world.AddDefaultSettlement();
        var before = world.CaptureSettlementState(scope);
        var residentId = before.Residents[0].Id;

        var result = world.ExecuteResidentInteraction(
            scope,
            residentId,
            ResidentInteractionChoice.Encourage);
        var after = world.CaptureSettlementState(scope);

        Assert.False(result.Success);
        Assert.Equal(SettlementResultCodes.PlayerRequired, result.Code);
        Assert.Equal(before.NextCommandId, after.NextCommandId);
        Assert.Equal(before.CommandReceipts.Count, after.CommandReceipts.Count);
        Assert.Equal(
            WorldInputKind.SettlementCommand,
            world.CaptureCheckpoint().Manifest.InputJournal[^1].Kind);
    }

    [Fact]
    public void ResidentInteractionRejectsPlayerFromDifferentSettlement()
    {
        var world = WorldRuntime.Create(new WorldSeed(9402));
        var playerScope = world.AddDefaultSettlement();
        var residentScope = world.AddDefaultSettlement();
        _ = world.AddPlayerActor(playerScope);
        var before = world.CaptureSettlementState(residentScope);
        var residentId = before.Residents[0].Id;

        var result = world.ExecuteResidentInteraction(
            residentScope,
            residentId,
            ResidentInteractionChoice.AskAboutWork);
        var after = world.CaptureSettlementState(residentScope);

        Assert.False(result.Success);
        Assert.Equal(SettlementResultCodes.PlayerScopeMismatch, result.Code);
        Assert.Equal(before.NextCommandId, after.NextCommandId);
        Assert.Equal(before.CommandReceipts.Count, after.CommandReceipts.Count);
    }

    [Fact]
    public void SettlementPresenceIsNotWildcardCoLocationForResidentInteraction()
    {
        var world = WorldRuntime.Create(new WorldSeed(9404));
        var scope = world.AddDefaultSettlement();
        _ = world.AddPlayerActor(scope);
        var before = world.CaptureSettlementState(scope);
        var residentId = before.Residents[0].Id;
        var playerLocation = Assert.IsType<SettlementActorLocationProjection>(world.ProjectPlayer().Location);
        var resident = world.ProjectSettlement(scope).Residents.Single(entry => entry.Id == residentId);
        var residentLocation = Assert.IsType<SettlementActorLocationProjection>(resident.Location);

        Assert.Equal(SettlementPlaceRef.Settlement, playerLocation.CurrentPlace);
        Assert.NotEqual(playerLocation.CurrentPlace, residentLocation.CurrentPlace);

        var result = world.ExecuteResidentInteraction(
            scope,
            residentId,
            ResidentInteractionChoice.Encourage);
        var after = world.CaptureSettlementState(scope);

        Assert.False(result.Success);
        Assert.Equal(SettlementResultCodes.InteractionNotCoLocated, result.Code);
        Assert.Equal(before.NextCommandId, after.NextCommandId);
        Assert.Equal(before.CommandReceipts.Count, after.CommandReceipts.Count);
        Assert.Equal(
            WorldInputKind.SettlementCommand,
            world.CaptureCheckpoint().Manifest.InputJournal[^1].Kind);
    }

    [Fact]
    public void ResidentInteractionRejectsPreparedTravelAndReplayKeepsSettlementUnchanged()
    {
        var world = WorldRuntime.Create(new WorldSeed(9403));
        var scope = world.AddDefaultSettlement();
        _ = world.AddPlayerActor(scope);
        var checkpoint = world.CaptureCheckpoint();
        var partition = Assert.Single(checkpoint.Partitions, entry => entry.ScopeId == scope);
        var resident = partition.Settlement.Residents[0];
        var household = Assert.Single(
            partition.Settlement.Households!,
            entry => entry.Id == resident.HouseholdId);
        var home = new SettlementPlaceRef(SettlementPlaceKind.Home, household.HomeId);
        var workplace = new SettlementPlaceRef(SettlementPlaceKind.Workplace, resident.WorkplaceId);
        var travellingResident = resident with
        {
            Location = new SettlementActorLocationState(
                SettlementActorLocationKind.Travelling,
                home,
                workplace,
                new SettlementTravelProgressState(
                    SettlementSimulation.HourMilliseconds,
                    0)),
        };
        var preparedPartition = partition with
        {
            Settlement = partition.Settlement with
            {
                Residents = partition.Settlement.Residents
                    .Select(entry => entry.Id == resident.Id ? travellingResident : entry)
                    .ToArray(),
            },
        };
        world = WorldRuntime.Restore(checkpoint with
        {
            Partitions = checkpoint.Partitions
                .Select(entry => entry.ScopeId == scope ? preparedPartition : entry)
                .ToArray(),
        });

        var before = world.CaptureSettlementState(scope);
        var residentId = resident.Id;
        var projected = world.ProjectSettlement(scope).Residents.Single(entry => entry.Id == residentId);
        var location = Assert.IsType<SettlementActorLocationProjection>(projected.Location);
        Assert.Equal(SettlementActorLocationKind.Travelling, location.Kind);

        var baseline = world.CaptureCheckpoint();
        var result = world.ExecuteResidentInteraction(
            scope,
            residentId,
            ResidentInteractionChoice.Encourage);
        var after = world.CaptureSettlementState(scope);
        var expected = world.CaptureCheckpoint();
        var tail = expected.Manifest.InputJournal
            .Where(entry => entry.Sequence >= baseline.Manifest.NextInputSequence)
            .ToArray();

        Assert.False(result.Success);
        Assert.Equal(SettlementResultCodes.InteractionActorTravelling, result.Code);
        Assert.Equal(before.NextCommandId, after.NextCommandId);
        Assert.Equal(before.CommandReceipts.Count, after.CommandReceipts.Count);
        Assert.Single(tail);
        Assert.Equal(WorldInputKind.SettlementCommand, tail[0].Kind);

        var replayed = WorldRuntime.ReplayFrom(baseline, tail);
        Assert.Equal(
            SettlementStateJson.Serialize(after),
            SettlementStateJson.Serialize(replayed.CaptureSettlementState(scope)));
        Assert.Equal(
            expected.Manifest.NextInputSequence,
            replayed.CaptureCheckpoint().Manifest.NextInputSequence);
    }
}
