# Audit — P3 On-Foot Travel Duration Calibration

Audit date: **2026-08-18**

Verdict: **PASS**

Model contract: `DESIGN/MODELS/P3_ON_FOOT_TRAVEL_DURATION_CALIBRATION.md`

Reviewed research/model SHA: `88b2e4a52fea63436437af7907dd36f978e0fe67`

## Scope

Independent audit of the narrow on-foot walking-speed calibration only. No production simulation code, travel departure command, actor capability system, terrain/environment model, load coefficient, fatigue model, or historical journey schedule is accepted by this audit.

The accepted parent contract `DESIGN/MODELS/P3_SEMANTIC_LOCATION_AND_TRAVEL.md` remains the structural authority. This audit asks only whether `1400 mm/s` (`1.4 m/s`) is defensible as a reference rate for an explicitly constrained `adult + level + unloaded + unimpaired + OnFoot` traversal class, with safe failure when applicability is unknown or materially different.

## Repository and CI

The exact reviewed SHA was branch HEAD `88b2e4a52fea63436437af7907dd36f978e0fe67` at audit time. That commit added only `DESIGN/MODELS/P3_ON_FOOT_TRAVEL_DURATION_CALIBRATION.md`; no production/runtime/test file was changed.

Required GitHub Actions on the reviewed SHA all passed:

- `ci #165` — success;
- `playable-prototype-gate #212` — success;
- `proof-a-measure #160` — success.

The `ci #165` jobs for scope, core, Godot and `ci-required` all completed successfully, including build, core tests, architecture tests, headless core smoke, production settlement scale smoke, Proof A workload smoke, Godot C# build and Godot headless integration smoke.

## Load-bearing fact re-check

### Browning et al. 2006 — preferred level walking speed near 1.42 m/s

Re-checked Raymond C. Browning, Emily A. Baker, Jessica A. Herron and Rodger Kram, “Effects of obesity and sex on the energetic cost and preferred speed of walking,” _Journal of Applied Physiology_ 100(2), 390–398 (2006).

https://pubmed.ncbi.nlm.nih.gov/16210434/

The paper studied 39 adults (19 class II obese, 20 normal weight), measured preferred walking speed, and measured metabolic variables while participants walked on a level treadmill across six speeds from 0.50 to 1.75 m/s. The abstract reports that preferred walking speed did not differ across the study groups and was `1.42 m/s`, near the speed minimizing gross energy cost per distance.

Audit conclusion: `~1.4 m/s` is directly supported as a physically plausible modern adult level-walking reference. The study does **not** establish one universal medieval/outdoor speed, long-duration sustainable pace, terrain/weather/load coefficients, or a speed valid for every age/health state. The contract preserves those limits.

### Bohannon 1997 — comfortable overground adult gait varies around 1.4 m/s

Re-checked Richard W. Bohannon, “Comfortable and maximum walking speed of adults aged 20–79 years: reference values and determinants,” _Age and Ageing_ 26(1), 15–19 (1997).

https://academic.oup.com/ageing/article/26/1/15/20634

The study used 230 healthy volunteers and timed gait over a 7.62 m floor course. Mean comfortable gait speed ranged from `127.2 cm/s` for women in their seventies to `146.2 cm/s` for men in their forties. Comfortable and maximum speeds also correlated with age, height and measured lower-extremity strength.

Audit conclusion: the observed comfortable overground range independently contains the rounded `1.4 m/s` reference while demonstrating meaningful inter-person/group variation. It does not establish medieval road speed, a universal resident stat, prolonged walking pace or coefficients by age/height/strength. The contract correctly uses it to reject false universality rather than to introduce those coefficients.

### Middleton et al. 2022 — substantial external load changes self-selected walking behavior

Re-checked Kane Middleton et al., “Mechanical Differences between Men and Women during Overground Load Carriage at Self-Selected Walking Speeds,” _International Journal of Environmental Research and Public Health_ 19(7), 3927 (2022).

https://www.mdpi.com/1660-4601/19/7/3927

Thirty adults completed 10-minute overground walking trials with external loads of 0%, 20% and 40% of body mass. The paper reports that increasing loads altered gait mechanics and that self-selected walking speed was lower in the 40% body-mass condition than in the lower-load conditions; the reported reduction was about `0.15 km/h` (approximately 3%).

Audit conclusion: carried load can be a material traversal input and cannot be silently absorbed into one universal baseline rate. The study does **not** justify a medieval load coefficient, a universal percentage slowdown, prolonged-load fatigue behavior, or a general rule for all types of carried load. The contract explicitly refuses to infer such a coefficient.

