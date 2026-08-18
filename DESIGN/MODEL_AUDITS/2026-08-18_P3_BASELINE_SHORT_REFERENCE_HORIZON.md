# Audit — P3 Baseline Short-Reference Horizon

Audit date: **2026-08-18**

Verdict: **PASS**

Model contract: `DESIGN/MODELS/P3_BASELINE_SHORT_REFERENCE_HORIZON.md`

Reviewed model SHA: `aa746dffc93d6001f970f1dbb2eb8f2773166d6d`

## Scope

Independent audit of the narrow positive producer for `TraversalHorizon = BaselineShortReferenceCompatible`.

The reviewed model proposes a conservative positive observation envelope at five reference minutes while preserving the already accepted one-sided 30-minute rejection bound.

This audit does **not** accept a universal fatigue-free duration, a sustained-journey pace, fatigue/recovery equations, rest schedules, traversal-delay production, a travel-plan duration snapshot, departure, progress, interruption, reroute, cancellation or arrival.

## Repository and CI

The exact reviewed SHA was branch/PR HEAD `aa746dffc93d6001f970f1dbb2eb8f2773166d6d` at audit time.

Required GitHub Actions on the reviewed SHA all passed:

- `ci #183` — success;
- `playable-prototype-gate #248` — success;
- `proof-a-measure #178` — success.

The reviewed commit added only `DESIGN/MODELS/P3_BASELINE_SHORT_REFERENCE_HORIZON.md`; no production/runtime/test file changed.

PR #4 had no inline review threads at audit time.

## Load-bearing claims re-checked

The audit independently re-checked the claims that matter to the five-minute positive envelope:

1. the accepted `1400 mm/s` calibration source actually exercised walking on a five-minute trial scale rather than only an instantaneous or very short measurement;
2. an independent ordinary overground study used a five-minute self-paced walking observation across a broad adult age range;
3. short preferred-speed research deliberately treats short bouts differently from longer exposure where duration/fatigue can matter;
4. five minutes cannot replace the separate actor-capability check because individual fatigability can already differ within a five-minute task;
5. none of the evidence establishes a universal rule that fatigue is absent for five minutes or that all walks shorter than 30 minutes are short-reference compatible.

All five claims were supported with the limitations stated below.

## Primary evidence re-check

### Browning et al. 2006 — accepted calibration source

Re-checked Raymond C. Browning, Emily A. Baker, Jessica A. Herron and Rodger Kram, “Effects of obesity and sex on the energetic cost and preferred speed of walking,” _Journal of Applied Physiology_ 100(2):390–398 (2006), DOI `10.1152/japplphysiol.00767.2005`.

Full text: https://journals.physiology.org/doi/full/10.1152/japplphysiol.00767.2005

The study included 39 adults and reported preferred walking speed of approximately `1.42 m/s` across the studied groups. Preferred speed was measured through repeated comfortable overground walking. For the treadmill energetic measurements at each tested speed, subjects were allowed three minutes to reach steady state and the final two minutes were averaged, giving a five-minute walking exposure per speed trial.

Audit conclusion: the evidence underlying the accepted `1400 mm/s` reference does directly include a five-minute walking trial scale. Using five reference minutes as an observation-envelope ceiling does not extend duration beyond that trial scale. This is an audit inference about calibration scope, not a claim that five minutes is physiologically fatigue-free.

### Zheng et al. 2022 — independent five-minute overground observation

Re-checked Peixuan Zheng et al., “Classification of moderate-intensity overground walking speed in 21- to 85-year-old adults,” _Journal of Sports Sciences_ 40(15):1732–1740 (2022), DOI `10.1080/02640414.2022.2103622`.

PubMed: https://pubmed.ncbi.nlm.nih.gov/35876127/

The study reports 248 healthy adults aged 21–85 completing a five-minute self-paced overground walking trial, with walking speed measured during that trial.

Audit conclusion: this independently supports five minutes as a real short observation window for ordinary self-paced overground walking across a broad adult age range. It does **not** establish `1.4 m/s` for every age or condition and does not prove absence of fatigue.

### Majed et al. 2024 — short-bout scope is deliberately protected from fatigue carryover

Re-checked Lina Majed et al., “Walking around the preferred speed: examination of metabolic, perceptual, spatiotemporal and stability parameters,” _Frontiers in Physiology_ 15:1357172 (2024), DOI `10.3389/fphys.2024.1357172`.

Full text: https://www.frontiersin.org/journals/physiology/articles/10.3389/fphys.2024.1357172/full

Thirty-four young sedentary adults completed seven three-minute walking trials around preferred speed. The trials were separated by three-minute rest periods explicitly intended to permit recovery and avoid fatigue effects. The paper also states that its findings concern short-term acute responses and that duration must be considered for longer walking.

Audit conclusion: this source supports keeping a strict distinction between short calibration bouts and sustained walking. It does **not** support turning three or five minutes into a universal no-fatigue threshold.

### Richardson et al. 2015 — actor capability remains separate

