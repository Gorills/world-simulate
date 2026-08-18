# P3 Traversal Horizon Rejection Bound

Status: **REVIEW_REQUIRED**

This contract answers one narrow question inside the accepted planned-traversal assessment model:

**Can production derive `ProlongedOrEnduranceRelevant` for some ordinary on-foot traversal candidates from authoritative route extent and the already accepted `1400 mm/s` short-reference calibration, without inventing a reciprocal universal "short trip" threshold?**

It does **not** define a general `BaselineShortReferenceCompatible` producer, sustained-journey speed, fatigue equation, rest schedule, delay model, travel duration plan, departure, progress, reroute, cancellation or arrival.

## Accepted prerequisites

This contract reuses without broadening:

- `DESIGN/MODELS/P3_SEMANTIC_LOCATION_AND_TRAVEL.md`;
- `DESIGN/MODELS/P3_ON_FOOT_TRAVEL_DURATION_CALIBRATION.md`;
- `DESIGN/MODELS/P3_ON_FOOT_TRAVERSAL_APPLICABILITY.md`;
- `DESIGN/MODELS/P3_PLANNED_TRAVERSAL_ASSESSMENT_CONTEXT.md`.

The accepted order remains:

`selected task -> destination -> unique known/authorized/mode-feasible route -> traversal assessment -> duration plan -> departure -> persistent progress -> arrival`

The accepted horizon states remain:

- `Unknown`;
- `BaselineShortReferenceCompatible`;
- `ProlongedOrEnduranceRelevant`.

No missing fact may become a favorable default.

## Reference context

Historical settlement context remains rural lowland England c. 1270–1348, with 1350–1450 as a separate stress regime.

The quantitative evidence below is modern biomechanics/exercise evidence used only to constrain human walking duration/fatigue plausibility. It is not evidence for medieval schedules, roads, footwear, daily ranges or exact historical journey pace.

## Evidence ledger

### 1. Browning et al. 2006 — accepted short-reference speed, not sustained journey pace

Raymond C. Browning, Emily A. Baker, Jessica A. Herron, Rodger Kram, “Effects of obesity and sex on the energetic cost and preferred speed of walking,” *Journal of Applied Physiology* 100(2), 390–398 (2006).

- DOI: https://doi.org/10.1152/japplphysiol.00767.2005
- 39 adults; level treadmill walking; preferred speed approximately `1.42 m/s`.
- Already supports the accepted rounded `1400 mm/s` reference.
- Does not establish that the same rate is fatigue-independent over prolonged journeys.

### 2. Majed et al. 2024 — preferred-speed physiology is measured in short bouts

Lina Majed, Rony Ibrahim, Merilyn Jean Lock, Georges Jabbour, “Walking around the preferred speed: examination of metabolic, perceptual, spatiotemporal and stability parameters,” *Frontiers in Physiology* 15:1357172 (2024).

- DOI: https://doi.org/10.3389/fphys.2024.1357172
- 34 young sedentary adults performed seven `3-minute` treadmill trials around individual preferred walking speed, with `3-minute` rests.
- Supports: preferred-speed metabolic/perceptual/gait measurements can be characterized in deliberately short experimental bouts.
- Does **not** establish `3 minutes` as a universal safe/no-fatigue threshold or justify classifying every traversal below any chosen duration as short-reference compatible.

### 3. Ten-minute preferred-speed protocols do not establish a universal short boundary

“Assessment of diurnal variation of stride time variability during continuous, overground walking in healthy young adults” (2020).

- PubMed: https://pubmed.ncbi.nlm.nih.gov/32387809/
- 31 healthy young adults completed two `10-minute` continuous overground trials at preferred walking speed.
- Average walking speed and average stride time did not differ significantly between the morning and afternoon sessions.
- Supports: ten-minute preferred-speed continuous walking is a plausible experimental observation window in healthy young adults.
- Does **not** prove absence of fatigue within every ten-minute traversal, does not cover all baseline-compatible adults, and does not justify a universal `<10 min => short` rule.

### 4. Kwon et al. 2023 — thirty minutes at preferred speed is fatigue-relevant even in healthy young adults

Yujin Kwon, Lillian K. Chilton, Hoon Kim, Jason R. Franz, “The effect of prolonged walking on leg muscle activity patterns and vulnerability to perturbations,” *Journal of Electromyography and Kinesiology* 73:102836 (2023).

- DOI: https://doi.org/10.1016/j.jelekin.2023.102836
- 18 healthy young adults walked for `30 minutes` at preferred walking speed.
- The study reports time-dependent neuromuscular changes interpreted as local muscle fatigue.
- Post-walk mediolateral center-of-mass displacement under perturbation averaged about `21%` larger than before the walk.
- Supports: by a 30-minute comfortable/preferred-speed bout, fatigue/endurance effects can already be materially relevant in healthy young adults.
- Does not establish that fatigue starts exactly at minute 30.

### 5. Thomas et al. 2017 — gait dynamics can change during thirty minutes of walking