## Calibration decision review

**PASS.**

The contract adopts `BaselineLevelUnloadedAdultWalkingSpeed = 1400 mm/s` as a rounded reference calibration rather than preserving false precision at `1.42 m/s`. This rounding is consistent with the independent Browning and Bohannon evidence and is explicitly not represented as a universal person stat or historical journey speed.

The deterministic arithmetic reference `duration_ms = ceil(distance_m * 1_000_000 / 1400)` is dimensionally correct for converting authoritative metres to integer milliseconds at `1400 mm/s`. Ceiling prevents integer conversion from making a represented traversal shorter than the reference rate. The rounding/ceiling choices are implementation-resolution choices, not historical claims.

## Applicability and causal review

**PASS.**

The baseline is authorized only when authoritative traversal facts explicitly establish the narrow baseline class: `OnFoot`, ordinary adult baseline capability for the calculation, no material carried load, no modeled injury/illness/fatigue modifier, a route/environment class explicitly compatible with the baseline (or an explicit calibration fixture), and no modeled stop/queue/ferry/obstruction delay.

Critically, the contract states that **absence of those facts is not evidence that the baseline applies**. Current production state does not yet encode all required applicability inputs, so this accepted calibration does not authorize automatic `BeginTravel` or duration assignment to every resident/route.

This preserves the accepted causal seam `route extent + mode + explicit applicable actor/load/route/environment inputs -> duration` rather than `route exists -> assume 1.4 m/s` or `one tick -> one journey`.

## Player/NPC symmetry review

**PASS.**

The reference and any future modifiers depend on ordinary actor/route/load/environment state, not controller type. HumanController and AIController must therefore receive the same authoritative duration for the same world facts. No player-only speed, instant travel or Godot animation value is admitted as authoritative.

## Rights and authorization review

**PASS.**

Duration calibration changes no ownership, passage, access, work, contract or resource-use right. The accepted separation remains `physical route feasibility != passage authorization != destination action authorization`.

## Persistence and determinism review

**PASS for model scope.**

The contract correctly requires a future travel plan to persist duration-driving authoritative inputs or an immutable calibration reference sufficient for deterministic replay, so save/load cannot silently recompute a different duration after later calibration/content changes. No production persistence field is introduced by this research task.

## Uncertainty and fixture-boundary review

**PASS.**

The contract explicitly keeps the following outside the accepted baseline: universal application of `1.4 m/s` to every resident; medieval outdoor surface/footwear/gradient/weather/darkness effects; load slowdown coefficients; age/illness/injury/fatigue coefficients; long-duration pace, stops and fatigue accumulation; mounted/cart/water duration models; treating absent terrain/capability/load data as baseline-compatible; deriving authoritative duration from Godot geometry or animation speed; and the existing one-hour prototype commute fixture.

These remain blockers when a production scenario materially requires them. They do not invalidate acceptance of the narrow physical reference itself.

## Long-horizon review

**PASS for this calibration-only contract; integrated travel timing remains later work.**

This model-only calibration changes no settlement economy, demography or resource balance by itself, so a new standalone ten-year run is not required here. A later production duration/departure implementation can alter labour capacity and economic timing and must be evaluated in its own bounded implementation/audit before P3 can pass.

## Deferred gaps

Accepted here: `1400 mm/s` as the explicit baseline reference for the declared narrow traversal class; distance/time causality for that class; and safe failure when baseline applicability is unknown or materially false.

Still deferred and not accepted by this audit: authoritative actor walking-capability/condition representation; route terrain/surface/gradient/environment representation; carried-load and companion/group modifiers; illness/injury/fatigue/age calibration; weather/mud/darkness effects; long-duration pace/stops/fatigue; mounted/cart/water timing; departure command/state transition, cancellation and reroute consequences; and automatic production use of the baseline for current fixture residents/routes.

## Overall verdict

**PASS.**

The load-bearing evidence supports `1.4 m/s` as a narrow modern-biomechanics reference for explicitly baseline-compatible adult on-foot traversal, and the contract keeps its historical, population, load, environment and duration limits explicit. No causal, symmetry, rights, persistence, uncertainty or fixture blocker prevents promotion from `REVIEW_REQUIRED` to `ACCEPTED` in this declared scope.

`ACCEPTED` does **not** authorize universal production walking duration or `BeginTravel`; the missing applicability state and later travel-plan/departure mechanics remain separate bounded work.