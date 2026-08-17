# P3 On-Foot Traversal Applicability

Status: **REVIEW_REQUIRED**

This contract defines the authoritative boundary for deciding whether the already accepted `1400 mm/s` on-foot reference calibration may be used for a concrete traversal.

It does **not** add a universal resident walking speed, terrain/weather coefficients, injury/fatigue equations, carried-load slowdown coefficients, departure commands, route choice, or automatic travel.

## Mechanic

Convert independently known actor, load, route and delay facts into one explicit answer to a narrow question:

`May the accepted baseline on-foot duration calibration be applied to this planned traversal?`

The answer is not inferred from missing data and is not stored as an immutable character stat.

## Intended feeling

When travel duration becomes visible, the player should be able to trust that the simulation used a physical rule because the relevant world facts justified it, not because every person or every road was silently assigned the same speed.

## Real-world process

Human walking speed depends on the walker and traversal conditions. The accepted `DESIGN/MODELS/P3_ON_FOOT_TRAVEL_DURATION_CALIBRATION.md` establishes `1400 mm/s` only for an explicitly constrained `adult + level + unloaded + unimpaired + OnFoot` reference class.

The parent `DESIGN/MODELS/P3_SEMANTIC_LOCATION_AND_TRAVEL.md` requires duration to derive from route extent plus materially relevant mode, actor, load and environment inputs.

This contract specifies how those accepted limits become an authoritative planning gate without inventing coefficients for non-baseline conditions.

## Reference context and evidence inheritance

Historical world context remains rural lowland England c. 1270–1348, with 1350–1450 as a separate stress/validation regime.

No new historical-human behavior claim is introduced here. This contract inherits reviewed evidence from:

- `DESIGN/MODELS/P3_SEMANTIC_LOCATION_AND_TRAVEL.md` and its audit record: movement is ordinary, route/mode/access state matters, and one universal journey speed is unsupported;
- `DESIGN/MODELS/P3_ON_FOOT_TRAVEL_DURATION_CALIBRATION.md` and its audit record: `1400 mm/s` is accepted only as a narrow modern-biomechanics reference, while actor variation and carried load reject universal application.

The underlying load-bearing sources already audited for this boundary are Browning et al. 2006, Bohannon 1997 and Middleton et al. 2022. This contract does not broaden their claims.

## Causal model

The authoritative gate is:

`selected task + destination + unique known/open OnFoot route`

`+ actor baseline-capability fact`

`+ carried-load applicability fact`

`+ route/environment baseline-class fact`

`+ traversal-delay applicability fact`

`-> baseline applicability decision`

Only `Applicable` may feed the accepted `1400 mm/s` duration formula.

Any required fact that is `Unknown` produces `Unresolved`, not `Applicable`.

Any required fact that explicitly contradicts the baseline produces `NotApplicable`, not an invented fallback coefficient.

## Authoritative applicability dimensions

### 1. Actor capability

Planning needs an explicit current fact answering whether the actor is within the ordinary adult unimpaired reference class **for this calculation**.

Minimum semantic states:

- `Unknown` — production does not have enough authoritative information;
- `BaselineCompatible` — current authoritative content/state explicitly establishes the narrow baseline capability class;
- `NonBaseline` — known age, impairment, illness, injury, fatigue or another represented condition makes the baseline class inapplicable.

`BaselineCompatible` is not a permanent biological identity and must not be interpreted as a universal personal speed.

Until richer lifecycle/health mechanics provide the fact causally, fixture/content may assert it only with explicit provenance and fixture/reconstruction marking where appropriate.

### 2. Carried load

Planning needs an explicit current fact about **material carried load for this traversal**, not ownership of inventory in general.

Minimum semantic states:

- `Unknown`;
- `NoMaterialLoad`;
- `MaterialLoadPresent`.

Owned goods are not automatically carried goods. Conversely, absence of a carried-load mechanic must not be interpreted as `NoMaterialLoad`.

This contract accepts no slowdown coefficient for `MaterialLoadPresent`; that state makes the baseline unresolved for duration until a separately accepted model exists.

### 3. Route/environment class

Every connection used by baseline duration must explicitly establish that its represented traversal conditions fit the accepted reference class.

Minimum semantic states:

- `Unknown`;
- `BaselineLevelUnobstructed`;
- `NonBaseline`.

`Passable` and `Supports OnFoot` are insufficient by themselves. They answer physical/mode feasibility, not whether level/unobstructed reference timing is justified.

Absence of terrain/surface/gradient/weather fields therefore remains `Unknown`, not `BaselineLevelUnobstructed`.

A multi-connection path is baseline-compatible only if **every** connection is explicitly baseline-compatible for this timing calculation.

### 4. Traversal delays/stops

Planning needs an explicit fact that no separately represented stop or traversal delay invalidates the simple distance/speed reference calculation.

Minimum semantic states:

- `Unknown`;
- `NoMaterialDelay`;
- `MaterialDelayPresent`.

Examples that may later produce `MaterialDelayPresent` include ferry waiting, queueing, known stop requirements or other modeled traversal delay. This contract does not invent their duration.

## Derived decision

The duration-planning gate has three outcomes:

- `Applicable` — all required dimensions explicitly match the accepted baseline class;
- `Unresolved` — at least one required dimension is unknown and none explicitly contradicts the baseline;
- `NotApplicable` — at least one required dimension is explicitly non-baseline/materially incompatible.

`Applicable` must be derived at planning time from authoritative facts. It must not be a user-editable or client-authored shortcut.

A boolean such as `UseBaselineSpeed=true` without the supporting dimension facts is insufficient authoritative state.

## Planning snapshot versus mutable source facts

