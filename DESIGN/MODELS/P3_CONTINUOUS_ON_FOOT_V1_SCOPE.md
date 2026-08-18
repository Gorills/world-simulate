# P3 Continuous On-Foot V1 Scope

Status: **REVIEW_REQUIRED**

## Narrow question

What is the coarsest on-foot travel scope that lets P3 deliver causal, scalable NPC movement without requiring the simulation to model every possible pause, gate, queue, ferry wait or crossing process before ordinary local travel can work?

## Why this detail is load-bearing now

The active P3 acceptance path needs real `task -> route -> duration -> departure -> progress -> arrival` behavior. The current accepted applicability model keeps `TraversalDelay` as an independent required dimension. Production therefore cannot use the accepted short on-foot duration until a separate no-delay producer exists.

Under the repository anti-overmodeling gate, a separate per-traversal proof of the absence of every possible delay is not justified for the first playable village: it adds state and model dependencies without adding a current player choice, observable feedback loop or required P3 scaling boundary.

The load-bearing requirement is instead that the first travel implementation have a clear supported route class that can later reject or hand off routes containing processes the simple model cannot represent.

## Decision

P3 v1 supports one deliberately coarse travel profile:

**continuous ordinary on-foot traversal**.

A path is inside this v1 profile only when all of its authoritative route connections are already eligible for the accepted simple on-foot timing class and the current runtime/content does not represent a separate traversal process on that path.

For that supported profile, the simple duration model has no added discrete-wait term. The existing applicability gate may therefore treat the delay dimension as `NoMaterialDelay` **by profile scope**, not because historians proved that every real route had no pauses and not because `Passable/Open` alone implies no delay.

This is a bounded implementation scope, not a universal claim about medieval travel.

## What is outside the v1 profile

A route that materially requires any separately represented process is outside the simple profile until that process has its own accepted mechanic. Examples include:

- ferry/service waiting;
- controlled gates/checks with meaningful waiting;
- queues;
- required scheduled stops;
- obstruction handling that is not already represented by the route being unavailable/non-baseline;
- magical transport or transition with its own cost, timing, access or consequence rules.

Such a route must not silently receive the simple duration. It can remain unsupported/unresolved or be handled by a later richer travel mechanic.

## Relationship to existing accepted contracts

If accepted, this contract narrows the current P3 implementation scope without changing the accepted historical proposition that travel conditions can vary.

For the **continuous ordinary on-foot v1 profile only**, it supersedes the requirement in `P3_ON_FOOT_TRAVERSAL_APPLICABILITY.md` and `P3_PLANNED_TRAVERSAL_ASSESSMENT_CONTEXT.md` that production must first create an independent traversal-delay producer before the simple baseline can become `Applicable`.

All other applicability dimensions remain load-bearing:

- actor capability;
- carried load;
- route/environment timing class;
- traversal horizon;
- exact actor/task/path/mode binding.

No change is made to the accepted short-reference horizon or to the accepted `1400 mm/s` calibration boundary.

## Causal model

`selected task`

`-> destination`

`-> unique known/authorized/passable OnFoot path`

`-> path is inside continuous ordinary on-foot v1 profile`

`-> actor/load/route-timing/horizon applicability`

`-> duration plan`

`-> departure -> persistent progress -> arrival`.

If a path later gains a represented process that the simple profile does not support, it no longer qualifies for new simple plans. Already departed plans preserve their snapshotted inputs and are handled by later interruption/reroute rules where applicable.

## Why this remains scalable

The simplification does not erase the future seam. Route identity, route mode, path binding and travel-plan identity remain authoritative.

Town/region/world scale can later add ferries, gates, queues, toll processes, magical transitions or other travel processes as explicit mechanics. Those mechanics can remove a route from the simple profile or provide a richer duration plan without changing the meaning of existing ordinary continuous routes.

We therefore preserve the architectural extension point without paying the state/research cost before gameplay requires it.

## Player/NPC symmetry

The profile is controller-neutral. HumanController and AIController use the same supported route class, applicability facts, duration and travel state.

Neither controller may turn an unsupported process-bearing route into a simple route or use Godot/navmesh state to bypass the simulation boundary.

## Historical and magical grounding

No new quantitative historical claim is introduced. This contract inherits the accepted P3 evidence that ordinary travel used explicit routes, that conditions varied, and that one universal journey rule is unsafe.

The simplification is a product/model-resolution decision: the first playable scope represents ordinary uninterrupted local walking and defers route processes that are not yet part of gameplay.

Magic follows the same boundary. A magical route/process is not forced into historical walking rules. When magical transport becomes gameplay-relevant, its source, access, limits, costs and consequences require their own explicit world law.

## Rules

1. `Passable` or `Open` alone still does not prove baseline timing compatibility.
2. The existing route/environment timing class remains required.
3. No separate persistent `ContinuousPassageCoverage` state is added for P3 v1.
4. Absence of an unimplemented global ferry/queue subsystem is not a historical claim that such delays never existed.
5. Current simple route content must not claim support for a process it cannot represent.
6. A represented discrete travel process makes the simple v1 profile inapplicable until a richer accepted mechanic handles it.
7. The simplification creates no route knowledge, rights, destination, duration or travel by itself.
8. Player and NPC actors use identical rules.

## Validation requirements for later implementation

Production implementation under this scope must show at least:

- a short path whose actor/load/route/horizon facts all match the baseline can reach `Applicable` without a new per-route no-delay state;
- unknown or non-baseline actor/load/route/horizon still blocks the simple duration;
- a path outside the supported simple profile does not receive the simple duration;
- no route knowledge, destination or departure is invented by this scope rule;
- player/NPC controller identity does not change the result;
- duration/progress remain authoritative and persist through save/load;
- Godot remains presentation-only.

## Deferred complexity

Deferred until a concrete gameplay/scaling need makes it load-bearing:

- ferry schedules and waits;
- queues and gate waiting;
- toll/check processing time;
- dynamic obstruction-handling time;
- crowding micro-delays;
- tiny incidental pauses;
- separate continuous-passage provenance for every route segment;
- magical transport timing and access rules;
- non-OnFoot travel modes.

These are not forbidden permanently. They are intentionally not blockers for the first causal P3 travel loop.
