using System.Text;
using Mws.Domain;

namespace Mws.Simulation.Runtime;

internal static class DeterministicSimulationHash
{
    public static ulong Rank(
        ulong worldSeed,
        SimulationScopeId scopeId,
        SimulationTime time,
        string domain,
        EntityId subjectId) =>
        Compute(worldSeed, scopeId.Value, time.Milliseconds, domain, subjectId.Value, 0);

    public static ulong BoundOutcome(
        ulong worldSeed,
        string domain,
        EntityId subjectId,
        long causalAttempt) =>
        Compute(worldSeed, 0, 0, domain, subjectId.Value, causalAttempt);

    private static ulong Compute(
        ulong worldSeed,
        ulong scopeId,
        long timeMilliseconds,
        string domain,
        long subjectId,
        long causalAttempt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(domain);

        const ulong offset = 14695981039346656037UL;
        const ulong prime = 1099511628211UL;
        var hash = offset;

        hash = MixNumber(hash, worldSeed, prime);
        hash = MixNumber(hash, scopeId, prime);
        hash = MixNumber(hash, unchecked((ulong)timeMilliseconds), prime);
        hash = MixNumber(hash, unchecked((ulong)subjectId), prime);
        hash = MixNumber(hash, unchecked((ulong)causalAttempt), prime);

        foreach (var value in Encoding.UTF8.GetBytes(domain))
        {
            hash ^= value;
            hash = unchecked(hash * prime);
        }

        return hash;
    }

    private static ulong MixNumber(ulong current, ulong value, ulong prime)
    {
        for (var index = 0; index < sizeof(ulong); index++)
        {
            current ^= (byte)(value >> (index * 8));
            current = unchecked(current * prime);
        }

        return current;
    }
}
