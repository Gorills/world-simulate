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
    private readonly List<ResidentState> _residents;
    private readonly List<ItemStackState> _itemStacks;
    private readonly List<WorkplaceState> _workplaces;
    private readonly List<SettlementEvent> _events;
    private readonly List<SettlementCommandReceipt> _commandReceipts;
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
        IEnumerable<SettlementCommandReceipt> commandReceipts)
    {
        _scopeId = scopeId;
        _worldSeed = worldSeed;
        Time = time;
        _nextEventId = nextEventId;
        _nextStackId = nextStackId;
        _nextCommandId = nextCommandId;
        _settlementOwnerId = settlementOwnerId;
        _residents = residents.OrderBy(resident => resident.Id.Value).ToList();
        _itemStacks = itemStacks.OrderBy(stack => stack.StackId).ToList();
        _workplaces = workplaces.OrderBy(workplace => workplace.Id.Value).ToList();
        _events = events.OrderBy(entry => entry.Id).ToList();
        _commandReceipts = commandReceipts.OrderBy(entry => entry.CommandId.Value).ToList();

        ValidateState();
        RebuildInventoryIndexes();
    }

    public SimulationScopeId ScopeId => _scopeId;

    public SimulationTime Time { get; private set; }

    public static SettlementSimulation CreateDefault(WorldSeed seed) =>
        CreateDefault(seed, SimulationScopeId.Root);

    public static SettlementSimulation CreateDefault(WorldSeed seed, SimulationScopeId scopeId)
    {
        var residents = SettlementPrototypeContent.CreateResidents();
        var itemStacks = SettlementPrototypeContent.CreateItemStacks();
        var workplaces = SettlementPrototypeContent.CreateWorkplaces();
        SettlementPrototypeContent.Validate(residents, itemStacks, workplaces);

        return new SettlementSimulation(
            scopeId,
            seed.Value,
            new SimulationTime(0),
            nextEventId: 1,
            nextStackId: 3,
            nextCommandId: 1,
            SettlementPrototypeContent.SettlementOwnerId,
            residents,
            itemStacks,
            workplaces,
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
            state.CommandReceipts);
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
        _residents.OrderBy(resident => resident.Id.Value).ToArray(),
        _itemStacks.OrderBy(stack => stack.StackId).ToArray(),
        _workplaces.OrderBy(workplace => workplace.Id.Value).ToArray(),
        _events.OrderBy(entry => entry.Id).ToArray(),
        _commandReceipts.OrderBy(entry => entry.CommandId.Value).ToArray());

    private CommandId AllocateCommandId()
    {
        if (_nextCommandId <= 0 || _nextCommandId == long.MaxValue)
        {
            throw new InvalidOperationException("Settlement command ID space is exhausted or invalid.");
        }

        var id = new CommandId(_nextCommandId);
        _nextCommandId = checked(_nextCommandId + 1);
        return id;
    }

    private void ValidateState()
    {
        EnsureUnique(_residents.Select(resident => resident.Id.Value), "resident");
        EnsureUnique(_itemStacks.Select(stack => stack.StackId), "item stack");
        EnsureUnique(_workplaces.Select(workplace => workplace.Id.Value), "workplace");
        EnsureUnique(_events.Select(entry => entry.Id), "event");
        EnsureUnique(_commandReceipts.Select(entry => entry.CommandId.Value), "command receipt");

        if (Time.Milliseconds < 0)
        {
            throw new InvalidOperationException("Settlement time cannot be negative.");
        }

        if (_settlementOwnerId.Value <= 0
            || _residents.Any(resident => resident.Id.Value <= 0)
            || _workplaces.Any(workplace => workplace.Id.Value <= 0)
            || _itemStacks.Any(stack => stack.StackId <= 0 || stack.OwnerId.Value <= 0)
            || _events.Any(entry => entry.Id <= 0)
            || _commandReceipts.Any(entry => entry.CommandId.Value <= 0))
        {
            throw new InvalidOperationException("Settlement persisted identifiers must be positive.");
        }

        if (_residents.Any(resident =>
            string.IsNullOrWhiteSpace(resident.Name)
            || resident.Hunger is < 0 or > 100
            || resident.Energy is < 0 or > 100))
        {
            throw new InvalidOperationException("Settlement state contains an invalid resident.");
        }

        if (_itemStacks.Any(stack => string.IsNullOrWhiteSpace(stack.ItemId) || stack.Quantity < 0))
        {
            throw new InvalidOperationException("Settlement state contains an invalid item stack.");
        }

        foreach (var workplace in _workplaces)
        {
            if (string.IsNullOrWhiteSpace(workplace.Name)
                || string.IsNullOrWhiteSpace(workplace.OutputItemId)
                || workplace.OutputQuantity <= 0
                || (workplace.InputItemId is null && workplace.InputQuantity != 0)
                || (workplace.InputItemId is not null
                    && (string.IsNullOrWhiteSpace(workplace.InputItemId) || workplace.InputQuantity <= 0)))
            {
                throw new InvalidOperationException($"Settlement workplace {workplace.Id.Value} is invalid.");
            }
        }

        foreach (var resident in _residents)
        {
            if (resident.WorkplaceId.Value == 0)
            {
                continue;
            }

            if (_workplaces.All(workplace =>
                workplace.Id != resident.WorkplaceId || workplace.Profession != resident.Profession))
            {
                throw new InvalidOperationException(
                    $"Resident {resident.Id.Value} references a missing or incompatible workplace.");
            }
        }

        if (_nextEventId <= _events.Select(entry => entry.Id).DefaultIfEmpty(0).Max()
            || _nextStackId <= _itemStacks.Select(stack => stack.StackId).DefaultIfEmpty(0).Max()
            || _nextCommandId <= _commandReceipts.Select(entry => entry.CommandId.Value).DefaultIfEmpty(0).Max())
        {
            throw new InvalidOperationException("Settlement next-ID markers must be greater than persisted IDs.");
        }
    }

    private static void EnsureVersion(string actual, string expected, string kind)
    {
        if (!string.Equals(actual, expected, StringComparison.Ordinal))
        {
            throw new NotSupportedException($"Settlement {kind} version '{actual}' is unsupported; expected '{expected}'.");
        }
    }

    private static void EnsureUnique(IEnumerable<long> values, string kind)
    {
        var ids = values.ToArray();
        if (ids.Distinct().Count() != ids.Length)
        {
            throw new InvalidOperationException($"Settlement state contains duplicate {kind} IDs.");
        }
    }
}
