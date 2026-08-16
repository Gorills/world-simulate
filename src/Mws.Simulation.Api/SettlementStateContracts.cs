using Mws.Domain;

namespace Mws.Simulation.Api;

public enum ResidentActivity
{
    Idle,
    Working,
    Eating,
    Resting,
}

public enum ResidentProfession
{
    Farmer,
    Cook,
    Forager,
}

public sealed record ResidentState(
    EntityId Id,
    string Name,
    int Hunger,
    int Energy,
    ResidentActivity Activity,
    ResidentProfession Profession,
    EntityId WorkplaceId,
    int Affinity);

public sealed record ItemStackState(
    long StackId,
    string ItemId,
    EntityId OwnerId,
    int Quantity);

public sealed record WorkplaceState(
    EntityId Id,
    string Name,
    ResidentProfession Profession,
    string? InputItemId,
    int InputQuantity,
    string OutputItemId,
    int OutputQuantity);

public sealed record SettlementFact(string Key, string Value);

public sealed record SettlementEvent(
    long Id,
    SimulationTime Time,
    string Kind,
    EntityId? SubjectId,
    IReadOnlyList<SettlementFact> Facts);

public sealed record SettlementCommandReceipt(
    CommandId CommandId,
    bool Success,
    string Code,
    EntityId? SubjectId,
    IReadOnlyList<SettlementFact> Facts);

public sealed record SettlementState(
    int SchemaVersion,
    ulong WorldSeed,
    SimulationTime Time,
    long NextEventId,
    long NextStackId,
    long NextCommandId,
    EntityId SettlementOwnerId,
    IReadOnlyList<ResidentState> Residents,
    IReadOnlyList<ItemStackState> ItemStacks,
    IReadOnlyList<WorkplaceState> Workplaces,
    IReadOnlyList<SettlementEvent> Events,
    IReadOnlyList<SettlementCommandReceipt> CommandReceipts);
