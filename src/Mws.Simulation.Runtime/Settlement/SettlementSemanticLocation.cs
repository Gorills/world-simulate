using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

internal static class SettlementSemanticLocation
{
    private const long LegacyPrototypeTravelDurationMilliseconds = 3_600_000;

    internal static SettlementActorLocationState Normalize(SettlementActorLocationState? location) =>
        Normalize(location, allowLegacyMissingTravelProgress: false);

    internal static SettlementActorLocationState NormalizeForRestore(
        SettlementActorLocationState? location,
        bool allowLegacyMissingTravelProgress) =>
        Normalize(location, allowLegacyMissingTravelProgress);

    private static SettlementActorLocationState Normalize(
        SettlementActorLocationState? location,
        bool allowLegacyMissingTravelProgress)
    {
        var normalized = location ?? SettlementActorLocationState.At(SettlementPlaceRef.Settlement);
        if (normalized.Kind == SettlementActorLocationKind.Travelling && normalized.Travel is null)
        {
            if (!allowLegacyMissingTravelProgress)
            {
                throw new InvalidOperationException("Travelling location is missing travel progress.");
            }

            normalized = normalized with
            {
                Travel = new SettlementTravelProgressState(
                    LegacyPrototypeTravelDurationMilliseconds,
                    ElapsedMilliseconds: 0),
            };
        }

        Validate(normalized);
        return normalized;
    }

    internal static SettlementActorLocationState? Capture(SettlementActorLocationState location)
    {
        location = Normalize(location);
        return location.Kind == SettlementActorLocationKind.AtPlace
            && location.CurrentPlace == SettlementPlaceRef.Settlement
            ? null
            : location;
    }

    internal static SettlementActorLocationProjection Project(SettlementActorLocationState location)
    {
        location = Normalize(location);
        return new SettlementActorLocationProjection(
            location.Kind,
            location.CurrentPlace,
            location.DestinationPlace,
            location.Travel);
    }

    internal static SettlementActorLocationState BeginTravel(
        SettlementActorLocationState location,
        SettlementPlaceRef destination,
        long durationMilliseconds,
        SettlementTravelPlanState? plan = null)
    {
        location = Normalize(location);
        ArgumentNullException.ThrowIfNull(destination);
        ValidatePlace(destination);
        if (durationMilliseconds <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(durationMilliseconds),
                durationMilliseconds,
                "Travel duration must be positive.");
        }

        if (location.Kind != SettlementActorLocationKind.AtPlace)
        {
            throw new InvalidOperationException("A new travel plan requires an actor at a semantic place.");
        }

        if (location.CurrentPlace == destination)
        {
            return location;
        }

        var travelling = new SettlementActorLocationState(
            SettlementActorLocationKind.Travelling,
            location.CurrentPlace,
            destination,
            new SettlementTravelProgressState(
                durationMilliseconds,
                ElapsedMilliseconds: 0,
                plan));
        Validate(travelling);
        return travelling;
    }

    internal static SettlementActorLocationState AdvanceTravel(
        SettlementActorLocationState location,
        long elapsedMilliseconds)
    {
        location = Normalize(location);
        if (elapsedMilliseconds < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(elapsedMilliseconds),
                elapsedMilliseconds,
                "Elapsed travel time cannot be negative.");
        }

        if (elapsedMilliseconds == 0 || location.Kind == SettlementActorLocationKind.AtPlace)
        {
            return location;
        }

        var travel = location.Travel
            ?? throw new InvalidOperationException("Travelling location is missing travel progress.");
        var remaining = checked(travel.DurationMilliseconds - travel.ElapsedMilliseconds);
        if (elapsedMilliseconds >= remaining)
        {
            return SettlementActorLocationState.At(location.DestinationPlace);
        }

        return location with
        {
            Travel = travel with
            {
                ElapsedMilliseconds = checked(travel.ElapsedMilliseconds + elapsedMilliseconds),
            },
        };
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

            if (location.Travel is not null)
            {
                throw new InvalidOperationException("At-place location cannot carry active travel progress.");
            }

            return;
        }

        if (location.Kind == SettlementActorLocationKind.Travelling)
        {
            if (location.CurrentPlace == location.DestinationPlace)
            {
                throw new InvalidOperationException("Travelling location requires distinct current and destination places.");
            }

            ValidateTravel(location.Travel);
            return;
        }

        throw new InvalidOperationException("Unknown settlement actor location kind.");
    }

    private static void ValidateTravel(SettlementTravelProgressState? travel)
    {
        ArgumentNullException.ThrowIfNull(travel);
        if (travel.DurationMilliseconds <= 0)
        {
            throw new InvalidOperationException("Travel duration must be positive.");
        }

        if (travel.ElapsedMilliseconds < 0
            || travel.ElapsedMilliseconds >= travel.DurationMilliseconds)
        {
            throw new InvalidOperationException("Active travel progress must be within its duration.");
        }

        if (travel.Plan is not null)
        {
            ValidateTravelPlan(travel.Plan);
        }
    }

    private static void ValidateTravelPlan(SettlementTravelPlanState plan)
    {
        if (plan.TaskId <= 0)
        {
            throw new InvalidOperationException("Travel plan task ID must be positive.");
        }

        if (plan.DepartedAt.Milliseconds < 0)
        {
            throw new InvalidOperationException("Travel plan departure time cannot be negative.");
        }

        ArgumentNullException.ThrowIfNull(plan.ConnectionIds);
        if (plan.ConnectionIds.Count == 0)
        {
            throw new InvalidOperationException("Travel plan requires at least one route connection.");
        }

        var connectionIds = new HashSet<long>();
        foreach (var connectionId in plan.ConnectionIds)
        {
            if (connectionId <= 0 || !connectionIds.Add(connectionId))
            {
                throw new InvalidOperationException(
                    "Travel plan route connection IDs must be positive and unique.");
            }
        }

        if (plan.TravelMode is not (
            SettlementTravelMode.OnFoot
            or SettlementTravelMode.MountedOrAnimalAssisted
            or SettlementTravelMode.CartWagonOrPack
            or SettlementTravelMode.Water))
        {
            throw new InvalidOperationException("Travel plan has an unknown travel mode.");
        }
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