Re-checked Catherine A. Richardson et al., “Walking Energetics, Fatigability, and Fatigue in Older Adults: The Study of Energy and Aging Pilot,” _Journal of Gerontology: Series A_ 70(4):487–494 (2015), DOI `10.1093/gerona/glu146`.

PubMed: https://pubmed.ncbi.nlm.nih.gov/25190069/

Thirty-six adults aged 70–89 completed five-minute treadmill walking tests at standard and preferred gait speeds. Slower walkers had lower aerobic capacity, higher energetic cost and greater fatigability measures.

Audit conclusion: a five-minute duration envelope cannot by itself prove that an actor belongs to the baseline capability class. The reviewed model correctly keeps actor capability independent from the horizon classification.

## Evidence interpretation review

**PASS.**

The reviewed contract uses five minutes as a **calibration-observation envelope**, not as a universal physiological threshold.

The accepted positive rule is therefore deliberately limited:

- `reference_horizon_ms <= 300_000` -> `BaselineShortReferenceCompatible`;
- `300_000 < reference_horizon_ms < 1_800_000` -> `Unknown`;
- `reference_horizon_ms >= 1_800_000` -> `ProlongedOrEnduranceRelevant` under the already accepted rejection model.

The middle five-to-thirty-minute gap remains unknown. The audit found no evidence supporting a favorable default in that interval.

At the accepted reference rate, five minutes corresponds to `420 m`; this is only a derived representation of the observation envelope. It is not a medieval commute distance, a route-design rule or a physiological discontinuity at 421 metres.

## Causal logic review

**PASS.**

The horizon classification can only be derived for a concrete OnFoot traversal assessment after a selected task, destination and exact authoritative route path exist.

It reads authoritative total path extent and the accepted calibration identity. It cannot create a motive, task, destination, route knowledge, passage right, duration plan, departure or arrival.

The rule therefore preserves cause-before-state and does not introduce a time-of-day shortcut.

## Historical grounding review

**PASS for the declared calibration-only scope.**

Model context remains rural lowland England approximately 1270–1348, with 1350–1450 retained as a separate stress/validation regime through the accepted P3 travel contract.

This contract introduces no new claim about medieval schedules, road quality, footwear, daily journey range, travel frequency or historical fatigue onset. The quantitative sources are modern walking studies used only to bound the observation horizon of the already accepted physical reference calibration.

The independent evidence set includes Browning et al. 2006 and Zheng et al. 2022 for five-minute walking observation, with Majed et al. 2024 and Richardson et al. 2015 constraining the interpretation so the result is not promoted into a universal fatigue rule.

## Player/NPC symmetry review

**PASS.**

Controller identity is absent from the horizon arithmetic. Identical actor/task/path/mode/calibration facts yield the same horizon result for player-controlled and AI-controlled people.

Neither controller may turn the five-to-thirty-minute `Unknown` band into a favorable result or use Godot locomotion/render state as timing authority.

## Ownership, rights and obligations review

**PASS.**

The horizon classification creates no right to use a road, enter property, perform work, consume a resource or act at the destination.

Route knowledge, physical/mode feasibility and passage authorization remain upstream requirements. Timing compatibility remains separate from social/property authorization.

## Uncertainty and fixture review

**PASS.**

The contract explicitly preserves the important uncertainties:

- five minutes is not claimed as the onset or absence of fatigue;
- `5–30 minutes` remains `Unknown`;
- actor capability, carried load, route conditions and traversal delay remain separate applicability dimensions;
- modern controlled walking evidence is not presented as direct medieval travel evidence;
- `420 m` is not a historical settlement-distance norm;
- current prototype route sizes, resident fixtures, one-hour travel compatibility and Godot geometry are not evidence.

## Long-horizon review

**NOT_APPLICABLE for this model-only classification.**

The contract starts no travel and consumes no simulation time, so it does not by itself alter labour capacity, task throughput, economy or demography.

Integrated duration/departure work remains subject to later P3 validation and later long-horizon economic validation where required.

## Remaining blockers outside this contract

1. Production implementation of the accepted combined short/unknown/prolonged horizon screen.
2. Traversal-delay production before the full baseline duration gate can become generally applicable.
3. Sustained pace/rest/fatigue modeling for medium and prolonged journeys.
4. Travel-plan duration snapshot/persistence.
5. Departure/progress/interruption/reroute/cancellation/arrival.
6. Richer actor capability and load/terrain/environment producers where those conditions become material.

## Overall verdict

**PASS.**

The reviewed model is conservative and evidence-bounded. Five reference minutes are acceptable as a positive short-observation envelope because the accepted walking calibration and an independent overground study both directly exercise five-minute walking, while additional evidence shows why this must not be interpreted as a universal fatigue-free rule.

The model preserves the `5–30 minute` uncertainty gap, keeps actor capability and other applicability dimensions independent, preserves controller symmetry and rights separation, and creates no travel by itself.

No blocker remains inside the declared short-reference-horizon scope. Promotion from `REVIEW_REQUIRED` to `ACCEPTED` is justified.