Kathleen S. Thomas et al., “The impact of speed and time on gait dynamics,” *Human Movement Science* 54 (2017).

- DOI: https://doi.org/10.1016/j.humov.2017.06.003
- 14 young adults walked `30 minutes` at preferred walking speed, 90% PWS and 80% PWS.
- Spatiotemporal measures were assessed over successive five-minute intervals; several measures changed during the 30-minute walking exposure.
- Supports: duration itself can matter over a 30-minute walking bout and must not be assumed irrelevant merely because speed is near preferred.

### 6. Yoshino et al. 2004 — prolonged free walking clearly produces fatigue-related adaptation

Kohzoh Yoshino, Tomoko Motoshige, Tsutomu Araki, Katsunori Matsuoka, “Effect of prolonged free-walking fatigue on gait and physiological rhythm,” *Journal of Biomechanics* 37(8), 1271–1280 (2004).

- DOI: https://doi.org/10.1016/j.jbiomech.2003.11.031
- 12 normal subjects walked for `3 hours` at preferred pace.
- The study reports subjective fatigue and fatigue-related EMG/gait-rhythm changes in the more fatigable subgroup.
- Supports: prolonged walking cannot safely inherit an indefinitely constant short-reference pace.

## Evidence conclusion

The evidence supports a **one-sided conservative conclusion**:

- a 30-minute preferred/comfortable walking exposure is already long enough for fatigue-related neuromuscular effects to be materially plausible in healthy young adults;
- longer free-walking exposure strengthens that conclusion;
- the evidence does **not** establish a reciprocal universal duration below which fatigue/endurance is guaranteed irrelevant.

Therefore production may adopt a conservative **rejection bound** for the short-reference calibration, but may not infer short-reference compatibility merely because a candidate falls below that bound.

## Decision

Define an evidence-backed one-sided horizon producer for otherwise baseline-candidate on-foot traversals.

Conceptually compute a **reference horizon screen**, not a travel plan duration:

`reference_horizon_ms = ceil(path_distance_m * 1_000_000 / 1400)`

The same accepted integer arithmetic is reused only as a screening transformation between authoritative route extent and the already accepted `1400 mm/s` reference.

Then:

- if `reference_horizon_ms >= 1_800_000` (`30 minutes`), derive `TraversalHorizon = ProlongedOrEnduranceRelevant`;
- if `reference_horizon_ms < 1_800_000`, do **not** derive `BaselineShortReferenceCompatible`; leave horizon `Unknown` unless another accepted, provenance-bound source explicitly establishes the short-reference case.

Equivalent distance at exactly `1400 mm/s` is:

`2520 m`.

This value is a derived implementation convenience for the one-sided screen, not a historical journey-distance law.

## Why the rule is one-sided

The evidence supports that thirty minutes can already be fatigue-relevant.

It does not support the inverse proposition:

`less than 30 minutes -> fatigue irrelevant`.

Therefore the following rule is forbidden:

`reference_horizon_ms < 30 min -> BaselineShortReferenceCompatible`.

The gap below the rejection bound remains intentionally conservative.

## Why using the accepted reference speed for screening is not departure/duration authorization

The screen answers only:

`Is this candidate definitely outside the narrow short-reference horizon on duration evidence alone?`

It does not answer:

`What is the actual journey duration?`

The screen may be evaluated only after the planner already has:

- an exact authoritative route path and extent;
- `OnFoot` mode;
- an actor/path planning candidate bound under the accepted traversal-assessment context.

For the narrow baseline-duration path, actor capability, carried load and route timing must still be independently evaluated by the accepted applicability gate.

If other facts already make the baseline `NotApplicable`, the horizon screen does not invent a substitute duration.

If another condition would make actual walking slower, using `1400 mm/s` for this rejection screen is conservative: a candidate whose reference movement time is already at least thirty minutes cannot become shorter in the unsupported slower case merely to recover short-reference eligibility.

No inference about actual non-baseline pace is accepted.

## Causal model

`selected task`

`+ exact destination`

`+ exact unique authorized OnFoot path`

`+ authoritative path extent`

`+ accepted calibration identity (1400 mm/s)`

`-> reference horizon screen`

`-> if >= 30 min: ProlongedOrEnduranceRelevant`

`-> otherwise: Unknown unless another accepted short-reference source exists`

`-> existing five-dimension applicability decision`

No duration plan or departure follows from the horizon producer alone.

## Binding and invalidation

The derived horizon belongs to the concrete traversal assessment and inherits the accepted binding:

- scope-qualified actor identity;
- selected `TaskId`;
- origin;
- destination;
- ordered route connection identities;
- travel mode;
- calibration/version identity used by the screen.

Changing any path connection, path extent, task, actor, origin/destination or mode invalidates the prior horizon assessment.

The derived `ProlongedOrEnduranceRelevant` result must not be cached as a permanent resident or route trait.

## Multi-edge paths

The screen uses the authoritative total extent of the exact ordered route path.

It must not:

