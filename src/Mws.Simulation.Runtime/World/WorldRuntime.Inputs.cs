using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class WorldRuntime
{
    public SimulationScopeId AddDefaultSettlement()
    {
        EnsureInputJournalCapacity(1);
        var recordedAt = Time;
        var scopeId = AddDefaultSettlementCore();
        RecordInput(CreateInput(
            recordedAt,
            WorldInputKind.AddDefaultSettlement,
            addDefaultSettlement: new WorldAddDefaultSettlementInput(scopeId)));
        return scopeId;
    }

    public WorldOperationId AllocateOperationId()
    {
        EnsureInputJournalCapacity(1);
        var recordedAt = Time;
        var operationId = AllocateOperationIdCore();
        RecordInput(CreateInput(
            recordedAt,
            WorldInputKind.AllocateOperationId,
            allocateOperationId: new WorldAllocateOperationIdInput(operationId)));
        return operationId;
    }

    private SimulationScopeId AddDefaultSettlementCore()
    {
        if (_nextScopeId == ulong.MaxValue)
        {
            throw new InvalidOperationException("World scope ID space is exhausted.");
        }

        if (_nextEntityId > long.MaxValue - SettlementPrototypeContent.EntityIdSpan)
        {
            throw new InvalidOperationException("World entity ID space cannot reserve another settlement block.");
        }

        var scopeId = new SimulationScopeId(_nextScopeId);
        var blockStart = _nextEntityId;
        var entityIdOffset = checked(blockStart - 1);
        var settlement = SettlementSimulation.CreateDefault(new WorldSeed(_worldSeed), scopeId, entityIdOffset);

        AddPartition(settlement, revision: 0);
        _nextScopeId = checked(_nextScopeId + 1);
        _nextEntityId = checked(blockStart + SettlementPrototypeContent.EntityIdSpan);
        return scopeId;
    }

    private WorldOperationId AllocateOperationIdCore()
    {
        if (_nextOperationId <= 0 || _nextOperationId == long.MaxValue)
        {
            throw new InvalidOperationException("World operation ID space is exhausted or invalid.");
        }

        var id = new WorldOperationId(_nextOperationId);
        _nextOperationId = checked(_nextOperationId + 1);
        return id;
    }
}
