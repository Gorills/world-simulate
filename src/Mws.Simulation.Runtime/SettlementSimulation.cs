using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed class SettlementSimulation
{
    public const int CurrentSchemaVersion = SettlementVersions.CurrentSchemaVersion;
    public const long HourMilliseconds = 3_600_000;
    public const long DayMilliseconds = HourMilliseconds * 24;

    private static readonly EntityId DefaultSettlementOwnerId = new(1_000);
    private readonly ulong _worldSeed;
    private readonly EntityId _settlementOwnerId;
    private readonly List<ResidentState> _residents;
    private readonly List<ItemStackState> _itemStacks;
    private readonly List<WorkplaceState> _workplaces;
    private readonly List<SettlementEvent> _events;
    private long _nextEventId;
    private long _nextStackId;

    private SettlementSimulation(
        ulong worldSeed,
        SimulationTime time,
        long nextEventId,
        long nextStackId,
        EntityId settlementOwnerId,
        IEnumerable<ResidentState> residents,
        IEnumerable<ItemStackState> itemStacks,
        IEnumerable<WorkplaceState> workplaces,
        IEnumerable<SettlementEvent> events)
    {
        _worldSeed = worldSeed;
        Time = time;
        _nextEventId = nextEventId;
        _nextStackId = nextStackId;
        _settlementOwnerId = settlementOwnerId;
        _residents = residents.OrderBy(resident => resident.Id.Value).ToList();
        _itemStacks = itemStacks.OrderBy(stack => stack.StackId).ToList();
        _workplaces = workplaces.OrderBy(workplace => workplace.Id.Value).ToList();
        _events = events.OrderBy(entry => entry.Id).ToList();
    }

    public SimulationTime Time { get; private set; }

    public static SettlementSimulation CreateDefault(WorldSeed seed)
    {
        var farmId = new EntityId(101);
        var kitchenId = new EntityId(102);
        var groveId = new EntityId(103);

        return new SettlementSimulation(
            seed.Value,
            new SimulationTime(0),
            1,
            3,
            DefaultSettlementOwnerId,
            [
                new ResidentState(new EntityId(1), "Mira", 20, 100, ResidentActivity.Idle, ResidentProfession.Farmer, farmId, 0),
                new ResidentState(new EntityId(2), "Tor", 25, 90, ResidentActivity.Idle, ResidentProfession.Cook, kitchenId, 0),
                new ResidentState(new EntityId(3), "Ena", 30, 80, ResidentActivity.Idle, ResidentProfession.Forager, groveId, 0),
            ],
            [
                new ItemStackState(1, SettlementItems.Ration, DefaultSettlementOwnerId, 6),
                new ItemStackState(2, SettlementItems.Grain, DefaultSettlementOwnerId, 4),
            ],
            [
                new WorkplaceState(farmId, "North Field", ResidentProfession.Farmer, null, 0, SettlementItems.Grain, 2),
                new WorkplaceState(kitchenId, "Common Kitchen", ResidentProfession.Cook, SettlementItems.Grain, 2, SettlementItems.Ration, 1),
                new WorkplaceState(groveId, "Herb Grove", ResidentProfession.Forager, null, 0, SettlementItems.Herb, 1),
            ],
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
            state.SettlementOwnerId,
            state.Residents,
            state.ItemStacks,
            state.Workplaces,
            state.Events);
    }

    public void AdvanceHours(int hours)
    {
        if (hours < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(hours), hours, "Hours cannot be negative.");
        }

        for (var index = 0; index < hours; index++)
        {
            AdvanceOneHour();
        }
    }

    public SettlementCommandResult FeedResident(EntityId residentId)
    {
        var index = FindResidentIndex(residentId);
        if (index < 0)
        {
            return ResidentNotFound(residentId);
        }

        if (!TryConsumeItem(_settlementOwnerId, SettlementItems.Ration, 1))
        {
            return new SettlementCommandResult(false, "NO_RATIONS", residentId, "The settlement stockpile has no rations.");
        }

        var resident = _residents[index];
        _residents[index] = resident with
        {
            Hunger = Math.Max(0, resident.Hunger - 45),
            Activity = ResidentActivity.Eating,
        };
        AppendEvent("player-fed", residentId, $"Player gave a ration to {resident.Name}.");
        return new SettlementCommandResult(true, "OK", residentId, $"{resident.Name} ate one ration.");
    }

    public SettlementCommandResult GiveItemToResident(EntityId residentId, string itemId, int quantity)
    {
        if (quantity <= 0)
        {
            return new SettlementCommandResult(false, "INVALID_QUANTITY", residentId, "Quantity must be positive.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(itemId);
        var index = FindResidentIndex(residentId);
        if (index < 0)
        {
            return ResidentNotFound(residentId);
        }

        if (!TryTransferItem(_settlementOwnerId, residentId, itemId, quantity))
        {
            return new SettlementCommandResult(false, "ITEM_NOT_AVAILABLE", residentId, $"Stockpile lacks {quantity} x {itemId}.");
        }

        var resident = _residents[index];
        AppendEvent("item-given", residentId, $"Player gave {quantity} x {itemId} to {resident.Name}.");
        return new SettlementCommandResult(true, "OK", residentId, $"{resident.Name} received {quantity} x {itemId}.");
    }

    public SettlementCommandResult InteractWithResident(EntityId residentId, ResidentInteractionChoice choice)
    {
        var index = FindResidentIndex(residentId);
        if (index < 0)
        {
            return ResidentNotFound(residentId);
        }

        var resident = _residents[index];
        switch (choice)
        {
            case ResidentInteractionChoice.AskAboutWork:
            {
                var workplace = FindWorkplace(resident.WorkplaceId);
                var workplaceName = workplace?.Name ?? "unassigned";
                AppendEvent("asked-about-work", residentId, $"{resident.Name} talked about work at {workplaceName}.");
                return new SettlementCommandResult(
                    true,
                    "OK",
                    residentId,
                    $"{resident.Name} is a {resident.Profession} working at {workplaceName}.");
            }

            case ResidentInteractionChoice.Encourage:
                _residents[index] = resident with
                {
                    Energy = Math.Min(100, resident.Energy + 10),
                    Affinity = checked(resident.Affinity + 1),
                };
                AppendEvent("encouraged", residentId, $"Player encouraged {resident.Name}.");
                return new SettlementCommandResult(true, "OK", residentId, $"{resident.Name} seems more confident.");

            case ResidentInteractionChoice.ShareRation:
                if (!TryConsumeItem(_settlementOwnerId, SettlementItems.Ration, 1))
                {
                    return new SettlementCommandResult(false, "NO_RATIONS", residentId, "The settlement stockpile has no rations.");
                }

                _residents[index] = resident with
                {
                    Hunger = Math.Max(0, resident.Hunger - 45),
                    Activity = ResidentActivity.Eating,
                    Affinity = checked(resident.Affinity + 2),
                };
                AppendEvent("shared-ration", residentId, $"Player shared a ration with {resident.Name}.");
                return new SettlementCommandResult(true, "OK", residentId, $"{resident.Name} appreciates the shared ration.");

            default:
                throw new ArgumentOutOfRangeException(nameof(choice), choice, "Unknown resident interaction.");
        }
    }

    public SettlementProjection Project()
    {
        var stockpile = ProjectInventory(_settlementOwnerId);
        var workplaces = _workplaces
            .OrderBy(workplace => workplace.Id.Value)
            .Select(workplace => new WorkplaceProjection(
                workplace.Id,
                workplace.Name,
                workplace.Profession,
                workplace.InputItemId,
                workplace.InputQuantity,
                workplace.OutputItemId,
                workplace.OutputQuantity))
            .ToArray();
        var residents = _residents
            .OrderBy(resident => resident.Id.Value)
            .Select(resident => new ResidentProjection(
                resident.Id,
                resident.Name,
                resident.Hunger,
                resident.Energy,
                resident.Activity,
                resident.Profession,
                FindWorkplace(resident.WorkplaceId)?.Name ?? "Unassigned",
                resident.Affinity,
                ProjectInventory(resident.Id)))
            .ToArray();
        var recentEvents = _events.TakeLast(8).ToArray();

        return new SettlementProjection(
            Time,
            checked((int)(Time.Milliseconds / DayMilliseconds)),
            checked((int)((Time.Milliseconds / HourMilliseconds) % 24)),
            ItemQuantity(_settlementOwnerId, SettlementItems.Ration),
            stockpile,
            workplaces,
            residents,
            recentEvents);
    }

    public SettlementState CaptureState() => new(
        CurrentSchemaVersion,
        _worldSeed,
        Time,
        _nextEventId,
        _nextStackId,
        _settlementOwnerId,
        _residents.OrderBy(resident => resident.Id.Value).ToArray(),
        _itemStacks.OrderBy(stack => stack.StackId).ToArray(),
        _workplaces.OrderBy(workplace => workplace.Id.Value).ToArray(),
        _events.OrderBy(entry => entry.Id).ToArray());

    private void AdvanceOneHour()
    {
        Time = Time.AddMilliseconds(HourMilliseconds);
        var hour = checked((int)((Time.Milliseconds / HourMilliseconds) % 24));
        var restingHours = hour >= 22 || hour < 6;

        for (var index = 0; index < _residents.Count; index++)
        {
            var resident = _residents[index];
            var hunger = Math.Min(100, resident.Hunger + 3);
            var energy = resident.Energy;
            var activity = ResidentActivity.Idle;

            if (hunger >= 70 && TryConsumeItem(_settlementOwnerId, SettlementItems.Ration, 1))
            {
                hunger = Math.Max(0, hunger - 45);
                activity = ResidentActivity.Eating;
            }
            else if (restingHours)
            {
                energy = Math.Min(100, energy + 12);
                activity = ResidentActivity.Resting;
            }
            else if (hour >= 8 && hour < 17 && energy >= 25 && TryWork(resident))
            {
                energy = Math.Max(0, energy - 6);
                activity = ResidentActivity.Working;
            }
            else
            {
                energy = Math.Max(0, energy - 1);
            }

            _residents[index] = resident with
            {
                Hunger = hunger,
                Energy = energy,
                Activity = activity,
            };
        }

        if (hour == 0)
        {
            var day = checked((int)(Time.Milliseconds / DayMilliseconds));
            AppendEvent(
                "day-began",
                null,
                $"Day {day} began with {ItemQuantity(_settlementOwnerId, SettlementItems.Ration)} rations.");
        }
    }

    private bool TryWork(ResidentState resident)
    {
        var workplace = FindWorkplace(resident.WorkplaceId);
        if (workplace is null || workplace.Profession != resident.Profession)
        {
            return false;
        }

        if (workplace.InputItemId is not null
            && !TryConsumeItem(_settlementOwnerId, workplace.InputItemId, workplace.InputQuantity))
        {
            return false;
        }

        AddItem(_settlementOwnerId, workplace.OutputItemId, workplace.OutputQuantity);
        return true;
    }

    private WorkplaceState? FindWorkplace(EntityId workplaceId) =>
        _workplaces.FirstOrDefault(workplace => workplace.Id == workplaceId);

    private int FindResidentIndex(EntityId residentId) =>
        _residents.FindIndex(resident => resident.Id == residentId);

    private static SettlementCommandResult ResidentNotFound(EntityId residentId) =>
        new(false, "RESIDENT_NOT_FOUND", residentId, "Resident does not exist.");

    private IReadOnlyList<ItemStackProjection> ProjectInventory(EntityId ownerId) =>
        _itemStacks
            .Where(stack => stack.OwnerId == ownerId && stack.Quantity > 0)
            .OrderBy(stack => stack.StackId)
            .Select(stack => new ItemStackProjection(stack.StackId, stack.ItemId, stack.Quantity))
            .ToArray();

    private int ItemQuantity(EntityId ownerId, string itemId) =>
        _itemStacks
            .Where(stack => stack.OwnerId == ownerId && string.Equals(stack.ItemId, itemId, StringComparison.Ordinal))
            .Sum(stack => stack.Quantity);

    private void AddItem(EntityId ownerId, string itemId, int quantity)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity, "Quantity must be positive.");
        }

        var index = _itemStacks.FindIndex(stack =>
            stack.OwnerId == ownerId && string.Equals(stack.ItemId, itemId, StringComparison.Ordinal));
        if (index >= 0)
        {
            var stack = _itemStacks[index];
            _itemStacks[index] = stack with { Quantity = checked(stack.Quantity + quantity) };
            return;
        }

        _itemStacks.Add(new ItemStackState(_nextStackId, itemId, ownerId, quantity));
        _nextStackId = checked(_nextStackId + 1);
    }

    private bool TryTransferItem(EntityId sourceOwnerId, EntityId destinationOwnerId, string itemId, int quantity)
    {
        if (!TryConsumeItem(sourceOwnerId, itemId, quantity))
        {
            return false;
        }

        AddItem(destinationOwnerId, itemId, quantity);
        return true;
    }

    private bool TryConsumeItem(EntityId ownerId, string itemId, int quantity)
    {
        if (quantity <= 0 || ItemQuantity(ownerId, itemId) < quantity)
        {
            return false;
        }

        var remaining = quantity;
        for (var index = 0; index < _itemStacks.Count && remaining > 0; index++)
        {
            var stack = _itemStacks[index];
            if (stack.OwnerId != ownerId
                || !string.Equals(stack.ItemId, itemId, StringComparison.Ordinal)
                || stack.Quantity == 0)
            {
                continue;
            }

            var consumed = Math.Min(stack.Quantity, remaining);
            _itemStacks[index] = stack with { Quantity = stack.Quantity - consumed };
            remaining -= consumed;
        }

        return remaining == 0;
    }

    private void AppendEvent(string kind, EntityId? subjectId, string summary)
    {
        _events.Add(new SettlementEvent(_nextEventId, Time, kind, subjectId, summary));
        _nextEventId = checked(_nextEventId + 1);
    }
}
