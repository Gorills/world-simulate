using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class SettlementSimulation
{
    public SettlementProjection Project()
    {
        var stockpile = ProjectInventory(_settlementOwnerId);
        var workplaces = _workplaces
            .OrderBy(workplace => workplace.Id.Value)
            .Select(workplace => new WorkplaceProjection(
                workplace.Id,
                workplace.Name,
                workplace.Profession,
                workplace.InputItemId,
                workplace.InputQuantity,
                workplace.OutputItemId,
                workplace.OutputQuantity))
            .ToArray();

        var residents = _residents
            .OrderBy(resident => resident.Id.Value)
            .Select(resident => new ResidentProjection(
                resident.Id,
                resident.Name,
                resident.Hunger,
                resident.Energy,
                resident.Activity,
                resident.Profession,
                FindWorkplace(resident.WorkplaceId)?.Name ?? "Unassigned",
                resident.Affinity,
                ProjectInventory(resident.Id)))
            .ToArray();

        return new SettlementProjection(
            _scopeId,
            Time,
            checked((int)(Time.Milliseconds / DayMilliseconds)),
            checked((int)((Time.Milliseconds / HourMilliseconds) % 24)),
            ItemQuantity(_settlementOwnerId, SettlementItems.Ration),
            stockpile,
            workplaces,
            residents,
            _events.TakeLast(8).ToArray());
    }

    private WorkplaceState? FindWorkplace(EntityId workplaceId) =>
        _workplaces.FirstOrDefault(workplace => workplace.Id == workplaceId);

    private int FindResidentIndex(EntityId residentId) =>
        _residents.FindIndex(resident => resident.Id == residentId);
}
