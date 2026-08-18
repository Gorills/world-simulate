using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class SettlementSimulation
{
    private static void ValidateRouteTimingClass(SettlementRouteConnectionState connection)
    {
        if (connection.OnFootTimingClass is not (
            SettlementOnFootRouteTimingClass.Unknown
            or SettlementOnFootRouteTimingClass.BaselineLevelUnobstructed
            or SettlementOnFootRouteTimingClass.NonBaseline))
        {
            throw new InvalidOperationException(
                "Route connection has an unknown on-foot timing class.");
        }
    }
}
