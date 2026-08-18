using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class SettlementSimulation
{
    internal SettlementActorLocationState BeginSelectedTaskTravelForActor(
        SettlementActorLocationState location,
        SettlementSelectedTaskState? selectedTask,
        IEnumerable<long>? knownRouteConnectionIds,
        SettlementOnFootActorCapabilityClass actorCapability,
        SettlementOnFootCarriedLoadClass carriedLoad,
        SimulationTime currentTime)
    {
        var normalized = SettlementSemanticLocation.Normalize(location);
        if (selectedTask is null || normalized.Kind != SettlementActorLocationKind.AtPlace)
        {
            return normalized;
        }

        ValidateExternalSelectedTask(selectedTask, currentTime);
        var destination = selectedTask.RequiredPlace;
        if (destination is null || IsAtPlace(normalized, destination))
        {
            return normalized;
        }

        var known = BuildValidatedKnownRouteSet(knownRouteConnectionIds);
        if (known.Count == 0)
        {
            return normalized;
        }

        var path = FindUniqueKnownOpenRoutePath(normalized.CurrentPlace, destination, known);
        if (path is null)
        {
            return normalized;
        }

        var routeTiming = AggregateOnFootRouteTiming(path.ConnectionIds);
        const SettlementOnFootTraversalDelayClass traversalDelay =
            SettlementOnFootTraversalDelayClass.NoMaterialDelay;
        var traversalHorizon = SettlementOnFootTraversalHorizonRules.ClassifyReferenceHorizon(
            path.TotalDistanceMeters);
        var applicability = SettlementOnFootTraversalApplicabilityRules.Evaluate(
            actorCapability,
            carriedLoad,
            routeTiming,
            traversalDelay,
            traversalHorizon);
        if (applicability != SettlementOnFootTraversalApplicabilityDecision.Applicable)
        {
            return normalized;
        }

        var durationMilliseconds = SettlementOnFootTravelDurationRules
            .CalculateBaselineDurationMilliseconds(path.TotalDistanceMeters);
        var plan = new SettlementTravelPlanState(
            selectedTask.TaskId,
            currentTime,
            path.ConnectionIds.ToArray(),
            SettlementTravelMode.OnFoot);
        return SettlementSemanticLocation.BeginTravel(
            normalized,
            destination,
            durationMilliseconds,
            plan);
    }

    internal SettlementActorLocationState AdvanceTravelForActor(
        SettlementActorLocationState location,
        long elapsedMilliseconds)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(elapsedMilliseconds);
        var normalized = SettlementSemanticLocation.Normalize(location);
        var travel = normalized.Travel;
        if (elapsedMilliseconds == 0
            || normalized.Kind != SettlementActorLocationKind.Travelling
            || travel?.Plan is null
            || !IsActiveTravelPlanTraversable(travel))
        {
            return normalized;
        }

        return SettlementSemanticLocation.AdvanceTravel(normalized, elapsedMilliseconds);
    }

    internal void ValidateExternalActorTravelState(
        SettlementActorLocationState location,
        SettlementSelectedTaskState? selectedTask,
        IEnumerable<long>? knownRouteConnectionIds,
        SimulationTime currentTime)
    {
        var normalized = SettlementSemanticLocation.Normalize(location);
        ValidateSettlementPlaceReference(normalized.CurrentPlace);
        ValidateSettlementPlaceReference(normalized.DestinationPlace);
        _ = BuildValidatedKnownRouteSet(knownRouteConnectionIds);

        if (selectedTask is not null)
        {
            ValidateExternalSelectedTask(selectedTask, currentTime);
        }

        var travel = normalized.Travel;
        if (travel?.Plan is null)
        {
            return;
        }

        var plan = travel.Plan;
        if (selectedTask is not null)
        {
            if (plan.TaskId != selectedTask.TaskId)
            {
                throw new InvalidOperationException("Travel plan task does not match the selected task.");
            }

            if (selectedTask.RequiredPlace is null
                || selectedTask.RequiredPlace != normalized.DestinationPlace)
            {
                throw new InvalidOperationException(
                    "Travel plan destination does not match its selected task.");
            }

            if (plan.DepartedAt.Milliseconds < selectedTask.SelectedAt.Milliseconds)
            {
                throw new InvalidOperationException(
                    "Travel plan departure predates its selected task.");
            }
        }
        // Player location encoding v1 predates persisted task provenance. Such snapshots
        // may already contain plan-bearing travel injected by older fixtures. Keep them
        // readable, while all newly produced player plans are sourced from SelectedTask.
        if (plan.DepartedAt.Milliseconds > currentTime.Milliseconds)
        {
            throw new InvalidOperationException("Travel plan departure time is in the future.");
        }

        var elapsedSinceDeparture = currentTime.Milliseconds - plan.DepartedAt.Milliseconds;
        if (travel.ElapsedMilliseconds > elapsedSinceDeparture)
        {
            throw new InvalidOperationException("Travel progress exceeds elapsed simulation time since departure.");
        }

        var currentPlace = normalized.CurrentPlace;
        foreach (var connectionId in plan.ConnectionIds)
        {
            if (!_routeConnectionsById.TryGetValue(connectionId, out var connection))
            {
                throw new InvalidOperationException("Travel plan references a missing route connection.");
            }

            if (connection.SupportedModes?.Contains(plan.TravelMode) != true)
            {
                throw new InvalidOperationException("Travel plan uses a mode unsupported by its route connection.");
            }

            if (connection.FirstPlace == currentPlace)
            {
                currentPlace = connection.SecondPlace;
                continue;
            }

            if (connection.SecondPlace == currentPlace)
            {
                currentPlace = connection.FirstPlace;
                continue;
            }

            throw new InvalidOperationException("Travel plan route connections do not form an ordered path.");
        }

        if (currentPlace != normalized.DestinationPlace)
        {
            throw new InvalidOperationException("Travel plan route does not end at its destination.");
        }
    }

    internal void ValidateExternalSelectedTask(
        SettlementSelectedTaskState task,
        SimulationTime currentTime)
    {
        ArgumentNullException.ThrowIfNull(task);
        if (task.TaskId <= 0)
        {
            throw new InvalidOperationException("Selected task ID must be positive.");
        }

        if (string.IsNullOrWhiteSpace(task.Kind))
        {
            throw new InvalidOperationException("Selected task kind is required.");
        }

        if (string.IsNullOrWhiteSpace(task.ReasonReference))
        {
            throw new InvalidOperationException("Selected task reason provenance is required.");
        }

        if (task.SelectedAt.Milliseconds < 0
            || task.SelectedAt.Milliseconds > currentTime.Milliseconds)
        {
            throw new InvalidOperationException("Selected task time must be within world simulation history.");
        }

        if (task.RequiredPlace is null)
        {
            return;
        }

        _ = SettlementSemanticLocation.Normalize(SettlementActorLocationState.At(task.RequiredPlace));
        ValidateSettlementPlaceReference(task.RequiredPlace);
    }

    private HashSet<long> BuildValidatedKnownRouteSet(IEnumerable<long>? connectionIds)
    {
        var result = new HashSet<long>();
        foreach (var connectionId in connectionIds ?? [])
        {
            if (connectionId <= 0
                || !result.Add(connectionId)
                || !_routeConnectionsById.ContainsKey(connectionId))
            {
                throw new InvalidOperationException(
                    "Actor route knowledge must reference unique existing connections.");
            }
        }

        return result;
    }
}
