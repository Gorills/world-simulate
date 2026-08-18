# P3 Baseline Short-Reference Horizon

Status: **REVIEW_REQUIRED**

This contract answers one narrow P3 model question:

**Can production derive `TraversalHorizon = BaselineShortReferenceCompatible` for a tightly bounded on-foot traversal without turning the accepted 30-minute rejection bound into the invalid inverse rule `<30 minutes => short-compatible`?**

It does **not** define a universal fatigue-free duration, sustained-journey pace, fatigue/recovery equation, rest schedule, traversal-delay producer, travel plan, departure, progress, reroute, cancellation or arrival.

## Mechanic

Provide a conservative positive producer for the short-reference horizon dimension by keeping the accepted `1400 mm/s` calibration inside a duration envelope that was actually exercised by short walking protocols, rather than extrapolating that calibration across the whole `<30 minute` gap.

## Intended feeling

A nearby trip may eventually receive a physically grounded duration when every other baseline condition is explicit, while medium and long trips continue to safe-fail instead of inheriting a convenient universal walking rule.

## Real-world process

The accepted baseline calibration is a short walking reference, not a sustained journey model. Human walking studies commonly measure preferred/comfortable gait over short bouts; longer exposure can introduce fatigue/endurance effects.

The horizon dimension therefore asks a limited evidence question:

`Does the planned traversal remain inside the time scale directly represented by the accepted short-reference evidence, or has duration itself outrun that evidence?`

This is separate from the actor-capability question. A five-minute path horizon does not make an impaired, fatigued, ill or otherwise non-baseline actor compatible; that remains the responsibility of the accepted actor-capability dimension.

## Accepted prerequisites

This contract reuses without broadening:

- `DESIGN/MODELS/P3_SEMANTIC_LOCATION_AND_TRAVEL.md`;
- `DESIGN/MODELS/P3_ON_FOOT_TRAVEL_DURATION_CALIBRATION.md`;
- `DESIGN/MODELS/P3_ON_FOOT_TRAVERSAL_APPLICABILITY.md`;
- `DESIGN/MODELS/P3_PLANNED_TRAVERSAL_ASSESSMENT_CONTEXT.md`;
- `DESIGN/MODELS/P3_TRAVERSAL_HORIZON_REJECTION_BOUND.md`.

The accepted order remains:

`selected task -> destination -> unique known/authorized/mode-feasible OnFoot path -> traversal assessment -> duration plan -> departure -> persistent progress -> arrival`

The accepted horizon states remain:

- `Unknown`;
- `BaselineShortReferenceCompatible`;
- `ProlongedOrEnduranceRelevant`.

The already accepted negative producer remains authoritative:

`reference_horizon_ms >= 1_800_000 -> ProlongedOrEnduranceRelevant`.

This contract only investigates a narrower positive producer at the opposite end of the duration range.

## Reference context

Historical settlement context remains **rural lowland England, approximately 1270–1348**, with **1350–1450** retained as a separate stress/validation regime.

The quantitative evidence here is modern biomechanics/exercise evidence used only to bound the duration horizon of the already accepted physical walking calibration. It is not evidence for medieval schedules, road quality, footwear, daily range, travel frequency or exact historical journey pace.

No claim is made that medieval people experienced no fatigue for five minutes. The simulation claim is narrower: for an actor/path already proven compatible on the other applicability dimensions, a reference duration no longer than the short protocols used to characterize ordinary preferred/comfortable walking does not extrapolate the calibration beyond its observed short-bout horizon.

## Evidence and sources

### 1. Browning et al. 2006 — the accepted calibration itself was exercised in five-minute walking trials

Raymond C. Browning, Emily A. Baker, Jessica A. Herron, Rodger Kram, “Effects of obesity and sex on the energetic cost and preferred speed of walking,” *Journal of Applied Physiology* 100(2), 390–398 (2006).

- DOI: https://doi.org/10.1152/japplphysiol.00767.2005
- Full text: https://journals.physiology.org/doi/full/10.1152/japplphysiol.00767.2005
- 39 adults, including normal-weight and class-II-obese women and men.
- Preferred walking speed was measured over repeated comfortable overground trials and was approximately `1.42 m/s` across the reported groups.
- For treadmill energetic measurements, each speed trial allowed `3 minutes` to reach steady state and used the final `2 minutes` for metabolic averaging: a five-minute walking exposure per speed.
- Supports: the evidence already underlying `1400 mm/s` directly exercised short walking behavior on a five-minute protocol scale; keeping the reference-duration screen at or below five minutes avoids extrapolating duration beyond that measurement horizon.
- Does **not** establish: zero fatigue for every person, a medieval five-minute law, a sustained-journey pace, or permission to ignore actor/load/route/delay applicability.

