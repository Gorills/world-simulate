# Audit — P3 Traversal Horizon Rejection Bound

Audit date: **2026-08-18**

Verdict: **PASS**

Model contract: `DESIGN/MODELS/P3_TRAVERSAL_HORIZON_REJECTION_BOUND.md`

Reviewed model SHA: `a19581c85f6a68241ce2b434f84a347d2a491b48`

## Scope

Independent audit of the narrow one-sided producer for `TraversalHorizon = ProlongedOrEnduranceRelevant`.

This audit does **not** accept a general positive producer for `BaselineShortReferenceCompatible`, a sustained-journey walking speed, fatigue/recovery equation, rest schedule, traversal-delay producer, travel duration plan, departure, progress, reroute, cancellation or arrival.

## Repository and CI

The exact reviewed SHA was branch/PR HEAD `a19581c85f6a68241ce2b434f84a347d2a491b48` at audit time.

Required GitHub Actions on the reviewed SHA all passed:

- `ci #179` — success;
- `playable-prototype-gate #240` — success;
- `proof-a-measure #174` — success.

The reviewed commit added only `DESIGN/MODELS/P3_TRAVERSAL_HORIZON_REJECTION_BOUND.md`; no production/runtime/test file changed.

## Accepted prerequisite re-check

Reopened/reused the accepted P3 chain:

- `DESIGN/MODELS/P3_SEMANTIC_LOCATION_AND_TRAVEL.md`;
- `DESIGN/MODELS/P3_ON_FOOT_TRAVEL_DURATION_CALIBRATION.md`;
- `DESIGN/MODELS/P3_ON_FOOT_TRAVERSAL_APPLICABILITY.md`;
- `DESIGN/MODELS/P3_PLANNED_TRAVERSAL_ASSESSMENT_CONTEXT.md`.

The prerequisite contracts consistently require duration applicability to safe-fail on unknown facts, distinguish a short-reference calibration from prolonged travel, bind traversal-specific facts to the concrete actor/task/path/mode assessment, and prohibit client/Godot authority over timing.

The reviewed horizon model preserves those constraints.

## Primary evidence re-check

### Kwon et al. 2023 — 30-minute preferred-speed walking is fatigue-relevant

Re-checked Yujin Kwon, Lillian K. Chilton, Hoon Kim and Jason R. Franz, “The effect of prolonged walking on leg muscle activity patterns and vulnerability to perturbations,” _Journal of Electromyography and Kinesiology_ 73:102836 (2023), DOI `10.1016/j.jelekin.2023.102836`.

PubMed: https://pubmed.ncbi.nlm.nih.gov/37979335/

Eighteen healthy young adults completed a 30-minute walking trial at preferred walking speed. The reported time-dependent EMG changes were interpreted as neuromuscular adaptations indicative of local muscle fatigue. Mean mediolateral centre-of-mass displacement following perturbations was approximately 21% larger after the walking exposure.

Audit conclusion: the study supports treating a 30-minute comfortable/preferred-speed exposure as already materially fatigue/endurance-relevant for the narrow purpose of refusing to extend a short-reference calibration without an endurance model. It does **not** establish that fatigue begins exactly at minute 30.

### Thomas et al. 2017 — time matters within a 30-minute walking exposure

Re-checked Kathleen S. Thomas et al., “The impact of speed and time on gait dynamics,” _Human Movement Science_ 54:320–330 (2017), DOI `10.1016/j.humov.2017.06.003`.

PubMed: https://pubmed.ncbi.nlm.nih.gov/28641172/

Fourteen young adults walked for 30 minutes at preferred walking speed, 90% PWS and 80% PWS. Measures were evaluated over successive five-minute intervals, and several gait measures changed during the walking exposure.

Audit conclusion: this independently supports duration as a load-bearing gait variable over a 30-minute bout; it does not supply a reciprocal universal short-safe threshold.

### Yoshino et al. 2004 — longer preferred-pace walking reinforces endurance relevance

Re-checked Kohzoh Yoshino, Tomoko Motoshige, Tsutomu Araki and Katsunori Matsuoka, “Effect of prolonged free-walking fatigue on gait and physiological rhythm,” _Journal of Biomechanics_ 37(8):1271–1280 (2004), DOI `10.1016/j.jbiomech.2003.11.031`.

PubMed: https://pubmed.ncbi.nlm.nih.gov/15212933/

Twelve subjects walked continuously for three hours at self-determined preferred pace. Subjective fatigue increased over time; the more fatigable subgroup showed fatigue-related EMG and gait-rhythm changes.

Audit conclusion: longer continuous walking clearly cannot be assumed equivalent to a short laboratory reference bout with fatigue/endurance absent from the causal model.

### Majed et al. 2024 — short preferred-speed protocols do not prove a universal short-safe rule

Re-checked Lina Majed et al., “Walking around the preferred speed: examination of metabolic, perceptual, spatiotemporal and stability parameters,” _Frontiers in Physiology_ 15:1357172 (2024), DOI `10.3389/fphys.2024.1357172`.

The protocol used seven three-minute walking trials with three-minute rest intervals. The paper explicitly describes those rest intervals as allowing recovery and avoiding fatigue effects.

Audit conclusion: this is useful short-bout preferred-speed evidence, but it cannot establish `3 minutes` or any larger extrapolated value as a universal no-fatigue boundary.

### Lordall et al. 2020 — ten-minute observation is not a short-safe threshold

