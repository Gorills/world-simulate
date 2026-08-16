using Mws.Domain;
using Mws.Simulation.Runtime;
using Xunit;

namespace Mws.Core.Tests;

public sealed class DeterministicHashCompatibilityTests
{
    [Fact]
    public void ExistingProofABoundOutcomeVectorRemainsStable()
    {
        var kernel = new ProofAKernel(new WorldSeed(14));
        var subject = kernel.CreateEntity();

        var outcome = kernel.ResolveBoundRandom("injury-check", subject, 1);

        Assert.Equal(13_072_070_663_325_891_937UL, outcome);
    }
}
