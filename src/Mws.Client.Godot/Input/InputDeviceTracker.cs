using Godot;

namespace Mws.Client.Godot.Input;

internal enum InputDeviceFamily
{
    KeyboardMouse,
    Gamepad,
}

internal sealed class InputDeviceTracker
{
    public InputDeviceFamily Current { get; private set; } = InputDeviceFamily.KeyboardMouse;

    public bool Observe(InputEvent inputEvent)
    {
        ArgumentNullException.ThrowIfNull(inputEvent);

        var next = inputEvent switch
        {
            InputEventJoypadButton => InputDeviceFamily.Gamepad,
            InputEventJoypadMotion => InputDeviceFamily.Gamepad,
            InputEventKey => InputDeviceFamily.KeyboardMouse,
            InputEventMouse => InputDeviceFamily.KeyboardMouse,
            _ => Current,
        };

        if (next == Current)
        {
            return false;
        }

        Current = next;
        return true;
    }
}
