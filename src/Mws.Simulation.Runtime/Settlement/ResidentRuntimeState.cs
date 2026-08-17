using Mws.Domain;
using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

internal sealed class ResidentRuntimeState
{
    internal ResidentRuntimeState(ResidentState state)
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
        Location = SettlementSemanticLocation.Normalize(state.Location);
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

    internal SettlementActorLocationState Location { get; set; }

    internal ResidentState Capture() => new(
        Id,
        Name,
        Hunger,
        Energy,
        Activity,
        Profession,
        WorkplaceId,
        Affinity,
        HouseholdId,
        SettlementSemanticLocation.Capture(Location));
}
