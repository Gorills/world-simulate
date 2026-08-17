# Playable Prototype Program v1

This program turns the current village branch into a playable, measurable systemic prototype without allowing architecture work to outrun gameplay evidence.

The program is intentionally sequential. A phase is not complete because its implementation compiles or its author is satisfied. Every implementation phase stops at `AUDIT_REQUIRED` and must receive an independent post-commit code review plus systems audit before any later phase may begin.

## State machine

Allowed phase states:

- `LOCKED` — implementation must not begin.
- `IMPLEMENTING` — this is the only phase whose production scope may change.
- `AUDIT_REQUIRED` — implementation is frozen; review the exact committed subject.
- `FAILED` — blockers were found; repair this same phase before anything later starts.
- `PASSED` — code review and systems audit both passed.

At most one phase may be in `IMPLEMENTING`, `AUDIT_REQUIRED`, or `FAILED`.

A later phase may leave `LOCKED` only when every dependency is `PASSED`.

The authoritative machine-readable state is `MACHINE/playable-prototype.json`.
Run `python TOOLS/validate_playable_prototype.py` before normal validation.

## Audit independence contract

Implementation and audit are separate passes.

When a phase reaches `AUDIT_REQUIRED`:

1. Stop implementation.
2. Audit the exact committed SHA. Do not silently fix the subject while reviewing it.
3. Review both the changed code and the connected systems it can affect.
4. Emit an audit record under `AUDIT_RESULTS/PLAYABLE_PROTOTYPE/`.
5. A phase is `PASSED` only when both `code_review` and `systems_audit` are `PASS`.
6. If either review fails, set the phase to `FAILED`. Repairs remain inside the same phase and require another post-commit audit.

Prefer a separate agent or human reviewer when available. When one agent performs both roles, the audit pass must still be post-commit, read-only with respect to the audited SHA, and must not rely on the implementation author's explanation as evidence.

An audit must examine, when applicable:

- authority and state ownership;
- mutation paths and public boundaries;
- determinism, replay, save/load and migrations;
- client/runtime integration;
- connected gameplay systems and semantic consistency;
- asymptotic work, allocations and likely scale cliffs;
- whether the phase advances the playable systemic loop rather than only presentation;
- whether a parallel or bypass path was introduced;
- validation evidence and missing coverage.

Green tests are evidence, not a substitute for this review.

## Program phases

| Phase | Goal | Required proof before PASS |
| --- | --- | --- |
| P0 `PROCESS_GATE` | Install this sequential audit contract and make it executable in local validation/CI. | Validator rejects invalid phase transitions; local/CI entry points run it; contract is discoverable to agents. |
| P1 `PLAYABLE_USES_WORLD_RUNTIME` | Make the playable client use `WorldRuntime` as its authoritative composition root. | Time, commands and projection flow through `WorldRuntime`; no client-owned `SettlementSimulation` authority path remains; current village behavior stays equivalent. |
| P2 `AUTHORITATIVE_PLAYER_ACTOR` | Represent the player as an authoritative simulation actor. | Stable player identity, owned inventory and semantic location survive save/load and deterministic replay; client sends intent only. |
| P3 `SEMANTIC_LOCATION_AND_TRAVEL` | Add authoritative place/travel semantics for player and residents without persisting render coordinates. | Location-dependent interactions cannot contradict authoritative activity/travel; Godot remains presentation/interpolation. |
| P4 `REMOVE_HOT_PATH_FULL_STATE_CLONE` | Remove whole-settlement snapshot/restore from ordinary world ticks and commands. | Mutation preserves safety/idempotency without full-state cloning; before/after runtime and allocation evidence is recorded. |
| P5 `FOOD_SHORTAGE_GAMEPLAY_LOOP` | Prove one costly systemic player choice can change the village trajectory. | Autonomous food pressure, at least two competing interventions, persistent consequences and materially different end-of-day outcomes. |
| P6 `MEASURABLE_VERTICAL_SLICE` | Measure the actual playable scenario end to end and decide the next scale work from evidence. | Gameplay outcome metrics and runtime metrics are emitted for agreed scale cases; final audit states what to optimize next and what not to build yet. |

## P1 acceptance boundary

P1 is deliberately narrow:

```text
Godot
  -> GameWorldSession
  -> WorldRuntime
  -> active SettlementSimulation partition
```

P1 must not also implement the player actor, travel semantics, new economy mechanics or presentation polish.

Acceptance:

- playable/headless client composition uses `WorldRuntime`;
- time advances through `WorldRuntime`;
- settlement commands execute through `WorldRuntime`;
- settlement projections are obtained through `WorldRuntime`;
- current village behavior remains functionally equivalent;
- no second authoritative mutation path remains in the client session;
- a world checkpoint/save seam is available at the session boundary;
- relevant core and Godot smoke validation is green.

## Scope freeze until P6 audit

Do not add these unless the current phase cannot be proven without them and the scope exception is recorded in that phase's audit:

- UI/design-system polish;
- lighting polish;
- animation polish;
- asset-pipeline expansion;
- new professions or extra buildings;
- combat;
- skill/progression systems;
- family simulation beyond what the active phase explicitly requires;
- crowd avoidance;
- unrelated RPG verbs.

The freeze is not a claim that these features are unimportant. It prevents presentation and breadth from masking unresolved authority, scaling and gameplay-causality problems.

## P5 scenario target

The default product proof is a food-shortage day:

`needs -> consumption -> inventory -> farm -> grain -> kitchen -> ration -> household -> player intervention -> persistent consequence`

The player must be able to observe the problem and choose between at least two interventions that compete for a limited resource, time or opportunity. By end of day, different choices must produce objectively different world state.

Minimum gameplay metrics:

- hungry residents;
- critical-hunger hours;
- work hours completed/missed;
- grain/rations produced and consumed;
- households without food;
- player actions and meaningful player actions;
- relationship changes caused by the scenario.

Minimum runtime metrics by P6:

- advance-hour latency;
- allocations per simulated hour;
- command latency;
- projection latency/allocations;
- checkpoint latency/bytes;
- dormant catch-up latency if dormant partitions are exercised.

Do not infer million-agent capacity from the current proof benchmarks. Measure the actual runtime path before selecting coarse LOD, projection deltas or persistence optimization as the next project.
