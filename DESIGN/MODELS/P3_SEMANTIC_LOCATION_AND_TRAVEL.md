# P3 Semantic Location and Travel — Model Contract

Status: **MODEL_UNDERDEFINED**. This contract blocks P3 PASS until the causal/historical model is repaired and independently reviewed.

## Mechanic
Represent where an ordinary person is, where they intend/need to go, and travel between meaningful places without persisting render coordinates.

## Real-world process
People occupy places and move between them because needs, obligations, opportunities, permissions and decisions create destinations. Travel is a consequence of an intended action or external displacement; clock time alone is not motive.

## Reference context
Historical grounding for routine work/travel is not yet accepted. A concrete region/period and sources must be selected before work schedules, household labor patterns or settlement routines become canonical.

## Causal model
Acceptable stable shape:

`person state + needs/obligations/opportunities -> selected intention/task -> destination -> travel -> presence -> action/consequence`

The current shortcut `hour -> commute/work/home` is a prototype fixture and is not accepted as world law.

## Player/NPC symmetry
The player controls an ordinary world person. AI-controlled and player-controlled people must use the same location, travel, ownership, permission and task rules. A separate controller may choose intent; it may not grant special physical or economic powers.

The current standalone authoritative player actor is an authority/persistence proof from P2, not permission to create a separate player-only species of simulation actor. P3 must not deepen that asymmetry.

## Ownership, rights and obligations
Being co-located is necessary for many physical interactions but is not sufficient to authorize them. Access to a home, workplace, stock, tool or good must eventually be explained by ownership, household membership, employment, permission, office, contract or another ordinary world relationship.

## Fixture boundary
The following current values/actions are explicitly noncanonical fixtures:

- 12 residents as a historically representative village population;
- even Farmer/Cook/Forager profession split;
- six two-person households;
- fixed all-residents-working-at-08:00 behavior;
- fixed 07:00 commute, 08:00 work start and 17:00 return-home constants;
- `AskAboutWork`, `Encourage`, `ShareRation` as relationship design;
- a single `Affinity` number as relationship semantics;
- any assumption that a player may freely consume or distribute settlement/communal inventory.

Regression tests that encode these fixtures are allowed only as temporary pipeline checks. They must be changed or deleted when a researched causal model conflicts with them.

## Long-horizon behavior
P3 itself does not establish the settlement economy/demography model, so a 10-year viability proof is not yet required. P3 must, however, avoid hard-coding travel/activity rules that would prevent later long-horizon economic modeling.

## Assumptions and uncertainty
- Semantic places (`Settlement`, `Home`, `Workplace`) are infrastructure categories, not a complete ontology of historical places.
- One-hour travel transitions are a temporary temporal-resolution artifact, not a claim about physical travel duration.
- Work schedules, household labor allocation and daily routines remain under-researched and must not be canonicalized in P3.

## Falsifiers / blockers
P3 must fail model review if any of these remain true at audit:

- activity is produced directly by fixed clock hours instead of a modeled obligation/task/decision chain;
- player-only powers bypass rules an ordinary person would need to satisfy;
- location is used as a substitute for ownership/permission;
- historical work/travel claims are accepted without a declared reference context and at least two credible sources;
- a prototype regression expectation is treated as evidence that the modeled behavior is realistic.

## Acceptance direction
P3 may PASS only when semantic location/travel remains authoritative and deterministic **and** its person-facing behavior is compatible with a researched, causal, player/NPC-symmetric model. If the research/model is still underdefined, the correct audit outcome is `FAIL` / `MODEL_UNDERDEFINED`, not an invented constant.
