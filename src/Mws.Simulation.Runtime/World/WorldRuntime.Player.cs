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

    public WorldPlayerProjection ProjectPlayer()
    {
        var player = _player
            ?? throw new InvalidOperationException("World does not contain an authoritative player actor.");
        var location = SettlementSemanticLocation.Normalize(player.Location);
        return new WorldPlayerProjection(
            player.Id,
            player.ScopeId,
            player.Inventory
                .Select(item => new WorldPlayerInventoryItemProjection(item.ItemId, item.Quantity))
                .ToArray(),
            SettlementSemanticLocation.Project(location));
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
            WorldPlayerLocationVersions.CurrentEncodingVersion);
        _entityLocations.Add(playerId.Value, scopeId);
        _nextEntityId = checked(_nextEntityId + 1);
        return playerId;
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
        var location = SettlementSemanticLocation.NormalizeForRestore(
            state.Location,
            state.LocationEncodingVersion == WorldPlayerLocationVersions.LegacyEncodingVersion);
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
            state.IsOnFootCarriedLoadFixture);
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
            _player.IsOnFootCarriedLoadFixture);
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
