using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class SettlementSimulation
{
    private readonly HourlyPlanWorkspace _hourlyPlanWorkspace;

    private void BuildHourlyPlan(SimulationTime targetTime, int hour)
    {
        var restingHours = hour >= 22 || hour < 6;
        var workHours = hour >= 8 && hour < 17;
        var plan = _hourlyPlanWorkspace;
        plan.Reset(_residents);

        var rationBudget = GetBudget(SettlementItems.Ration, plan.AvailableInputs, plan.ProjectedFinal);
        for (var index = 0; index < _residents.Length; index++)
        {
            if (_residents[index].SelectedTask is not null)
            {
                continue;
            }

            if (plan.Hunger[index] >= 70)
            {
                plan.HungryCandidates.Add(index);
            }
        }

        plan.HungryCandidates.Sort((left, right) =>
        {
            var hunger = plan.Hunger[right].CompareTo(plan.Hunger[left]);
            if (hunger != 0)
            {
                return hunger;
            }

            var leftRank = DeterministicSimulationHash.Rank(
                _worldSeed,
                _scopeId,
                targetTime,
                "resident-auto-eat",
                _residents[left].Id);
            var rightRank = DeterministicSimulationHash.Rank(
                _worldSeed,
                _scopeId,
                targetTime,
                "resident-auto-eat",
                _residents[right].Id);
            var rank = leftRank.CompareTo(rightRank);
            return rank != 0 ? rank : _residents[left].Id.Value.CompareTo(_residents[right].Id.Value);
        });

        foreach (var index in plan.HungryCandidates)
        {
            if (rationBudget == 0)
            {
                break;
            }

            plan.Hunger[index] = Math.Max(0, plan.Hunger[index] - 45);
            plan.Activity[index] = ResidentActivity.Eating;
            plan.Eating[index] = true;
            rationBudget--;
            plan.AvailableInputs[SettlementItems.Ration] = rationBudget;
            plan.ProjectedFinal[SettlementItems.Ration]--;
            AddQuantity(plan.Consumed, SettlementItems.Ration, 1);
        }

        for (var index = 0; index < _residents.Length; index++)
        {
            if (plan.Eating[index])
            {
                continue;
            }

            var resident = _residents[index];
            if (resident.SelectedTask is not null)
            {
                // The selected task is authoritative. Until task execution is implemented,
                // keep compatibility eat/rest/work selection from replacing it implicitly.
                plan.Energy[index] = Math.Max(0, resident.Energy - 1);
                continue;
            }

            if (restingHours && IsResidentAtHome(resident))
            {
                plan.Energy[index] = Math.Min(100, resident.Energy + 12);
                plan.Activity[index] = ResidentActivity.Resting;
                continue;
            }

            if (workHours
                && resident.Energy >= 25
                && FindWorkplace(resident.WorkplaceId) is not null
                && IsResidentAtWorkplace(resident))
            {
                plan.WorkCandidates.Add(index);
                continue;
            }

            plan.Energy[index] = Math.Max(0, resident.Energy - 1);
        }

        plan.WorkCandidates.Sort((left, right) =>
        {
            var leftRank = DeterministicSimulationHash.Rank(
                _worldSeed,
                _scopeId,
                targetTime,
                "resident-work-reservation",
                _residents[left].Id);
            var rightRank = DeterministicSimulationHash.Rank(
                _worldSeed,
                _scopeId,
                targetTime,
                "resident-work-reservation",
                _residents[right].Id);
            var rank = leftRank.CompareTo(rightRank);
            return rank != 0 ? rank : _residents[left].Id.Value.CompareTo(_residents[right].Id.Value);
        });

        foreach (var index in plan.WorkCandidates)
        {
            var resident = _residents[index];
            var workplace = FindWorkplace(resident.WorkplaceId);
            if (workplace is null || workplace.Profession != resident.Profession)
            {
                plan.Energy[index] = Math.Max(0, resident.Energy - 1);
                continue;
            }

            if (!CanReserveWork(workplace, plan.AvailableInputs, plan.ProjectedFinal))
            {
                plan.Energy[index] = Math.Max(0, resident.Energy - 1);
                continue;
            }

            ReserveWork(workplace, plan.AvailableInputs, plan.ProjectedFinal, plan.Consumed, plan.Produced);
            plan.Energy[index] = Math.Max(0, resident.Energy - 6);
            plan.Activity[index] = ResidentActivity.Working;
        }
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

    private static void ReserveWork(
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

    private sealed class HourlyPlanWorkspace
    {
        internal HourlyPlanWorkspace(int residentCount)
        {
            Hunger = new int[residentCount];
            Energy = new int[residentCount];
            Activity = new ResidentActivity[residentCount];
            Eating = new bool[residentCount];
            HungryCandidates = new List<int>(residentCount);
            WorkCandidates = new List<int>(residentCount);
        }

        internal int[] Hunger { get; }

        internal int[] Energy { get; }

        internal ResidentActivity[] Activity { get; }

        internal bool[] Eating { get; }

        internal List<int> HungryCandidates { get; }

        internal List<int> WorkCandidates { get; }

        internal Dictionary<string, int> AvailableInputs { get; } = new(StringComparer.Ordinal);

        internal Dictionary<string, int> ProjectedFinal { get; } = new(StringComparer.Ordinal);

        internal Dictionary<string, int> Consumed { get; } = new(StringComparer.Ordinal);

        internal Dictionary<string, int> Produced { get; } = new(StringComparer.Ordinal);

        internal void Reset(ResidentRuntimeState[] residents)
        {
            HungryCandidates.Clear();
            WorkCandidates.Clear();
            AvailableInputs.Clear();
            ProjectedFinal.Clear();
            Consumed.Clear();
            Produced.Clear();
            Array.Clear(Eating);

            for (var index = 0; index < residents.Length; index++)
            {
                Hunger[index] = Math.Min(100, residents[index].Hunger + 3);
                Energy[index] = residents[index].Energy;
                Activity[index] = ResidentActivity.Idle;
            }
        }
    }
}
