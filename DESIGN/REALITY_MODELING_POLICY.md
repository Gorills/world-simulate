# Reality Modeling Policy v1

Status: **blocking global simulation policy**.

World Simulate is not allowed to substitute convenient game-state scripting for a causally coherent model of human life. This policy outranks prototype fixtures, regression tests, presentation convenience and phase-local implementation shortcuts. If a fixture/test conflicts with this policy, fix or delete the fixture/test instead of preserving the wrong model.

## North star

The authoritative simulation should explain **why** people, households, institutions and resources change state. The same world rules apply whether an actor is controlled by AI or by the player. Historical human systems must be grounded in an explicit reference context and evidence before they become canonical simulation law.

Determinism is required, but determinism does not mean scripted behavior. A deterministic decision can still depend on needs, obligations, skills, ownership, relationships, season, weather, available work, prices and state-bound seeded tie-breaking.

## Hard rules

1. **Cause before state.** Do not encode `time -> activity` or similar shortcuts as world law when the activity should result from obligations, needs, opportunities or decisions. Clock/calendar may constrain choices; it must not fabricate motive by itself.
2. **Player/NPC symmetry.** The player is a controller over an ordinary world actor, not a privileged simulation species. Any exceptional ability must come from ordinary state such as ownership, office, permission, contract, skill, physical access or social relationship.
3. **Ownership and rights are explicit.** Moving, consuming or giving a resource requires a modeled owner/right/access path. "Player can use settlement stock" is invalid unless the world state explains why.
4. **Evidence before canon.** Human economic/social/institutional behavior requires a declared reference region, period and at least two credible source citations before a model audit may PASS.
5. **Fixtures stay fixtures.** Prototype professions, schedules, inventories, relationship numbers and UI verbs must never become permanent rules merely because tests already depend on them.
6. **Long-horizon effects are tested.** Mechanics that establish settlement economy/demography/resource balance must be exercised over at least 10 simulated years before they are accepted as a viable world model. Failure is allowed; unexplained failure is not.
7. **Uncertainty is explicit.** Historical ambiguity, regional variation and deliberate simplifications must be recorded instead of hidden behind one invented universal rule.
8. **Model failures block delivery.** Green build/tests/replay/CI cannot compensate for failed causal, historical or actor-symmetry review.

## Historical grounding standard

For historical human behavior, the mechanic/model contract must state:

- reference region/culture or institutional context;
- reference period;
- at least two independent credible sources, preferring primary evidence, academic monographs/articles, scholarly datasets or reputable institutional publications;
- what each source actually supports;
- known disagreement or regional variation;
- explicit simplifications and why they preserve the causal structure we care about.

Generic infrastructure (serialization, IDs, deterministic replay, allocation optimizations) may mark historical grounding `NOT_APPLICABLE`, but only with an explicit reason. A phase whose acceptance itself changes human behavior/economy cannot waive a required historical review.

## Required model contract

Any phase/mechanic that changes authoritative human, economic, social, institutional or physical-world behavior must create or update a short contract under `DESIGN/MODELS/` before that model is treated as canonical. Use `DESIGN/MECHANIC_CONTRACT_TEMPLATE.md`.

The contract must cover:

- real-world process being modeled;
- causal chain from state/pressure to decision/action/consequence;
- reference context and sources;
- player/NPC symmetry cases;
- ownership/rights/obligations;
- assumptions and uncertainty;
- long-horizon expectations when relevant;
- which current values/actions are still prototype fixtures;
- observations that would falsify or force revision of the model.

`MODEL_UNDERDEFINED` is a valid blocker and is preferable to inventing an unsupported rule.

## Playable-prototype audit dimensions

Starting with P3, verdict audit evidence must contain `model_review` entries for:

- `causal_logic`;
- `historical_grounding`;
- `player_npc_symmetry`;
- `long_horizon`.

Each entry has `verdict`, `summary` and `evidence`. Historical PASS also records at least two sources plus `model_context`. Long-horizon PASS records `horizon_years >= 10`.

Required PASS dimensions:

- **P3 `SEMANTIC_LOCATION_AND_TRAVEL`:** causal logic, historical grounding, player/NPC symmetry.
- **P4 `REMOVE_HOT_PATH_FULL_STATE_CLONE`:** all dimensions must still be explicitly reviewed, but may be `NOT_APPLICABLE` when the phase changes no world-model semantics.
- **P5 `FOOD_SHORTAGE_GAMEPLAY_LOOP`:** all four dimensions PASS, including >=10 simulated years of economic/resource trajectory evidence.
- **P6 `MEASURABLE_VERTICAL_SLICE`:** all four dimensions PASS.

Under the existing audit schema v1, any reality-model `FAIL` must also make `systems_audit` and `overall` fail. The separate reality gate enforces these requirements without rewriting historical P0-P2 audit records.

## Known provisional fixtures

These are useful test/content fixtures, **not historical/world-model canon**:

- the current 12-resident village population baseline;
- the even `Farmer/Cook/Forager` profession split;
- six two-person households;
- `grain/herb/ration` as the complete food economy;
- fixed all-residents-working-at-08:00 behavior;
- fixed commute/work-hour constants;
- `AskAboutWork / Encourage / ShareRation` as relationship gameplay;
- a single `Affinity` number as relationship semantics;
- any assumption that the player may freely consume or distribute communal/settlement inventory.

Do not preserve these when a better researched causal model requires changing them.
