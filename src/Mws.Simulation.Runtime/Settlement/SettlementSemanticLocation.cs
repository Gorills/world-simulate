using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

internal static class SettlementSemanticLocation
{
    internal static SettlementActorLocationState Normalize(SettlementActorLocationState? location)
    {
        var normalized = location ?? SettlementActorLocationState.At(SettlementPlaceRef.Settlement);
        Validate(normalized);
        return normalized;
    }

    internal static SettlementActorLocationProjection Project(SettlementActorLocationState location)
    {
        ArgumentNullException.ThrowIfNull(location);
        Validate(location);
        return new SettlementActorLocationProjection(location.Kind, location.CurrentPlace, location.DestinationPlace);
    }

    private static void Validate(SettlementActorLocationState location)
    {
        ArgumentNullException.ThrowIfNull(location);
        ArgumentNullException.ThrowIfNull(location.CurrentPlace);
        ArgumentNullException.ThrowIfNull(location.DestinationPlace);
        ValidatePlace(location.CurrentPlace);
        ValidatePlace(location.DestinationPlace);

        if (location.Kind == SettlementActorLocationKind.AtPlace)
        {
            if (location.CurrentPlace != location.DestinationPlace)
            {
                throw new InvalidOperationException("At-place location requires matching current and destination places.");
            }

            return;
        }

        if (location.Kind == SettlementActorLocationKind.Travelling)
        {
            if (location.CurrentPlace == location.DestinationPlace)
            {
                throw new InvalidOperationException("Travelling location requires distinct current and destination places.");
            }

            return;
        }

        throw new InvalidOperationException("Unknown settlement actor location kind.");
    }

    private static void ValidatePlace(SettlementPlaceRef place)
    {
        if (place.Kind == SettlementPlaceKind.Settlement)
        {
            if (place.EntityId != default(EntityId))
            {
                throw new InvalidOperationException("Settlement place cannot carry an entity ID.");
            }

            return;
        }

        if (place.Kind is SettlementPlaceKind.Home or SettlementPlaceKind.Workplace)
        {
            if (place.EntityId.Value <= 0)
            {
                throw new InvalidOperationException("Home and workplace places require a positive entity ID.");
            }

            return;
        }

        throw new InvalidOperationException("Unknown settlement place kind.");
    }
}
