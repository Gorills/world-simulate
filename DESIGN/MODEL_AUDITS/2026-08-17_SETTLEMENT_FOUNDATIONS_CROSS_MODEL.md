# Cross-model audit — Settlement foundations

Audit date: **2026-08-17**

Verdict: **FOUNDATION_COHERENCE_PASS / PRODUCTION_GATE_BLOCKED**

Reviewed branch/head SHA: `a8e72e00639baaecb13a1e709cf2247fe0298dbf`

## Scope

This audit checks the interfaces among the four accepted settlement foundation contracts:

- `DESIGN/MODELS/SETTLEMENT_PERSON_HOUSEHOLD_LIFECYCLE.md`;
- `DESIGN/MODELS/SETTLEMENT_PROPERTY_TENURE_COMMON_RIGHTS.md`;
- `DESIGN/MODELS/SETTLEMENT_AGRICULTURAL_YEAR_SEED_LABOUR_LIVESTOCK.md`;
- `DESIGN/MODELS/SETTLEMENT_EXCHANGE_CONTRACTS_MIGRATION.md`.

It asks whether the four accepted contracts contradict each other, whether shared entities/relationships have compatible causal meanings, and whether the combined foundation is sufficient to authorize behavior-changing production implementation.

No production code is changed or accepted by this audit. This record does not replace the four contract-specific audits and does not reopen their complete literature searches.

## Applicable prior audit evidence

The four contracts each have an applicable append-only PASS audit record at the reviewed HEAD:

- `DESIGN/MODEL_AUDITS/2026-08-17_SETTLEMENT_PERSON_HOUSEHOLD_LIFECYCLE.md`;
- `DESIGN/MODEL_AUDITS/2026-08-17_SETTLEMENT_PROPERTY_TENURE_COMMON_RIGHTS.md`;
- `DESIGN/MODEL_AUDITS/2026-08-17_SETTLEMENT_AGRICULTURAL_YEAR_SEED_LABOUR_LIVESTOCK.md`;
- `DESIGN/MODEL_AUDITS/2026-08-17_SETTLEMENT_EXCHANGE_CONTRACTS_MIGRATION.md`.

No new historical claim is introduced by this cross-model audit. The load-bearing historical premises remain those already independently re-checked in the applicable audits. This pass therefore focuses on interface consistency and changed dependency status rather than repeating the same literature search.

## Interface review

### Person identity, household membership and migration

**PASS.**

The Person/Household contract makes `Person` the persistent controller-neutral actor and requires household membership changes to be explicit events rather than consequences of travel.

The Exchange/Contracts/Migration contract preserves the same `Person` through departure, travel and arrival, preserves pre-existing debts/claims/relationships unless ordinary events change them, and explicitly rejects `SettlementId = destination` as the migration model.

There is no identity contradiction: migration changes location and may lead to later household/residence/work/tenure transitions; it does not create a new person or silently rewrite membership.

### Household membership versus ownership and resource authority

**PASS for topology; implementation authority remains underdefined.**

The Person/Household contract states that household membership is not universal ownership. The Property/Tenure contract makes household sharing an explicit authorization basis over specified resources rather than transferable title to all household-associated assets.

Agriculture and Exchange both preserve this distinction: household association does not authorize cultivation, sale, transfer or use of another holder's resource merely through membership or co-location.

However, the Person/Household contract explicitly defers the exact authority structure inside a household. That matters when later contracts speak of a household allocating labour/resources, accepting credit or acting as a contract counterparty. No accepted rule yet determines who may bind a household, commit household-controlled resources, authorize a member's labour, or represent the household to another party.

This is a **production blocker for household-level economic decisions**, not a contradiction in the accepted topology.

### Rights, holdings and obligations across property and exchange

**PASS.**

The Property/Tenure contract treats rents, services and dues as explicit obligation references rather than properties of a person class.

The Exchange/Contracts contract supplies a compatible generic obligation/debt concept whose basis may be a contract or holding. Obligations can therefore survive movement, household changes and save/load without being duplicated into separate incompatible systems.

Property authorization remains action-specific; Exchange requires transfer authority and does not treat physical possession, market presence or payment as automatic legal authority.

### Agriculture, rights and output provenance

**PASS for structural compatibility.**

Agriculture requires recognized land/resource rights before work is authorized and requires crop/resource provenance rather than `profession + clock -> output`.

The Property contract can express cultivation, removal, pasture, transfer and management rights separately. The Exchange contract can then transfer resources/rights only through authorized counterparties and can create deferred obligations when consideration is not immediate.

Agricultural output is therefore not forced into a settlement-global stockpile. Output claims can be represented through holdings, contracts, household sharing rules and other explicit rights/obligations.

Exact local rules that decide competing claims to output in a particular tenure/contract context remain calibration/model-context work. Production code must not invent a universal claimant when the selected context is unresolved.

### Labour across household, agriculture and contracts

**PASS for finite-labour topology; decision/authority bridge remains underdefined.**

The Person/Household contract treats available household members as a labour pool without making the household owner of those people.

Agriculture treats labour as finite and allocatable across household work, hired work, service and holding obligations. Exchange models hired/service work as explicit agreements and requires labour commitments not to duplicate person capacity.

The contracts therefore agree that `Profession` is not a labour source and that one person's time cannot be cloned.

What remains missing is an accepted bridge deciding when a person is obliged, expected or willing to contribute household labour and who can commit household-level labour decisions. Implementing such behavior from a fixed schedule or arbitrary household-head rule would violate the accepted foundations.

### Resource reservations, shortages and exchange

