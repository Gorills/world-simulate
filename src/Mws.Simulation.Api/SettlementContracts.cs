using Mws.Domain;

namespace Mws.Simulation.Api;

public static class SettlementVersions
{
    public const int CurrentSchemaVersion = 2;
}

public static class SettlementItems
{
    public const string Ration = "ration";
    public const string Grain = "grain";
    public const string Herb = "herb";
}

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

public enum ResidentInteractionChoice
{
    AskAboutWork,
    Encourage,
    ShareRation,
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

public sealed record SettlementEvent(
    long Id,
    SimulationTime Time,
    string Kind,
    EntityId? SubjectId,
    string Summary);

public sealed record SettlementState(
    int SchemaVersion,
    ulong WorldSeed,
    SimulationTime Time,
    long NextEventId,
    long NextStackId,
    EntityId SettlementOwnerId,
    IReadOnlyList<ResidentState> Residents,
    IReadOnlyList<ItemStackState> ItemStacks,
    IReadOnlyList<WorkplaceState> Workplaces,
    IReadOnlyList<SettlementEvent> Events);

public sealed record ItemStackProjection(
    long StackId,
    string ItemId,
    int Quantity);

public sealed record WorkplaceProjection(
    EntityId Id,
    string Name,
    ResidentProfession Profession,
    string? InputItemId,
    int InputQuantity,
    string OutputItemId,
    int OutputQuantity);

public sealed record ResidentProjection(
    EntityId Id,
    string Name,
    int Hunger,
    int Energy,
    ResidentActivity Activity,
    ResidentProfession Profession,
    string WorkplaceName,
    int Affinity,
    IReadOnlyList<ItemStackProjection> Inventory);

public sealed record SettlementProjection(
    SimulationTime Time,
    int Day,
    int Hour,
    int PantryRations,
    IReadOnlyList<ItemStackProjection> Stockpile,
    IReadOnlyList<WorkplaceProjection> Workplaces,
    IReadOnlyList<ResidentProjection> Residents,
    IReadOnlyList<SettlementEvent> RecentEvents);

public sealed record SettlementCommandResult(
    bool Success,
    string Code,
    EntityId? SubjectId,
    string Message);
