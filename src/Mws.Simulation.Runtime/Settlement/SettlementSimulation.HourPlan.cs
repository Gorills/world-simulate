using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class SettlementSimulation
{
    private HourlyPlan BuildHourlyPlan(SimulationTime targetTime, int hour)
    {
        var restingHours = hour >= 22 || hour < 6;
        var workHours = hour >= 8 && hour < 17;
        var nextResidents = _residents
            .Select(resident => resident with
            {
                Hunger = Math.Min(100, resident.Hunger + 3),
                Activity = ResidentActivity.Idle,
            })
            .ToArray();

        var availableInputs = new Dictionary<string, int>(StringComparer.Ordinal);
        var projectedFinal = new Dictionary<string, int>(StringComparer.Ordinal);
        var consumed = new Dictionary<string, int>(StringComparer.Ordinal);
        var produced = new Dictionary<string, int>(StringComparer.Ordinal);
        var eating = new HashSet<int>();
        var rationBudget = GetBudget(SettlementItems.Ration, availableInputs, projectedFinal);

        var hungryCandidates = nextResidents
            .Select((resident, index) => (Resident: resident, Index: index))
            .Where(entry => entry.Resident.Hunger >= 70)
            .OrderByDescending(entry => entry.Resident.Hunger)
            .ThenBy(entry => DeterministicSimulationHash.Rank(
                _worldSeed,
                _scopeId,
                targetTime,
                "resident-auto-eat",
                entry.Resident.Id))
            .ThenBy(entry => entry.Resident.Id.Value)
            .Select(entry => entry.Index);

        foreach (var index in hungryCandidates)
        {
            if (rationBudget == 0)
            {
                break;
            }

            var resident = nextResidents[index];
            nextResidents[index] = resident with
            {
                Hunger = Math.Max(0, resident.Hunger - 45),
                Activity = ResidentActivity.Eating,
            };
            eating.Add(index);
            rationBudget--;
            availableInputs[SettlementItems.Ration] = rationBudget;
            projectedFinal[SettlementItems.Ration]--;
            AddQuantity(consumed, SettlementItems.Ration, 1);
        }

        var workCandidates = new List<int>();
        for (var index = 0; index < nextResidents.Length; index++)
        {
            if (eating.Contains(index))
            {
                continue;
            }

            var resident = nextResidents[index];
            if (restingHours)
            {
                nextResidents[index] = resident with
                {
                    Energy = Math.Min(100, resident.Energy + 12),
                    Activity = ResidentActivity.Resting,
                };
                continue;
            }

            if (workHours && resident.Energy >= 25 && FindWorkplace(resident.WorkplaceId) is not null)
            {
                workCandidates.Add(index);
                continue;
            }

            nextResidents[index] = resident with { Energy = Math.Max(0, resident.Energy - 1) };
        }

        foreach (var index in workCandidates
            .OrderBy(index => DeterministicSimulationHash.Rank(
                _worldSeed,
                _scopeId,
                targetTime,
                "resident-work-reservation",
                nextResidents[index].Id))
            .ThenBy(index => nextResidents[index].Id.Value))
        {
            var resident = nextResidents[index];
            var workplace = FindWorkplace(resident.WorkplaceId);
            if (workplace is null || workplace.Profession != resident.Profession)
            {
                nextResidents[index] = resident with { Energy = Math.Max(0, resident.Energy - 1) };
                continue;
            }

            if (!CanReserveWork(workplace, availableInputs, projectedFinal))
            {
                nextResidents[index] = resident with { Energy = Math.Max(0, resident.Energy - 1) };
                continue;
            }

            ReserveWork(workplace, availableInputs, projectedFinal, consumed, produced);
            nextResidents[index] = resident with
            {
                Energy = Math.Max(0, resident.Energy - 6),
                Activity = ResidentActivity.Working,
            };
        }

        return new HourlyPlan(nextResidents, consumed, produced);
    }

    private int GetBudget(
        string itemId,
        IDictionary<string, int> availableInputs,
        IDictionary<string, int> projectedFinal)
    {
        if (!availableInputs.TryGetValue(itemId, out var available))
        {
            available = ItemQuantity(_settlementOwnerId, itemId);
            availableInputs[itemId] = available;
            projectedFinal[itemId] = available;
        }

        return available;
    }

    private bool CanReserveWork(
        WorkplaceState workplace,
        IDictionary<string, int> availableInputs,
        IDictionary<string, int> projectedFinal)
    {
        if (workplace.InputItemId is not null)
        {
            var available = GetBudget(workplace.InputItemId, availableInputs, projectedFinal);
            if (available < workplace.InputQuantity)
            {
                return false;
            }
        }

        _ = GetBudget(workplace.OutputItemId, availableInputs, projectedFinal);
        return projectedFinal[workplace.OutputItemId] <= int.MaxValue - workplace.OutputQuantity;
    }

    private void ReserveWork(
        WorkplaceState workplace,
        IDictionary<string, int> availableInputs,
        IDictionary<string, int> projectedFinal,
        IDictionary<string, int> consumed,
        IDictionary<string, int> produced)
    {
        if (workplace.InputItemId is not null)
        {
            availableInputs[workplace.InputItemId] -= workplace.InputQuantity;
            projectedFinal[workplace.InputItemId] -= workplace.InputQuantity;
            AddQuantity(consumed, workplace.InputItemId, workplace.InputQuantity);
        }

        projectedFinal[workplace.OutputItemId] += workplace.OutputQuantity;
        AddQuantity(produced, workplace.OutputItemId, workplace.OutputQuantity);
    }

    private static void AddQuantity(IDictionary<string, int> totals, string itemId, int quantity)
    {
        totals.TryGetValue(itemId, out var current);
        totals[itemId] = checked(current + quantity);
    }

    private sealed record HourlyPlan(
        ResidentState[] Residents,
        IReadOnlyDictionary<string, int> Consumed,
        IReadOnlyDictionary<string, int> Produced);
}
