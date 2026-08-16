using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class SettlementSimulation
{
    public const int CurrentSchemaVersion = SettlementVersions.CurrentSchemaVersion;
    public const long HourMilliseconds = 3_600_000;
    public const long DayMilliseconds = HourMilliseconds * 24;

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
    }

    public SimulationTime Time { get; private set; }

    public static SettlementSimulation CreateDefault(WorldSeed seed)
    {
        var residents = SettlementPrototypeContent.CreateResidents();
        var itemStacks = SettlementPrototypeContent.CreateItemStacks();
        var workplaces = SettlementPrototypeContent.CreateWorkplaces();
        SettlementPrototypeContent.Validate(residents, itemStacks, workplaces);

        return new SettlementSimulation(
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

        return new SettlementSimulation(
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
        SettlementPrototypeContent.Validate(_residents, _itemStacks, _workplaces);

        EnsureUnique(_events.Select(entry => entry.Id), "event");
        EnsureUnique(_commandReceipts.Select(entry => entry.CommandId.Value), "command receipt");

        if (_residents.Any(resident =>
            resident.Hunger is < 0 or > 100 || resident.Energy is < 0 or > 100))
        {
            throw new InvalidOperationException("Settlement state contains out-of-range resident needs.");
        }

        if (_nextEventId <= _events.Select(entry => entry.Id).DefaultIfEmpty(0).Max()
            || _nextStackId <= _itemStacks.Select(stack => stack.StackId).DefaultIfEmpty(0).Max()
            || _nextCommandId <= _commandReceipts.Select(entry => entry.CommandId.Value).DefaultIfEmpty(0).Max())
        {
            throw new InvalidOperationException("Settlement next-ID markers must be greater than persisted IDs.");
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
