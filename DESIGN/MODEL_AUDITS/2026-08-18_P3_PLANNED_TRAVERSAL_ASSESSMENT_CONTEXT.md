# Audit — P3 Planned Traversal Assessment Context

Audit date: **2026-08-18**

Verdict: **PASS**

Model contract: `DESIGN/MODELS/P3_PLANNED_TRAVERSAL_ASSESSMENT_CONTEXT.md`

Reviewed model SHA: `ccd28109a8f099293d364fdeac91c9a86c90f136`

## Scope

Independent audit of the ownership/lifetime boundary for traversal-specific `TraversalDelay` and `TraversalHorizon` applicability facts.

This audit does **not** accept a delay producer, a short-versus-prolonged production classifier, numeric delay, sustained-journey pace, travel duration, departure, progress, reroute, cancellation or arrival.

## Repository and CI

The exact reviewed SHA was branch/PR HEAD `ccd28109a8f099293d364fdeac91c9a86c90f136` at audit time.

Required GitHub Actions on the reviewed SHA all passed:

- `ci #177` — success;
- `playable-prototype-gate #236` — success;
- `proof-a-measure #172` — success.

The reviewed commit changed only `DESIGN/MODELS/P3_PLANNED_TRAVERSAL_ASSESSMENT_CONTEXT.md`; no production/runtime/test file was changed.

`ci #177` completed `scope`, `core`, `godot` and `ci-required` successfully. Core included build, core tests, architecture tests, headless core smoke, production settlement scale smoke and Proof A workload smoke. Godot included C# build and headless integration smoke.

## Accepted prerequisite re-check

Reopened at the exact reviewed SHA:

- `DESIGN/MODELS/P3_SEMANTIC_LOCATION_AND_TRAVEL.md` — `ACCEPTED`;
- `DESIGN/MODELS/P3_ON_FOOT_TRAVEL_DURATION_CALIBRATION.md` — `ACCEPTED`;
- `DESIGN/MODELS/P3_ON_FOOT_TRAVERSAL_APPLICABILITY.md` — `ACCEPTED`;
- `DESIGN/MODEL_AUDITS/2026-08-18_P3_ON_FOOT_TRAVERSAL_APPLICABILITY.md` — prior independent PASS audit.

No new historical-human behavior claim is introduced by the reviewed contract. Its load-bearing premises are state-ownership and causal-order consequences of the already accepted contracts, so no new empirical coefficient or historical source is promoted here.

The prerequisite chain consistently requires:

`selected task -> destination -> known/authorized/mode-feasible route -> timing applicability -> duration plan -> departure -> progress -> arrival`

and consistently rejects interpreting missing capability/load/route/delay/horizon information as favorable defaults.

## Ownership review

**PASS.**

`TraversalDelay` and `TraversalHorizon` are correctly modeled as facts of one concrete planned traversal assessment rather than permanent resident/player traits or immutable route-connection properties.

The same person may face different delay/horizon conditions on different traversals, while the same connection may participate in both short and prolonged paths or different stop/wait contexts. Static actor/route ownership would therefore conflate distinct causal situations.

Actor capability and carried load remain separate current actor inputs; route/environment timing class remains separate route authority. The reviewed model does not erase those distinctions.

## Assessment binding review

**PASS.**

The minimum conceptual binding contains:

- authoritative actor reference;
- selected `TaskId`;
- origin;
- destination;
- ordered route connection identities;
- travel mode;
- provenance/source identity for explicit traversal-specific delay/horizon facts.

Ordered path identity is load-bearing: an assessment for one path cannot be reused for another path with the same endpoints.

The proposed actor identity is controller-neutral and scope-qualified (`SimulationScopeId + EntityId` or equivalent), preventing controller type from becoming timing authority and avoiding partition-local identity collision.

## Invalidation review

**PASS.**

The contract requires re-assessment when any load-bearing planning input changes, including task, actor/scope, origin/destination, ordered path, mode, actor capability, carried load, route timing, delay source or horizon source.

No stale pre-departure assessment may authorize duration or departure.

This is consistent with the parent applicability requirement that duration-driving inputs be snapshotted or immutably referenced once a future accepted travel plan departs.

## Delay and horizon uncertainty review

**PASS.**

The contract preserves the accepted three-state dimensions:

- delay: `Unknown | NoMaterialDelay | MaterialDelayPresent`;
- horizon: `Unknown | BaselineShortReferenceCompatible | ProlongedOrEnduranceRelevant`.

Absence of a modeled delay producer remains `Unknown`, not `NoMaterialDelay`.

Absence of an endurance model remains `Unknown`, not `BaselineShortReferenceCompatible`.

No universal distance/time threshold, numeric delay, fallback speed or sustained-journey pace is introduced.

Explicit bounded fixture/reconstruction assertions require non-empty provenance and binding to the concrete actor/task/path/mode assessment. They cannot become default production settlement facts.

## Rights and route-authority review

**PASS.**

The contract preserves:

`knowledge + route physical/mode feasibility + passage authorization -> concrete route path -> traversal assessment -> duration plan`.

An assessment cannot open a restricted route, make a blocked route passable, create route knowledge, choose between multiple feasible routes or create a selected task.

Presence/access/action rights remain separate from timing applicability.

## Player/NPC symmetry review

**PASS.**

Controller identity is excluded from the assessment binding. Equivalent world/scope actor identity, task, path, mode and source facts produce the same assessment for player-controlled and AI-controlled humans.

Godot geometry, animation speed and client-authored `UseBaselineSpeed`/delay/horizon/decision shortcuts remain non-authoritative.

## Persistence and replay review

**PASS for model scope.**

Before departure, a derived assessment may be recomputed rather than persisted as a second mutable truth when current authoritative source facts are sufficient.

If traversal-specific source facts are persisted, their binding and provenance must survive save/load.

After a future accepted departure, duration-driving inputs must be snapshotted or immutably referenced so later world/content changes cannot rewrite elapsed history.

The final travel-plan persistence representation remains explicitly deferred.

## Current production boundary review

**PASS.**

Current production may continue projecting delay/horizon as `Unknown`. Therefore the existing partial applicability projection may produce `Unresolved` or `NotApplicable`, but must not produce production `Applicable` until accepted authoritative producers for both missing dimensions exist.

The reviewed model does not authorize `1400 mm/s` duration or departure.

## Long-horizon review

**PASS for ownership-only scope.**

This contract starts no travel, consumes no simulation time and changes no settlement economy/demography. Integrated long-horizon validation remains required when production duration/departure begins affecting labour capacity or economic timing.

## Remaining blockers outside this contract

1. Traversal-delay producer model: which world processes establish `NoMaterialDelay` or `MaterialDelayPresent`, including temporal scope/provenance.
2. Traversal-horizon producer model: an evidence-backed production rule for short-reference versus endurance-relevant traversal without inventing a universal threshold.
3. Full travel-plan duration input snapshot/persistence.
4. Departure/progress/interruption/reroute/cancellation/arrival.

## Overall verdict

**PASS.**

The model correctly assigns delay/horizon applicability to a concrete engine-owned traversal assessment, binds it strongly enough to prevent cross-task/path reuse, preserves uncertainty/rights/controller boundaries, and remains compatible with deterministic save/load/replay requirements.

No blocker remains inside the declared ownership/lifetime scope. Promotion from `REVIEW_REQUIRED` to `ACCEPTED` is justified.
