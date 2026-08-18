using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class SettlementScalingTests
{
    [Fact]
    public void ScarcityResolutionIsStableForInputOrderAndActuallyUsesSeed()
    {
        var distinctWinnerSets = new HashSet<string>(StringComparer.Ordinal);

        for (ulong seed = 1; seed <= 16; seed++)
        {
            var state = CreateContendedState(seed, SimulationScopeId.Root, residentCount: 32, rationCount: 2);
            var forward = SettlementSimulation.Restore(state);
            var reversed = SettlementSimulation.Restore(state with
            {
                Residents = state.Residents.Reverse().ToArray(),
            });

            forward.AdvanceHours(1);
            reversed.AdvanceHours(1);

            Assert.Equal(
                SettlementStateJson.Serialize(forward.CaptureState()),
                SettlementStateJson.Serialize(reversed.CaptureState()));

            var winners = forward.Project().Residents
                .Where(resident => resident.Activity == ResidentActivity.Eating)
                .Select(resident => resident.Id.Value)
                .OrderBy(id => id)
                .ToArray();
            Assert.Equal(2, winners.Length);
            distinctWinnerSets.Add(string.Join(",", winners));
        }

        Assert.True(distinctWinnerSets.Count > 1, "World seed must affect deterministic scarcity arbitration.");
    }

    [Fact]
    public void ScopeIdentitySurvivesSaveLoadAndSeparatesSettlementPartitions()
    {
        var left = SettlementSimulation.CreateDefault(new WorldSeed(42), new SimulationScopeId(1001));
        var right = SettlementSimulation.CreateDefault(new WorldSeed(42), new SimulationScopeId(1002));

        left.AdvanceHours(48);
        right.AdvanceHours(48);

        var restoredLeft = SettlementSimulation.Restore(
            SettlementStateJson.Deserialize(SettlementStateJson.Serialize(left.CaptureState())));
        var restoredRight = SettlementSimulation.Restore(
            SettlementStateJson.Deserialize(SettlementStateJson.Serialize(right.CaptureState())));

        Assert.Equal(new SimulationScopeId(1001), restoredLeft.ScopeId);
        Assert.Equal(new SimulationScopeId(1002), restoredRight.ScopeId);
        Assert.Equal(restoredLeft.ScopeId, restoredLeft.Project().ScopeId);
        Assert.NotEqual(restoredLeft.ScopeId, restoredRight.ScopeId);
    }

    [Fact]
    public void AdvanceToMatchesIncrementalHourlyStepping()
    {
        var incremental = SettlementSimulation.CreateDefault(new WorldSeed(77), new SimulationScopeId(7));
        var scheduled = SettlementSimulation.CreateDefault(new WorldSeed(77), new SimulationScopeId(7));

        incremental.AdvanceHours(240);
        scheduled.AdvanceTo(new SimulationTime(240 * SettlementSimulation.HourMilliseconds));

        Assert.Equal(
            SettlementStateJson.Serialize(incremental.CaptureState()),
            SettlementStateJson.Serialize(scheduled.CaptureState()));

        var subHourTarget = scheduled.Time.AddMilliseconds(
            SettlementSimulation.HourMilliseconds / 2);
        scheduled.AdvanceTo(subHourTarget);

        Assert.Equal(subHourTarget, scheduled.Time);
    }

    [Fact]
    public void DestinationCapacityFailureDoesNotPartiallyTransferInventory()
    {
        var baseState = SettlementSimulation.CreateDefault(new WorldSeed(88)).CaptureState();
        var residentId = baseState.Residents[0].Id;
        var extraStacks = new[]
        {
            new ItemStackState(3, SettlementItems.Herb, baseState.SettlementOwnerId, 1),
            new ItemStackState(4, SettlementItems.Herb, residentId, int.MaxValue),
        };
        var simulation = SettlementSimulation.Restore(baseState with
        {
            ItemStacks = baseState.ItemStacks.Concat(extraStacks).ToArray(),
            NextStackId = 5,
        });

        var result = simulation.GiveItemToResident(residentId, SettlementItems.Herb, 1);
        var state = simulation.CaptureState();

        Assert.False(result.Success);
        Assert.Equal(SettlementResultCodes.InventoryCapacityExceeded, result.Code);
        Assert.Equal(1, Quantity(state, baseState.SettlementOwnerId, SettlementItems.Herb));
        Assert.Equal(int.MaxValue, Quantity(state, residentId, SettlementItems.Herb));
    }

    [Fact]
    public void RulesetMismatchCannotSilentlyResumeOldWorldPhysics()
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(99)).CaptureState();

        Assert.Throws<NotSupportedException>(() => SettlementSimulation.Restore(state with
        {
            RulesVersion = "settlement-rules-unknown",
        }));
        Assert.Throws<NotSupportedException>(() => SettlementSimulation.Restore(state with
        {
            ContentVersion = "settlement-content-unknown",
        }));
    }

    [Fact]
    public void LegacyV3SnapshotMigratesDeterministicallyIntoVersionedState()
    {
        var current = SettlementSimulation.CreateDefault(new WorldSeed(101)).CaptureState();
        var legacyPayload = JsonSerializer.Serialize(new
        {
            SchemaVersion = SettlementVersions.LegacySchemaVersion,
            current.WorldSeed,
            current.Time,
            current.NextEventId,
            current.NextStackId,
            current.NextCommandId,
            current.SettlementOwnerId,
            current.Residents,
            current.ItemStacks,
            current.Workplaces,
            current.Events,
            current.CommandReceipts,
        });
        var checksum = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(legacyPayload)));
        var envelope = JsonSerializer.Serialize(new
        {
            SchemaVersion = SettlementVersions.LegacySchemaVersion,
            Payload = legacyPayload,
            Checksum = checksum,
        });

        var migrated = SettlementStateJson.Deserialize(envelope);
        var restored = SettlementSimulation.Restore(migrated);

        Assert.Equal(SettlementVersions.CurrentSchemaVersion, migrated.SchemaVersion);
        Assert.Equal(SettlementVersions.CurrentRulesVersion, migrated.RulesVersion);
        Assert.Equal(SettlementVersions.CurrentContentVersion, migrated.ContentVersion);
        Assert.Equal(SimulationScopeId.Root, migrated.ScopeId);
        Assert.Equal(current.WorldSeed, restored.CaptureState().WorldSeed);
    }

    [Fact]
    public void DailySaveReloadMatchesDirectThirtyDayVillageScaleRun()
    {
        var state = CreateLargeVillageState(new WorldSeed(202), new SimulationScopeId(9), residentCount: 256);
        var direct = SettlementSimulation.Restore(state);
        var resumed = SettlementSimulation.Restore(state);

        direct.AdvanceHours(30 * 24);
        for (var day = 0; day < 30; day++)
        {
            resumed.AdvanceHours(24);
            resumed = SettlementSimulation.Restore(
                SettlementStateJson.Deserialize(SettlementStateJson.Serialize(resumed.CaptureState())));
        }

        Assert.Equal(
            SettlementStateJson.Serialize(direct.CaptureState()),
            SettlementStateJson.Serialize(resumed.CaptureState()));
        Assert.Equal(256, resumed.Project().Residents.Count);
        Assert.All(resumed.Project().Residents, resident =>
        {
            Assert.InRange(resident.Hunger, 0, 100);
            Assert.InRange(resident.Energy, 0, 100);
        });
    }

    private static SettlementState CreateContendedState(
        ulong seed,
        SimulationScopeId scopeId,
        int residentCount,
        int rationCount)
    {
        var state = SettlementSimulation.CreateDefault(new WorldSeed(seed), scopeId).CaptureState();
        var farm = state.Workplaces.Single(workplace => workplace.Profession == ResidentProfession.Farmer);
        var residents = Enumerable.Range(1, residentCount)
            .Select(index => new ResidentState(
                new EntityId(index),
                $"Resident {index}",
                100,
                100,
                ResidentActivity.Idle,
                ResidentProfession.Farmer,
                farm.Id,
                0))
            .ToArray();

        return state with
        {
            Residents = residents,
            ItemStacks = [new ItemStackState(1, SettlementItems.Ration, state.SettlementOwnerId, rationCount)],
            NextStackId = 2,
            Events = [],
            CommandReceipts = [],
            NextEventId = 1,
            NextCommandId = 1,
        };
    }

    private static SettlementState CreateLargeVillageState(
        WorldSeed seed,
        SimulationScopeId scopeId,
        int residentCount)
    {
        var state = SettlementSimulation.CreateDefault(seed, scopeId).CaptureState();
        var farm = state.Workplaces.Single(workplace => workplace.Profession == ResidentProfession.Farmer);
        var residents = Enumerable.Range(1, residentCount)
            .Select(index => new ResidentState(
                new EntityId(index),
                $"Resident {index}",
                10 + (index % 50),
                50 + (index % 51),
                ResidentActivity.Idle,
                ResidentProfession.Farmer,
                farm.Id,
                index % 7))
            .ToArray();
        var stacks = state.ItemStacks
            .Select(stack => stack.ItemId == SettlementItems.Ration ? stack with { Quantity = residentCount * 2 } : stack)
            .ToArray();

        return state with
        {
            Residents = residents,
            ItemStacks = stacks,
        };
    }

    private static int Quantity(SettlementState state, EntityId ownerId, string itemId) =>
        state.ItemStacks
            .Where(stack => stack.OwnerId == ownerId && string.Equals(stack.ItemId, itemId, StringComparison.Ordinal))
            .Sum(stack => stack.Quantity);
}