### 2. Zheng et al. 2022 — five-minute preferred/normal overground walking is used across a broad healthy adult age span

Peixuan Zheng, Scott W. Ducharme, Christopher C. Moore, Catrine Tudor-Locke, Elroy J. Aguiar, “Classification of moderate-intensity overground walking speed in 21- to 85-year-old adults,” *Journal of Sports Sciences* 40(15), 1732–1740 (2022).

- DOI: https://doi.org/10.1080/02640414.2022.2103622
- PubMed: https://pubmed.ncbi.nlm.nih.gov/35876127/
- 248 generally healthy ambulatory adults aged 21–85 performed one `5-minute` self-paced overground trial in a level indoor hallway.
- Participants were instructed to walk at their normal/usual daily-life pace.
- Supports: five minutes is an independently used short observation window for ordinary self-paced overground walking across a broad adult age range, not only a young athletic sample.
- Does **not** establish: `1.4 m/s` for every age, absence of fatigability, medieval outdoor equivalence, or a positive horizon for actors whose capability is unknown/non-baseline.

### 3. Majed et al. 2024 — deliberately short preferred-speed trials explicitly control fatigue carryover

Lina Majed, Rony Ibrahim, Merilyn Jean Lock, Georges Jabbour, “Walking around the preferred speed: examination of metabolic, perceptual, spatiotemporal and stability parameters,” *Frontiers in Physiology* 15:1357172 (2024).

- DOI: https://doi.org/10.3389/fphys.2024.1357172
- PubMed: https://pubmed.ncbi.nlm.nih.gov/38405123/
- 34 young sedentary adults performed seven `3-minute` walking trials around individual preferred walking speed.
- Trials were separated by `3-minute` rest intervals stated to allow recovery and avoid fatigue effects.
- The authors explicitly describe their findings as short-term acute responses and note that duration matters for longer walking.
- Supports: the preferred-speed literature distinguishes deliberately short measurement bouts from longer exposures where fatigue/endurance can become material.
- Does **not** establish: `3 minutes` or `5 minutes` as a universal fatigue-free threshold.

### 4. Richardson et al. 2015 — actor capability remains load-bearing even inside five minutes

Catherine A. Richardson, Nancy W. Glynn, Luigi G. Ferrucci, Dawn C. Mackey, “Walking Energetics, Fatigability, and Fatigue in Older Adults: The Study of Energy and Aging Pilot,” *Journal of Gerontology: Series A* 70(4), 487–494 (2015).

- DOI: https://doi.org/10.1093/gerona/glu146
- PubMed: https://pubmed.ncbi.nlm.nih.gov/25190069/
- 36 adults aged 70–89 completed `5-minute` treadmill walking tests at standard and preferred gait speeds.
- Slower walkers had lower aerobic capacity, higher energetic cost and greater reported fatigability.
- Supports: a five-minute horizon cannot replace the actor-capability dimension; individual capability/condition can be material even within a short bout.
- Does **not** refute use of five minutes as a protocol-bound duration envelope for actors independently established as baseline-compatible.

## Evidence conclusion

The evidence does **not** support a universal proposition:

`walking <= 5 minutes -> fatigue impossible`.

It does support a narrower proposition that matches the accepted applicability architecture:

- the `1400 mm/s` calibration is derived from ordinary preferred/comfortable walking evidence exercised on short protocol scales, including five-minute walking trials;
- an independent large overground study uses a five-minute normal/usual walking trial across adults aged 21–85;
- short preferred-speed studies explicitly distinguish their acute short-bout scope from longer-duration behavior;
- individual fatigability remains a separate actor fact and therefore must not be hidden in the horizon producer.

Therefore five minutes can be used as a **positive calibration-observation bound**, not as a universal physiological fatigue threshold.

## Decision

For an already constructed concrete OnFoot traversal assessment, compute the same accepted reference-horizon screen:

`reference_horizon_ms = ceil(path_distance_m * 1_000_000 / 1400)`.

Then classify duration horizon as follows:

- if `reference_horizon_ms <= 300_000` (`5 minutes`), derive `TraversalHorizon = BaselineShortReferenceCompatible`;
- if `300_000 < reference_horizon_ms < 1_800_000`, keep `TraversalHorizon = Unknown`;
- if `reference_horizon_ms >= 1_800_000`, use the already accepted `TraversalHorizon = ProlongedOrEnduranceRelevant` rejection producer.

At exactly `1400 mm/s`, five minutes corresponds to:

`420 m`.

`420 m` is only the distance representation of the five-minute calibration-observation envelope under the accepted reference rate. It is **not** a historical trip-distance rule and does not say that every 421 m trip is meaningfully different in human physiology.

The intentional middle gap remains:

`420 m < reference-equivalent extent < 2520 m -> Unknown`.

