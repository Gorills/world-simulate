using Mws.Domain;

namespace Mws.Simulation.Api;

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
