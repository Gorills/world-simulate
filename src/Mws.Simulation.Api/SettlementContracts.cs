using Mws.Domain;

namespace Mws.Simulation.Api;

public static class SettlementVersions
{
    public const int CurrentSchemaVersion = 1;
}

public enum ResidentActivity
{
    Idle,
    Working,
    Eating,
    Resting,
}

public sealed record ResidentState(
    EntityId Id,
    string Name,
    int Hunger,
    int Energy,
    ResidentActivity Activity);

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
    int PantryRations,
    IReadOnlyList<ResidentState> Residents,
    IReadOnlyList<SettlementEvent> Events);

public sealed record ResidentProjection(
    EntityId Id,
    string Name,
    int Hunger,
    int Energy,
    ResidentActivity Activity);

public sealed record SettlementProjection(
    SimulationTime Time,
    int Day,
    int Hour,
    int PantryRations,
    IReadOnlyList<ResidentProjection> Residents,
    IReadOnlyList<SettlementEvent> RecentEvents);

public sealed record SettlementCommandResult(
    bool Success,
    string Code,
    EntityId? SubjectId,
    string Message);
