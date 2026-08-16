using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class SettlementSimulation
{
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
                SettlementEventKinds.DayBegan,
                null,
                IntFact(SettlementFactKeys.Day, day),
                IntFact(SettlementFactKeys.Rations, ItemQuantity(_settlementOwnerId, SettlementItems.Ration)));
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
}
