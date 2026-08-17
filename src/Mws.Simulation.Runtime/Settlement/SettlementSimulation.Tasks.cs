using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class SettlementSimulation
{
    private void ValidateResidentSelectedTask(SettlementSelectedTaskState task)
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

        if (task.SelectedAt.Milliseconds < 0 || task.SelectedAt.Milliseconds > Time.Milliseconds)
        {
            throw new InvalidOperationException("Selected task time must be within settlement simulation history.");
        }

        var requiredPlace = task.RequiredPlace;
        if (requiredPlace is null)
        {
            return;
        }

        _ = SettlementSemanticLocation.Normalize(SettlementActorLocationState.At(requiredPlace));
        ValidateSettlementPlaceReference(requiredPlace);
    }

    private static SettlementSelectedTaskProjection? ProjectSelectedTask(ResidentRuntimeState resident)
    {
        var task = resident.SelectedTask;
        return task is null
            ? null
            : new SettlementSelectedTaskProjection(
                task.TaskId,
                task.Kind,
                task.ReasonReference,
                task.SelectedAt,
                task.RequiredPlace);
    }

    private static SettlementDestinationRequestProjection? ProjectDestinationRequest(ResidentRuntimeState resident)
    {
        var task = resident.SelectedTask;
        var requiredPlace = task?.RequiredPlace;
        if (task is null
            || requiredPlace is null
            || IsAtPlace(resident.Location, requiredPlace))
        {
            return null;
        }

        return new SettlementDestinationRequestProjection(task.TaskId, requiredPlace);
    }
}
