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
        _events.Add(new SettlementEvent(_nextEventId, Time, kind, subjectId, facts));
        _nextEventId = checked(_nextEventId + 1);
    }
}
