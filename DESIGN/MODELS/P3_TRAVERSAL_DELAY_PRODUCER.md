# P3 Traversal Delay Producer

Status: **REVIEW_REQUIRED**

## Narrow question

What authoritative facts may derive `TraversalDelay = NoMaterialDelay` or `MaterialDelayPresent` for one concrete on-foot traversal without treating route existence, passability, or missing mechanics as proof that no delay exists?

Deferred: numeric delay duration, departure, route choice, walking calibration, fatigue, weather, ferry schedules, queues, default village routes, task generation, reroute and arrival.

## Accepted prerequisites

Reuses without broadening:

- `P3_SEMANTIC_LOCATION_AND_TRAVEL.md`;
- `P3_ON_FOOT_TRAVEL_DURATION_CALIBRATION.md`;
- `P3_ON_FOOT_TRAVERSAL_APPLICABILITY.md`;
- `P3_PLANNED_TRAVERSAL_ASSESSMENT_CONTEXT.md`;
- `P3_BASELINE_SHORT_REFERENCE_HORIZON.md`.

The accepted order remains:

`selected task -> destination -> known/authorized/mode-feasible path -> traversal assessment -> duration plan -> departure -> progress -> arrival`.

`TraversalDelay` belongs to one concrete traversal assessment, not permanently to a resident, controller, or route.

## Mechanic

Separate continuous locomotion from discrete waiting/stopping. A traversal can be `NoMaterialDelay` only when its complete ordered path is explicitly covered by authoritative continuous-passage facts and no applicable delay process exists.

## Reference context and evidence

Baseline remains rural lowland England, approximately 1270–1348.

### Alan Cooper, “Once a highway, always a highway: roads and English law, c. 1150–1300” (2016)

https://academic.oup.com/manchester-scholarship-online/book/16180/chapter-abstract/171231557

Cooper describes medieval legal action around highway obstructions, tolls, and maintenance. This supports treating obstruction/toll/maintenance conditions as facts separate from route existence or recognition. It does not establish universal delay durations or that every local path was a highway.

### David Harrison, “Stability: Bridges and the Road System After 1250” (2007)

https://academic.oup.com/book/6206/chapter/149821861

Harrison distinguishes bridges, ferries, and fords and notes that reliable bridge crossing depended on repair. This supports treating crossing form and condition as explicit traversal inputs. It does not establish ferry waits, ford speeds, or that every bridge was immediately traversable.

### Valerie Allen, “When things break: mending roads, being social” (2016)

https://academic.oup.com/manchester-scholarship-online/book/16180/chapter-abstract/171232067

Allen examines road/street disrepair from wear, weather, damage, or neglect and the interventions used to restore passage. This supports current route condition as state rather than a timeless map property. It does not establish one slowdown or repair-time coefficient.

Evidence therefore does not justify:

`Open + Passable -> NoMaterialDelay`.

## Vocabulary

### Continuous locomotion

Time explained by route extent and the applicable on-foot locomotion calibration is not a traversal delay. Terrain/surface effects already owned by `OnFootRouteTimingClass` are not counted again here.

### Discrete traversal delay

Additional authoritative waiting/stopping not explained by simple continuous locomotion. Examples, only when represented by accepted state, include ferry/service waiting, a controlled crossing, queue, required stop/check, or obstruction-handling process.

This contract introduces no numeric delay amount.

### ContinuousPassageCoverage

An explicit source-controlled fact that a bounded route connection/transition is modeled as continuously traversable for the relevant mode and current state/content scope, with no separate represented waiting/stop mechanism for that covered portion.

It requires provenance and coverage scope.

It is not equivalent to:

- `PhysicalState = Passable`;
- `PassageStatus = Open`;
- `OnFootTimingClass = BaselineLevelUnobstructed`;
- absence of a delay mechanic in code;
- a permanent promise that the route can never gain a delay process.

A source-controlled reconstructed village pedestrian connection may carry such coverage when the content explicitly models it as an uninterrupted walking connection. That is bounded reconstruction authority, not a universal historical claim.

### TraversalDelayProcess

An authoritative process/state reference bound to part of the concrete traversal that requires additional waiting/stopping beyond continuous locomotion. It must have enough identity/provenance for deterministic replay when it affects a departed plan.

## Derivation

For one concrete OnFoot assessment bound to actor/task/origin/destination/ordered connections/mode:

