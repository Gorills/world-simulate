# Godot Client Agent Guide

Scope: everything under `src/Mws.Client.Godot/`.

## First choose the owner

- Authoritative RPG/world rule -> **not here**. Change the simulation core/API first.
- Client orchestration / projection selection -> `Session/`.
- Key, mouse-button, gamepad binding or device detection -> `Input/`.
- World-facing visual representation -> `World/`.
- HUD, panel, menu or reusable Control -> `UI/Screens/` (or a future `UI/Components/`).
- Colors, typography, focus styling, spacing conventions -> `UI/Theme/`.
- Application lifecycle and sibling wiring only -> `App/`.

Do not put a feature in `App/Main.cs` because it is convenient. The composition root only creates/wires top-level collaborators and runs the bounded headless client smoke.

## State rule

Godot is a presentation/input client. UI and world views must never directly mutate Hunger, Energy, Affinity, inventory, jobs or other authoritative state.

Flow:

`mouse / keyboard / gamepad -> semantic action/control -> GameWorldSession -> WorldRuntime -> active settlement partition -> projection -> view`

`GameWorldSession` may orchestrate `WorldRuntime`, select the active settlement scope and expose projections/checkpoint seams. It must not own or construct `SettlementSimulation` directly.

The authoritative player actor also lives in `WorldRuntime`: stable player identity, owned inventory and coarse settlement scope come from `GameWorldSession.Player`. `World/Player/` owns only Godot locomotion/camera/presentation state; never mirror authoritative inventory or player identity there.

Only `Session/` may reference `Mws.Simulation.Runtime` from client C# code.

## Input rule

- Gameplay code uses semantic actions from `Input/GameInput.cs`.
- Raw `Key`, `JoyButton`, `JoyAxis` and `InputMap` binding code stays in `Input/`.
- Every required gameplay action keeps both keyboard and gamepad coverage.
- Pointer interaction may use normal Godot Control signals; do not map left-click globally just to imitate controller confirm.
- Built-in `ui_*` actions are for Control focus/navigation, not gameplay rules.

## Player control rule

`DESIGN/PLAYER_CONTROL_SYSTEM.md` is the contract for third-person control feel.

- Raw device bindings and deadzones stay in `Input/`.
- Locomotion/camera tuning belongs in `World/Player/PlayerControlProfile.cs`; do not scatter speed, gravity, jump, turn or camera-feel constants through scene scripts.
- `PlayerMotor` owns CharacterBody3D velocity, braking, acceleration, jump forgiveness and locomotion state.
- `ThirdPersonCameraController` owns orbit response, pitch limits, spring-arm tuning and FOV response.
- `ThirdPersonPlayer` composes input, motor, camera and interaction targeting; it is not a second tuning store or an authoritative player actor.
- Animation/audio/FX consume `PlayerMotionState`; they do not re-decide movement rules.
- A new control mode should normally be a new/reused profile plus explicitly owned motor/camera behaviour, not copied player code.

## UI and focus rule

Every interactive screen must be usable without a mouse. Provide a deterministic initial focus and a path back to the world/previous control. If automatic focus navigation becomes ambiguous, set explicit focus neighbors.

Views emit intent/events. Parents wire sibling components. Avoid deep cross-feature `GetNode()` paths.

## Design system rule

`DESIGN/UI_DESIGN_SYSTEM.md` is the visual contract.

Feature code chooses semantic roles through `DesignSystem`; it does not:
- call `AddTheme*Override`;
- construct `StyleBox` resources;
- read `DesignTokens` directly;
- hardcode UI/debug palette colors;
- write `theme_override_*` values into `.tscn` scenes.

Use `UiSurface`, `UiTextRole`, `UiButtonRole`, `UiTone`, `UiGap` and `UiDataColor`. Add reusable semantics in `UI/Theme/` before adding feature-specific styling.

Debug UI uses the same design system. Density may differ, visual rules do not.

## Scene ownership

A scene's owning C# script lives in the same folder as the `.tscn`. Reusable child scenes are composed as `PackedScene` resources instead of reaching into unrelated scene internals.

## Size budgets

- `App/Main.cs`: <= 180 lines.
- Any Godot client `.cs`: <= 300 lines.

Split by responsibility instead of raising these limits.

## Fast validation

For ordinary core work: `python TOOLS/dev.py fast`.

For Godot/client work before push: `python TOOLS/dev.py godot`.
This always builds the Godot C# adapter; when a Godot binary is available it also runs the headless client smoke.

Do not run full Proof A benchmarks for normal client changes.

## Done checklist

Before considering a Godot change complete:

- authoritative rules remain outside views;
- playable authority flows through `GameWorldSession -> WorldRuntime`;
- no client-owned `SettlementSimulation` path exists;
- player identity/inventory/scope are read from the world player projection, not Godot nodes;
- keyboard and gamepad paths both exist for new gameplay actions;
- controller focus can enter and leave new UI;
- player-control tuning stays behind the profile/motor/camera seams;
- styling is routed through semantic `UI/Theme/` roles;
- scenes contain structure/layout, not local theme overrides;
- owning scene/script are colocated;
- file budgets remain green;
- headless smoke still boots the same authoritative state.
