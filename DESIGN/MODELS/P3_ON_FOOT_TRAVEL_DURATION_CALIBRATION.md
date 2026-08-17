# P3 On-Foot Travel Duration Calibration

Status: **REVIEW_REQUIRED**

This contract answers one narrow P3 calibration question: what physically plausible reference speed may be used for **ordinary adult on-foot travel** before route, load, condition and environment modifiers are modeled?

It does **not** define a universal medieval walking speed, a full journey-duration model, a route-choice rule, a departure rule, fatigue/rest scheduling, terrain/weather coefficients, or loaded travel coefficients.

## Mechanic

Derive a physically grounded reference traversal rate for a deliberately narrow `adult + level + unloaded + unimpaired` on-foot case so route extent can eventually produce sub-hour travel duration without reusing the prototype one-hour fixture.

## Intended feeling

Travel time should visibly depend on physical route extent rather than on the simulation clock tick. A nearby destination should not consume the same authoritative hour as a much longer equivalent route merely because time advances hourly.

## Real-world process

Walking covers physical distance over time. Preferred/comfortable speed varies between people and conditions, but modern human locomotion evidence provides a useful physical reference for level, unloaded adult walking.

The accepted structural dependency remains `DESIGN/MODELS/P3_SEMANTIC_LOCATION_AND_TRAVEL.md`:

`selected task -> destination -> known/authorized/mode-feasible route -> travel plan -> departure -> persistent progress -> arrival`

This calibration contract only constrains one input to a future travel plan.

## Reference context

Historical world context remains rural lowland England c. 1270–1348, with 1350–1450 as a separate stress/validation regime, as accepted by the parent P3 contract.

The quantitative walking-speed evidence below is **modern biomechanics**, used only as a physical calibration bound. It is not evidence for medieval schedules, road quality, daily range, footwear effects, fatigue, loads, weather, darkness, or route choice.

## Evidence and sources

### 1. Browning et al. 2006 — preferred level walking near 1.4 m/s

Raymond C. Browning, Emily A. Baker, Jessica A. Herron, Rodger Kram, “Effects of obesity and sex on the energetic cost and preferred speed of walking,” *Journal of Applied Physiology* 100(2), 390–398 (2006).

- DOI: https://doi.org/10.1152/japplphysiol.00767.2005
- Study: 39 adults; preferred speed measured and metabolic cost measured during level treadmill walking from 0.50 to 1.75 m/s.
- Result relevant here: preferred speed was approximately 1.42 m/s across the reported groups and near the speed minimizing gross energy cost per distance.
- Supports: `~1.4 m/s` is a physically plausible reference for short, level adult walking rather than `one route = one hour`.
- Does not establish: a universal speed across age, illness/injury, prolonged journeys, carried load, rough/muddy ground, gradient, weather or medieval road conditions.

### 2. Bohannon 1997 — comfortable overground walking varies around that reference

Richard W. Bohannon, “Comfortable and maximum walking speed of adults aged 20–79 years: reference values and determinants,” *Age and Ageing* 26(1), 15–19 (1997).

- DOI: https://doi.org/10.1093/ageing/26.1.15
- PubMed: https://pubmed.ncbi.nlm.nih.gov/9143432/
- Study: 230 healthy volunteers, timed over a 7.62 m floor course.
- Result relevant here: mean comfortable gait speed across reported age/sex groups ranged approximately 1.272–1.462 m/s.
- The study also reports meaningful associations with age, height and lower-extremity strength.
- Supports: a rounded `1.4 m/s` reference lies inside observed comfortable adult values, while also demonstrating that speed is not a universal person-independent constant.
- Does not establish: rural outdoor or medieval travel speed, long-duration sustainable pace, loaded travel, terrain coefficients or impairment handling.

### 3. Middleton et al. 2022 — carried load can alter self-selected speed

Kane Middleton et al., “Mechanical Differences between Men and Women during Overground Load Carriage at Self-Selected Walking Speeds,” *International Journal of Environmental Research and Public Health* 19(7), 3927 (2022).

- DOI: https://doi.org/10.3390/ijerph19073927
- Study: 30 adults completed 10-minute overground trials carrying 0%, 20% and 40% of body mass.
- Result relevant here: increasing load changed gait mechanics; self-selected speed was lower in the 40% body-mass condition than in the lower-load conditions.
- Supports: carried load is a material traversal input and must not be silently folded into a universal baseline speed.
- Does not establish: a medieval load-carriage coefficient, a universal percentage slowdown, non-military load behavior, or long-journey fatigue.

## Calibration decision

For the narrowly defined baseline case, use:

`BaselineLevelUnloadedAdultWalkingSpeed = 1400 mm/s` (`1.4 m/s`).

This is a **reference calibration value**, not a resident stat and not a claim that all people walk at exactly this speed.

Why `1.4` rather than preserving `1.42` exactly:

- Browning et al. directly observed preferred speed around 1.42 m/s;
- Bohannon’s overground healthy-adult values span both below and above 1.4 m/s;
- the evidence does not support false precision at hundredths of a metre per second for the simulation’s historical outdoor context;
- an integer `1400 mm/s` is deterministic and transparent.

## Causal model

Future duration derivation for the accepted baseline class is:

`authoritative route extent + OnFoot mode + explicit baseline-compatible traversal inputs -> reference traversal duration`

The arithmetic reference is:

`duration_ms = ceil(distance_m * 1_000_000 / 1400)`

Ceiling is an implementation-resolution choice so integer millisecond conversion never makes traversal shorter than the represented reference rate. It is not a historical claim.

This formula is **not authorized** when a material modeled condition says the baseline class does not apply.

