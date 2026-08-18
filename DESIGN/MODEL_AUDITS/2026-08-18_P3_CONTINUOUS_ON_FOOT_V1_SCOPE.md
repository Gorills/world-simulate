# Audit — P3 Continuous On-Foot V1 Scope

Audit date: **2026-08-18**

Verdict: **PASS**

Model contract: `DESIGN/MODELS/P3_CONTINUOUS_ON_FOOT_V1_SCOPE.md`

Reviewed research/model SHA: `042faf91fc8a6f1bd2d411c2b908da7a8a3e4d2e`

Acceptance/status change: **same commit as this audit record**. The exact self-referential commit SHA is intentionally not embedded in its own contents; repository history identifies the commit containing this file and the `REVIEW_REQUIRED -> ACCEPTED` status change.

## Scope

Independent audit of the deliberately coarse P3 v1 travel-profile decision only. No production simulation code is accepted or changed by this audit. The reviewed commit adds `P3_CONTINUOUS_ON_FOOT_V1_SCOPE.md` and removes the unaccepted `P3_TRAVERSAL_DELAY_PRODUCER.md` research branch.

This audit asks whether a separate per-traversal model for proving absence of every possible wait/stop is load-bearing for the first causal P3 travel loop, or whether a narrower supported-profile boundary preserves causality and future scaling with less state and research.

## Repository and CI

At audit time PR #5 head was exactly `042faf91fc8a6f1bd2d411c2b908da7a8a3e4d2e` on `feature/p3-travel-lifecycle`; no PR conversation comments or inline review threads were present.

Required GitHub Actions on the exact reviewed SHA all passed:

- `ci #191` — success;
- `proof-a-measure #185` — success;
- `playable-prototype-gate #259` — success.

No local build/test execution is claimed in this environment.

## Load-bearing checks

### 1. Current production really is blocked only by the independent delay dimension

Re-opened `src/Mws.Simulation.Runtime/Settlement/SettlementSimulation.TraversalApplicability.cs` at the reviewed SHA.

Production derives actor, carried-load, route-timing and horizon inputs, but hard-codes `TraversalDelay = Unknown`; the five-dimension gate therefore cannot become `Applicable` for an otherwise baseline-compatible short route.

Audit conclusion: the reviewed contract is addressing a real current blocker rather than inventing a future concern.

### 2. Current route authority has no separate ferry/queue/wait process to model

Re-opened `src/Mws.Simulation.Api/SettlementRouteContracts.cs` at the reviewed SHA.

The route contract contains stable connection identity, endpoints, distance, physical state, passage status, provenance, supported travel modes and the on-foot timing class. It does not currently contain a separate traversal-process reference for ferry waiting, queueing, gate service, scheduled stops or magical transition timing.

Audit conclusion: requiring a new persistent `ContinuousPassageCoverage`/delay-source layer now would primarily prove the absence of mechanics that are not yet represented. Under the repository anti-overmodeling rule, that is not load-bearing for the first playable continuous walking loop.

### 3. The simplification does not turn `Passable/Open` into a universal timing rule

Re-opened the accepted `P3_ON_FOOT_TRAVERSAL_APPLICABILITY.md` and `P3_PLANNED_TRAVERSAL_ASSESSMENT_CONTEXT.md` boundaries.

The reviewed scope keeps actor capability, carried load, route/environment timing class, traversal horizon and exact actor/task/path/mode binding load-bearing. It explicitly preserves the rule that `Passable`/`Open` alone is insufficient for baseline timing.

Audit conclusion: the new scope removes only the independent no-delay proof for the supported continuous v1 profile. It does not erase the other safety gates or permit arbitrary routes to use `1400 mm/s`.

### 4. Future process-bearing routes still have a hard extension boundary

The reviewed contract states that a route requiring a separately represented ferry, queue, controlled wait, scheduled stop, obstruction process or magical transition is outside the simple v1 profile until that process has an accepted mechanic.

Route identity, travel mode, ordered path binding and travel-plan identity remain authoritative seams.

Audit conclusion: this is a scope reduction, not an architectural dead end. A future town/region/magic mechanic can reject the simple profile or provide richer timing without redefining ordinary continuous walking.

### 5. Historical evidence is not broadened

No new historical-human quantitative claim is introduced by this contract. Re-opened the accepted audit record `DESIGN/MODEL_AUDITS/2026-08-17_P3_SEMANTIC_LOCATION_AND_TRAVEL.md` for the inherited premises.

That audit already established that travel used explicit transport connections, travel conditions varied, one universal journey rule is unsafe, and detailed ferries/tolls/maintenance/wayfinding remain deferred unless they become material.

Audit conclusion: new external source research is **not load-bearing** for this scope-resolution decision. The contract does not claim that medieval local travel was universally uninterrupted; it defines which travel class P3 v1 chooses to simulate first.

## Causal model review

**PASS.**

The accepted causal order remains:

`selected task -> destination -> known/authorized/passable OnFoot path -> supported continuous v1 profile -> actor/load/route/horizon applicability -> duration plan -> departure -> progress -> arrival`.

The scope creates no task, destination, knowledge, right, route, duration or departure by itself.

## Player/NPC symmetry review

**PASS.**

The supported profile and all remaining applicability inputs are controller-neutral. HumanController and AIController cannot receive different travel physics from identical authoritative facts.

## Rights and authorization review

**PASS.**

Nothing in the scope opens a restricted route, bypasses route knowledge, creates passage rights or grants access at the destination.

## Scaling / anti-overmodeling review

**PASS.**

A separate per-segment/per-traversal proof of absence of every possible delay does not currently change a player-observable choice, P3 acceptance criterion, persistence invariant or necessary scaling boundary. The coarser supported-profile abstraction preserves the extension seam while removing unnecessary state and research.

If a ferry, gate queue, magical transition or another discrete travel process becomes gameplay- or scaling-relevant, that process becomes load-bearing at that time and must receive its own accepted world rule.

## Persistence and determinism review

**PASS for model scope.**

The scope does not introduce new mutable persisted state. Existing route/path/task bindings and future duration-plan snapshots remain responsible for deterministic replay. Production implementation must still persist authoritative duration/progress before P3 can pass.

## Long-horizon review

**NOT_APPLICABLE to this scope-only decision.**

This contract starts no travel and changes no settlement economic/demographic rate by itself. Integrated travel cost remains subject to later P3 implementation review and P5/P6 long-horizon validation where it affects labour/economy.

## Deferred gaps

Intentionally deferred rather than blockers for continuous on-foot P3 v1:

- ferry schedules/waits;
- queues and meaningful gate waiting;
- toll/check service time;
- dynamic obstruction-handling time;
- crowding micro-delays and incidental pauses;
- magical transport timing/access/costs;
- non-OnFoot travel modes.

These become blockers only when a concrete gameplay, scaling or world-law requirement needs them.

## Overall verdict

**PASS.**

`P3_CONTINUOUS_ON_FOOT_V1_SCOPE.md` may be promoted from `REVIEW_REQUIRED` to `ACCEPTED`.

The previously proposed independent traversal-delay producer is correctly not accepted as a P3-v1 prerequisite. Production may next implement the accepted continuous-profile interpretation, but that implementation is a separate bounded task and is not authorized by this audit to invent tasks, routes, duration, departure or other missing travel lifecycle behavior.
