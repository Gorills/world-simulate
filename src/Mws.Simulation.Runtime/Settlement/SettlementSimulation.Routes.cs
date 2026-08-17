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
        var path = FindKnownOpenRoutePath(origin, request.Destination, knownConnectionIds);
        return path is null
            ? null
            : new SettlementRoutePathProjection(
                request.TaskId,
                origin,
                request.Destination,
                path.ConnectionIds,
                path.TotalDistanceMeters);
    }

    private RoutePathResult? FindKnownOpenRoutePath(
        SettlementPlaceRef origin,
        SettlementPlaceRef destination,
        HashSet<long> knownConnectionIds)
    {
        var distances = new Dictionary<SettlementPlaceRef, long>
        {
            [origin] = 0,
        };
        var predecessors = new Dictionary<SettlementPlaceRef, RoutePredecessor>();
        var frontier = new PriorityQueue<
            SettlementPlaceRef,
            (long Distance, int Kind, long EntityId)>();
        frontier.Enqueue(origin, (0, (int)origin.Kind, origin.EntityId.Value));

        while (frontier.TryDequeue(out var place, out var priority))
        {
            if (!distances.TryGetValue(place, out var currentDistance)
                || priority.Distance != currentDistance)
            {
                continue;
            }

            if (place == destination)
            {
                return ReconstructRoutePath(origin, destination, currentDistance, predecessors);
            }

            if (!_routeConnectionsByPlace.TryGetValue(place, out var connections))
            {
                continue;
            }

            foreach (var connection in connections)
            {
                if (!knownConnectionIds.Contains(connection.ConnectionId)
                    || connection.PhysicalState != SettlementRoutePhysicalState.Passable
                    || connection.PassageStatus != SettlementRoutePassageStatus.Open)
                {
                    continue;
                }

                var nextPlace = connection.FirstPlace == place
                    ? connection.SecondPlace
                    : connection.FirstPlace;
                if (currentDistance > long.MaxValue - connection.DistanceMeters)
                {
                    continue;
                }

                var candidateDistance = currentDistance + connection.DistanceMeters;
                if (distances.TryGetValue(nextPlace, out var existingDistance)
                    && candidateDistance >= existingDistance)
                {
                    continue;
                }

                distances[nextPlace] = candidateDistance;
                predecessors[nextPlace] = new RoutePredecessor(place, connection.ConnectionId);
                frontier.Enqueue(
                    nextPlace,
                    (candidateDistance, (int)nextPlace.Kind, nextPlace.EntityId.Value));
            }
        }

        return null;
    }

    private static RoutePathResult ReconstructRoutePath(
        SettlementPlaceRef origin,
        SettlementPlaceRef destination,
        long totalDistanceMeters,
        IReadOnlyDictionary<SettlementPlaceRef, RoutePredecessor> predecessors)
    {
        var connectionIds = new List<long>();
        var current = destination;
        while (current != origin)
        {
            if (!predecessors.TryGetValue(current, out var predecessor))
            {
                throw new InvalidOperationException("Route path predecessor chain is incomplete.");
            }

            connectionIds.Add(predecessor.ConnectionId);
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
