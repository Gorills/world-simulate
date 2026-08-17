using Mws.Simulation.Api;

namespace Mws.Client.Godot.UI.Screens.Hud;

public partial class GameHud
{
    internal SettlementProjection? CaptureDebugProjection() => _session?.Projection;
}