1. Inspect the entire ordered path, including modeled transitions between connections.
2. If an applicable represented process requires non-continuous waiting/stopping, derive `MaterialDelayPresent` for the narrow simple baseline.
3. Otherwise derive `NoMaterialDelay` only if every path portion has explicit current `ContinuousPassageCoverage` and no applicable delay process exists.
4. Missing coverage, unresolved process state, or mismatched provenance/scope yields `Unknown`.

For this simple baseline, any represented nonzero discrete wait is material because the baseline formula has no term for extra waiting. This is a model-scope statement, not a claim that every real pause is historically important. No numeric threshold is introduced.

## No favorable default

Production must not reason:

`no ferry/queue mechanic implemented -> NoMaterialDelay`

or:

`route is Passable/Open -> NoMaterialDelay`.

Missing information remains `Unknown`.

## Why this is not a permanent route flag

A route connection may contribute coverage source facts, but the final delay class is derived for the concrete path and current state. Coverage is invalidated or superseded if the ordered path, mode, route process/condition, or content authority changes.

## Binding and persistence

Delay derivation inherits the accepted assessment binding:

- scope-qualified actor;
- `TaskId`;
- origin/destination;
- ordered connection identities;
- travel mode;
- coverage provenance identities;
- applicable delay-process identities/state where material.

Before departure the result may be recomputed. A departed plan must snapshot or immutably reference the duration-driving delay/coverage provenance so later content/state changes cannot rewrite elapsed history.

## Player/NPC symmetry

Controller identity cannot affect delay classification. HumanController and AIController with identical world facts get the same result. Neither may author `NoMaterialDelay`, bypass a represented wait process, or use Godot geometry as authority.

## Rights and feasibility

Delay assessment grants no passage rights and chooses no route. The order remains:

`knowledge + physical/mode feasibility + passage authorization -> concrete path -> delay/horizon assessment -> duration plan`.

A blocked/restricted route cannot become usable because it has continuous-passage coverage. An authorized route can still contain a delay process.

## Validation requirements

Later implementation must prove at least:

- exact-path complete coverage + no active delay process -> `NoMaterialDelay`;
- one uncovered connection -> `Unknown`;
- same endpoints but a different ordered path cannot reuse coverage;
- represented ferry/queue/controlled-stop process -> `MaterialDelayPresent`;
- `Passable/Open/BaselineLevelUnobstructed` without coverage remains `Unknown`;
- path/process changes recompute before departure;
- controller identity does not affect classification;
- delay derivation alone starts no travel;
- short horizon + `NoMaterialDelay` can make the existing five-dimension gate `Applicable` only if actor/load/route timing are also explicit baseline-compatible.

## Acceptance scenario

1. Person P has a selected task requiring place B.
2. One known, authorized, passable OnFoot path exists from A to B.
3. Every ordered path portion has explicit current continuous-passage coverage with provenance.
4. No applicable discrete delay process exists.
5. Engine derives `NoMaterialDelay`.
6. If horizon, actor, load and route timing are also baseline-compatible, the existing applicability gate may become `Applicable`.
7. This contract still emits no duration and starts no travel.
8. Removing coverage from one path portion changes delay to `Unknown`.
9. Adding a represented waiting process changes delay to `MaterialDelayPresent` for the simple baseline.
10. Player- and AI-controlled people produce the same result from the same facts.

## Assumptions and uncertainty

- No ferry, queue, toll/check, gate, obstruction, or repair delay amount is claimed.
- Exact historical prevalence of uninterrupted local routes is not claimed.
- Prototype reconstruction coverage is content authority, not evidence for one historical village.
- Continuous slowdown from crowding belongs to a timing model unless represented as an actual wait/queue process.

## Fixture boundary

Invalid shortcuts:

- every village path is delay-free because the map is small;
- all `Open` or `Passable` routes are delay-free;
- no delay mechanic means no delay;
- a bridge is always delay-free;
- an available ferry is delay-free;
- Godot/navmesh traversal proves authoritative delay state.

## Deferred complexity

Separate bounded work remains for production representation/implementation of coverage and delay processes, duration-plan snapshot, departure/progress/arrival, default village route graph/knowledge, canonical default-resident task generation, and dynamic ferries/gates/queues/congestion/weather/obstructions/non-OnFoot travel.
