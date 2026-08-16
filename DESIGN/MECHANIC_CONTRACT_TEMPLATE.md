# Mechanic Contract Template

Copy this file for any mechanic that changes the player's RPG experience. Keep it short.

## Mechanic
Name and one-sentence player verb.

## Intended feeling
What should the player feel or understand after using it?

## Reference aspect
Game/system/reference and the exact aspect being studied. State `original experiment` if none.

## Player decision
Why would the player choose this action instead of another action or doing nothing?

## Rules
Authoritative inputs, state transitions, costs, outcomes and failure cases.

## Feedback
What is visible/audible immediately? What appears in history/trace later?

## Persistence
Which identities, resources, relationships, timers or consequences must survive save/load?

## Input flow
Mouse/keyboard path and gamepad-only path. No pointer-only required step.

## Projection/UI
What data does Godot receive? UI must not own the mechanic's authoritative state.

## Acceptance scenario
A small end-to-end scenario that proves the intended choice and consequence, not only code coverage.

## Deferred complexity
What is deliberately *not* being solved in this iteration?
