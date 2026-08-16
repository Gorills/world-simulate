using Mws.Domain;
using Mws.Persistence.Json;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class ProofAKernelTests
{
    [Fact]
    public void IdentitySurvivesRestartAndDestroyedIdIsNotReused()
    {
        var kernel = new ProofAKernel(new WorldSeed(10));
        var owner = kernel.CreateEntity(initialResource: 5);
        var subject = kernel.CreateEntity(owner, initialResource: 3);
        var snapshot = ProofAKernelJson.Serialize(kernel.CaptureState());
        var loaded = ProofAKernelJson.Deserialize(snapshot);
        var restored = ProofAKernel.Restore(loaded.State);

        Assert.True(restored.TryGetEntity(subject, out var restoredSubject));
        Assert.Equal(subject, restoredSubject!.Id);

        var destroy = restored.DestroyEntity(restored.AllocateCommandId(), owner, subject, "proof fixture removal");
        Assert.True(destroy.Success);
        Assert.True(restored.TryGetTombstone(subject, out var tombstone));
        Assert.Equal(subject, tombstone!.Id);

        var replacement = restored.CreateEntity(owner);
        Assert.True(replacement.Value > subject.Value);
        Assert.NotEqual(subject, replacement);
    }

    [Fact]
    public void OwnerMediatedMutationRejectsCrossOwnerAndCommandIsIdempotent()
    {
        var kernel = new ProofAKernel(new WorldSeed(11));
        var ownerA = kernel.CreateEntity();
        var ownerB = kernel.CreateEntity();
        var subject = kernel.CreateEntity(ownerA, initialResource: 10);

        var rejected = kernel.AdjustResource(kernel.AllocateCommandId(), ownerB, subject, 5);
        Assert.False(rejected.Success);
        Assert.Equal("OWNER_MISMATCH", rejected.Code);

        var commandId = kernel.AllocateCommandId();
        var first = kernel.AdjustResource(commandId, ownerA, subject, 5);
        var restored = ProofAKernel.Restore(ProofAKernelJson.Deserialize(ProofAKernelJson.Serialize(kernel.CaptureState())).State);
        var duplicate = restored.AdjustResource(commandId, ownerA, subject, 5);

        Assert.True(first.Success);
        Assert.Equal(first, duplicate);
        Assert.True(restored.TryGetEntity(subject, out var state));
        Assert.Equal(15, state!.Resource);
    }

    [Fact]
    public void SameTimeConflictResolutionIsIndependentOfInputIterationOrder()
    {
        static (IReadOnlyList<ProofATransferResolution> Results, ProofAKernelState State) Run(bool reverse)
        {
            var kernel = new ProofAKernel(new WorldSeed(12));
            var owner = kernel.CreateEntity();
            var source = kernel.CreateEntity(owner, initialResource: 10);
            var left = kernel.CreateEntity(owner);
            var right = kernel.CreateEntity(owner);
            var first = new ProofATransferIntent(new CommandId(100), new SimulationTime(1_000), 0, owner, source, left, 8);
            var second = new ProofATransferIntent(new CommandId(101), new SimulationTime(1_000), 0, owner, source, right, 8);
            var intents = reverse ? new[] { second, first } : new[] { first, second };
            return (kernel.ResolveSameTimeTransfers(intents), kernel.CaptureState());
        }

        var forward = Run(false);
        var reverse = Run(true);

        Assert.Equal(forward.Results, reverse.Results);
        Assert.Equal(
            ProofAKernelJson.Serialize(forward.State),
            ProofAKernelJson.Serialize(reverse.State));
        Assert.Contains(forward.Results, result => result.CommandId == new CommandId(100) && result.Success);
        Assert.Contains(forward.Results, result => result.CommandId == new CommandId(101) && result.Code == "RESERVATION_CONFLICT");
    }

    [Fact]
    public void FailedAtomicTransferRollsBackAllParticipants()
    {
        var kernel = new ProofAKernel(new WorldSeed(13));
        var owner = kernel.CreateEntity();
        var from = kernel.CreateEntity(owner, initialResource: 10);
        var to = kernel.CreateEntity(owner, initialResource: 2);

        var result = kernel.AtomicTransfer(kernel.AllocateCommandId(), owner, from, to, 4, simulateFailure: true);

        Assert.False(result.Success);
        Assert.Equal("ROLLED_BACK", result.Code);
        Assert.True(kernel.TryGetEntity(from, out var fromState));
        Assert.True(kernel.TryGetEntity(to, out var toState));
        Assert.Equal(10, fromState!.Resource);
        Assert.Equal(2, toState!.Resource);
    }

    [Fact]
    public void BoundRandomOutcomeSurvivesSaveReloadWithoutReroll()
    {
        var kernel = new ProofAKernel(new WorldSeed(14));
        var subject = kernel.CreateEntity();
        var first = kernel.ResolveBoundRandom("injury-check", subject, 1);
        var loaded = ProofAKernelJson.Deserialize(ProofAKernelJson.Serialize(kernel.CaptureState()));
        var restored = ProofAKernel.Restore(loaded.State);
        var second = restored.ResolveBoundRandom("injury-check", subject, 1);

        Assert.Equal(first, second);
    }

    [Fact]
    public void PendingProcessSurvivesSnapshotAndCompletesAfterResume()
    {
        var kernel = new ProofAKernel(new WorldSeed(15));
        var owner = kernel.CreateEntity();
        var subject = kernel.CreateEntity(owner, initialResource: 10);
        var processId = kernel.StartLongProcess(kernel.AllocateCommandId(), owner, subject, 5_000, 4);
        Assert.True(processId > 0);

        kernel.AdvanceTo(new SimulationTime(2_000));
        var restored = ProofAKernel.Restore(ProofAKernelJson.Deserialize(ProofAKernelJson.Serialize(kernel.CaptureState())).State);
        Assert.Equal(1, restored.PendingProcessCount);
        restored.AdvanceTo(new SimulationTime(5_000));

        Assert.Equal(0, restored.PendingProcessCount);
        Assert.True(restored.TryGetEntity(subject, out var state));
        Assert.Equal(14, state!.Resource);
    }

    [Fact]
    public void InterruptingPendingProcessReturnsReservation()
    {
        var kernel = new ProofAKernel(new WorldSeed(16));
        var owner = kernel.CreateEntity();
        var subject = kernel.CreateEntity(owner, initialResource: 10);
        var processId = kernel.StartLongProcess(kernel.AllocateCommandId(), owner, subject, 5_000, 4);
        var result = kernel.InterruptLongProcess(kernel.AllocateCommandId(), owner, processId);

        Assert.True(result.Success);
        Assert.Equal(0, kernel.PendingProcessCount);
        Assert.True(kernel.TryGetEntity(subject, out var state));
        Assert.Equal(10, state!.Resource);
    }

    [Fact]
    public void ReplayFromIdenticalAntecedentsRemainsEquivalent()
    {
        var kernel = new ProofAKernel(new WorldSeed(17));
        var owner = kernel.CreateEntity();
        var subject = kernel.CreateEntity(owner, initialResource: 20);
        var antecedent = ProofAKernelJson.Serialize(kernel.CaptureState());

        static string Replay(string snapshot, EntityId ownerId, EntityId subjectId)
        {
            var replay = ProofAKernel.Restore(ProofAKernelJson.Deserialize(snapshot).State);
            replay.AdjustResource(new CommandId(50), ownerId, subjectId, -3);
            _ = replay.ResolveBoundRandom("replay-outcome", subjectId, 2);
            replay.AdvanceTo(new SimulationTime(1_000));
            return ProofAKernelJson.Serialize(replay.CaptureState());
        }

        Assert.Equal(Replay(antecedent, owner, subject), Replay(antecedent, owner, subject));
    }

    [Fact]
    public void SchemaMigrationAndCompatibilityModesAreExplicit()
    {
        var kernel = new ProofAKernel(new WorldSeed(18));
        _ = kernel.CreateEntity();
        var current = kernel.CaptureState();
        var legacyJson = ProofAKernelJson.SerializeLegacyV1Fixture(current);
        var migrated = ProofAKernelJson.Deserialize(legacyJson);

        Assert.Equal(SnapshotCompatibility.CompatibleDecode, ProofAKernel.AssessCompatibility(current));
        Assert.Equal(SnapshotCompatibility.DeterministicMigration, migrated.Compatibility);
        Assert.Equal(SnapshotCompatibility.DeterministicMigration, ProofAKernel.AssessCompatibility(migrated.State));
        Assert.Equal(
            SnapshotCompatibility.LossyMigrationRequired,
            ProofAKernel.AssessCompatibility(current with { ConfigurationVersion = ProofAVersions.LossyLegacyConfigurationVersion }));
        Assert.Equal(
            SnapshotCompatibility.Unsupported,
            ProofAKernel.AssessCompatibility(current with { ModelVersion = "unknown-model" }));
        _ = ProofAKernel.Restore(migrated.State);
    }

    [Fact]
    public void CorruptSnapshotFailsSafely()
    {
        var kernel = new ProofAKernel(new WorldSeed(19));
        _ = kernel.CreateEntity();
        var json = ProofAKernelJson.Serialize(kernel.CaptureState());
        var corrupt = json.Replace("proof-a-model-v1", "proof-a-model-vX", StringComparison.Ordinal);

        Assert.False(ProofAKernelJson.TryDeserialize(corrupt, out var result, out var error));
        Assert.Null(result);
        Assert.NotEmpty(error);
        Assert.False(ProofAKernelJson.TryDeserialize(json[..^8], out _, out _));
    }

    [Fact]
    public void LodRoundTripPreservesIdentityConservationRareTailAndPendingProcess()
    {
        var kernel = new ProofAKernel(new WorldSeed(20));
        var owner = kernel.CreateEntity();
        var common = kernel.CreateEntity(owner, initialResource: 7);
        var rare = kernel.CreateEntity(owner, initialResource: 11, rare: true);
        var processId = kernel.StartLongProcess(kernel.AllocateCommandId(), owner, rare, 10_000, 3);
        var aggregate = kernel.AggregateRegion([common, rare]);
        var materialized = ProofAKernel.MaterializeRegion(aggregate);

        Assert.Equal(15, aggregate.TotalResource);
        Assert.Contains(rare, aggregate.RareEntityIds);
        Assert.Contains(processId, aggregate.PendingProcessIds);
        Assert.Equal(new[] { common, rare }, materialized.Select(member => member.Id).ToArray());
        Assert.Equal(aggregate.TotalResource, materialized.Sum(member => member.Resource));
    }

    [Fact]
    public void CausalTraceReconstructsVisibleConsequence()
    {
        var kernel = new ProofAKernel(new WorldSeed(21));
        var owner = kernel.CreateEntity();
        var subject = kernel.CreateEntity(owner, initialResource: 10);
        var processId = kernel.StartLongProcess(kernel.AllocateCommandId(), owner, subject, 1_000, 2);
        Assert.True(processId > 0);
        kernel.AdvanceTo(new SimulationTime(1_000));
        var state = kernel.CaptureState();
        var completion = state.Trace.Single(entry => entry.Kind == "long-process-completed");
        var chain = kernel.ReconstructConsequence(completion.TraceId);

        Assert.Equal(2, chain.Count);
        Assert.Equal("long-process-started", chain[0].Kind);
        Assert.Equal("long-process-completed", chain[1].Kind);
    }

    [Fact]
    public void CanonicalTimeIsIntegerMillisecondsAndMonotonic()
    {
        var kernel = new ProofAKernel(new WorldSeed(22));
        kernel.AdvanceTo(new SimulationTime(1234));
        Assert.Equal(1234, kernel.Time.Milliseconds);
        Assert.Throws<InvalidOperationException>(() => kernel.AdvanceTo(new SimulationTime(1233)));
    }
}
