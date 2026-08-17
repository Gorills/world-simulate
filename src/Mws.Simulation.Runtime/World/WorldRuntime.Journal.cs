using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class WorldRuntime
{
    private const int MaxRetainedInputJournalEntries = 4_096;

    public static WorldRuntime ReplayFrom(
        WorldCheckpointState checkpoint,
        IEnumerable<WorldInputJournalEntry> journalTail)
    {
        ArgumentNullException.ThrowIfNull(checkpoint);
        ArgumentNullException.ThrowIfNull(journalTail);

        var world = Restore(checkpoint);
        foreach (var entry in journalTail)
        {
            world.ReplayInput(entry);
        }

        return world;
    }

    private void RestoreInputJournal(IEnumerable<WorldInputJournalEntry> entries)
    {
        var retained = entries.ToArray();
        if (retained.Length > MaxRetainedInputJournalEntries)
        {
            throw new InvalidOperationException("World input journal exceeds its retention bound.");
        }

        if (retained.Length == 0)
        {
            if (_inputJournalFloor != _nextInputSequence)
            {
                throw new InvalidOperationException("Empty world input journal has an invalid floor.");
            }

            return;
        }

        var expectedSequence = _inputJournalFloor;
        var previousTime = new SimulationTime(0);
        for (var index = 0; index < retained.Length; index++)
        {
            var entry = retained[index];
            ValidateInputEntry(entry);
            if (entry.Sequence != expectedSequence)
            {
                throw new InvalidOperationException("World input journal sequence is not contiguous.");
            }

            if (entry.RecordedAt.Milliseconds > Time.Milliseconds
                || (index > 0 && entry.RecordedAt.Milliseconds < previousTime.Milliseconds))
            {
                throw new InvalidOperationException("World input journal time ordering is invalid.");
            }

            _inputJournal.Enqueue(entry);
            previousTime = entry.RecordedAt;
            expectedSequence = checked(expectedSequence + 1);
        }

        if (expectedSequence != _nextInputSequence)
        {
            throw new InvalidOperationException("World input journal next-sequence marker is invalid.");
        }
    }

    private void ReplayInput(WorldInputJournalEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ValidateInputEntry(entry);
        if (entry.Sequence != _nextInputSequence)
        {
            throw new InvalidOperationException(
                $"Replay expected input sequence {_nextInputSequence}, received {entry.Sequence}.");
        }

        if (entry.RecordedAt != Time)
        {
            throw new InvalidOperationException(
                $"Replay input {entry.Sequence} was recorded at {entry.RecordedAt.Milliseconds}, "
                + $"but world time is {Time.Milliseconds}.");
        }

        EnsureInputJournalCapacity(1);

        switch (entry.Kind)
        {
            case WorldInputKind.AddDefaultSettlement:
            {
                var actual = AddDefaultSettlementCore();
                if (actual != entry.AddDefaultSettlement!.CreatedScopeId)
                {
                    throw new InvalidOperationException("Replay settlement scope allocation diverged.");
                }

                break;
            }

            case WorldInputKind.AllocateOperationId:
            {
                var actual = AllocateOperationIdCore();
                if (actual != entry.AllocateOperationId!.AllocatedOperationId)
                {
                    throw new InvalidOperationException("Replay operation ID allocation diverged.");
                }

                break;
            }

            case WorldInputKind.AdvanceTo:
                AdvanceToCore(entry.AdvanceTo!.TargetTime);
                break;

            case WorldInputKind.SettlementCommand:
                _ = ExecuteSettlementCommandCore(
                    entry.SettlementCommand!.ScopeId,
                    ToSettlementCommand(entry.SettlementCommand));
                break;

            case WorldInputKind.ResidentMigration:
                _ = ResolveMigrationCore(entry.ResidentMigration!);
                break;

            case WorldInputKind.EnqueueResidentMigration:
            {
                var messageId = EnqueueResidentMigrationCore(
                    entry.EnqueueResidentMigration!,
                    entry.Sequence);
                if (messageId.SourceInputSequence != entry.Sequence || messageId.Ordinal != 0)
                {
                    throw new InvalidOperationException("Replay transport message allocation diverged.");
                }

                break;
            }

            case WorldInputKind.DispatchOutbox:
            {
                var expected = entry.DispatchOutbox!;
                var actual = DispatchOutboxCore(expected.MaxMessages);
                if (actual != expected.ExpectedProcessedCount)
                {
                    throw new InvalidOperationException("Replay outbox dispatch count diverged.");
                }

                break;
            }

            case WorldInputKind.DeliverInbox:
            {
                var expected = entry.DeliverInbox!;
                var actual = DeliverInboxCore(expected.MaxMessages);
                if (actual.CompletedCount != expected.ExpectedProcessedCount
                    || !string.Equals(actual.BlockedCode, expected.ExpectedBlockedCode, StringComparison.Ordinal))
                {
                    throw new InvalidOperationException("Replay inbox delivery result diverged.");
                }

                break;
            }

            default:
                throw new ArgumentOutOfRangeException(nameof(entry), entry.Kind, "Unknown world input kind.");
        }

        RecordInput(entry);
    }

    private void EnsureInputJournalCapacity(int count)
    {
        if (count < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, "Journal reservation cannot be negative.");
        }

        if ((long)count > long.MaxValue - _nextInputSequence)
        {
            throw new InvalidOperationException("World input journal sequence space is exhausted.");
        }
    }

    private void RecordInput(WorldInputJournalEntry entry)
    {
        if (entry.Sequence != _nextInputSequence)
        {
            throw new InvalidOperationException("World input journal entry has an unexpected sequence.");
        }

        _inputJournal.Enqueue(entry);
        _nextInputSequence = checked(_nextInputSequence + 1);

        while (_inputJournal.Count > MaxRetainedInputJournalEntries)
        {
            var oldest = _inputJournal.Dequeue();
            _inputJournalFloor = checked(oldest.Sequence + 1);
        }
    }

    private WorldInputJournalEntry CreateInput(
        SimulationTime recordedAt,
        WorldInputKind kind,
        WorldAddDefaultSettlementInput? addDefaultSettlement = null,
        WorldAllocateOperationIdInput? allocateOperationId = null,
        WorldAdvanceToInput? advanceTo = null,
        WorldSettlementCommandInput? settlementCommand = null,
        ResidentMigrationIntent? residentMigration = null,
        WorldQueuedResidentMigration? enqueueResidentMigration = null,
        WorldTransportBatchInput? dispatchOutbox = null,
        WorldTransportBatchInput? deliverInbox = null) =>
        new(
            _nextInputSequence,
            recordedAt,
            kind,
            addDefaultSettlement,
            allocateOperationId,
            advanceTo,
            settlementCommand,
            residentMigration,
            enqueueResidentMigration,
            dispatchOutbox,
            deliverInbox);
}
