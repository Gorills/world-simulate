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
        var residents = ProjectResidentRange(0, _residents.Length);

        return new SettlementProjection(
            _scopeId,
            Time,
            checked((int)(Time.Milliseconds / DayMilliseconds)),
            checked((int)((Time.Milliseconds / HourMilliseconds) % 24)),
            ItemQuantity(_settlementOwnerId, SettlementItems.Ration),
            stockpile,
            workplaces,
            residents,
            _events.TakeLast(8).ToArray(),
            ProjectHomes(),
            ProjectHouseholds());
    }

    public ResidentProjectionPage ProjectResidents(int offset, int limit)
    {
        if (offset < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(offset),
                offset,
                "Resident projection offset cannot be negative.");
        }

        if (limit is <= 0 or > 1_000)
        {
            throw new ArgumentOutOfRangeException(
                nameof(limit),
                limit,
                "Resident projection limit must be 1..1000.");
        }

        var count = Math.Min(limit, Math.Max(0, _residents.Length - offset));
        return new ResidentProjectionPage(
            _scopeId,
            Time,
            offset,
            _residents.Length,
            ProjectResidentRange(offset, count));
    }

    private ResidentProjection[] ProjectResidentRange(int offset, int count)
    {
        if (offset >= _residents.Length || count == 0)
        {
            return [];
        }

        var result = new ResidentProjection[count];
        for (var index = 0; index < count; index++)
        {
            var resident = _residents[offset + index];
            var household = FindHousehold(resident.HouseholdId);
            var home = household is null ? null : FindHome(household.HomeId);
            var selectedTask = ProjectSelectedTask(resident);
            var destinationRequest = ProjectDestinationRequest(resident);
            result[index] = new ResidentProjection(
                resident.Id,
                resident.Name,
                resident.Hunger,
                resident.Energy,
                resident.Activity,
                resident.Profession,
                FindWorkplace(resident.WorkplaceId)?.Name ?? "Unassigned",
                resident.Affinity,
                ProjectInventory(resident.Id),
                resident.WorkplaceId,
                resident.HouseholdId,
                household?.Name ?? string.Empty,
                home?.Id ?? default,
                home?.Name ?? string.Empty,
                SettlementSemanticLocation.Project(resident.Location),
                selectedTask,
                destinationRequest,
                ProjectRoutePath(resident, destinationRequest));
        }

        return result;
    }

    private WorkplaceState? FindWorkplace(EntityId workplaceId) =>
        _workplacesById.TryGetValue(workplaceId, out var workplace) ? workplace : null;

    private int FindResidentIndex(EntityId residentId) =>
        _residentIndicesById.TryGetValue(residentId, out var index) ? index : -1;
}