Re-checked Jackson Lordall, Paul Bruno and Nicholas Ryan, “Assessment of diurnal variation of stride time variability during continuous, overground walking in healthy young adults,” _Gait & Posture_ 79:108–110 (2020), DOI `10.1016/j.gaitpost.2020.04.024`.

PubMed: https://pubmed.ncbi.nlm.nih.gov/32387809/

Thirty-one healthy young adults completed two ten-minute continuous overground walking trials at preferred speed. The study concerned diurnal variation and reliability, not the onset or absence of fatigue.

Audit conclusion: ten-minute trials do not justify `<=10 min` or `<30 min` as automatic `BaselineShortReferenceCompatible` production facts.

### Counter-check: real-world long bouts do not imply a forced slower speed

Also checked Loubna Baroudi et al., “Investigating walking speed variability of young adults in the real world,” _Gait & Posture_ 98:69–77 (2022), DOI `10.1016/j.gaitpost.2022.08.012`.

PubMed: https://pubmed.ncbi.nlm.nih.gov/36057208/

The study found that duration and continuity explain some real-world walking-speed variability and reported about 1.41 m/s for long continuous bouts in its healthy-young-adult sample.

Audit conclusion: the rejection producer must **not** be interpreted as evidence that actual speed necessarily drops at 30 minutes. The reviewed contract correctly classifies endurance/fatigue as materially relevant while leaving actual sustained pace unresolved.

## One-sided inference review

**PASS.**

The reviewed rule is deliberately asymmetric:

`reference_horizon_ms >= 1_800_000 -> ProlongedOrEnduranceRelevant`

but:

`reference_horizon_ms < 1_800_000 -> Unknown`

unless another independently accepted, provenance-bound source establishes `BaselineShortReferenceCompatible`.

This does not assert that fatigue begins at 30 minutes and does not assert that every shorter traversal is fatigue-free. It establishes only a conservative rejection boundary for extending the accepted short-reference `1400 mm/s` calibration.

The category name is load-bearing: `ProlongedOrEnduranceRelevant` means endurance/fatigue may materially affect timing and therefore cannot be omitted from the baseline proof. It is not a claim that every actor must slow at exactly 30 minutes.

## Arithmetic review

**PASS.**

The screening transformation reuses the accepted reference arithmetic:

`reference_horizon_ms = ceil(path_distance_m * 1_000_000 / 1400)`.

At `1400 mm/s`, 30 minutes corresponds to exactly `2520 m`:

`1.4 m/s * 1800 s = 2520 m`.

`2520 m` is therefore only a derived convenience for this one-sided reference screen. It is not a medieval trip-distance law and is not a positive short-trip threshold.

Using the accepted reference rate for the rejection screen is conservative for unsupported slower conditions: if the optimistic reference screen is already at least 30 minutes, an unmodeled slower condition cannot make the candidate shorter merely to recover short-reference eligibility. No actual non-baseline speed is inferred.

## Causal and authority review

**PASS.**

The horizon producer runs only after a concrete selected-task destination and exact known/authorized/mode-feasible OnFoot route path exist.

It consumes authoritative total ordered-path extent and the accepted calibration identity. It cannot create route knowledge, choose among multiple routes, grant passage rights, create a task/destination, emit a duration plan or cause departure.

Multi-edge paths are screened by total authoritative path extent rather than per-edge classification, endpoint straight-line distance or Godot/navigation geometry.

## Binding, invalidation and replay review

**PASS.**

The result inherits the accepted concrete assessment binding: scope-qualified actor, `TaskId`, origin, destination, ordered route identities, mode and calibration/version identity.

Changing the bound path/task/actor/mode/extent invalidates the pre-departure result. The horizon classification is not a permanent actor or route trait.

A future departed travel plan must snapshot or immutably reference the duration-driving applicability/calibration inputs so later calibration/content changes cannot rewrite elapsed history.

## Player/NPC symmetry and rights review

**PASS.**

Controller identity does not enter the screening arithmetic. The same authoritative path extent and calibration identity produce the same horizon result for player-controlled and AI-controlled humans.

Timing classification grants no route, property, work, resource or destination-action right.

## Uncertainty and fixture review

**PASS.**

The contract explicitly limits the 30-minute evidence to a conservative modern-biomechanics rejection use and rejects treating `2520 m` as a universal human or medieval journey boundary.

Below the rejection bound, horizon remains `Unknown` unless an independently accepted, provenance-bound bounded scenario supplies the positive short-reference fact. No default settlement content receives implicit short compatibility.

## Long-horizon simulation review

**PASS for model-only scope.**

The contract starts no travel and consumes no simulation time, so it does not by itself alter labour capacity, task throughput, economy or demography. Integrated long-horizon validation remains required once duration/departure becomes production-authoritative.

## Remaining blockers outside this contract

1. General positive production producer for `BaselineShortReferenceCompatible`.
2. Sustained-journey speed/rest/fatigue model after `ProlongedOrEnduranceRelevant`.
3. Traversal-delay producer.
4. Travel-plan duration snapshot/persistence.
5. Departure/progress/interruption/reroute/cancellation/arrival.

## Overall verdict

**PASS.**

The evidence supports a one-sided 30-minute rejection bound for the narrow purpose of preventing the accepted short-reference walking calibration from being extended into clearly endurance-relevant traversal candidates. The reviewed model correctly refuses the inverse `<30 minutes => short-compatible` inference, preserves authoritative path binding, controller symmetry, rights separation and replay requirements, and does not invent an actual sustained journey speed.

No blocker remains inside the declared rejection-bound scope. Promotion from `REVIEW_REQUIRED` to `ACCEPTED` is justified.
