using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class SettlementSimulation
{
    public void AdvanceHours(long hours)
    {
        if (hours < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(hours), hours, "Hours cannot be negative.");
        }

        for (long index = 0; index < hours; index++)
        {
            AdvanceOneHour();
        }
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

        AdvanceHours(delta / HourMilliseconds);
    }

    private void AdvanceOneHour()
    {
        var targetTime = Time.AddMilliseconds(HourMilliseconds);
        var hour = checked((int)((targetTime.Milliseconds / HourMilliseconds) % 24));
        int? day = null;

        if (hour == 0)
        {
            if (_nextEventId <= 0 || _nextEventId == long.MaxValue)
            {
                throw new InvalidOperationException("Settlement event ID space cannot commit the next day boundary.");
            }

            day = checked((int)(targetTime.Milliseconds / DayMilliseconds));
        }

        BuildHourlyPlan(targetTime, hour);
        ApplySettlementInventoryDelta(_hourlyPlanWorkspace.Consumed, _hourlyPlanWorkspace.Produced);
        CommitHourlyResidentPlan();
        Time = targetTime;

        if (day is not null)
        {
            AppendEvent(
                SettlementEventKinds.DayBegan,
                null,
                IntFact(SettlementFactKeys.Day, day.Value),
                IntFact(SettlementFactKeys.Rations, ItemQuantity(_settlementOwnerId, SettlementItems.Ration)));
        }
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
