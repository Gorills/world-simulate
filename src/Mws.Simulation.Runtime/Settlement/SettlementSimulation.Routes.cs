using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class SettlementSimulation
{
    private readonly List<SettlementRouteConnectionState> _routeConnections;
    private readonly List<SettlementResidentRouteKnowledgeState> _residentRouteKnowledge;
    private readonly Dictionary<long, SettlementRouteConnectionState> _routeConnectionsById;
    private readonly Dictionary<SettlementPlaceRef, List<SettlementRouteConnectionState>> _routeConnectionsByPlace;
    private readonly Dictionary<EntityId, HashSet<long>> _knownRouteConnectionIdsByResident;
    private void ValidateRouteConnections()
    {
        var connectionIds = new HashSet<long>();
        foreach (var connection in _routeConnections)
        {
            if (connection.ConnectionId <= 0 || !connectionIds.Add(connection.ConnectionId))
            {
                throw new InvalidOperationException("Route connection IDs must be positive and unique.");
            }

            if (connection.DistanceMeters <= 0)
            {
                throw new InvalidOperationException("Route connection distance must be positive.");
            }

            if (connection.FirstPlace == connection.SecondPlace)
            {
                throw new InvalidOperationException("Route connection endpoints must be distinct.");
            }

            if (string.IsNullOrWhiteSpace(connection.ProvenanceReference))
            {
                throw new InvalidOperationException("Route connection provenance is required.");
            }

            if (connection.PhysicalState is not (
                SettlementRoutePhysicalState.Passable or SettlementRoutePhysicalState.Blocked))
            {
                throw new InvalidOperationException("Route connection has an unknown physical state.");
            }

            if (connection.PassageStatus is not (
                SettlementRoutePassageStatus.Open or SettlementRoutePassageStatus.Restricted))
            {
                throw new InvalidOperationException("Route connection has an unknown passage status.");
            }

            ValidateRouteModeSupport(connection);
            _ = SettlementSemanticLocation.Normalize(SettlementActorLocationState.At(connection.FirstPlace));
            _ = SettlementSemanticLocation.Normalize(SettlementActorLocationState.At(connection.SecondPlace));
            ValidateSettlementPlaceReference(connection.FirstPlace);
            ValidateSettlementPlaceReference(connection.SecondPlace);
        }
    }
    private void RebuildRouteIndexes()
    {
        _routeConnectionsById.Clear();
        _routeConnectionsByPlace.Clear();
        foreach (var connection in _routeConnections)
        {
            _routeConnectionsById.Add(connection.ConnectionId, connection);
            AddRouteConnectionToPlaceIndex(connection.FirstPlace, connection);
            AddRouteConnectionToPlaceIndex(connection.SecondPlace, connection);
        }
    }
    private void AddRouteConnectionToPlaceIndex(
        SettlementPlaceRef place,
        SettlementRouteConnectionState connection)
    {
        if (!_routeConnectionsByPlace.TryGetValue(place, out var connections))
        {
            connections = [];
            _routeConnectionsByPlace.Add(place, connections);
        }

        connections.Add(connection);
    }
    private void RebuildResidentRouteKnowledgeIndex()
    {
        _knownRouteConnectionIdsByResident.Clear();
        foreach (var knowledge in _residentRouteKnowledge)
        {
            if (FindResidentIndex(knowledge.ResidentId) < 0)
            {
                throw new InvalidOperationException("Route knowledge references a missing resident.");
            }

            if (_knownRouteConnectionIdsByResident.ContainsKey(knowledge.ResidentId))
            {
                throw new InvalidOperationException("Resident route knowledge must have one entry per resident.");
            }

            var knownIds = new HashSet<long>();
            foreach (var connectionId in knowledge.KnownConnectionIds)
            {
                if (!knownIds.Add(connectionId)
                    || !_routeConnectionsById.ContainsKey(connectionId))
                {
                    throw new InvalidOperationException(
                        "Resident route knowledge must reference unique existing connections.");
                }
            }

            _knownRouteConnectionIdsByResident.Add(knowledge.ResidentId, knownIds);
        }
    }
    private SettlementRouteConnectionState[] CaptureRouteConnections() =>
        _routeConnections
            .OrderBy(connection => connection.ConnectionId)
            .ToArray();
    private SettlementResidentRouteKnowledgeState[] CaptureResidentRouteKnowledge() =>
        _residentRouteKnowledge
            .OrderBy(entry => entry.ResidentId.Value)
            .Select(entry => new SettlementResidentRouteKnowledgeState(
                entry.ResidentId,
                entry.KnownConnectionIds.OrderBy(connectionId => connectionId).ToArray()))
            .ToArray();

    private SettlementRoutePathProjection? ProjectRoutePath(
        ResidentRuntimeState resident,
        SettlementDestinationRequestProjection? request)
    {
        if (request is null
            || resident.Location.Kind != SettlementActorLocationKind.AtPlace
            || !_knownRouteConnectionIdsByResident.TryGetValue(resident.Id, out var knownConnectionIds)
            || knownConnectionIds.Count == 0)
        {
            return null;
        }

        var origin = resident.Location.CurrentPlace;
        var path = FindUniqueKnownOpenRoutePath(origin, request.Destination, knownConnectionIds);
        return path is null
            ? null
            : new SettlementRoutePathProjection(
                request.TaskId,
                origin,
                request.Destination,
                path.ConnectionIds,
                path.TotalDistanceMeters,
                SettlementTravelMode.OnFoot);
    }

    private RoutePathResult? FindUniqueKnownOpenRoutePath(
        SettlementPlaceRef origin,
        SettlementPlaceRef destination,
        HashSet<long> knownConnectionIds)
    {
        var path = FindKnownOpenRoutePath(
            origin,
            destination,
            knownConnectionIds,
            excludedConnectionId: null);
        if (path is null)
        {
            return null;
        }

        foreach (var connectionId in path.ConnectionIds)
        {
            if (FindKnownOpenRoutePath(
                    origin,
                    destination,
                    knownConnectionIds,
                    connectionId) is not null)
            {
                // More than one feasible path exists. Route choice is intentionally
                // deferred until its causal selection rules are modeled.
                return null;
            }
        }

        return path;
    }

    private RoutePathResult? FindKnownOpenRoutePath(
        SettlementPlaceRef origin,
        SettlementPlaceRef destination,
        HashSet<long> knownConnectionIds,
        long? excludedConnectionId)
    {
        var predecessors = new Dictionary<SettlementPlaceRef, RoutePredecessor>();
        var visited = new HashSet<SettlementPlaceRef> { origin };
        var frontier = new Queue<SettlementPlaceRef>();
        frontier.Enqueue(origin);

        while (frontier.TryDequeue(out var place))
        {
            if (!_routeConnectionsByPlace.TryGetValue(place, out var connections))
            {
                continue;
            }

            foreach (var connection in connections)
            {
                if (!IsKnownOpenOnFootConnection(
                        connection,
                        knownConnectionIds,
                        excludedConnectionId))
                {
                    continue;
                }

                var nextPlace = connection.FirstPlace == place
                    ? connection.SecondPlace
                    : connection.FirstPlace;
                if (!visited.Add(nextPlace))
                {
                    continue;
                }

                predecessors[nextPlace] = new RoutePredecessor(place, connection.ConnectionId);
                if (nextPlace == destination)
                {
                    return ReconstructRoutePath(origin, destination, predecessors);
                }

                frontier.Enqueue(nextPlace);
            }
        }

        return null;
    }

    private RoutePathResult? ReconstructRoutePath(
        SettlementPlaceRef origin,
        SettlementPlaceRef destination,
        IReadOnlyDictionary<SettlementPlaceRef, RoutePredecessor> predecessors)
    {
        var connectionIds = new List<long>();
        var totalDistanceMeters = 0L;
        var current = destination;
        while (current != origin)
        {
            if (!predecessors.TryGetValue(current, out var predecessor)
                || !_routeConnectionsById.TryGetValue(predecessor.ConnectionId, out var connection)
                || totalDistanceMeters > long.MaxValue - connection.DistanceMeters)
            {
                return null;
            }

            connectionIds.Add(predecessor.ConnectionId);
            totalDistanceMeters += connection.DistanceMeters;
            current = predecessor.PreviousPlace;
        }

        connectionIds.Reverse();
        return new RoutePathResult(connectionIds.ToArray(), totalDistanceMeters);
    }

    private readonly record struct RoutePredecessor(
        SettlementPlaceRef PreviousPlace,
        long ConnectionId);

    private sealed record RoutePathResult(
        IReadOnlyList<long> ConnectionIds,
        long TotalDistanceMeters);
}
