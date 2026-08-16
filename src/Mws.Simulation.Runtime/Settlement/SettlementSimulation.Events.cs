using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class SettlementSimulation
{
    private void EnsureEventCapacity()
    {
        if (_nextEventId <= 0 || _nextEventId == long.MaxValue)
        {
            throw new InvalidOperationException("Settlement event ID space is exhausted or invalid.");
        }
    }

    private void AppendEvent(
        string kind,
        EntityId? subjectId,
        params SettlementFact[] facts)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);
        EnsureEventCapacity();

        var eventId = _nextEventId;
        _events.Add(new SettlementEvent(eventId, Time, kind, subjectId, facts));
        _nextEventId = checked(eventId + 1);
        RetainRecentEvents();
    }
}
