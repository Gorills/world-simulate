using Mws.Domain;
using Mws.Simulation.Api;
using Mws.Simulation.Runtime;

namespace Mws.Client.Godot.Session;

internal sealed class GameSession
{
    private readonly SettlementSimulation _simulation;

    public GameSession(WorldSeed seed)
    {
        _simulation = SettlementSimulation.CreateDefault(seed);
        SelectedResidentId = _simulation.Project().Residents[0].Id;
    }

    public event Action? Changed;

    public EntityId SelectedResidentId { get; private set; }

    public SettlementProjection Projection => _simulation.Project();

    public ResidentProjection SelectedResident =>
        Projection.Residents.Single(resident => resident.Id == SelectedResidentId);

    public void SelectResident(EntityId residentId)
    {
        if (!Projection.Residents.Any(resident => resident.Id == residentId))
        {
            return;
        }

        SelectedResidentId = residentId;
        Changed?.Invoke();
    }

    public void SelectRelative(int offset)
    {
        var residents = Projection.Residents;
        var currentIndex = residents
            .Select((resident, index) => (resident, index))
            .First(entry => entry.resident.Id == SelectedResidentId)
            .index;
        var nextIndex = (currentIndex + offset) % residents.Count;
        if (nextIndex < 0)
        {
            nextIndex += residents.Count;
        }

        SelectedResidentId = residents[nextIndex].Id;
        Changed?.Invoke();
    }

    public SettlementCommandResult InteractSelected(ResidentInteractionChoice choice)
    {
        var result = _simulation.InteractWithResident(SelectedResidentId, choice);
        Changed?.Invoke();
        return result;
    }

    public void AdvanceHours(int hours)
    {
        _simulation.AdvanceHours(hours);
        Changed?.Invoke();
    }
}
