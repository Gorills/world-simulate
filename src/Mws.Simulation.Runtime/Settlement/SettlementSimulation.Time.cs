using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class SettlementSimulation
{
    private static readonly DeterministicCadenceScheduler SystemScheduler = new(
        new SettlementSystemSchedule(SettlementSystemKind.ResidentHourly, 100, HourMilliseconds),
        new SettlementSystemSchedule(SettlementSystemKind.DayBoundary, 200, DayMilliseconds));

    public void AdvanceHours(long hours)
    {
        if (hours < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(hours), hours, "Hours cannot be negative.");
        }

        var delta = checked(hours * HourMilliseconds);
        AdvanceTo(Time.AddMilliseconds(delta));
    }

    public void AdvanceTo(SimulationTime target)
    {
        if (target.Milliseconds < Time.Milliseconds)
        {
            throw new InvalidOperationException("Settlement simulation time is monotonic.");
        }

        var delta = checked(target.Milliseconds - Time.Milliseconds);
        if (delta % HourMilliseconds != 0)
        {
            throw new ArgumentException("Settlement simulation advances on canonical whole-hour boundaries.", nameof(target));
        }

        Span<SettlementSystemKind> dueSystems = stackalloc SettlementSystemKind[SystemScheduler.ScheduleCount];
        while (Time.Milliseconds < target.Milliseconds)
        {
            var activeSystems = SettlementSystemKind.DayBoundary;
            if (_residents.Length > 0)
            {
                activeSystems |= SettlementSystemKind.ResidentHourly;
            }

            var nextTime = SystemScheduler.NextDueAfter(Time, target, activeSystems);
            var dueCount = SystemScheduler.WriteDueSystems(nextTime, activeSystems, dueSystems);
            var dayBoundaryDue = false;
            var day = 0;

            for (var index = 0; index < dueCount; index++)
            {
                if (dueSystems[index] == SettlementSystemKind.DayBoundary)
                {
                    EnsureEventCapacity();
                    day = checked((int)(nextTime.Milliseconds / DayMilliseconds));
                    dayBoundaryDue = true;
                }
            }

            for (var index = 0; index < dueCount; index++)
            {
                if (dueSystems[index] == SettlementSystemKind.ResidentHourly)
                {
                    ExecuteHourlyResidentSystem(nextTime);
                }
            }

            Time = nextTime;
            if (dayBoundaryDue)
            {
                AppendEvent(
                    SettlementEventKinds.DayBegan,
                    null,
                    IntFact(SettlementFactKeys.Day, day),
                    IntFact(SettlementFactKeys.Rations, ItemQuantity(_settlementOwnerId, SettlementItems.Ration)));
            }
        }
    }

    private void ExecuteHourlyResidentSystem(SimulationTime targetTime)
    {
        var hour = checked((int)((targetTime.Milliseconds / HourMilliseconds) % 24));
        AdvanceResidentSemanticLocations(hour);
        BuildHourlyPlan(targetTime, hour);
        ApplySettlementInventoryDelta(_hourlyPlanWorkspace.Consumed, _hourlyPlanWorkspace.Produced);
        CommitHourlyResidentPlan();
    }

    private void CommitHourlyResidentPlan()
    {
        for (var index = 0; index < _residents.Length; index++)
        {
            var resident = _residents[index];
            resident.Hunger = _hourlyPlanWorkspace.Hunger[index];
            resident.Energy = _hourlyPlanWorkspace.Energy[index];
            resident.Activity = _hourlyPlanWorkspace.Activity[index];
        }
    }
}
