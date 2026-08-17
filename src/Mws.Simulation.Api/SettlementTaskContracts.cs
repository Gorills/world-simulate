using Mws.Domain;

namespace Mws.Simulation.Api;

public sealed record SettlementSelectedTaskState(
    long TaskId,
    string Kind,
    string ReasonReference,
    SimulationTime SelectedAt,
    SettlementPlaceRef? RequiredPlace);

public sealed record SettlementSelectedTaskProjection(
    long TaskId,
    string Kind,
    string ReasonReference,
    SimulationTime SelectedAt,
    SettlementPlaceRef? RequiredPlace);

public sealed record SettlementDestinationRequestProjection(
    long TaskId,
    SettlementPlaceRef Destination);
