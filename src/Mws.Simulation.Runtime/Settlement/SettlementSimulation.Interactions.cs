using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class SettlementSimulation
{
    private SettlementCommandResult AskAboutWork(ResidentState resident)
    {
        var workplace = FindWorkplace(resident.WorkplaceId);
        var workplaceName = workplace?.Name ?? string.Empty;
        AppendEvent(SettlementEventKinds.AskedAboutWork, resident.Id);

        return Result(
            true,
            SettlementResultCodes.WorkInfo,
            resident.Id,
            Fact(SettlementFactKeys.ResidentName, resident.Name),
            Fact(SettlementFactKeys.Profession, resident.Profession.ToString()),
            Fact(SettlementFactKeys.WorkplaceName, workplaceName));
    }

    private SettlementCommandResult Encourage(int index, ResidentState resident)
    {
        EnsureEventCapacity();
        var affinity = IncreaseAffinity(resident.Affinity, 1);
        _residents[index] = resident with
        {
            Energy = Math.Min(100, resident.Energy + 10),
            Affinity = affinity.Value,
        };
        AppendEvent(SettlementEventKinds.Encouraged, resident.Id, IntFact(SettlementFactKeys.AffinityDelta, affinity.Delta));

        return Result(
            true,
            SettlementResultCodes.Encouraged,
            resident.Id,
            Fact(SettlementFactKeys.ResidentName, resident.Name),
            IntFact(SettlementFactKeys.AffinityDelta, affinity.Delta));
    }

    private SettlementCommandResult ShareRation(int index, ResidentState resident)
    {
        if (ItemQuantity(_settlementOwnerId, SettlementItems.Ration) < 1)
        {
            return Result(false, SettlementResultCodes.NoRations, resident.Id);
        }

        EnsureEventCapacity();
        var affinity = IncreaseAffinity(resident.Affinity, 2);
        if (!TryConsumeItem(_settlementOwnerId, SettlementItems.Ration, 1))
        {
            throw new InvalidOperationException("Validated ration reservation could not be committed.");
        }

        _residents[index] = resident with
        {
            Hunger = Math.Max(0, resident.Hunger - 45),
            Activity = ResidentActivity.Eating,
            Affinity = affinity.Value,
        };
        AppendEvent(
            SettlementEventKinds.SharedRation,
            resident.Id,
            Fact(SettlementFactKeys.ItemId, SettlementItems.Ration),
            IntFact(SettlementFactKeys.AffinityDelta, affinity.Delta));

        return Result(
            true,
            SettlementResultCodes.RationShared,
            resident.Id,
            Fact(SettlementFactKeys.ResidentName, resident.Name),
            Fact(SettlementFactKeys.ItemId, SettlementItems.Ration),
            IntFact(SettlementFactKeys.AffinityDelta, affinity.Delta));
    }

    private static (int Value, int Delta) IncreaseAffinity(int value, int requestedDelta)
    {
        var next = Math.Min((long)int.MaxValue, (long)value + requestedDelta);
        return ((int)next, checked((int)(next - value)));
    }
}
