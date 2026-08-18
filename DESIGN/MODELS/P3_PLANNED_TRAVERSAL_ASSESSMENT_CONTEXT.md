# P3 Planned Traversal Assessment Context

Status: **ACCEPTED**

This contract defines the authoritative ownership and lifetime boundary for traversal-specific applicability facts that cannot correctly live as permanent actor or route properties.

It does **not** add travel duration, departure, progress, reroute, cancellation, arrival, a universal short-trip threshold, delay coefficients, or a controller command for travel.

## Narrow question

Where should the accepted on-foot applicability dimensions `TraversalDelay` and `TraversalHorizon` live, and what must bind them to a concrete traversal, before a later authoritative `TravelPlan` exists?

Deferred scope:

- how ordinary-world processes causally produce concrete ferry/queue/stop delays;
- an evidence-backed production derivation of short versus prolonged travel;
- duration calculation;
- departure and persistent travel progress;
- non-OnFoot timing.

## Accepted prerequisites

This contract reuses without broadening:

- `DESIGN/MODELS/P3_SEMANTIC_LOCATION_AND_TRAVEL.md`;
- `DESIGN/MODELS/P3_ON_FOOT_TRAVEL_DURATION_CALIBRATION.md`;
- `DESIGN/MODELS/P3_ON_FOOT_TRAVERSAL_APPLICABILITY.md`.

The accepted causal order remains:

`selected task/intention -> destination -> known/authorized/mode-feasible route -> timing applicability -> duration plan -> departure -> persistent progress -> arrival`

The accepted applicability dimensions remain:

1. actor capability;
2. carried load;
3. route/environment timing class;
4. traversal delay;
5. traversal horizon.

No new historical-human behavior claim is introduced here. This is an authoritative-state ownership contract for already accepted dimensions.

## Decision

`TraversalDelay` and `TraversalHorizon` are **planned-traversal assessment facts**.

They are not permanent properties of:

- a resident;
- the player actor;
- a route connection;
- a place;
- a controller;
- Godot geometry or animation state.

They belong to an engine-owned assessment of one concrete actor/task/path/mode combination.

Conceptually:

`current actor facts`

`+ selected task`

`+ exact destination`

`+ exact unique route path`

`+ travel mode`

`+ current traversal-specific delay/horizon source facts`

`-> TraversalAssessment`

`-> baseline applicability decision`

A later accepted `TravelPlan` may snapshot the assessment inputs that actually drove duration.

## Why actor ownership is wrong

A person can have two different traversals with different delay and horizon facts without changing identity.

Examples:

- the same actor may take a short nearby footpath and later a prolonged journey;
- the same actor may encounter no separately modeled stop on one traversal and a ferry/queue/required stop on another.

Therefore `NoMaterialDelay` and `BaselineShortReferenceCompatible` must not become resident/player defaults or personal traits.

Actor-owned capability and current carried-load facts remain separate accepted inputs.

## Why route-connection ownership is insufficient

A route connection can contribute authoritative physical/environment facts, but traversal delay and horizon are not generally immutable connection attributes.

A connection may be traversed:

- as part of a short path or a prolonged multi-edge path;
- under a currently represented stop/wait process or without one;
- by different planned traversals whose relevant conditions differ.

Therefore a static route field such as `AlwaysNoMaterialDelay=true` or `AlwaysShortReferenceCompatible=true` is not sufficient authority for the traversal-wide dimensions.

Route connections may later provide causal **source facts** used by a planner, but the derived delay/horizon applicability belongs to the concrete traversal assessment.

## Assessment binding

A traversal assessment is valid only for the concrete planning candidate that produced it.

Minimum conceptual binding:

- authoritative actor reference;
- selected `TaskId`;
- origin place;
- destination place;
- ordered route connection identities;
- travel mode;
- assessment source/provenance identity for any explicit delay or horizon fact.

The binding must be strong enough that an assessment for one path cannot be reused for another path merely because origin and destination are equal.

Changing any load-bearing binding input requires re-assessment before duration planning.

## Actor reference and player/NPC symmetry

Conceptually, actor identity is world/scope-qualified rather than controller-qualified.

A suitable semantic identity is:

`SimulationScopeId + EntityId`

or an equivalent authoritative actor reference that cannot collide across partitions.

HumanController versus AIController is not part of timing applicability.

The same actor/task/path/mode/source facts must produce the same assessment regardless of who controls the human.

## Delay assessment

The accepted states remain:

- `Unknown`;
- `NoMaterialDelay`;
- `MaterialDelayPresent`.

Rules:

1. No modeled delay producer does **not** imply `NoMaterialDelay`.
2. `NoMaterialDelay` requires an explicit authoritative source or an explicitly marked bounded fixture/reconstruction assertion.
3. `MaterialDelayPresent` may later be produced by modeled ferry waiting, queues, required stops, obstruction handling, or another accepted traversal process.
4. This contract accepts no numeric added-delay amount.
5. `MaterialDelayPresent` makes the narrow simple baseline duration `NotApplicable`; it does not authorize a fallback duration.

## Horizon assessment

The accepted states remain:

- `Unknown`;
- `BaselineShortReferenceCompatible`;
- `ProlongedOrEnduranceRelevant`.

Rules:

