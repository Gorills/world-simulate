using System.Globalization;
using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class SettlementSimulation
{
    internal CommandId NextCommandId
    {
        get
        {
            if (_nextCommandId <= 0 || _nextCommandId == long.MaxValue)
            {
                throw new InvalidOperationException("Settlement command ID space is exhausted or invalid.");
            }

            return new CommandId(_nextCommandId);
        }
    }

    internal bool WouldMutateCommandState(SettlementCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.Id.Value <= 0 || command.Id.Value == long.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(command), "Command ID must be positive and allocatable.");
        }

        if (TryGetCommandReceipt(command.Id, out _)
            || command.Id.Value < _nextCommandId)
        {
            return false;
        }

        if (command is not FeedResidentCommand
            && command is not GiveItemToResidentCommand
            && command is not InteractWithResidentCommand)
        {
            throw new ArgumentOutOfRangeException(
                nameof(command),
                command.GetType().Name,
                "Unknown settlement command.");
        }

        return true;
    }

    public SettlementCommandResult Execute(SettlementCommand command)
    {
        ArgumentNullException.ThrowIfNull(command);
        if (command.Id.Value <= 0 || command.Id.Value == long.MaxValue)
        {
            throw new ArgumentOutOfRangeException(nameof(command), "Command ID must be positive and allocatable.");
        }

        if (TryGetCommandReceipt(command.Id, out var recorded))
        {
            return new SettlementCommandResult(
                recorded.Success,
                recorded.Code,
                recorded.SubjectId,
                recorded.Facts.ToArray());
        }

        if (command.Id.Value < _nextCommandId)
        {
            return Result(false, SettlementResultCodes.StaleCommand, null);
        }

        var result = command switch
        {
            FeedResidentCommand feed => ExecuteFeedResident(feed.ResidentId),
            GiveItemToResidentCommand give => ExecuteGiveItem(give.ResidentId, give.ItemId, give.Quantity),
            InteractWithResidentCommand interact => ExecuteInteraction(interact.ResidentId, interact.Choice),
            _ => throw new ArgumentOutOfRangeException(nameof(command), command.GetType().Name, "Unknown settlement command."),
        };

        _nextCommandId = checked(command.Id.Value + 1);
        RecordCommandReceipt(new SettlementCommandReceipt(
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

        if (ItemQuantity(_settlementOwnerId, SettlementItems.Ration) < 1)
        {
            return Result(false, SettlementResultCodes.NoRations, residentId);
        }

        EnsureEventCapacity();
        if (!TryConsumeItem(_settlementOwnerId, SettlementItems.Ration, 1))
        {
            throw new InvalidOperationException("Validated ration reservation could not be committed.");
        }

        var resident = _residents[index];
        resident.Hunger = Math.Max(0, resident.Hunger - 45);
        resident.Activity = ResidentActivity.Eating;

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

        if (ItemQuantity(_settlementOwnerId, itemId) < quantity)
        {
            return Result(
                false,
                SettlementResultCodes.ItemNotAvailable,
                residentId,
                Fact(SettlementFactKeys.ItemId, itemId),
                IntFact(SettlementFactKeys.Quantity, quantity));
        }

        if (!CanAddItem(residentId, itemId, quantity))
        {
            return Result(
                false,
                SettlementResultCodes.InventoryCapacityExceeded,
                residentId,
                Fact(SettlementFactKeys.ItemId, itemId),
                IntFact(SettlementFactKeys.Quantity, quantity));
        }

        EnsureEventCapacity();
        if (TryTransferItem(_settlementOwnerId, residentId, itemId, quantity) != ItemTransferStatus.Success)
        {
            throw new InvalidOperationException("Validated inventory transfer could not be committed.");
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
            ResidentInteractionChoice.Encourage => Encourage(resident),
            ResidentInteractionChoice.ShareRation => ShareRation(resident),
            _ => throw new ArgumentOutOfRangeException(nameof(choice), choice, "Unknown resident interaction."),
        };
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
