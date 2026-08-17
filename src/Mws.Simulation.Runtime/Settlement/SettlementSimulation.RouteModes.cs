using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

public sealed partial class SettlementSimulation
{
    private readonly bool _allowLegacyMissingRouteModeSupport;

    private void ValidateRouteModeSupport(SettlementRouteConnectionState connection)
    {
        var supportedModes = connection.SupportedModes;
        if (supportedModes is null)
        {
            if (_allowLegacyMissingRouteModeSupport)
            {
                return;
            }

            throw new InvalidOperationException(
                "Current route mode encoding requires explicit supported travel modes.");
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

    private int CaptureRouteModeEncodingVersion() =>
        _routeConnections.All(connection => connection.SupportedModes is not null)
            ? SettlementVersions.CurrentRouteModeEncodingVersion
            : SettlementVersions.LegacyRouteModeEncodingVersion;

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