## Applicability boundary

The `1400 mm/s` baseline may drive authoritative duration only when future authoritative state/content explicitly establishes all of the following for the traversal:

- travel mode is `OnFoot`;
- the actor is in an ordinary adult baseline capability state for this calculation;
- no carried load requiring a modeled modifier applies;
- no known injury/illness/fatigue state requiring a modeled modifier applies;
- the route/environment traversal class is explicitly compatible with the level/unobstructed baseline, or the scenario is explicitly marked as a calibration fixture for that case;
- no modeled stop, ferry, queue, obstruction or other traversal delay applies.

**Absence of those facts must not be interpreted as proof that the baseline applies.**

Current production state does not yet represent all of these applicability inputs. Therefore this research result does not by itself authorize `BeginTravel` or automatic duration generation for every existing resident/route.

## Player/NPC symmetry

The baseline and all future modifiers apply to an ordinary person regardless of controller. HumanController and AIController must receive the same duration for the same actor state, route, mode, load and environment.

No player-only walking speed, instant movement or client-side Godot movement value may alter authoritative duration.

## Ownership, rights and obligations

This calibration changes no ownership or access rights. A physically traversable duration does not grant passage rights, building access, resource use, work authority or contract performance.

The existing separation remains:

`physical route feasibility != passage authorization != destination action authorization`.

## Rules

1. `1400 mm/s` is only the baseline calibration for the explicit level/unloaded/unimpaired adult on-foot class.
2. Route distance remains authoritative simulation data; Godot geometry does not determine duration.
3. Equivalent longer route extent must produce longer reference duration.
4. Hourly simulation resolution must not quantize every route to one hour; persistent sub-hour progress/remaining duration is required by the parent P3 contract.
5. Unknown or materially non-baseline conditions must not receive invented coefficients merely to produce a duration.
6. Loaded, impaired, terrain-affected, weather-affected or prolonged travel requires separately modeled inputs/calibration when material.
7. The prototype `PrototypeTravelDurationMilliseconds = HourMilliseconds` remains compatibility fixture only and is not evidence for this model.

## Long-horizon behavior

This calibration alone does not alter settlement economy, demography or resource balance and therefore does not require a new ten-year validation scenario.

A later production travel-duration implementation may affect daily labour capacity and economic timing; that implementation must be evaluated in its own bounded task before P3 can pass.

## Assumptions and uncertainty

- Modern measured preferred/comfortable walking is used as a human-physics reference, not direct medieval journey evidence.
- The baseline is deliberately rounded to 1.4 m/s; inter-person variability is real.
- Medieval outdoor surfaces, footwear, gradient, weather, darkness, fatigue and prolonged travel may materially change speed.
- Current resident state does not yet encode a sufficient capability/condition profile to infer that every resident qualifies for the baseline.
- Current route state does not yet encode terrain/surface/gradient/environment classes.
- No load slowdown coefficient is accepted from Middleton et al.; that study only makes load dependence load-bearing.
- No age, sex, height or strength coefficient is accepted from Bohannon; those findings only reject universality.

## Fixture boundary

The following must not become canon:

- one-hour prototype commute duration;
- treating every current resident as exactly `1.4 m/s` by default;
- treating every current route as level/smooth merely because terrain fields are absent;
- deriving duration from Godot path length or animation speed;
- treating fixture route distances as historically measured roads unless their provenance says so.

## Falsifiers

Revise this calibration if:

- independent review finds Browning or Bohannon do not support the stated level/comfortable adult range;
- stronger evidence shows 1.4 m/s is materially inappropriate even as a short level/unloaded healthy-adult reference;
- the first production scenario requires a population or route class for which the applicability assumptions are false;
- implementation begins silently applying the baseline to missing/unknown capability, load or terrain state.

## Feedback

When duration becomes production-authoritative, projection may expose route extent, mode and planned/remaining traversal duration. Human-readable speed explanations belong in presentation/localization, not authoritative state.

## Persistence

A future travel plan must persist the duration-driving authoritative inputs or an immutable calibration reference sufficient for deterministic replay. Save/load may not recompute a different duration because later content/calibration values changed.

This research contract itself adds no production persistence field.

## Input flow

No player input flow is introduced by this calibration task.

## Projection/UI

No Godot/UI change is introduced. Godot must remain a consumer of authoritative travel progress rather than the source of traversal speed.

## Acceptance scenario

Model-level acceptance scenario for a later implementation:

1. An ordinary person has a selected task requiring another semantic place.
2. One known, authorized, passable route path exists and supports `OnFoot`.
3. Route extent is authoritative.
4. Traversal inputs explicitly satisfy the baseline applicability boundary.
5. Planned duration is derived from `1400 mm/s`, not from an hourly fixture.
6. A longer otherwise-equivalent route produces a longer duration.
7. Save/load preserves the same planned duration/progress.
8. If baseline applicability is unknown or a material unsupported modifier is present, duration planning safe-fails instead of inventing a coefficient.
9. The same facts produce the same result for player-controlled and AI-controlled people.

## Deferred complexity

Deferred to later bounded model/implementation tasks:

- authoritative actor walking-capability/condition representation;
- route terrain/surface/gradient/environment classes;
- load and companion/group modifiers;
- age/illness/injury/fatigue calibration;
- weather/mud/darkness effects;
- long-duration pace, stops and fatigue accumulation;
- mounted/cart/water duration models;
- departure command/state transition and cancellation/reroute consequences.

These are not needed to accept `1.4 m/s` as a narrow reference calibration, but they remain blockers to treating that reference as a universal production duration rule.
