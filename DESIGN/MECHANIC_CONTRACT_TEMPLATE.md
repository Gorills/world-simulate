# Mechanic Contract Template

Copy this file for any mechanic that changes the player's RPG experience or authoritative human/economic/social/world behavior. Keep it short. `DESIGN/REALITY_MODELING_POLICY.md` is blocking.

Status: **MODEL_UNDERDEFINED**

Allowed model statuses: `MODEL_UNDERDEFINED`, `REVIEW_REQUIRED`, `ACCEPTED`. A phase cannot PASS while a required referenced contract is not `ACCEPTED`.

## Mechanic
Name and one-sentence player/world verb.

## Intended feeling
What should the player feel or understand after using it?

## Real-world process
What process in the simulated world is this mechanic representing? State `infrastructure-only` only when no world-model semantics are introduced.

## Reference context
Region/culture/institution, historical period and why that context is the baseline. If historical grounding is not applicable, explain why.

## Evidence and sources
At least two credible citations for historical human behavior. State exactly what each source supports, relevant disagreement/variation, and which claims remain uncertain.

## Causal model
Inputs/pressures -> decision/selection -> action -> consequences. Explain why the state changes; do not use clock time or a UI verb as a substitute for motive.

## Player/NPC symmetry
Show how the same action/rule works for AI-controlled and player-controlled people. Any exception must come from ordinary world state such as ownership, office, permission, contract, skill or physical access.

## Ownership, rights and obligations
Who owns/controls affected land, goods, tools, money or authority? What gives an actor permission or duty to act?

## Player decision
Why would the player choose this action instead of another action or doing nothing?

## Rules
Authoritative inputs, state transitions, costs, outcomes and failure cases.

## Long-horizon behavior
What should happen over years if this rule runs without the player? For settlement economy/demography/resource balance, define a >=10 simulated-year validation scenario and explain plausible failure versus model failure.

## Assumptions and uncertainty
List simplifications, unresolved research questions and regional/historical variation. Use `MODEL_UNDERDEFINED` instead of inventing an unsupported universal rule.

## Fixture boundary
Which current values/actions are temporary prototype fixtures and must not become canon or regression constraints?

## Falsifiers
What observation, source, scenario or long-run result would force us to revise this model?

## Feedback
What is visible/audible immediately? What appears in history/trace later?

## Persistence
Which identities, resources, relationships, timers or consequences must survive save/load?

## Input flow
Mouse/keyboard path and gamepad-only path. No pointer-only required step.

## Projection/UI
What data does Godot receive? UI must not own the mechanic's authoritative state.

## Acceptance scenario
A small end-to-end scenario that proves the intended choice and consequence, not only code coverage. Include causal/model evidence, not only technical success.

## Deferred complexity
What is deliberately *not* being solved in this iteration, and why does that omission not invalidate the current causal model?
