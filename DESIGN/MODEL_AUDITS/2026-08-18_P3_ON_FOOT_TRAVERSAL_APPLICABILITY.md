# Audit — P3 On-Foot Traversal Applicability

Audit date: **2026-08-18**

Verdict: **PASS**

Model contract: `DESIGN/MODELS/P3_ON_FOOT_TRAVERSAL_APPLICABILITY.md`

Reviewed model SHA: `3014245c252e3d80a2c1463225543cc82309b971`

## Scope

Independent audit of the on-foot baseline applicability boundary only. No production travel duration, departure command, health/lifecycle system, carried-load system, terrain/weather coefficient, endurance threshold, mounted/cart/water timing, or route-choice rule is accepted by this audit.

The accepted parent contracts remain:

- `DESIGN/MODELS/P3_SEMANTIC_LOCATION_AND_TRAVEL.md`;
- `DESIGN/MODELS/P3_ON_FOOT_TRAVEL_DURATION_CALIBRATION.md`.

This audit asks whether production may safely decide when the accepted `1400 mm/s` short-reference on-foot calibration is applicable without turning missing actor/load/route/delay/endurance information into hidden defaults.

## Repository and CI

The exact reviewed SHA was branch/PR HEAD `3014245c252e3d80a2c1463225543cc82309b971` at audit time. That repair changed only `DESIGN/MODELS/P3_ON_FOOT_TRAVERSAL_APPLICABILITY.md`.

Required GitHub Actions on the reviewed SHA all passed:

- `ci #168` — success;
- `playable-prototype-gate #218` — success;
- `proof-a-measure #163` — success.

No production/runtime/test file was changed by the reviewed model repair.

## Load-bearing evidence re-check

### Browning et al. 2006 — preferred level walking reference, not universal journey pace

Re-checked Raymond C. Browning, Emily A. Baker, Jessica A. Herron and Rodger Kram, “Effects of obesity and sex on the energetic cost and preferred speed of walking,” _Journal of Applied Physiology_ 100(2), 390–398 (2006).

https://pubmed.ncbi.nlm.nih.gov/16210434/

The study measured 39 adults and reports preferred walking speed of `1.42 m/s`; metabolic measurements were performed during level treadmill walking across six speeds from 0.50 to 1.75 m/s.

Audit conclusion: the already accepted rounded `1.4 m/s` value remains defensible as a narrow level adult reference. This study does not establish a sustained all-day, multi-kilometre, medieval outdoor or fatigue-independent journey pace.

### Bohannon 1997 — comfortable gait evidence is explicitly short-course

Re-checked Richard W. Bohannon, “Comfortable and maximum walking speed of adults aged 20–79 years: reference values and determinants,” _Age and Ageing_ 26(1), 15–19 (1997).

https://academic.oup.com/ageing/article/26/1/15/20634

The study used 230 healthy volunteers and timed gait over a `7.62 m` floor course. Mean comfortable gait speeds ranged approximately `127.2–146.2 cm/s`, with associations to age, height and lower-extremity strength.

Audit conclusion: the evidence independently supports a short/reference gait calibration and rejects a universal person-independent speed. It does not provide an endurance-duration threshold or justify extending the same rate indefinitely.

### Middleton et al. 2022 — traversal conditions and longer-duration uncertainty matter

Re-checked Kane Middleton et al., “Mechanical Differences between Men and Women during Overground Load Carriage at Self-Selected Walking Speeds,” _International Journal of Environmental Research and Public Health_ 19(7), 3927 (2022).

https://www.mdpi.com/1660-4601/19/7/3927

Thirty adults completed 10-minute overground trials with 0%, 20% and 40% body-mass loads. Increasing load reduced self-selected walking speed and changed gait mechanics. The paper also identifies prolonged load-carriage durations beyond its 10-minute protocol as a future-research need for understanding fatigue-relevant behavior.

Audit conclusion: material load must remain an applicability input, and the evidence does not support silently treating arbitrary prolonged travel as equivalent to the short/reference calibration case.

## Traversal-horizon repair review

**PASS.**

The repaired contract adds a fifth required dimension:

- `Unknown`;
- `BaselineShortReferenceCompatible`;
- `ProlongedOrEnduranceRelevant`.

