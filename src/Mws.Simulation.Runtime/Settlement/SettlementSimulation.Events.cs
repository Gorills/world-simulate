using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class SettlementSimulation
{
    private void AppendEvent(
        string kind,
        EntityId? subjectId,
        params SettlementFact[] facts)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(kind);
        if (_nextEventId <= 0 || _nextEventId == long.MaxValue)
        {
            throw new InvalidOperationException("Settlement event ID space is exhausted or invalid.");
        }

        var eventId = _nextEventId;
        var nextEventId = checked(eventId + 1);
        _events.Add(new SettlementEvent(eventId, Time, kind, subjectId, facts));
        _nextEventId = nextEventId;
        RetainRecentEvents();
    }
}
