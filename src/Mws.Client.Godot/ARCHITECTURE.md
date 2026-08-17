# Godot Client Architecture v0.2

Godot is a presentation/input client over the authoritative C# world runtime. It is not a second game-state owner.

## Flow

`mouse / keyboard / gamepad → GameInput → GameWorldSession → WorldRuntime → active settlement partition → projection → views`

Views send intent (`select resident`, `interact`, `advance time`). They never set Hunger, Affinity, inventory or job state directly.

`GameWorldSession` is the client orchestration boundary. It owns one `WorldRuntime`, remembers the active settlement scope, routes time/commands/projection through the world runtime, and exposes a world-checkpoint seam. It does not own a `SettlementSimulation`.

## Structure

- `App/` — composition root and process lifecycle only.
- `Session/` — client-side orchestration around `WorldRuntime`, active scope selection, projections and checkpoint/restore seams; no Godot node code.
- `Input/` — semantic gameplay actions and last-used-device tracking.
- `World/` — world-facing views such as settlement/resident representations.
- `UI/Screens/` — screen/panel interaction components.
- `UI/Theme/` — design tokens and reusable styling helpers.

Only `Session/` may reference `Mws.Simulation.Runtime` from client C# code.

## File budgets

Architecture tests enforce:
- `App/Main.cs` ≤ 180 lines.
- every Godot client `.cs` file ≤ 300 lines.

When a file approaches the budget, split by responsibility. Do not raise the limit to avoid refactoring.

## Input policy

Gameplay code refers to semantic actions (`game_interact`, `game_advance_time`, etc.), never raw key codes.
Default bindings are source-controlled in `Input/GameInput.cs`.

Godot's built-in `ui_*` actions are reserved for Control focus/navigation/activation. They are not gameplay actions.
Mouse users interact directly with controls. Keyboard/gamepad users always receive a focusable path.

Future rebinding changes the action-event mapping, not gameplay code.

## Design system v0.1

`UI/Theme/DesignTokens.cs` contains the initial color, spacing, control-height and type scale.
`UI/Theme/DesignSystem.cs` is the only place for reusable control styling.

Feature code should not invent one-off fonts/colors unless a mechanic requires a deliberately distinct semantic state.
The visual language is intentionally simple until art direction is established.

## Scene ownership

`App/Main.tscn` composes the application.
`GameHud.tscn` composes the current playable HUD.
Feature scenes own their internal nodes and expose narrow C# methods/events.

Do not use deep cross-scene `GetNode()` calls from unrelated features. The parent/composition layer wires siblings.

## Headless CI

When `DisplayServer.GetName() == "headless"`, `App/Main.cs` executes a bounded client-boundary smoke and quits. The smoke proves that the playable session advances and interacts through `WorldRuntime` and that its checkpoint restores the same visible world state.
Normal desktop execution stays open as a playable client.