That gap is evidence-preserving, not an implementation defect.

## Why five minutes is not a universal fatigue threshold

The classification name is `BaselineShortReferenceCompatible`, not `FatigueImpossible`.

The positive producer says only that **duration itself** has not exceeded the directly exercised short-bout scale used to characterize the reference calibration.

The full applicability decision still independently requires:

- actor capability `BaselineCompatible`;
- carried load `NoMaterialLoad`;
- every route connection `BaselineLevelUnobstructed`;
- traversal delay `NoMaterialDelay`;
- `OnFoot` mode and the accepted concrete assessment binding.

If actor capability is `Unknown` or `NonBaseline`, a five-minute route does not become `Applicable` merely because its horizon is short-reference compatible.

## Causal model

`selected task`

`+ exact destination`

`+ exact unique known/authorized/mode-feasible OnFoot path`

`+ authoritative total path extent`

`+ accepted 1400 mm/s calibration identity`

`-> reference horizon screen`

`-> <= 5 min: BaselineShortReferenceCompatible`

`-> >5 min and <30 min: Unknown`

`-> >=30 min: ProlongedOrEnduranceRelevant`

`-> existing five-dimension applicability gate`

`-> only if every dimension is explicit baseline-compatible: Applicable`

No duration plan or departure follows from the horizon classification by itself.

## Assessment binding and invalidation

The positive horizon result inherits the accepted traversal-assessment binding:

- scope-qualified actor identity;
- selected `TaskId`;
- origin;
- destination;
- ordered route connection identities;
- travel mode;
- calibration/version identity.

Changing actor/task/path/mode/extent/calibration invalidates the prior pre-departure horizon result.

The result is not a permanent resident trait, route property or destination property.

## Player/NPC symmetry

Controller identity does not enter the reference-horizon arithmetic.

For identical authoritative actor/task/path/mode/calibration facts:

- HumanController and AIController receive the same horizon classification;
- neither controller can convert the middle `Unknown` band into a favorable result;
- neither controller can bypass an actor/load/route/delay incompatibility;
- Godot locomotion speed cannot change the authoritative screen.

## Ownership, rights and obligations

The short-reference horizon producer creates no passage, property, work, contract, building-entry or resource-use right.

The order remains:

`route knowledge + physical/mode feasibility + passage authorization -> concrete path -> timing assessment -> duration plan`.

A short path can still be closed, restricted, unknown, physically unsuitable or socially unauthorized.

## Player decision

No new player command or direct decision is introduced.

A human-controlled actor may choose an ordinary task or route through the same authoritative planning rules as an AI-controlled actor. The horizon classification is engine-derived from the resulting concrete path and cannot be selected as a UI option.

## Rules

1. The positive producer applies only to an exact concrete OnFoot path with authoritative total extent.
2. `reference_horizon_ms <= 300_000` derives `BaselineShortReferenceCompatible`.
3. `300_000 < reference_horizon_ms < 1_800_000` remains `Unknown`.
4. `reference_horizon_ms >= 1_800_000` remains governed by the accepted prolonged/endurance rejection model.
5. The positive bound is an observation/calibration envelope, not a universal fatigue-free duration.
6. Actor capability remains independently load-bearing; horizon cannot hide age, illness, impairment or current fatigue.
7. Carried load, route timing and traversal delay remain independently load-bearing.
8. Multi-edge paths use total authoritative ordered-path extent, not per-edge classification.
9. Straight-line endpoint distance and Godot/navmesh distance are not authoritative substitutes.
10. No favorable default is produced for missing path extent, missing calibration identity or an unbound assessment.
11. Horizon classification cannot create route knowledge, rights, duration, departure or arrival.
12. A future richer endurance model may narrow or replace the positive envelope; old departed plans must preserve their original duration-driving inputs.

## Long-horizon behavior

This contract starts no travel and consumes no simulation time, so it does not establish a settlement-scale economic/demographic law and does not require a new ten-year run for model acceptance.

Integrated travel implementation still requires later validation because even short-trip duration can alter labour capacity, task completion and economic timing.

## Assumptions and uncertainty

- Modern laboratory/indoor evidence is used as a physical calibration horizon, not as direct medieval journey evidence.
- Five minutes is chosen because it is directly represented in multiple ordinary/preferred walking protocols, including the accepted calibration source; it is not claimed as the onset of fatigue.
- The accepted `1400 mm/s` value remains a rounded baseline reference with real inter-person variation.
- A `BaselineCompatible` actor fact must continue to exclude represented conditions that make the baseline inappropriate even for a short traversal.
- Outdoor terrain, mud, footwear, weather, darkness, prior exertion and nutrition remain outside this horizon producer and belong to other applicability dimensions/models when material.
- The evidence does not currently justify extending the positive rule beyond five minutes. The `5–30 minute` interval deliberately remains `Unknown`.
- The evidence does not justify converting `420 m` into a historical settlement-distance norm.