The source facts may later change. A future accepted travel plan therefore needs to persist either:

1. the duration-driving applicability snapshot used at departure, including provenance/version needed for deterministic replay; or
2. an immutable calibration/input reference that reconstructs the same result without consulting later-mutated content.

After departure, changing a resident health/load field or route content must not silently rewrite elapsed history or retroactively change the originally planned duration. Interruption/reroute rules decide whether a new plan is needed.

## Player/NPC symmetry

HumanController and AIController use the same applicability dimensions and derived decision.

Controller type cannot:

- turn `Unknown` into `Applicable`;
- ignore a material load or impairment;
- treat Godot animation speed as authoritative capability;
- bypass a non-baseline route condition;
- receive a private faster baseline.

The same actor/route/load/delay facts produce the same planning result regardless of controller.

## Rights and authorization

This gate changes no passage, property, work, contract or resource-use rights.

The order remains:

`known route + physical/mode feasibility + passage authorization -> timing applicability -> duration plan`

A baseline-compatible route is not automatically open or authorized. An authorized route is not automatically baseline-compatible for timing.

## Rules

1. Missing applicability data never implies baseline compatibility.
2. `Passable` and `OnFoot` support do not imply level/unobstructed timing conditions.
3. Inventory ownership does not imply carried load; missing carried-load state does not imply unloaded.
4. Actor baseline capability is current traversal input, not a permanent `WalkingSpeed` resident stat.
5. Non-baseline states do not receive convenient fallback coefficients in this contract.
6. Multi-edge paths require every timing-relevant connection to satisfy the baseline route/environment class.
7. The derived applicability decision is engine-side authoritative; Godot may display it but cannot create it.
8. No departure occurs merely because applicability becomes `Applicable`.
9. No current prototype `one hour` duration is used as fallback when applicability is unresolved.

## Current production gap

At the time of this contract:

- `ResidentState` has hunger, energy, activity, profession, household/workplace references, semantic location and selected task, but no accepted actor baseline-capability or carried-load applicability state;
- `SettlementRouteConnectionState` has endpoints, distance, physical state, passage status, provenance and supported modes, but no route/environment timing class;
- route projection can produce a unique known/open `OnFoot` path, but cannot yet prove baseline timing applicability;
- legacy one-hour compatibility travel remains separate fixture behavior for residents without authoritative selected-task travel planning.

Therefore production must not derive `1400 mm/s` duration yet.

## Persistence

Any new authoritative applicability source fields must survive save/load when they are part of world state.

A later planned-travel snapshot must preserve enough inputs to replay the same duration deterministically. Optional backward-compatible fields require an explicit version/legacy boundary when omission would otherwise be confused with a current valid value.

## Validation and failure cases

Implementation must test at least:

- all dimensions explicit baseline-compatible -> `Applicable`;
- actor unknown -> `Unresolved`;
- carried load unknown -> `Unresolved`;
- route timing class unknown on any path edge -> `Unresolved`;
- delay state unknown -> `Unresolved`;
- explicit non-baseline actor/load/route/delay -> `NotApplicable`;
- missing data never emits baseline duration;
- save/load retains the same source facts/derived outcome;
- player/AI controller identity does not alter outcome;
- Godot/render coordinates are absent from authority.

## Long-horizon behavior

This applicability gate by itself does not change settlement economy or demography because it does not start travel or consume time.

A later duration/departure implementation can alter labour capacity and economic timing and therefore remains subject to its own P3 implementation audit and later integrated long-horizon requirements.

## Fixture boundary

Allowed only as explicit fixture/reconstruction input where needed to exercise the seam:

- asserting a resident is baseline-capable for a test/calibration scenario;
- asserting a route connection is `BaselineLevelUnobstructed` for a test/calibration scenario;
- asserting `NoMaterialLoad` or `NoMaterialDelay` for that bounded scenario.

Such fixture assertions must not be generalized into statements that all residents are unimpaired adults, all routes are level, no one carries goods, or no journey has stops.

## Falsifiers

Revise this model if:

- a required applicability dimension cannot be represented without conflating rights, knowledge or controller state;
- a future health/load/terrain model shows this three-state boundary loses a materially causal distinction needed before planning;
- implementation can emit baseline duration while any required dimension is unknown;
- fixture assertions leak into default canonical world content without evidence/provenance;
- save/load or replay can change a plan's duration because later source facts/calibration changed.

## Acceptance scenario

1. A resident has a selected task requiring another place.
2. Exactly one known, authorized, passable route path exists and supports `OnFoot`.
3. Actor capability is explicitly `BaselineCompatible`.
4. Carried-load applicability is explicitly `NoMaterialLoad`.
5. Every route connection is explicitly `BaselineLevelUnobstructed` for timing.
6. Traversal-delay applicability is explicitly `NoMaterialDelay`.
7. Engine derives `Applicable`.
8. Replacing any one required dimension with `Unknown` derives `Unresolved` and produces no baseline duration.
9. Replacing any one dimension with an explicit non-baseline state derives `NotApplicable` and produces no invented duration.
10. Player-controlled and AI-controlled instances with the same facts produce the same result.

## Deferred complexity

Deferred to later bounded tasks:

- lifecycle/health system that causally produces actor capability;
- carried inventory/load representation and quantitative slowdown;
- terrain/surface/gradient/weather classes and coefficients;
- long-duration fatigue/stops;
- mounted/cart/water timing;
- creation and persistence of a full travel plan;
- departure, progress, interruption, reroute, cancellation and arrival.

These remain separate blockers where material. This contract only defines the proof boundary that must be satisfied before the accepted narrow `1400 mm/s` reference can become authoritative duration.