1. Route distance alone does not currently imply either non-unknown horizon state.
2. This contract introduces no universal distance/time threshold.
3. `BaselineShortReferenceCompatible` requires an authoritative bounded-scenario/content source with provenance until a separately accepted production derivation exists.
4. `ProlongedOrEnduranceRelevant` requires an accepted source establishing that sustained pace, fatigue, rest/stops, or endurance calibration materially matters.
5. Missing endurance mechanics do not make a path short-reference compatible.

## Source facts versus derived assessment

The assessment is derived engine output, not a user-authored boolean.

A client/controller must not submit:

- `UseBaselineSpeed = true`;
- `TraversalDelay = NoMaterialDelay`;
- `TraversalHorizon = BaselineShortReferenceCompatible`;
- `Decision = Applicable`;

as an unrestricted authoritative shortcut.

Future commands may create or change real world facts that a planner consumes, but the planner derives the assessment from those facts.

## Current production boundary

Current production already has authoritative sources for:

- resident actor capability;
- resident carried-load applicability;
- route/environment timing class;
- selected task/destination;
- unique known/open OnFoot route path.

Current production does **not** yet have accepted causal producers for traversal delay or traversal horizon.

Therefore the current engine behavior of projecting these two dimensions as `Unknown` is the correct safe-fail boundary.

The partial applicability projection may emit:

- `Unresolved` when no explicit incompatibility is known;
- `NotApplicable` when an already represented actor/load/route fact contradicts the baseline.

It must not emit production `Applicable` until authoritative delay and horizon sources exist.

## Persistence and replay

Before departure, a derived assessment need not become a second mutable source of truth if it can be deterministically reconstructed from current authoritative source facts.

If explicit traversal-specific source facts are stored in world state, those source facts must survive save/load with their binding and provenance.

Once a later `TravelPlan` is accepted and departure occurs, the duration-driving assessment inputs must be snapshotted or immutably referenced so later mutation of actor/load/route/content facts cannot rewrite elapsed history.

This contract does not yet define that final travel-plan persistence shape.

## Invalidation

A pre-departure assessment is stale and must be recomputed or ignored when any load-bearing planning input changes, including:

- selected task identity;
- actor identity or scope;
- origin/destination;
- ordered path connection identities;
- travel mode;
- actor capability;
- carried-load fact;
- route timing class on any used connection;
- traversal-specific delay source;
- traversal-horizon source.

No stale assessment may authorize duration/departure.

## Rights and route authority

Assessment ownership does not change passage rights or route choice.

The order remains:

`knowledge + route physical/mode feasibility + passage authorization -> concrete route path -> traversal assessment -> duration plan`

A timing assessment cannot:

- open a restricted route;
- make a blocked connection passable;
- choose between multiple feasible routes;
- create route knowledge;
- create a selected task.

## Fixture boundary

Until causal delay/horizon producers exist, tests or bounded reconstruction scenarios may explicitly supply traversal-specific source facts only when:

- they are marked as fixture/reconstruction input;
- provenance is non-empty;
- the assertion is bound to the concrete actor/task/path/mode assessment;
- it does not become default settlement content;
- it does not generalize into a universal threshold or no-delay assumption.

Fixture assertions may exercise the already accepted applicability truth table, but they do not establish a production historical rule.

## Validation requirements for later implementation

A production representation implementing this contract must test at least:

- assessment binds to exact actor/task/path/mode;
- same origin/destination but different ordered path cannot reuse the assessment;
- task replacement invalidates old assessment;
- route timing mutation changes the derived result before departure;
- absent delay producer stays `Unknown`;
- absent horizon producer stays `Unknown`;
- explicit fixture source requires provenance;
- player and AI actor references obey identical rules;
- save/load retains any persisted traversal-specific source facts and binding;
- no assessment starts travel;
- no assessment invents duration;
- Godot/client cannot author the derived applicability decision.

## Acceptance scenario

1. An actor has a selected task requiring another place.
2. Exactly one known, authorized, passable OnFoot route path exists.
3. Actor capability, carried load and every route timing class are authoritative.
4. Engine constructs the concrete assessment binding from actor/task/path/mode.
5. No accepted delay/horizon producers exist, so those dimensions remain `Unknown`.
6. Derived applicability is `Unresolved` unless another explicit dimension already makes it `NotApplicable`.
7. Replacing the task or route invalidates the previous assessment binding.
8. No duration is emitted and no departure occurs.

## Falsifiers

Revise this model if:

- a traversal-wide delay or horizon fact can only be represented by making it a permanent actor/controller property;
- a static route flag can silently classify arbitrary multi-edge journeys as short/no-delay;
- an assessment can survive a changed task/path and still authorize planning;
- a client-authored assessment bypasses authoritative source facts;
- save/load changes an explicit traversal-specific source fact or its binding;
- production emits `Applicable` while delay or horizon remains unsupported/unknown.

## Deferred blockers

This contract deliberately leaves two production blockers unresolved:

1. **Traversal-delay producer model** — which world processes can establish `NoMaterialDelay` or `MaterialDelayPresent`, and with what temporal scope/provenance?
2. **Traversal-horizon producer model** — what evidence-backed rule can classify an ordinary production traversal as short-reference compatible versus endurance-relevant without inventing a universal threshold?

Until those are separately modeled and accepted, current production must keep their values `Unknown` and must not derive the accepted `1400 mm/s` duration for ordinary live travel.
