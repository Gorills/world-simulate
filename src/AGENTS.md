# C# Simulation Agent Guide

Scope: C# source under `src/`, except `Mws.Client.Godot/` which has its own more specific guide.

The goal is fast solo development with a deterministic authoritative simulation. Prefer plain C# and explicit ownership over framework layers.

## Reality-model rule
Authoritative rules must also follow `DESIGN/REALITY_MODELING_POLICY.md`.

- A technically deterministic rule can still be an invalid simulation model. Ask what world cause produces the transition and what evidence supports human/economic/social assumptions.
- Do not promote `SettlementPrototypeContent`, fixed schedules, profession labels, interaction verbs or existing regression expectations into canonical world law without a model contract/evidence.
- Player-controlled and AI-controlled people must obey the same world rules; differences come from ordinary authoritative state, not a player-only mutation path.
- Ownership, permission, obligations and access must be explicit before an actor can consume, transfer or command resources.
- If the real-world model is underdefined, stop with `MODEL_UNDERDEFINED`; do not fill the gap with convenient constants.

## Project ownership

- `Mws.Domain` — tiny engine-free value types that are genuinely shared.
- `Mws.Simulation.Api` — stable commands, state/persistence contracts, projections, result/event codes.
- `Mws.Simulation.Runtime` — authoritative rules and mutation. This is the production simulation.
- `Mws.Persistence.Json` — serialization, checksums and migrations. It must not own gameplay rules.
- `Mws.Headless` — composition/smoke runner only.
- `Mws.Client.Godot` — presentation/input client; follow its nested `AGENTS.md`.

Do not add a project merely to create a layer. Add one only when there is a real dependency boundary.

## Production versus proof code

`Mws.Simulation.Runtime/Verification/ProofA/` is frozen executable proof machinery. It exists to preserve Foundation evidence and regression coverage.

Do not implement gameplay by extending `ProofAKernel`. Production gameplay goes through `Settlement/SettlementSimulation` and must preserve the same invariants: stable identity, deterministic time/order, owner-mediated mutation, idempotent commands, persistence continuity and safe failure.

The obsolete toy `DeterministicWorldSimulation` path must not return.

## Mutation rule

External/player mutations use a typed `SettlementCommand` and `SettlementSimulation.Execute(...)`.

Convenience methods may construct commands, but new gameplay must not create a second mutation path that bypasses command receipts.

A repeated `CommandId` must return the recorded outcome without applying the mutation twice. Command receipts are authoritative state and survive save/load.

Time progression is an authoritative simulation operation, not wall-clock time.

## Determinism rule

Authoritative code must not use ambient nondeterminism such as:

- `DateTime.Now` / `DateTime.UtcNow`
- `Guid.NewGuid`
- `Random.Shared` / `new Random(...)`
- `Task.Run`, `Parallel.*`, ad-hoc threads

Use integer simulation time and state-bound deterministic randomness when a mechanic actually needs randomness. Never use real time to decide world state.

## State and presentation rule

Core outputs structured codes/facts. Human-readable UI sentences belong in the client/localization layer.

Persist stable IDs and authoritative facts, not Godot nodes or presentation objects.

Prototype content is centralized in `SettlementPrototypeContent`; do not scatter item/job/NPC fixture definitions through rule code. When content becomes canonical, promote it only after the reality/model contract and validation required by `DESIGN/REALITY_MODELING_POLICY.md`; do not hard-code fixtures into algorithms.

## File responsibility

Production files under `Mws.Simulation.Runtime/Settlement/` stay <= 260 lines. Split by responsibility rather than raising the budget.

Expected ownership:
- `SettlementSimulation.cs` — state lifecycle and save capture/restore.
- `.Commands.cs` — command dispatch and interaction mutations.
- `.Time.cs` — time/needs/work progression.
- `.Inventory.cs` — owned item-stack operations.
- `.Projection.cs` — read models only.
- `.Events.cs` — structured event append only.
- `SettlementPrototypeContent.cs` — temporary vertical-slice fixture data.

## Testing

During implementation:
`python TOOLS/dev.py fast`

Before push:
`python TOOLS/dev.py check`

For a new simulation mechanic, cover at least:
- intended success;
- safe failure;
- save/load continuity when state changes;
- deterministic replay when ordering/randomness matters;
- invariant preservation (no negative resources, duplicate IDs, or out-of-range bounded state);
- a causal/model acceptance scenario from its `DESIGN/MODELS/` contract when the mechanic changes world behavior.

Do not run Godot or Proof A benchmarks for ordinary core-only edits unless the changed public boundary requires them.
