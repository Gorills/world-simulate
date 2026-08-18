namespace Mws.Simulation.Api;

public enum SettlementOnFootActorCapabilityClass
{
    Unknown = 0,
    BaselineCompatible = 1,
    NonBaseline = 2,
}

public enum SettlementOnFootCarriedLoadClass
{
    Unknown = 0,
    NoMaterialLoad = 1,
    MaterialLoadPresent = 2,
}

public enum SettlementOnFootTraversalDelayClass
{
    Unknown = 0,
    NoMaterialDelay = 1,
    MaterialDelayPresent = 2,
}

public enum SettlementOnFootTraversalHorizonClass
{
    Unknown = 0,
    BaselineShortReferenceCompatible = 1,
    ProlongedOrEnduranceRelevant = 2,
}

public enum SettlementOnFootTraversalApplicabilityDecision
{
    Unresolved = 0,
    Applicable = 1,
    NotApplicable = 2,
}

public sealed record SettlementOnFootTraversalApplicabilityProjection(
    long TaskId,
    SettlementOnFootActorCapabilityClass ActorCapability,
    SettlementOnFootCarriedLoadClass CarriedLoad,
    SettlementOnFootRouteTimingClass RouteTiming,
    SettlementOnFootTraversalDelayClass TraversalDelay,
    SettlementOnFootTraversalHorizonClass TraversalHorizon,
    SettlementOnFootTraversalApplicabilityDecision Decision);
