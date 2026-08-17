using Mws.Domain;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class ProofATransferAtomicityTests
{
    [Fact]
    public void SameTimeDestinationOverflowIsRejectedBeforeAnyPartialSourceMutation()
    {
        var kernel = new ProofAKernel(new WorldSeed(201));
        var owner = kernel.CreateEntity();
        var sourceA = kernel.CreateEntity(owner, initialResource: 10);
        var sourceB = kernel.CreateEntity(owner, initialResource: 10);
        var destination = kernel.CreateEntity(owner, initialResource: long.MaxValue - 10);
        var dueAt = new SimulationTime(1_000);
        var first = new ProofATransferIntent(new CommandId(100), dueAt, 0, owner, sourceA, destination, 6);
        var second = new ProofATransferIntent(new CommandId(101), dueAt, 0, owner, sourceB, destination, 6);

        var results = kernel.ResolveSameTimeTransfers([first, second]);

        Assert.Contains(results, result => result.CommandId == first.CommandId && result.Success);
        Assert.Contains(results, result => result.CommandId == second.CommandId && result.Code == "RESOURCE_OVERFLOW");
        Assert.True(kernel.TryGetEntity(sourceA, out var sourceAState));
        Assert.True(kernel.TryGetEntity(sourceB, out var sourceBState));
        Assert.True(kernel.TryGetEntity(destination, out var destinationState));
        Assert.Equal(4, sourceAState!.Resource);
        Assert.Equal(10, sourceBState!.Resource);
        Assert.Equal(long.MaxValue - 4, destinationState!.Resource);
    }

    [Fact]
    public void SameEntityTransferIsRejectedWithoutChangingResource()
    {
        var kernel = new ProofAKernel(new WorldSeed(202));
        var owner = kernel.CreateEntity();
        var entity = kernel.CreateEntity(owner, initialResource: 10);

        var result = kernel.AtomicTransfer(kernel.AllocateCommandId(), owner, entity, entity, 4);

        Assert.False(result.Success);
        Assert.Equal("SAME_ENTITY_TRANSFER", result.Code);
        Assert.True(kernel.TryGetEntity(entity, out var state));
        Assert.Equal(10, state!.Resource);
    }
}