This closes the material loophole in the original four-dimension draft, which could otherwise have allowed an arbitrarily long level/unloaded route to receive the accepted short-reference rate indefinitely.

The repair correctly refuses to invent a universal distance or time threshold. `BaselineShortReferenceCompatible` may only be supplied by an explicitly bounded calibration/test/content scenario with provenance until a later evidence-backed model can derive the horizon causally. Missing fatigue/endurance mechanics therefore remain `Unknown`, not implicit evidence that a route is short.

`ProlongedOrEnduranceRelevant` makes the short-reference baseline `NotApplicable`; this contract does not invent a sustained-pace substitute.

## Causal-model review

**PASS.**

Accepted gate topology:

`selected task + destination + unique known/open OnFoot route`

`+ actor capability + carried-load applicability + route/environment timing class + traversal-delay state + traversal horizon`

`-> Applicable | Unresolved | NotApplicable`

Only `Applicable` may feed the already accepted `1400 mm/s` reference formula.

Any required `Unknown` produces `Unresolved` unless another required fact explicitly makes the short-reference baseline inapplicable. Explicit non-baseline actor/load/route/delay/horizon facts produce `NotApplicable`, not a convenient fallback coefficient.

The derived decision is planning authority, not a stored `WalkingSpeed` character stat and not a client-authored `UseBaselineSpeed` switch.

## Rights and route-authority review

**PASS.**

The contract preserves separation between:

`known route + physical/mode feasibility + passage authorization -> timing applicability -> duration plan`.

Timing compatibility grants no passage, property, work, resource-use or destination-action right. Conversely, an authorized/passable `OnFoot` connection is not automatically level/unobstructed or short-reference compatible for timing.

## Player/NPC symmetry review

**PASS.**

HumanController and AIController use identical actor/load/route/delay/horizon facts and the same derived result. Controller identity cannot turn `Unknown` into `Applicable`, ignore load/impairment, bypass a non-baseline route, classify an arbitrary long player journey as short, or substitute Godot animation speed for simulation authority.

## Persistence and replay review

**PASS for model scope.**

Authoritative applicability source fields must persist when introduced. A future travel plan must snapshot the duration-driving applicability facts/provenance or an immutable input/calibration reference sufficient to reproduce the same duration without consulting later-mutated health/load/route/content state.

The contract correctly requires an explicit version/legacy boundary for optional persisted fields when omission could otherwise be confused with a valid current value.

## Fixture and uncertainty review

**PASS.**

Fixture assertions may exercise `BaselineCompatible`, `NoMaterialLoad`, `BaselineLevelUnobstructed`, `NoMaterialDelay`, and `BaselineShortReferenceCompatible`, but only as explicitly bounded/provenanced scenario facts. They must not become claims that all residents are healthy adults, all routes are level, nobody carries goods, no journey waits/stops, or every local route is short enough for the short-reference rate.

No universal short/prolonged threshold, load coefficient, terrain/weather coefficient, fatigue equation or sustained journey pace is accepted here.

## Long-horizon review

**PASS for this applicability-only contract.**

The model itself starts no travel and consumes no simulation time, so it does not change settlement economy/demography. A later duration/departure implementation can change labour capacity and economic timing and requires its own P3 implementation audit and later integrated long-horizon validation.

## Remaining blockers outside this contract

- production has no authoritative actor baseline-capability source;
- production has no carried-load applicability source;
- route connections have no accepted route/environment timing class;
- production has no causal short-versus-prolonged horizon derivation;
- full travel-plan persistence is not implemented;
- departure/progress/interruption/reroute/cancellation/arrival remain separate work;
- current one-hour compatibility travel remains noncanonical fixture behavior.

These gaps block automatic production application of the baseline but do not invalidate the accepted applicability boundary.

## Overall verdict

**PASS.**

The repaired five-dimension applicability model is conservative, evidence-compatible, causally separated from rights/controller/presentation state, persistence-aware and explicit about uncertainty. No remaining model blocker prevents promotion from `REVIEW_REQUIRED` to `ACCEPTED` in its declared scope.

`ACCEPTED` does **not** authorize production to calculate `1400 mm/s` duration for current residents/routes or to start travel. Production must first represent authoritative applicability facts and prove `Applicable` for the concrete bounded traversal.
