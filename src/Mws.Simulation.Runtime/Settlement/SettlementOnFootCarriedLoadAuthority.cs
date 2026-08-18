using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

internal static class SettlementOnFootCarriedLoadAuthority
{
    internal static void Validate(
        SettlementOnFootCarriedLoadClass carriedLoad,
        string? provenanceReference,
        bool isFixture)
    {
        if (carriedLoad is not (
            SettlementOnFootCarriedLoadClass.Unknown
            or SettlementOnFootCarriedLoadClass.NoMaterialLoad
            or SettlementOnFootCarriedLoadClass.MaterialLoadPresent))
        {
            throw new InvalidOperationException(
                "On-foot carried load has an unknown classification.");
        }

        if (carriedLoad == SettlementOnFootCarriedLoadClass.Unknown)
        {
            if (provenanceReference is not null || isFixture)
            {
                throw new InvalidOperationException(
                    "Unknown on-foot carried load must not carry provenance or fixture authority.");
            }

            return;
        }

        if (string.IsNullOrWhiteSpace(provenanceReference))
        {
            throw new InvalidOperationException(
                "Explicit on-foot carried load requires provenance.");
        }
    }
}
