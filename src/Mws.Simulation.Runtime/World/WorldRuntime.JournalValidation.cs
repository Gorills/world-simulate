using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class WorldRuntime
{
    private static void ValidateInputEntry(WorldInputJournalEntry entry)
    {
        if (entry.Sequence <= 0
            || entry.Sequence == long.MaxValue
            || entry.RecordedAt.Milliseconds < 0
            || entry.RecordedAt.Milliseconds % SettlementSimulation.HourMilliseconds != 0)
        {
            throw new InvalidOperationException("World input journal entry has invalid sequence or time.");
        }

        var payloadCount = (entry.AddDefaultSettlement is null ? 0 : 1)
            + (entry.AllocateOperationId is null ? 0 : 1)
            + (entry.AdvanceTo is null ? 0 : 1)
            + (entry.SettlementCommand is null ? 0 : 1)
            + (entry.ResidentMigration is null ? 0 : 1)
            + (entry.EnqueueResidentMigration is null ? 0 : 1)
            + (entry.DispatchOutbox is null ? 0 : 1)
            + (entry.DeliverInbox is null ? 0 : 1)
            + (entry.UnloadSettlement is null ? 0 : 1)
            + (entry.LoadSettlement is null ? 0 : 1)
            + (entry.AddPlayerActor is null ? 0 : 1);
        if (payloadCount != 1)
        {
            throw new InvalidOperationException("World input journal entry must contain exactly one payload.");
        }

        var shapeIsValid = entry.Kind switch
        {
            WorldInputKind.AddDefaultSettlement =>
                entry.AddDefaultSettlement is not null
                && entry.AddDefaultSettlement.CreatedScopeId.Value > 0,
            WorldInputKind.AllocateOperationId =>
                entry.AllocateOperationId is not null
                && entry.AllocateOperationId.AllocatedOperationId.Value > 0
                && entry.AllocateOperationId.AllocatedOperationId.Value < long.MaxValue,
            WorldInputKind.AdvanceTo =>
                entry.AdvanceTo is not null
                && entry.AdvanceTo.TargetTime.Milliseconds > entry.RecordedAt.Milliseconds
                && entry.AdvanceTo.TargetTime.Milliseconds % SettlementSimulation.HourMilliseconds == 0,
            WorldInputKind.SettlementCommand =>
                entry.SettlementCommand is not null && SettlementCommandShapeIsValid(entry.SettlementCommand),
            WorldInputKind.ResidentMigration => entry.ResidentMigration is not null,
            WorldInputKind.EnqueueResidentMigration =>
                entry.EnqueueResidentMigration is not null
                && QueuedResidentMigrationIsValid(entry.EnqueueResidentMigration),
            WorldInputKind.DispatchOutbox =>
                entry.DispatchOutbox is not null
                && TransportBatchShapeIsValid(entry.DispatchOutbox, allowBlocked: false),
            WorldInputKind.DeliverInbox =>
                entry.DeliverInbox is not null
                && TransportBatchShapeIsValid(entry.DeliverInbox, allowBlocked: true),
            WorldInputKind.UnloadSettlement => ResidencyShapeIsValid(entry.UnloadSettlement),
            WorldInputKind.LoadSettlement => ResidencyShapeIsValid(entry.LoadSettlement),
            WorldInputKind.AddPlayerActor =>
                entry.AddPlayerActor is not null
                && entry.AddPlayerActor.CreatedPlayerId.Value > 0
                && entry.AddPlayerActor.CreatedPlayerId.Value < long.MaxValue
                && entry.AddPlayerActor.ScopeId.Value > 0,
            _ => false,
        };

        if (!shapeIsValid)
        {
            throw new InvalidOperationException("World input journal payload does not match its kind.");
        }
    }

    private static bool SettlementCommandShapeIsValid(WorldSettlementCommandInput input)
    {
        if (input.ScopeId.Value == 0
            || input.CommandId.Value <= 0
            || input.CommandId.Value == long.MaxValue)
        {
            return false;
        }

        return input.CommandKind switch
        {
            WorldSettlementCommandKind.FeedResident =>
                input.ItemId is null && input.Quantity == 0 && input.InteractionChoice is null,
            WorldSettlementCommandKind.GiveItemToResident =>
                input.InteractionChoice is null,
            WorldSettlementCommandKind.InteractWithResident =>
                input.ItemId is null && input.Quantity == 0 && input.InteractionChoice is not null,
            _ => false,
        };
    }

    private static bool TransportBatchShapeIsValid(WorldTransportBatchInput input, bool allowBlocked)
    {
        if (input.MaxMessages <= 0
            || input.ExpectedProcessedCount < 0
            || input.ExpectedProcessedCount > input.MaxMessages)
        {
            return false;
        }

        if (!allowBlocked)
        {
            return input.ExpectedBlockedCode is null;
        }

        if (input.ExpectedBlockedCode is null)
        {
            return true;
        }

        if (input.ExpectedProcessedCount >= input.MaxMessages)
        {
            return false;
        }

        return string.Equals(
                input.ExpectedBlockedCode,
                WorldTransportCodes.SourcePartitionUnavailable,
                StringComparison.Ordinal)
            || string.Equals(
                input.ExpectedBlockedCode,
                WorldTransportCodes.DestinationPartitionUnavailable,
                StringComparison.Ordinal);
    }

    private static bool ResidencyShapeIsValid(WorldPartitionResidencyInput? input) =>
        input is not null && input.ScopeId.Value > 0;

    private static SettlementCommand ToSettlementCommand(WorldSettlementCommandInput input) =>
        input.CommandKind switch
        {
            WorldSettlementCommandKind.FeedResident =>
                new FeedResidentCommand(input.CommandId, input.ResidentId),
            WorldSettlementCommandKind.GiveItemToResident =>
                new GiveItemToResidentCommand(input.CommandId, input.ResidentId, input.ItemId!, input.Quantity),
            WorldSettlementCommandKind.InteractWithResident =>
                new InteractWithResidentCommand(
                    input.CommandId,
                    input.ResidentId,
                    input.InteractionChoice!.Value),
            _ => throw new ArgumentOutOfRangeException(
                nameof(input),
                input.CommandKind,
                "Unknown settlement command kind."),
        };
}
