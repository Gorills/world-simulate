using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed class SettlementSimulation
{
    public const int CurrentSchemaVersion = SettlementVersions.CurrentSchemaVersion;
    public const long HourMilliseconds = 3_600_000;
    public const long DayMilliseconds = HourMilliseconds * 24;

    private readonly ulong _worldSeed;
    private readonly List<ResidentState> _residents;
    private readonly List<SettlementEvent> _events;
    private long _nextEventId;
    private int _pantryRations;

    private SettlementSimulation(
        ulong worldSeed,
        SimulationTime time,
        long nextEventId,
        int pantryRations,
        IEnumerable<ResidentState> residents,
        IEnumerable<SettlementEvent> events)
    {
        _worldSeed = worldSeed;
        Time = time;
        _nextEventId = nextEventId;
        _pantryRations = pantryRations;
        _residents = residents.OrderBy(resident => resident.Id.Value).ToList();
        _events = events.OrderBy(entry => entry.Id).ToList();
    }

    public SimulationTime Time { get; private set; }

    public static SettlementSimulation CreateDefault(WorldSeed seed) => new(
        seed.Value,
        new SimulationTime(0),
        1,
        6,
        [
            new ResidentState(new EntityId(1), "Mira", 20, 100, ResidentActivity.Idle),
            new ResidentState(new EntityId(2), "Tor", 25, 90, ResidentActivity.Idle),
            new ResidentState(new EntityId(3), "Ena", 30, 80, ResidentActivity.Idle),
        ],
        []);

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
            state.PantryRations,
            state.Residents,
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
        var index = _residents.FindIndex(resident => resident.Id == residentId);
        if (index < 0)
        {
            return new SettlementCommandResult(false, "RESIDENT_NOT_FOUND", residentId, "Resident does not exist.");
        }

        if (_pantryRations <= 0)
        {
            return new SettlementCommandResult(false, "NO_RATIONS", residentId, "The settlement pantry is empty.");
        }

        var resident = _residents[index];
        _pantryRations--;
        var updated = resident with
        {
            Hunger = Math.Max(0, resident.Hunger - 45),
            Activity = ResidentActivity.Eating,
        };
        _residents[index] = updated;
        AppendEvent("player-fed", residentId, $"Player gave a ration to {resident.Name}.");
        return new SettlementCommandResult(true, "OK", residentId, $"{resident.Name} ate one ration.");
    }

    public SettlementProjection Project()
    {
        var residents = _residents
            .OrderBy(resident => resident.Id.Value)
            .Select(resident => new ResidentProjection(
                resident.Id,
                resident.Name,
                resident.Hunger,
                resident.Energy,
                resident.Activity))
            .ToArray();
        var recentEvents = _events.TakeLast(8).ToArray();
        return new SettlementProjection(
            Time,
            checked((int)(Time.Milliseconds / DayMilliseconds)),
            checked((int)((Time.Milliseconds / HourMilliseconds) % 24)),
            _pantryRations,
            residents,
            recentEvents);
    }

    public SettlementState CaptureState() => new(
        CurrentSchemaVersion,
        _worldSeed,
        Time,
        _nextEventId,
        _pantryRations,
        _residents.OrderBy(resident => resident.Id.Value).ToArray(),
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

            if (hunger >= 70 && _pantryRations > 0)
            {
                _pantryRations--;
                hunger = Math.Max(0, hunger - 45);
                activity = ResidentActivity.Eating;
            }
            else if (restingHours)
            {
                energy = Math.Min(100, energy + 12);
                activity = ResidentActivity.Resting;
            }
            else if (hour >= 8 && hour < 17 && energy >= 25)
            {
                energy = Math.Max(0, energy - 6);
                _pantryRations++;
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
            AppendEvent("day-began", null, $"Day {day} began with {_pantryRations} pantry rations.");
        }
    }

    private void AppendEvent(string kind, EntityId? subjectId, string summary)
    {
        _events.Add(new SettlementEvent(_nextEventId, Time, kind, subjectId, summary));
        _nextEventId = checked(_nextEventId + 1);
    }
}
