using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class ProofAKernel
{
    public ProofARegionAggregate AggregateRegion(IEnumerable<EntityId> entityIds)
    {
        ArgumentNullException.ThrowIfNull(entityIds);

        var ids = entityIds.Select(id => id.Value).Distinct().OrderBy(value => value).ToArray();
        var members = new List<ProofALodMember>(ids.Length);
        long total = 0;

        foreach (var id in ids)
        {
            if (!_entities.TryGetValue(id, out var entity))
            {
                throw new InvalidOperationException("LOD aggregate requested an unknown entity.");
            }

            members.Add(new ProofALodMember(entity.Id, entity.OwnerId, entity.Resource, entity.Rare));
            total = checked(total + entity.Resource);
        }

        var selected = ids.ToHashSet();
        var rareIds = members.Where(member => member.Rare).Select(member => member.Id).ToArray();
        var processIds = _pendingProcesses.Values
            .Where(process => selected.Contains(process.SubjectId.Value))
            .Select(process => process.ProcessId)
            .OrderBy(id => id)
            .ToArray();
        return new ProofARegionAggregate(members, total, rareIds, processIds);
    }

    public static IReadOnlyList<ProofALodMember> MaterializeRegion(ProofARegionAggregate aggregate)
    {
        ArgumentNullException.ThrowIfNull(aggregate);
        return aggregate.Members.OrderBy(member => member.Id.Value).ToArray();
    }

    public IReadOnlyList<ProofACausalTraceEntry> ReconstructConsequence(long traceId)
    {
        var byId = _trace.ToDictionary(entry => entry.TraceId);
        var chain = new List<ProofACausalTraceEntry>();
        long? current = traceId;

        while (current is not null)
        {
            if (!byId.TryGetValue(current.Value, out var entry))
            {
                return [];
            }

            chain.Add(entry);
            current = entry.ParentTraceId;
        }

        chain.Reverse();
        return chain;
    }

    public ProofAKernelState CaptureState() => new(
        ProofAVersions.CurrentSchemaVersion,
        ProofAVersions.CurrentModelVersion,
        ProofAVersions.CurrentConfigurationVersion,
        _worldSeed,
        Time,
        _nextEntityId,
        _nextCommandId,
        _nextProcessId,
        _nextTraceId,
        _entities.Values.ToArray(),
        _tombstones.Values.ToArray(),
        _pendingProcesses.Values.ToArray(),
        _boundRandomOutcomes.Values.ToArray(),
        _commandLedger.Values.ToArray(),
        _trace.OrderBy(entry => entry.TraceId).ToArray());

    public static SnapshotCompatibility AssessCompatibility(ProofAKernelState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (!string.Equals(state.ModelVersion, ProofAVersions.CurrentModelVersion, StringComparison.Ordinal))
        {
            return SnapshotCompatibility.Unsupported;
        }

        if (string.Equals(state.ConfigurationVersion, ProofAVersions.CurrentConfigurationVersion, StringComparison.Ordinal))
        {
            return SnapshotCompatibility.CompatibleDecode;
        }

        if (string.Equals(state.ConfigurationVersion, ProofAVersions.LegacyConfigurationVersion, StringComparison.Ordinal))
        {
            return SnapshotCompatibility.DeterministicMigration;
        }

        if (string.Equals(state.ConfigurationVersion, ProofAVersions.LossyLegacyConfigurationVersion, StringComparison.Ordinal))
        {
            return SnapshotCompatibility.LossyMigrationRequired;
        }

        return SnapshotCompatibility.Unsupported;
    }

    public static ProofAKernel Restore(ProofAKernelState state, bool traceEnabled = true)
    {
        ArgumentNullException.ThrowIfNull(state);
        var compatibility = AssessCompatibility(state);
        var compatibleState = compatibility switch
        {
            SnapshotCompatibility.CompatibleDecode => state,
            SnapshotCompatibility.DeterministicMigration => state with
            {
                ConfigurationVersion = ProofAVersions.CurrentConfigurationVersion,
                SchemaVersion = ProofAVersions.CurrentSchemaVersion,
            },
            SnapshotCompatibility.LossyMigrationRequired => throw new InvalidOperationException("Lossy migration requires an explicit external decision."),
            _ => throw new NotSupportedException("Snapshot model/configuration is unsupported."),
        };
        return new ProofAKernel(compatibleState, traceEnabled);
    }
}
