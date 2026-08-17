using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class SettlementSimulation
{
    public const int CurrentSchemaVersion = SettlementVersions.CurrentSchemaVersion;
    public const long HourMilliseconds = 3_600_000;
    public const long DayMilliseconds = HourMilliseconds * 24;

    private readonly SimulationScopeId _scopeId;
    private readonly ulong _worldSeed;
    private readonly EntityId _settlementOwnerId;
    private readonly ResidentRuntimeState[] _residents;
    private readonly List<ItemStackState> _itemStacks;
    private readonly List<WorkplaceState> _workplaces;
    private readonly List<SettlementEvent> _events;
    private readonly List<SettlementCommandReceipt> _commandReceipts;
    private readonly Dictionary<EntityId, int> _residentIndicesById;
    private readonly Dictionary<EntityId, WorkplaceState> _workplacesById;
    private long _nextEventId;
    private long _nextStackId;
    private long _nextCommandId;

    private SettlementSimulation(
        SimulationScopeId scopeId,
        ulong worldSeed,
        SimulationTime time,
        long nextEventId,
        long nextStackId,
        long nextCommandId,
        EntityId settlementOwnerId,
        IEnumerable<ResidentState> residents,
        IEnumerable<ItemStackState> itemStacks,
        IEnumerable<WorkplaceState> workplaces,
        IEnumerable<SettlementEvent> events,
        IEnumerable<SettlementCommandReceipt> commandReceipts,
        IEnumerable<HomeState> homes,
        IEnumerable<HouseholdState> households,
        int residentLocationEncodingVersion,
        IEnumerable<SettlementRouteConnectionState> routeConnections,
        IEnumerable<SettlementResidentRouteKnowledgeState> residentRouteKnowledge)
    {
        _scopeId = scopeId;
        _worldSeed = worldSeed;
        Time = time;
        _nextEventId = nextEventId;
        _nextStackId = nextStackId;
        _nextCommandId = nextCommandId;
        _settlementOwnerId = settlementOwnerId;
        var allowLegacyMissingTravelProgress = residentLocationEncodingVersion
            == SettlementVersions.LegacyResidentLocationEncodingVersion;
        _residents = residents
            .OrderBy(resident => resident.Id.Value)
            .Select(resident => new ResidentRuntimeState(
                resident,
                allowLegacyMissingTravelProgress))
            .ToArray();
        _itemStacks = itemStacks.OrderBy(stack => stack.StackId).ToList();
        _workplaces = workplaces.OrderBy(workplace => workplace.Id.Value).ToList();
        _events = events.OrderBy(entry => entry.Id).ToList();
        _commandReceipts = commandReceipts.OrderBy(entry => entry.CommandId.Value).ToList();
        _homes = homes.OrderBy(home => home.Id.Value).ToList();
        _households = households.OrderBy(household => household.Id.Value).ToList();
        _routeConnections = routeConnections.OrderBy(connection => connection.ConnectionId).ToList();
        _residentRouteKnowledge = residentRouteKnowledge
            .OrderBy(entry => entry.ResidentId.Value)
            .Select(entry => new SettlementResidentRouteKnowledgeState(
                entry.ResidentId,
                entry.KnownConnectionIds?.OrderBy(connectionId => connectionId).ToArray()
                    ?? throw new InvalidOperationException("Resident route knowledge is missing connection IDs.")))
            .ToList();
        _residentIndicesById = new Dictionary<EntityId, int>(_residents.Length);
        _workplacesById = new Dictionary<EntityId, WorkplaceState>(_workplaces.Count);
        _routeConnectionsById = new Dictionary<long, SettlementRouteConnectionState>(_routeConnections.Count);
        _routeConnectionsByPlace = new Dictionary<SettlementPlaceRef, List<SettlementRouteConnectionState>>();
        _knownRouteConnectionIdsByResident = new Dictionary<EntityId, HashSet<long>>(_residentRouteKnowledge.Count);
        _hourlyPlanWorkspace = new HourlyPlanWorkspace(_residents.Length);

        ValidateState();
        ValidateResidenceState();
        RebuildEntityIndexes();
        RebuildResidenceIndexes();
        RestoreOmittedResidentSemanticLocations(residentLocationEncodingVersion);
        ValidateResidentSemanticLocationReferences();
        ValidateRouteConnections();
        RebuildRouteIndexes();
        RebuildResidentRouteKnowledgeIndex();
        RebuildInventoryIndexes();
        ValidateInventoryTotals();
        RebuildHistoryIndexes();
    }

    public SimulationScopeId ScopeId => _scopeId;

    public SimulationTime Time { get; private set; }

    public static SettlementSimulation CreateDefault(WorldSeed seed) =>
        CreateDefault(seed, SimulationScopeId.Root);

    public static SettlementSimulation CreateDefault(WorldSeed seed, SimulationScopeId scopeId) =>
        CreateDefault(seed, scopeId, entityIdOffset: 0);

    internal static SettlementSimulation CreateDefault(
        WorldSeed seed,
        SimulationScopeId scopeId,
        long entityIdOffset)
    {
        var residents = SettlementPrototypeContent.CreateResidents(entityIdOffset);
        var itemStacks = SettlementPrototypeContent.CreateItemStacks(entityIdOffset);
        var workplaces = SettlementPrototypeContent.CreateWorkplaces(entityIdOffset);
        var homes = SettlementPrototypeContent.CreateHomes(entityIdOffset);
        var households = SettlementPrototypeContent.CreateHouseholds(entityIdOffset);
        SettlementPrototypeContent.Validate(residents, itemStacks, workplaces, homes, households);

        return new SettlementSimulation(
            scopeId,
            seed.Value,
            new SimulationTime(0),
            nextEventId: 1,
            nextStackId: 3,
            nextCommandId: 1,
            SettlementPrototypeContent.GetSettlementOwnerId(entityIdOffset),
            residents,
            itemStacks,
            workplaces,
            [],
            [],
            homes,
            households,
            SettlementVersions.CurrentResidentLocationEncodingVersion,
            [],
            []);
    }

    public static SettlementSimulation Restore(SettlementState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (state.SchemaVersion != CurrentSchemaVersion)
        {
            throw new NotSupportedException($"Settlement schema {state.SchemaVersion} is unsupported.");
        }

        EnsureVersion(state.ModelVersion, SettlementVersions.CurrentModelVersion, "model");
        EnsureVersion(state.RulesVersion, SettlementVersions.CurrentRulesVersion, "rules");
        EnsureVersion(state.ContentVersion, SettlementVersions.CurrentContentVersion, "content");
        if (state.ResidentLocationEncodingVersion is not (
            SettlementVersions.LegacyResidentLocationEncodingVersion
            or SettlementVersions.CurrentResidentLocationEncodingVersion))
        {
            throw new NotSupportedException(
                $"Resident location encoding {state.ResidentLocationEncodingVersion} is unsupported.");
        }

        return new SettlementSimulation(
            state.ScopeId,
            state.WorldSeed,
            state.Time,
            state.NextEventId,
            state.NextStackId,
            state.NextCommandId,
            state.SettlementOwnerId,
            state.Residents,
            state.ItemStacks,
            state.Workplaces,
            state.Events,
            state.CommandReceipts,
            state.Homes ?? [],
            state.Households ?? [],
            state.ResidentLocationEncodingVersion,
            state.RouteConnections ?? [],
            state.ResidentRouteKnowledge ?? []);
    }

    public SettlementState CaptureState() => new(
        CurrentSchemaVersion,
        SettlementVersions.CurrentModelVersion,
        SettlementVersions.CurrentRulesVersion,
        SettlementVersions.CurrentContentVersion,
        _scopeId,
        _worldSeed,
        Time,
        _nextEventId,
        _nextStackId,
        _nextCommandId,
        _settlementOwnerId,
        _residents
            .Select(resident => resident.Capture(CaptureResidentSemanticLocation(resident)))
            .ToArray(),
        _itemStacks.OrderBy(stack => stack.StackId).ToArray(),
        _workplaces.OrderBy(workplace => workplace.Id.Value).ToArray(),
        _events.OrderBy(entry => entry.Id).ToArray(),
        _commandReceipts.OrderBy(entry => entry.CommandId.Value).ToArray(),
        _homes.OrderBy(home => home.Id.Value).ToArray(),
        _households.OrderBy(household => household.Id.Value).ToArray(),
        SettlementVersions.CurrentResidentLocationEncodingVersion,
        CaptureRouteConnections(),
        CaptureResidentRouteKnowledge());

    private CommandId AllocateCommandId()
    {
        if (_nextCommandId <= 0 || _nextCommandId == long.MaxValue)
        {
            throw new InvalidOperationException("Settlement command ID space is exhausted or invalid.");
        }

        return new CommandId(_nextCommandId);
    }

    private void RebuildEntityIndexes()
    {
        _residentIndicesById.Clear();
        for (var index = 0; index < _residents.Length; index++)
        {
            _residentIndicesById.Add(_residents[index].Id, index);
        }

        _workplacesById.Clear();
        foreach (var workplace in _workplaces)
        {
            _workplacesById.Add(workplace.Id, workplace);
        }
    }
}