- classify each edge independently and then treat a sequence of individually short edges as a short journey;
- ignore repeated/looping extent;
- use straight-line endpoint distance instead of authoritative path extent;
- use Godot navigation/render length as the simulation source of truth.

## Route rights and knowledge

The horizon producer runs only after route knowledge, physical/mode feasibility and passage authorization produce the concrete candidate path.

It cannot:

- open a closed route;
- grant passage rights;
- choose among multiple feasible paths;
- create route knowledge;
- create a destination or selected task.

## Player/NPC symmetry

The same path extent and accepted calibration identity produce the same reference horizon screen regardless of controller.

Player control cannot lower the screen value, bypass `ProlongedOrEnduranceRelevant`, or substitute Godot locomotion speed.

AI control cannot receive a hidden slower/faster threshold.

## Persistence and replay

Before departure, the horizon result may be recomputed from current authoritative path binding and the accepted calibration identity.

A future travel plan that actually departs must snapshot or immutably reference the duration-driving applicability inputs/calibration as already required by the accepted parent contracts.

If the calibration identity changes in a future accepted model, old departed plans must not be retroactively re-screened from new content.

This contract adds no persistence field by itself.

## Fixture/content short-reference source

`BaselineShortReferenceCompatible` remains available only through an independently accepted source under the existing applicability/assessment contracts.

Until a general positive producer is accepted, bounded tests/reconstructions may explicitly assert it only when:

- provenance is non-empty;
- the assertion is bound to the exact actor/task/path/mode assessment;
- the scenario is explicitly short-reference/calibration content;
- it does not become a default for every route below 30 minutes or 2520 m.

This contract does not promote such assertions into a general live-world rule.

## Validation requirements for later implementation

A production implementation of this narrow rejection producer must test at least:

- exact 30-minute reference screen -> `ProlongedOrEnduranceRelevant`;
- greater than 30 minutes -> `ProlongedOrEnduranceRelevant`;
- just below 30 minutes -> `Unknown`, not `BaselineShortReferenceCompatible`;
- 2520 m at 1400 mm/s maps to exactly 30 minutes;
- shorter path remains `Unknown` without an explicit short-reference source;
- ordered multi-edge extent is summed from authoritative route data;
- changing path/extent invalidates/recomputes the result;
- Godot geometry is not consulted;
- player and AI controller types do not alter the result;
- horizon derivation alone does not create duration or start travel.

## Long-horizon behavior

This contract itself starts no travel and consumes no simulation time, so no new settlement-scale economic/demographic run is required for model acceptance.

A later duration/departure implementation still requires integrated validation because travel time can alter labour capacity, task completion and settlement economics.

## Uncertainty and limitations

- The 30-minute evidence is modern and primarily from healthy young adults; it is used conservatively to reject extension of the short-reference calibration, not to reconstruct medieval endurance.
- `30 minutes` is not asserted as the moment fatigue begins.
- `2520 m` is not a universal human or medieval trip boundary; it is the distance corresponding to the accepted reference rate over the evidence-backed rejection duration.
- Evidence for 3–10 minute preferred-speed protocols does not establish a reciprocal general short-safe boundary.
- Outdoor terrain, footwear, weather, darkness, nutrition and accumulated prior fatigue remain outside this narrow producer.
- A richer endurance model may later reject some candidates much earlier or provide sustainable pace/rest behavior.

## Falsifiers

Revise this model if:

- independent review finds the cited 30-minute preferred/comfortable walking studies do not support fatigue/endurance relevance;
- a stronger accepted model demonstrates that the one-sided screen misclassifies baseline-candidate traversals;
- implementation turns `<30 minutes` into automatic short-reference compatibility;
- route straight-line distance or Godot geometry replaces authoritative path extent;
- the screen becomes an actual duration plan or departure shortcut;
- controller identity changes the horizon classification.

## Deferred blockers

This contract deliberately leaves unresolved:

1. a general positive production producer for `BaselineShortReferenceCompatible`;
2. sustained-journey walking pace after `ProlongedOrEnduranceRelevant`;
3. fatigue accumulation/recovery and planned rests;
4. traversal-delay producer;
5. travel-plan duration snapshot/persistence;
6. departure/progress/interruption/reroute/cancellation/arrival.

These remain separate bounded tasks.

## Acceptance scenario

1. An ordinary actor has a selected task requiring another place.
2. Exactly one known, authorized, passable route path exists and supports `OnFoot`.
3. The ordered path has authoritative total extent.
4. Engine computes the reference horizon screen from that extent and the accepted `1400 mm/s` calibration identity.
5. A path whose reference screen is at least 30 minutes receives `ProlongedOrEnduranceRelevant` and therefore cannot use the narrow short-reference duration formula.
6. A path whose screen is below 30 minutes remains `Unknown` unless another accepted provenance-bound short-reference source exists.
7. No actual travel duration is emitted and no departure occurs from this classification alone.
8. The same facts produce the same result for player-controlled and AI-controlled humans.
