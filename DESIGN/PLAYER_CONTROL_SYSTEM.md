# Player Control System

Status: village exploration foundation v0.1

## Goal

Player control is a reusable presentation system, not a pile of tuning constants inside one Godot scene script. The target feel is a grounded, responsive third-person exploration controller with deliberate acceleration, readable turning, camera weight and forgiving jumps. The Witcher 3 is an experiential reference for trailing-camera exploration and character weight; this project does not copy its implementation, timings, assets or input layout.

## Ownership

The player-control stack lives entirely in the Godot client. It does not own authoritative world simulation state.

`raw device input -> GameInput semantic actions -> ThirdPersonPlayer composition -> PlayerMotor / ThirdPersonCameraController -> PlayerMotionState -> visual / future animation`

### Input

`Input/GameInput.cs` owns keyboard/gamepad bindings, deadzones and semantic input sampling. World/player code never binds raw keys or controller axes.

### Control profile

`World/Player/PlayerControlProfile.cs` is the single source of locomotion and camera tuning. Speeds, acceleration, braking, gravity, jump forgiveness, turn response, camera sensitivity, camera distance and FOV belong here.

Do not add feel constants to `ThirdPersonPlayer`, `PlayerMotor` or feature UI.

Profiles are replaceable. `Exploration` is the first profile; future `Combat`, `Horse`, `Indoor` or accessibility variants may reuse the same motor/camera contracts with different values.

### Motor

`PlayerMotor` owns CharacterBody3D locomotion:

- camera-relative desired movement;
- analog movement magnitude;
- walk/run speed targets;
- smooth ground acceleration;
- slower walk-to-run sprint ramp;
- explicit braking and direction-change response;
- reduced air control;
- gravity and grounded floor handling;
- jump buffer and coyote time;
- body-facing rotation from actual horizontal velocity;
- motion-state publication.

The motor uses Godot `CharacterBody3D` collision/slope/floor behaviour. Render coordinates remain presentation-only.

### Camera

`ThirdPersonCameraController` owns camera feel independently from the motor:

- mouse and right-stick look;
- pitch limits;
- smoothed yaw/pitch response;
- shoulder offset;
- spring-arm distance and collision margin;
- normal/run FOV transition.

The SpringArm uses a shape cast rather than a single ray so the camera has volume near walls.

### Motion state

`PlayerMotionState` is the stable seam for future animation/audio/FX. Consumers should use semantic facts such as:

- `Idle`, `Walking`, `Running`, `Airborne`;
- horizontal speed and desired speed;
- grounded;
- sprint requested;
- just jumped;
- just landed.

Animations must consume this state; they must not re-decide locomotion rules.

## Default exploration input

Keyboard / mouse:

- WASD: move
- Shift: sprint
- Space: jump
- Mouse: camera
- F: interact
- Tab: world/menu
- Q/E: target selection
- T: prototype +1 simulation hour
- Escape: UI cancel

Gamepad:

- left stick: move
- L3: sprint
- B: jump in world / cancel while UI is open
- right stick: camera
- A: interact
- Start: world/menu
- LB/RB: target selection
- Y: prototype +1 simulation hour

Input contexts make the shared B binding safe: when HUD is open, player input is disabled and B is UI cancel; in the world it is jump.

## Feel contract

The exploration profile should preserve these qualitative properties even when values are tuned:

1. Starting movement is responsive, but reaching full run speed is visibly gradual.
2. Releasing movement brakes faster than sprint acceleration so stopping remains controllable.
3. Reversing direction carries a short sense of mass rather than instantly replacing velocity.
4. Partial analog-stick input permits slow walking.
5. Jumping is grounded and compact rather than floaty.
6. A jump pressed shortly before landing or shortly after leaving an edge should still succeed within bounded forgiveness windows.
7. Character facing follows actual travel direction, not raw stick direction.
8. Camera rotation is responsive but not frame-snapped; sprint widens FOV subtly rather than producing a dramatic zoom.
9. Camera collision remains independent of world interaction targeting.
10. No locomotion feel value is duplicated in a scene or gameplay feature.

## Extension rules

Before adding a new movement feature, choose its owner:

- new binding / deadzone / device mapping -> `Input/`;
- tunable feel value -> `PlayerControlProfile`;
- collision/velocity/jump/slope rule -> `PlayerMotor`;
- orbit/FOV/shoulder/camera collision behaviour -> `ThirdPersonCameraController`;
- animation/audio/FX reaction -> consume `PlayerMotionState`;
- authoritative stamina/injury/mount/world rule -> simulation/API first, then project it into the client.

Dodge, vault, climb, combat lock-on, stamina, horse movement and root-motion animation are intentionally outside this first foundation. They should extend these seams instead of bypassing them.
