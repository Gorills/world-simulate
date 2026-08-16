using Mws.Domain;

namespace Mws.Simulation.Api;

public enum ResidentInteractionChoice
{
    AskAboutWork,
    Encourage,
    ShareRation,
}

public abstract record SettlementCommand(CommandId Id);

public sealed record FeedResidentCommand(
    CommandId Id,
    EntityId ResidentId) : SettlementCommand(Id);

public sealed record GiveItemToResidentCommand(
    CommandId Id,
    EntityId ResidentId,
    string ItemId,
    int Quantity) : SettlementCommand(Id);

public sealed record InteractWithResidentCommand(
    CommandId Id,
    EntityId ResidentId,
    ResidentInteractionChoice Choice) : SettlementCommand(Id);

public sealed record SettlementCommandResult(
    bool Success,
    string Code,
    EntityId? SubjectId,
    IReadOnlyList<SettlementFact> Facts);
