using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class SettlementSimulation
{
    private static void ValidateRouteModeSupport(SettlementRouteConnectionState connection)
    {
        var supportedModes = connection.SupportedModes;
        if (supportedModes is null)
        {
            // Backward-compatible unresolved route content remains loadable but is
            // not traversable until a mode is declared explicitly.
            return;
        }

        if (supportedModes.Count == 0)
        {
            throw new InvalidOperationException(
                "Route connection must declare at least one supported travel mode.");
        }

        var uniqueModes = new HashSet<SettlementTravelMode>();
        foreach (var mode in supportedModes)
        {
            if (mode is not (
                SettlementTravelMode.OnFoot
                or SettlementTravelMode.MountedOrAnimalAssisted
                or SettlementTravelMode.CartWagonOrPack
                or SettlementTravelMode.Water))
            {
                throw new InvalidOperationException(
                    "Route connection has an unknown supported travel mode.");
            }

            if (!uniqueModes.Add(mode))
            {
                throw new InvalidOperationException(
                    "Route connection supported travel modes must be unique.");
            }
        }
    }

    private static bool IsKnownOpenOnFootConnection(
        SettlementRouteConnectionState connection,
        HashSet<long> knownConnectionIds,
        long? excludedConnectionId) =>
        connection.ConnectionId != excludedConnectionId
        && knownConnectionIds.Contains(connection.ConnectionId)
        && connection.PhysicalState == SettlementRoutePhysicalState.Passable
        && connection.PassageStatus == SettlementRoutePassageStatus.Open
        && connection.SupportedModes?.Contains(SettlementTravelMode.OnFoot) == true;
}
