using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class SettlementSimulation
{
    private void ValidateResidentSelectedTask(SettlementSelectedTaskState task) =>
        ValidateExternalSelectedTask(task, Time);

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
