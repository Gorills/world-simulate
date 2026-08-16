using System.Text;
using Mws.Domain;

namespace Mws.Simulation.Runtime;

internal static class DeterministicSimulationHash
{
    private const ulong Offset = 14695981039346656037UL;
    private const ulong Prime = 1099511628211UL;

    public static ulong Rank(
        ulong worldSeed,
        SimulationScopeId scopeId,
        SimulationTime time,
        string domain,
        EntityId subjectId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(domain);
        var hash = Offset;
        hash = MixNumber(hash, worldSeed);
        hash = MixNumber(hash, scopeId.Value);
        hash = MixNumber(hash, unchecked((ulong)time.Milliseconds));
        hash = MixNumber(hash, unchecked((ulong)subjectId.Value));
        return MixText(hash, domain);
    }

    public static ulong BoundOutcome(
        ulong worldSeed,
        string domain,
        EntityId subjectId,
        long causalAttempt)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(domain);
        var hash = Offset;
        hash = MixNumber(hash, worldSeed);
        hash = MixNumber(hash, unchecked((ulong)subjectId.Value));
        hash = MixNumber(hash, unchecked((ulong)causalAttempt));
        return MixText(hash, domain);
    }

    private static ulong MixNumber(ulong current, ulong value)
    {
        for (var index = 0; index < sizeof(ulong); index++)
        {
            current ^= (byte)(value >> (index * 8));
            current = unchecked(current * Prime);
        }

        return current;
    }

    private static ulong MixText(ulong current, string value)
    {
        foreach (var item in Encoding.UTF8.GetBytes(value))
        {
            current ^= item;
            current = unchecked(current * Prime);
        }

        return current;
    }
}
