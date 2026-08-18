using Mws.Domain;

namespace Mws.Simulation.Api;

public enum SettlementPlaceKind
{
    Settlement,
    Home,
    Workplace,
}

public sealed record SettlementPlaceRef(
    SettlementPlaceKind Kind,
    EntityId EntityId)
{
    public static SettlementPlaceRef Settlement { get; } =
        new(SettlementPlaceKind.Settlement, default);
}

public enum SettlementActorLocationKind
{
    AtPlace,
    Travelling,
}

public sealed record SettlementTravelPlanState(
    long TaskId,
    SimulationTime DepartedAt,
    IReadOnlyList<long> ConnectionIds,
    SettlementTravelMode TravelMode);

public sealed record SettlementTravelProgressState(
    long DurationMilliseconds,
    long ElapsedMilliseconds,
    SettlementTravelPlanState? Plan = null);

public sealed record SettlementActorLocationState(
    SettlementActorLocationKind Kind,
    SettlementPlaceRef CurrentPlace,
    SettlementPlaceRef DestinationPlace,
    SettlementTravelProgressState? Travel = null)
{
    public static SettlementActorLocationState At(SettlementPlaceRef place)
    {
        ArgumentNullException.ThrowIfNull(place);
        return new SettlementActorLocationState(
            SettlementActorLocationKind.AtPlace,
            place,
            place);
    }
}

public sealed record SettlementActorLocationProjection(
    SettlementActorLocationKind Kind,
    SettlementPlaceRef CurrentPlace,
    SettlementPlaceRef DestinationPlace,
    SettlementTravelProgressState? Travel = null);