**PASS.**

Agricultural reservations such as seed/food/fodder are explicit planned/recognized uses rather than magical physical locks. Exchange permits an ordinary actor to sell/consume reserved seed when authorized, with consequences for later production.

This interface preserves meaningful trade-offs: shortage can cause acquisition/credit decisions, while present consumption or sale can reduce future productive capacity.

### Player/NPC symmetry

**PASS.**

All four foundations use one ordinary `Person` actor under either AI or Human controller. Controller type does not grant ownership, residence, cultivation rights, free goods, credit, labour, migration, contract enforcement or resource access.

No cross-model interface requires a privileged player species.

### Persistence and provenance

**PASS.**

The contracts consistently require persistent identities/history for persons, household memberships, rights/claims, obligations/contracts, crop/resource provenance and migration state.

None permits reconstruction of authoritative social/economic state from profession, clock hour, co-location or current settlement membership.

## Missing causal bridge: intention/task selection

**PRODUCTION BLOCKER.**

The accepted foundations repeatedly depend on a causal step of the form:

`pressures / needs / obligations / opportunities / rights / available resources -> selected intention or task -> destination/action`

but there is no accepted contract yet defining the shared intention/task selection boundary for autonomous ordinary people.

This gap is load-bearing for the active P3 phase because semantic travel requires a destination produced by an intended action rather than by a fixed commute schedule. It is also load-bearing for Agriculture and Exchange because they defer choice among feasible labour sources, shortages, offers, contracts and competing tasks to a later controller/economic decision model.

Production behavior must not fill this gap with `hour -> activity`, a profession switch, a generic `wander` urge, omniscient opportunity selection or a player-only command path.

A bounded model task is required before schedule replacement/autonomous task selection can become canonical.

## Material external dependency: P3 semantic travel

**PRODUCTION BLOCKER FOR MOVEMENT/TRAVEL.**

`DESIGN/MODELS/P3_SEMANTIC_LOCATION_AND_TRAVEL.md` is still `MODEL_UNDERDEFINED` at the reviewed HEAD.

The Exchange/Migration foundation intentionally relies only on the stable seam:

`intention -> destination -> travel -> presence -> action/consequence`

and explicitly does not accept the prototype one-hour travel resolution or fixed commute/work schedule as canonical.

The active playable-prototype phase is still `P3_SEMANTIC_LOCATION_AND_TRAVEL` in `IMPLEMENTING` state. Therefore this cross-model audit does **not** authorize further movement/travel behavior as canonical until P3 itself receives a sufficient causal/historical model and independent phase/model audit.

Generic location/travel infrastructure may remain useful, but behavior-changing production implementation cannot use the underdefined P3 seam as permission to invent travel duration, routine destinations or commute schedules.

## Quantitative and later-gate dependencies

These are not contradictions among the four foundations, but they remain explicit blockers wherever a production mechanic needs them:

- demographic fertility/mortality/marriage/service/migration rates;
- crop yields, seed rates, labour coefficients, herd/fodder/grazing parameters and storage losses;
- prices, wages, bargaining/price formation, credit availability/default rates and market matching/frequency;
- manor/local inheritance, transfer, rent/service and common-capacity parameters;
- institution-specific dispute/enforcement procedures;
- travel duration/routing/cost calibration;
- newcomer integration probabilities and social-information/knowledge rules.

P5/P6 economic/demographic PASS remains impossible without the policy's >=10 simulated-year evidence using accepted/calibrated mechanics. A structurally coherent foundation is not long-horizon viability proof.

## Reality Modeling Policy verdicts

### Causal logic

**FOUNDATION PASS / PRODUCTION BLOCKED.**

The four contracts are mutually causal and do not require a hidden game-only state transition at their interfaces. The missing intention/task-selection bridge and P3 travel model prevent behavior-changing implementation from being considered complete.

### Historical grounding

**PASS for the accepted foundation scopes.**

No new historical rule is introduced here. Existing accepted audit evidence remains applicable. Deferred local/quantitative questions remain deferred rather than promoted to universal rules.

### Player/NPC symmetry

**PASS.**

No cross-model privilege is introduced by controller type.

### Ownership, rights and obligations

**PASS for structural interfaces / household representation underdefined.**

The rights/obligation topology is coherent. Household-level authority to bind resources/labour/contracts remains a required later model rather than being inferred from membership.

### Uncertainty and fixture boundary

**PASS.**

Known prototype schedules, professions, settlement stock, player-only inventory powers and automatic migration resets remain noncanonical. Quantitative gaps remain explicit.

### Long horizon

**NOT YET SATISFIED FOR IMPLEMENTED ECONOMY/DEMOGRAPHY.**

The foundations specify compatible invariants but do not constitute the required >=10-year implementation evidence.

## Overall conclusion

The four accepted settlement foundations are **mutually coherent and reusable** in their declared scopes. No accepted contract needs to be rolled back solely because of this cross-model review.

The combined foundation is **not yet sufficient to authorize behavior-changing production implementation**. The immediate material blockers are:

1. an accepted intention/task-selection causal bridge for ordinary persons/controllers;
2. an accepted household authority/representation rule sufficient for household-level labour/resource/contract decisions;
3. completion of the underdefined P3 semantic travel model before canonical movement/travel behavior changes.

Quantitative economy/demography and institution-specific gaps remain later blockers when the relevant mechanics are implemented.

This verdict intentionally preserves the Reality Modeling Gate: green CI and four local `ACCEPTED` contracts do not turn unresolved cross-model dependencies into implicit game rules.