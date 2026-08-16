using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class SettlementSimulation
{
    public const int MaxRetainedEvents = 512;
    public const int MaxRetainedCommandReceipts = 4_096;

    private readonly Dictionary<long, SettlementCommandReceipt> _commandReceiptById = new();

    private void RebuildHistoryIndexes()
    {
        _commandReceiptById.Clear();
        foreach (var receipt in _commandReceipts)
        {
            _commandReceiptById.Add(receipt.CommandId.Value, receipt);
        }
    }

    private bool TryGetCommandReceipt(CommandId commandId, out SettlementCommandReceipt receipt) =>
        _commandReceiptById.TryGetValue(commandId.Value, out receipt!);

    private void RecordCommandReceipt(SettlementCommandReceipt receipt)
    {
        _commandReceipts.Add(receipt);
        _commandReceiptById.Add(receipt.CommandId.Value, receipt);

        var overflow = _commandReceipts.Count - MaxRetainedCommandReceipts;
        if (overflow <= 0)
        {
            return;
        }

        for (var index = 0; index < overflow; index++)
        {
            _commandReceiptById.Remove(_commandReceipts[index].CommandId.Value);
        }

        _commandReceipts.RemoveRange(0, overflow);
    }

    private void RetainRecentEvents()
    {
        var overflow = _events.Count - MaxRetainedEvents;
        if (overflow > 0)
        {
            _events.RemoveRange(0, overflow);
        }
    }
}
