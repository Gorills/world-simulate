using System.Collections.ObjectModel;
using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class WorldRuntime
{
    private WorldPlayerActorState? _player;

    public EntityId? PlayerId => _player?.Id;

    public EntityId AddPlayerActor(SimulationScopeId scopeId)
    {
        EnsureInputJournalCapacity(1);
        var recordedAt = Time;
        var playerId = AddPlayerActorCore(scopeId);
        RecordInput(CreateInput(
            recordedAt,
            WorldInputKind.AddPlayerActor,
            addPlayerActor: new WorldAddPlayerActorInput(playerId, scopeId)));
        return playerId;
    }

    public void SelectPlayerTask(
        long taskId,
        string kind,
        string reasonReference,
        SettlementPlaceRef? requiredPlace)
    {
        EnsureInputJournalCapacity(1);
        var input = new WorldSelectPlayerTaskInput(
            taskId,
            kind,
            reasonReference,
            Time,
            requiredPlace);
        var recordedAt = Time;
        SelectPlayerTaskCore(input);
        RecordInput(CreateInput(
            recordedAt,
            WorldInputKind.SelectPlayerTask,
            selectPlayerTask: input));
    }

    public WorldPlayerProjection ProjectPlayer()
    {
        var player = _player
            ?? throw new InvalidOperationException("World does not contain an authoritative player actor.");
        var location = SettlementSemanticLocation.Normalize(player.Location);
        var selectedTask = player.SelectedTask is null
            ? null
            : new SettlementSelectedTaskProjection(
                player.SelectedTask.TaskId,
                player.SelectedTask.Kind,
                player.SelectedTask.ReasonReference,
                player.SelectedTask.SelectedAt,
                player.SelectedTask.RequiredPlace);
        return new WorldPlayerProjection(
            player.Id,
            player.ScopeId,
            player.Inventory
                .Select(item => new WorldPlayerInventoryItemProjection(item.ItemId, item.Quantity))
                .ToArray(),
            SettlementSemanticLocation.Project(location),
            selectedTask,
            (player.KnownRouteConnectionIds ?? []).ToArray());
    }

    private EntityId AddPlayerActorCore(SimulationScopeId scopeId)
    {
        _ = GetPartition(scopeId);
        if (_player is not null)
        {
            throw new InvalidOperationException("World already contains an authoritative player actor.");
        }

        if (_nextEntityId <= 0 || _nextEntityId == long.MaxValue)
        {
            throw new InvalidOperationException("World entity ID space is exhausted or invalid.");
        }

        var playerId = new EntityId(_nextEntityId);
        if (_entityLocations.ContainsKey(playerId.Value))
        {
            throw new InvalidOperationException("Allocated player entity ID collides with the world entity directory.");
        }

        var inventory = CanonicalPlayerInventory(WorldPlayerPrototypeContent.CreateStartingInventory());
        _player = new WorldPlayerActorState(
            playerId,
            scopeId,
            inventory,
            SettlementActorLocationState.At(SettlementPlaceRef.Settlement),
            WorldPlayerLocationVersions.CurrentEncodingVersion,
            KnownRouteConnectionIds: []);
        _entityLocations.Add(playerId.Value, scopeId);
        _nextEntityId = checked(_nextEntityId + 1);
        return playerId;
    }

    private void SelectPlayerTaskCore(WorldSelectPlayerTaskInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var player = _player
            ?? throw new InvalidOperationException("World does not contain an authoritative player actor.");
        if (input.SelectedAt != Time)
        {
            throw new InvalidOperationException("Player task selection must be recorded at current world time.");
        }

        var location = SettlementSemanticLocation.Normalize(player.Location);
        if (location.Kind == SettlementActorLocationKind.Travelling)
        {
            throw new InvalidOperationException(
                "Player travel reconsideration requires an accepted cancellation/reroute mechanic.");
        }

        var task = new SettlementSelectedTaskState(
            input.TaskId,
            input.Kind,
            input.ReasonReference,
            input.SelectedAt,
            input.RequiredPlace);
        var authority = CreatePlayerTravelAuthority(player.ScopeId);
        authority.ValidateExternalSelectedTask(task, Time);
        _player = player with { SelectedTask = task };
    }

    private void RestorePlayer(WorldPlayerActorState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (_player is not null
            || state.Id.Value <= 0
            || state.Id.Value >= _nextEntityId
            || !_partitions.ContainsKey(state.ScopeId.Value)
            || _entityLocations.ContainsKey(state.Id.Value))
        {
            throw new InvalidOperationException("World player actor metadata is invalid or collides globally.");
        }

        if (state.LocationEncodingVersion is not (
            WorldPlayerLocationVersions.LegacyEncodingVersion
            or WorldPlayerLocationVersions.CurrentEncodingVersion))
        {
            throw new NotSupportedException(
                $"World player location encoding {state.LocationEncodingVersion} is unsupported.");
        }

        SettlementOnFootActorCapabilityAuthority.Validate(
            state.OnFootCapability,
            state.OnFootCapabilityProvenanceReference,
            state.IsOnFootCapabilityFixture);
        SettlementOnFootCarriedLoadAuthority.Validate(
            state.OnFootCarriedLoad,
            state.OnFootCarriedLoadProvenanceReference,
            state.IsOnFootCarriedLoadFixture);
        var inventory = CanonicalPlayerInventory(state.Inventory);
        var knownRouteConnectionIds = CanonicalPlayerKnownRouteConnectionIds(
            state.KnownRouteConnectionIds);
        var location = SettlementSemanticLocation.NormalizeForRestore(
            state.Location,
            state.LocationEncodingVersion == WorldPlayerLocationVersions.LegacyEncodingVersion);
        var authority = CreatePlayerTravelAuthority(state.ScopeId);
        authority.ValidateExternalActorTravelState(
            location,
            state.SelectedTask,
            knownRouteConnectionIds,
            Time);
        _player = new WorldPlayerActorState(
            state.Id,
            state.ScopeId,
            inventory,
            location,
            WorldPlayerLocationVersions.CurrentEncodingVersion,
            state.OnFootCapability,
            state.OnFootCapabilityProvenanceReference,
            state.IsOnFootCapabilityFixture,
            state.OnFootCarriedLoad,
            state.OnFootCarriedLoadProvenanceReference,
            state.IsOnFootCarriedLoadFixture,
            state.SelectedTask,
            knownRouteConnectionIds);
        _entityLocations.Add(state.Id.Value, state.ScopeId);
    }

    private WorldPlayerActorState? CapturePlayerState()
    {
        if (_player is null)
        {
            return null;
        }

        return new WorldPlayerActorState(
            _player.Id,
            _player.ScopeId,
            _player.Inventory
                .Select(item => new WorldPlayerInventoryItemState(item.ItemId, item.Quantity))
                .ToArray(),
            SettlementSemanticLocation.Capture(
                SettlementSemanticLocation.Normalize(_player.Location)),
            WorldPlayerLocationVersions.CurrentEncodingVersion,
            _player.OnFootCapability,
            _player.OnFootCapabilityProvenanceReference,
            _player.IsOnFootCapabilityFixture,
            _player.OnFootCarriedLoad,
            _player.OnFootCarriedLoadProvenanceReference,
            _player.IsOnFootCarriedLoadFixture,
            _player.SelectedTask,
            CanonicalPlayerKnownRouteConnectionIds(_player.KnownRouteConnectionIds));
    }

    private SettlementSimulation CreatePlayerTravelAuthority(SimulationScopeId scopeId)
    {
        var state = CapturePartitionStateAtCurrentTime(GetPartition(scopeId));
        return SettlementSimulation.Restore(state);
    }

    private static ReadOnlyCollection<long> CanonicalPlayerKnownRouteConnectionIds(
        IEnumerable<long>? connectionIds)
    {
        var ids = (connectionIds ?? []).ToArray();
        if (ids.Any(id => id <= 0) || ids.Distinct().Count() != ids.Length)
        {
            throw new InvalidOperationException(
                "World player route knowledge contains invalid or duplicate connection IDs.");
        }

        Array.Sort(ids);
        return Array.AsReadOnly(ids);
    }

    private static ReadOnlyCollection<WorldPlayerInventoryItemState> CanonicalPlayerInventory(
        IEnumerable<WorldPlayerInventoryItemState> inventory)
    {
        ArgumentNullException.ThrowIfNull(inventory);
        var items = inventory.ToArray();
        if (items.Any(item => string.IsNullOrWhiteSpace(item.ItemId) || item.Quantity <= 0)
            || items.Select(item => item.ItemId).Distinct(StringComparer.Ordinal).Count() != items.Length)
        {
            throw new InvalidOperationException("World player inventory contains invalid or duplicate items.");
        }

        var ordered = items.OrderBy(item => item.ItemId, StringComparer.Ordinal).ToArray();
        if (!items.SequenceEqual(ordered))
        {
            throw new InvalidOperationException("World player inventory must use canonical item ordering.");
        }

        return Array.AsReadOnly(ordered);
    }
}
