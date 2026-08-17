using Mws.Domain;

namespace Mws.Simulation.Runtime;

[Flags]
internal enum SettlementSystemKind
{
    None = 0,
    ResidentHourly = 1 << 0,
    DayBoundary = 1 << 1,
}

internal readonly record struct SettlementSystemSchedule(
    SettlementSystemKind Kind,
    int Phase,
    long CadenceMilliseconds);

internal sealed class DeterministicCadenceScheduler
{
    private readonly SettlementSystemSchedule[] _schedules;

    internal DeterministicCadenceScheduler(params SettlementSystemSchedule[] schedules)
    {
        ArgumentNullException.ThrowIfNull(schedules);
        if (schedules.Length == 0)
        {
            throw new ArgumentException("At least one simulation system schedule is required.", nameof(schedules));
        }

        if (schedules.Any(schedule =>
            !IsSingleSystem(schedule.Kind)
            || schedule.Phase < 0
            || schedule.CadenceMilliseconds <= 0))
        {
            throw new ArgumentException("Simulation system schedules must have one kind, non-negative phase, and positive cadence.", nameof(schedules));
        }

        if (schedules.Select(schedule => schedule.Kind).Distinct().Count() != schedules.Length)
        {
            throw new ArgumentException("Simulation system kinds must be unique within a cadence scheduler.", nameof(schedules));
        }

        _schedules = schedules
            .OrderBy(schedule => schedule.Phase)
            .ThenBy(schedule => (int)schedule.Kind)
            .ToArray();
    }

    internal int ScheduleCount => _schedules.Length;

    internal SimulationTime NextDueAfter(
        SimulationTime current,
        SimulationTime target,
        SettlementSystemKind activeSystems)
    {
        if (current.Milliseconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(current), "Simulation cadence time cannot be negative.");
        }

        if (target.Milliseconds < current.Milliseconds)
        {
            throw new ArgumentOutOfRangeException(nameof(target), "Simulation cadence target cannot precede the current time.");
        }

        if (target == current)
        {
            return target;
        }

        var next = target.Milliseconds;
        foreach (var schedule in _schedules)
        {
            if (!IsEnabled(activeSystems, schedule.Kind))
            {
                continue;
            }

            var due = NextBoundaryAfter(current.Milliseconds, schedule.CadenceMilliseconds);
            if (due < next)
            {
                next = due;
            }
        }

        return new SimulationTime(next);
    }

    internal int WriteDueSystems(
        SimulationTime time,
        SettlementSystemKind activeSystems,
        Span<SettlementSystemKind> destination)
    {
        if (time.Milliseconds < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(time), "Simulation cadence time cannot be negative.");
        }

        if (destination.Length < _schedules.Length)
        {
            throw new ArgumentException("Due-system destination is smaller than the scheduler.", nameof(destination));
        }

        var count = 0;
        foreach (var schedule in _schedules)
        {
            if (IsEnabled(activeSystems, schedule.Kind)
                && time.Milliseconds > 0
                && time.Milliseconds % schedule.CadenceMilliseconds == 0)
            {
                destination[count++] = schedule.Kind;
            }
        }

        return count;
    }

    private static bool IsEnabled(SettlementSystemKind activeSystems, SettlementSystemKind system) =>
        (activeSystems & system) != 0;

    private static bool IsSingleSystem(SettlementSystemKind system)
    {
        var value = (int)system;
        return value > 0 && (value & (value - 1)) == 0;
    }

    private static long NextBoundaryAfter(long currentMilliseconds, long cadenceMilliseconds)
    {
        var remainder = currentMilliseconds % cadenceMilliseconds;
        var delta = remainder == 0
            ? cadenceMilliseconds
            : cadenceMilliseconds - remainder;
        return checked(currentMilliseconds + delta);
    }
}
