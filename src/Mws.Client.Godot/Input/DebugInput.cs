using Godot;

namespace Mws.Client.Godot.Input;

internal static class DebugInput
{
    internal static readonly StringName ToggleVillageObserver = "debug_village_observer";
    internal static readonly StringName StartP3TravelPlaytest = "debug_start_p3_travel_playtest";

    internal static void ConfigureDefaults()
    {
        BindKey(ToggleVillageObserver, Key.F3);
        BindKey(StartP3TravelPlaytest, Key.F4);
    }

    internal static bool IsToggle(InputEvent inputEvent) =>
        inputEvent is not InputEventKey { Echo: true }
        && inputEvent.IsActionPressed(ToggleVillageObserver);

    internal static bool IsStartP3TravelPlaytest(InputEvent inputEvent) =>
        inputEvent is not InputEventKey { Echo: true }
        && inputEvent.IsActionPressed(StartP3TravelPlaytest);

    private static void BindKey(StringName action, Key key)
    {
        if (!InputMap.HasAction(action))
        {
            InputMap.AddAction(action);
        }

        var binding = new InputEventKey { PhysicalKeycode = key };
        if (!InputMap.ActionHasEvent(action, binding))
        {
            InputMap.ActionAddEvent(action, binding);
        }
    }
}