## Fixture boundary

The following remain invalid shortcuts:

- `<30 minutes => BaselineShortReferenceCompatible`;
- `<2520 m => BaselineShortReferenceCompatible`;
- every current village route is short because the prototype map is small;
- every current resident is baseline-capable because tests say so;
- five minutes means fatigue is impossible;
- 420 m is a medieval commute-distance law;
- Godot walking speed or render distance determines authoritative horizon;
- the existing one-hour travel fixture proves any duration model.

Tests may use exact boundary distances to exercise the producer, but those values are calibration fixtures rather than historical route evidence.

## Falsifiers

Revise this contract if independent audit finds that:

- the accepted calibration source did not in fact exercise walking over the claimed five-minute trial horizon;
- five-minute preferred/normal walking protocols cannot support even a protocol-bound short-reference envelope;
- the horizon producer is interpreted or implemented as proof of zero fatigue;
- actor capability stops screening current age/health/fatigue conditions independently;
- implementation fills the `5–30 minute` gap with a favorable default;
- controller identity, Godot geometry or straight-line distance changes the result;
- a stronger accepted model establishes a different evidence-backed short-reference envelope.

## Feedback

Debug/audit projection may expose:

- authoritative path extent;
- reference-horizon milliseconds;
- resulting horizon class;
- calibration identity/version;
- the other applicability dimensions and final decision.

Player-facing UI may summarize expected travel time only after a later accepted duration plan exists. The horizon classification itself is primarily diagnostic/model evidence.

## Persistence

Before departure, the positive horizon result may be deterministically recomputed from the concrete assessment binding, authoritative path extent and accepted calibration identity.

A later departed travel plan must snapshot or immutably reference the duration-driving applicability/calibration inputs so content/model changes cannot rewrite elapsed history.

This contract adds no persistence field by itself.

## Input flow

No keyboard, mouse or gamepad flow is introduced.

A future player route/task choice uses ordinary controller intent; the horizon result is derived server/simulation-side and cannot be authored by input.

## Projection/UI

No Godot/UI authority change is introduced.

Godot may display projected horizon/applicability diagnostics but cannot author the route extent, horizon class or final applicability decision.

## Validation requirements for later implementation

A production implementation of this combined positive/negative horizon screen must test at least:

- exactly `420 m` at the accepted reference -> `BaselineShortReferenceCompatible`;
- `419 m` -> `BaselineShortReferenceCompatible`;
- `421 m` -> `Unknown`;
- a representative middle value remains `Unknown`;
- `2519 m` remains `Unknown`;
- exactly `2520 m` -> `ProlongedOrEnduranceRelevant`;
- greater than `2520 m` -> `ProlongedOrEnduranceRelevant`;
- no route/path -> no horizon assessment;
- multi-edge total extent drives classification;
- changing bound path/extent recomputes the classification;
- short horizon plus actor `Unknown` does not make the full applicability result `Applicable`;
- short horizon plus any explicit non-baseline actor/load/route/delay remains `NotApplicable`;
- player and AI controller identities do not alter classification;
- horizon derivation alone creates no duration or travel state.

## Acceptance scenario

1. An ordinary actor has a selected task requiring another place.
2. Exactly one known, authorized, passable OnFoot route path exists.
3. The concrete traversal assessment is bound to actor/task/origin/destination/ordered path/mode/calibration.
4. The path has authoritative total extent `<= 420 m` under the accepted `1400 mm/s` reference screen.
5. Engine derives `BaselineShortReferenceCompatible` for the horizon dimension.
6. Actor capability, carried load, route timing and traversal delay are still checked independently.
7. If every other dimension is explicitly baseline-compatible, the existing applicability gate may derive `Applicable`; this contract still emits no duration and starts no travel.
8. Increasing the same path to `421 m` changes only the horizon dimension to `Unknown`; production does not infer short compatibility.
9. Increasing the reference-equivalent path to `2520 m` derives `ProlongedOrEnduranceRelevant` under the accepted rejection model.
10. The same facts produce the same result for player-controlled and AI-controlled people.

## Deferred complexity

This task deliberately does not solve:

- a sustained pace/rest/fatigue model for the `5–30 minute` middle gap or longer travel;
- richer actor capability production from lifecycle/health/fatigue state;
- quantitative carried-load slowdown;
- terrain/surface/gradient/weather coefficients;
- traversal-delay production;
- travel-plan duration snapshot/persistence;
- departure/progress/interruption/reroute/cancellation/arrival;
- mounted/cart/water timing.

Those remain separate bounded tasks. This contract only defines a conservative positive short-reference horizon producer that can be independently audited before production implementation.
