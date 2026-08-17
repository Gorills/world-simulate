using Godot;

namespace Mws.Client.Godot.Input;

internal static class DebugInput
{
    internal static readonly StringName ToggleVillageObserver = "debug_village_observer";

    internal static void ConfigureDefaults()
    {
        if (!InputMap.HasAction(ToggleVillageObserver))
        {
            InputMap.AddAction(ToggleVillageObserver);
        }

        var binding = new InputEventKey { PhysicalKeycode = Key.F3 };
        if (!InputMap.ActionHasEvent(ToggleVillageObserver, binding))
        {
            InputMap.ActionAddEvent(ToggleVillageObserver, binding);
        }
    }

    internal static bool IsToggle(InputEvent inputEvent) =>
        inputEvent is not InputEventKey { Echo: true }
        && inputEvent.IsActionPressed(ToggleVillageObserver);
}
