using System.Globalization;
using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class SettlementSimulation
{
    public SettlementCommandResult Execute(SettlementCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.Id.Value <= 0 || command.Id.Value == long.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(command), "Command ID must be positive and allocatable.");
        }

        var recorded = _commandReceipts.FirstOrDefault(receipt => receipt.CommandId == command.Id);
        if (recorded is not null)
        {
            return new SettlementCommandResult(
                recorded.Success,
                recorded.Code,
                recorded.SubjectId,
                recorded.Facts.ToArray());
        }

        _nextCommandId = Math.Max(_nextCommandId, checked(command.Id.Value + 1));

        var result = command switch
        {
            FeedResidentCommand feed => ExecuteFeedResident(feed.ResidentId),
            GiveItemToResidentCommand give => ExecuteGiveItem(give.ResidentId, give.ItemId, give.Quantity),
            InteractWithResidentCommand interact => ExecuteInteraction(interact.ResidentId, interact.Choice),
            _ => throw new ArgumentOutOfRangeException(nameof(command), command.GetType().Name, "Unknown settlement command."),
        };

        _commandReceipts.Add(new SettlementCommandReceipt(
            command.Id,
            result.Success,
            result.Code,
            result.SubjectId,
            result.Facts.ToArray()));

        return result;
    }

    public SettlementCommandResult FeedResident(EntityId residentId) =>
        Execute(new FeedResidentCommand(AllocateCommandId(), residentId));

    public SettlementCommandResult GiveItemToResident(EntityId residentId, string itemId, int quantity) =>
        Execute(new GiveItemToResidentCommand(AllocateCommandId(), residentId, itemId, quantity));

    public SettlementCommandResult InteractWithResident(EntityId residentId, ResidentInteractionChoice choice) =>
        Execute(new InteractWithResidentCommand(AllocateCommandId(), residentId, choice));

    private SettlementCommandResult ExecuteFeedResident(EntityId residentId)
    {
        var index = FindResidentIndex(residentId);
        if (index < 0)
        {
            return ResidentNotFound(residentId);
        }

        if (!TryConsumeItem(_settlementOwnerId, SettlementItems.Ration, 1))
        {
            return Result(false, SettlementResultCodes.NoRations, residentId);
        }

        var resident = _residents[index];
        _residents[index] = resident with
        {
            Hunger = Math.Max(0, resident.Hunger - 45),
            Activity = ResidentActivity.Eating,
        };

        AppendEvent(
            SettlementEventKinds.PlayerFed,
            residentId,
            Fact(SettlementFactKeys.ItemId, SettlementItems.Ration),
            IntFact(SettlementFactKeys.Quantity, 1));

        return Result(
            true,
            SettlementResultCodes.FedResident,
            residentId,
            Fact(SettlementFactKeys.ResidentName, resident.Name),
            Fact(SettlementFactKeys.ItemId, SettlementItems.Ration),
            IntFact(SettlementFactKeys.Quantity, 1));
    }

    private SettlementCommandResult ExecuteGiveItem(EntityId residentId, string itemId, int quantity)
    {
        if (quantity <= 0)
        {
            return Result(
                false,
                SettlementResultCodes.InvalidQuantity,
                residentId,
                IntFact(SettlementFactKeys.Quantity, quantity));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(itemId);
        var index = FindResidentIndex(residentId);
        if (index < 0)
        {
            return ResidentNotFound(residentId);
        }

        var transfer = TryTransferItem(_settlementOwnerId, residentId, itemId, quantity);
        if (transfer == ItemTransferStatus.SourceUnavailable)
        {
            return Result(
                false,
                SettlementResultCodes.ItemNotAvailable,
                residentId,
                Fact(SettlementFactKeys.ItemId, itemId),
                IntFact(SettlementFactKeys.Quantity, quantity));
        }

        if (transfer == ItemTransferStatus.DestinationCapacityExceeded)
        {
            return Result(
                false,
                SettlementResultCodes.InventoryCapacityExceeded,
                residentId,
                Fact(SettlementFactKeys.ItemId, itemId),
                IntFact(SettlementFactKeys.Quantity, quantity));
        }

        var resident = _residents[index];
        AppendEvent(
            SettlementEventKinds.ItemGiven,
            residentId,
            Fact(SettlementFactKeys.ItemId, itemId),
            IntFact(SettlementFactKeys.Quantity, quantity));

        return Result(
            true,
            SettlementResultCodes.ItemGiven,
            residentId,
            Fact(SettlementFactKeys.ResidentName, resident.Name),
            Fact(SettlementFactKeys.ItemId, itemId),
            IntFact(SettlementFactKeys.Quantity, quantity));
    }

    private SettlementCommandResult ExecuteInteraction(EntityId residentId, ResidentInteractionChoice choice)
    {
        var index = FindResidentIndex(residentId);
        if (index < 0)
        {
            return ResidentNotFound(residentId);
        }

        var resident = _residents[index];
        return choice switch
        {
            ResidentInteractionChoice.AskAboutWork => AskAboutWork(resident),
            ResidentInteractionChoice.Encourage => Encourage(index, resident),
            ResidentInteractionChoice.ShareRation => ShareRation(index, resident),
            _ => throw new ArgumentOutOfRangeException(nameof(choice), choice, "Unknown resident interaction."),
        };
    }

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
        var affinity = IncreaseAffinity(resident.Affinity, 2);
        if (!TryConsumeItem(_settlementOwnerId, SettlementItems.Ration, 1))
        {
            return Result(false, SettlementResultCodes.NoRations, resident.Id);
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

    private static SettlementCommandResult ResidentNotFound(EntityId residentId) =>
        Result(false, SettlementResultCodes.ResidentNotFound, residentId);

    private static SettlementCommandResult Result(
        bool success,
        string code,
        EntityId? subjectId,
        params SettlementFact[] facts) =>
        new(success, code, subjectId, facts);

    private static SettlementFact Fact(string key, string value) => new(key, value);

    private static SettlementFact IntFact(string key, int value) =>
        new(key, value.ToString(CultureInfo.InvariantCulture));
}
