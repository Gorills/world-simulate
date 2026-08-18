using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

internal static class SettlementOnFootActorCapabilityAuthority
{
    internal static void Validate(
        SettlementOnFootActorCapabilityClass capability,
        string? provenanceReference,
        bool isFixture)
    {
        if (capability is not (
            SettlementOnFootActorCapabilityClass.Unknown
            or SettlementOnFootActorCapabilityClass.BaselineCompatible
            or SettlementOnFootActorCapabilityClass.NonBaseline))
        {
            throw new InvalidOperationException(
                "On-foot actor capability has an unknown classification.");
        }

        if (capability == SettlementOnFootActorCapabilityClass.Unknown)
        {
            if (provenanceReference is not null || isFixture)
            {
                throw new InvalidOperationException(
                    "Unknown on-foot actor capability must not carry provenance or fixture authority.");
            }

            return;
        }

        if (string.IsNullOrWhiteSpace(provenanceReference))
        {
            throw new InvalidOperationException(
                "Explicit on-foot actor capability requires provenance.");
        }
    }
}
