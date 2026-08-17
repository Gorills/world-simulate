using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

internal sealed class ResidentRuntimeState
{
    internal ResidentRuntimeState(
        ResidentState state,
        bool allowLegacyMissingTravelProgress)
    {
        ArgumentNullException.ThrowIfNull(state);
        Id = state.Id;
        Name = state.Name;
        Hunger = state.Hunger;
        Energy = state.Energy;
        Activity = state.Activity;
        Profession = state.Profession;
        WorkplaceId = state.WorkplaceId;
        HouseholdId = state.HouseholdId;
        Affinity = state.Affinity;
        LocationWasOmitted = state.Location is null;
        Location = SettlementSemanticLocation.NormalizeForRestore(
            state.Location,
            allowLegacyMissingTravelProgress);
        SelectedTask = state.SelectedTask;
    }

    internal EntityId Id { get; }

    internal string Name { get; }

    internal int Hunger { get; set; }

    internal int Energy { get; set; }

    internal ResidentActivity Activity { get; set; }

    internal ResidentProfession Profession { get; }

    internal EntityId WorkplaceId { get; }

    internal EntityId HouseholdId { get; }

    internal int Affinity { get; set; }

    internal bool LocationWasOmitted { get; }

    internal SettlementActorLocationState Location { get; set; }

    internal SettlementSelectedTaskState? SelectedTask { get; }

    internal ResidentState Capture() =>
        Capture(SettlementSemanticLocation.Capture(Location));

    internal ResidentState Capture(SettlementActorLocationState? location) => new(
        Id,
        Name,
        Hunger,
        Energy,
        Activity,
        Profession,
        WorkplaceId,
        Affinity,
        HouseholdId,
        location,
        SelectedTask);
}
