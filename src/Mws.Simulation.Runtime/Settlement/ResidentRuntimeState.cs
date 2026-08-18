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
        SettlementOnFootActorCapabilityAuthority.Validate(
            state.OnFootCapability,
            state.OnFootCapabilityProvenanceReference,
            state.IsOnFootCapabilityFixture);
        SettlementOnFootCarriedLoadAuthority.Validate(
            state.OnFootCarriedLoad,
            state.OnFootCarriedLoadProvenanceReference,
            state.IsOnFootCarriedLoadFixture);
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
        OnFootCapability = state.OnFootCapability;
        OnFootCapabilityProvenanceReference = state.OnFootCapabilityProvenanceReference;
        IsOnFootCapabilityFixture = state.IsOnFootCapabilityFixture;
        OnFootCarriedLoad = state.OnFootCarriedLoad;
        OnFootCarriedLoadProvenanceReference = state.OnFootCarriedLoadProvenanceReference;
        IsOnFootCarriedLoadFixture = state.IsOnFootCarriedLoadFixture;
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

    internal SettlementOnFootActorCapabilityClass OnFootCapability { get; }

    internal string? OnFootCapabilityProvenanceReference { get; }

    internal bool IsOnFootCapabilityFixture { get; }

    internal SettlementOnFootCarriedLoadClass OnFootCarriedLoad { get; }

    internal string? OnFootCarriedLoadProvenanceReference { get; }

    internal bool IsOnFootCarriedLoadFixture { get; }

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
        SelectedTask,
        OnFootCapability,
        OnFootCapabilityProvenanceReference,
        IsOnFootCapabilityFixture,
        OnFootCarriedLoad,
        OnFootCarriedLoadProvenanceReference,
        IsOnFootCarriedLoadFixture);
}
