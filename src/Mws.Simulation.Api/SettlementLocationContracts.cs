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

public sealed record SettlementActorLocationState(
    SettlementActorLocationKind Kind,
    SettlementPlaceRef CurrentPlace,
    SettlementPlaceRef DestinationPlace)
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
    SettlementPlaceRef DestinationPlace);
