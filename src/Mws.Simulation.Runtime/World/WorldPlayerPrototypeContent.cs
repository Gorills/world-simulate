using Mws.Simulation.Api;

namespace Mws.Simulation.Runtime;

internal static class WorldPlayerPrototypeContent
{
    internal static WorldPlayerInventoryItemState[] CreateStartingInventory() =>
    [
        new WorldPlayerInventoryItemState(SettlementItems.Ration, 2),
    ];
}
