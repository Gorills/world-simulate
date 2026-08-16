using Godot;

namespace Mws.Client.Godot.Input;

internal static class GameInput
{
    public static readonly StringName MoveLeft = "game_move_left";
    public static readonly StringName MoveRight = "game_move_right";
    public static readonly StringName MoveUp = "game_move_up";
    public static readonly StringName MoveDown = "game_move_down";
    public static readonly StringName PreviousTarget = "game_previous_target";
    public static readonly StringName NextTarget = "game_next_target";
    public static readonly StringName Interact = "game_interact";
    public static readonly StringName AdvanceTime = "game_advance_time";
    public static readonly StringName Cancel = "game_cancel";
    public static readonly StringName Menu = "game_menu";

    public static void ConfigureDefaults()
    {
        BindKey(MoveLeft, Key.A);
        BindKey(MoveRight, Key.D);
        BindKey(MoveUp, Key.W);
        BindKey(MoveDown, Key.S);
        BindAxis(MoveLeft, JoyAxis.LeftX, -1.0f);
        BindAxis(MoveRight, JoyAxis.LeftX, 1.0f);
        BindAxis(MoveUp, JoyAxis.LeftY, -1.0f);
        BindAxis(MoveDown, JoyAxis.LeftY, 1.0f);

        BindKey(PreviousTarget, Key.Q);
        BindButton(PreviousTarget, JoyButton.LeftShoulder);
        BindKey(NextTarget, Key.E);
        BindButton(NextTarget, JoyButton.RightShoulder);

        BindKey(Interact, Key.F);
        BindButton(Interact, JoyButton.A);
        BindKey(AdvanceTime, Key.Space);
        BindButton(AdvanceTime, JoyButton.Y);
        BindKey(Cancel, Key.Escape);
        BindButton(Cancel, JoyButton.B);
        BindKey(Menu, Key.Tab);
        BindButton(Menu, JoyButton.Start);
    }

    private static void BindKey(StringName action, Key key)
    {
        var inputEvent = new InputEventKey { PhysicalKeycode = key };
        Bind(action, inputEvent);
    }

    private static void BindButton(StringName action, JoyButton button)
    {
        var inputEvent = new InputEventJoypadButton { ButtonIndex = button };
        Bind(action, inputEvent);
    }

    private static void BindAxis(StringName action, JoyAxis axis, float value)
    {
        var inputEvent = new InputEventJoypadMotion { Axis = axis, AxisValue = value };
        Bind(action, inputEvent, 0.25f);
    }

    private static void Bind(StringName action, InputEvent inputEvent, float deadzone = 0.2f)
    {
        if (!InputMap.HasAction(action))
        {
            InputMap.AddAction(action, deadzone);
        }

        if (!InputMap.ActionHasEvent(action, inputEvent))
        {
            InputMap.ActionAddEvent(action, inputEvent);
        }
    }
}
