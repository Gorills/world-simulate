# Settlement Intention and Task Selection — Model Contract

Status: **ACCEPTED**

This contract defines the minimum causal bridge between an ordinary person's current world state and the task/intention that a controller chooses next. It does **not** define one universal medieval preference function, one fixed daily schedule, exact numerical utility weights, personality psychology, household command authority, travel duration/routing, demographic rates or economic calibration.

## Mechanic

Represent ordinary behavior as a sequence in which world state produces **reasons and candidate tasks**, accepted rules determine which candidates are physically feasible and authorized, and a controller selects among those candidates before any destination, travel or action is executed.

The canonical seam is:

`world state -> reasons/pressures/obligations/opportunities -> candidate intentions/tasks -> feasibility + authorization -> controller selection -> committed intention/task -> destination/travel if required -> action/process -> consequences -> updated world state`

There is no canonical transition of the form:

`clock hour -> profession activity`

or:

`profession -> fixed destination -> work`.

## Intended feeling

A person should appear to act because something in their life gives them a reason to act:

- an accepted obligation is due;
- a household or dependant requires something under an accepted relationship;
- an agricultural process has entered a limited work window;
- a paid/service commitment requires performance;
- a useful opportunity is known and feasible;
- an existing task remains worth continuing;
- a shortage or loss makes acquisition, negotiation or relocation attractive;
- a controller deliberately chooses neglect, breach or a worse alternative and the ordinary consequences follow.

The same person may perform different kinds of work over time. A label such as `Farmer`, `Cook`, `Forager` or `Servant` must never be sufficient cause for the selected task.

## Dependencies

This contract reuses four accepted settlement foundations:

- `DESIGN/MODELS/SETTLEMENT_PERSON_HOUSEHOLD_LIFECYCLE.md` — persistent ordinary `Person`, current intention/task seam, household pressures, lifecycle relationships and controller symmetry;
- `DESIGN/MODELS/SETTLEMENT_PROPERTY_TENURE_COMMON_RIGHTS.md` — physical possibility is separate from authorization; rights/claims/obligations are action-specific;
- `DESIGN/MODELS/SETTLEMENT_AGRICULTURAL_YEAR_SEED_LABOUR_LIVESTOCK.md` — finite labour, seasonal/environmental task windows, material inputs and production consequences;
- `DESIGN/MODELS/SETTLEMENT_EXCHANGE_CONTRACTS_MIGRATION.md` — explicit counterparties/contracts/debts, labour commitments, opportunities and migration intentions.

The cross-model audit `DESIGN/MODEL_AUDITS/2026-08-17_SETTLEMENT_FOUNDATIONS_CROSS_MODEL.md` identified this decision seam as a production blocker.

### Boundary with household authority

This contract does **not** decide who may bind a household, command another member, commit household-controlled resources or represent the household in a contract.

A household-level expectation, request, resource plan or labour commitment may become a person's decision input only when its authority/basis is established by an accepted relationship or later household-authority model. Until then, `HouseholdId` is not permission to assign tasks to every member.

### Boundary with P3 travel

`DESIGN/MODELS/P3_SEMANTIC_LOCATION_AND_TRAVEL.md` remains `MODEL_UNDERDEFINED`.

This contract supplies only the upstream seam:

`selected task -> required target/place -> destination request`

It does not accept current one-hour travel duration, 07/08/17 commute constants or a fixed home/workplace schedule. Travel feasibility, duration and route remain P3 work.

## Reference context

Baseline: rural lowland England, approximately **1270–1348** for first calibration, with **1350–1450** retained as a separate stress/validation regime where labour supply, land access, bargaining and mobility changed materially after the Black Death.

The purpose of the historical evidence here is deliberately limited. It establishes that ordinary work and movement arose from overlapping household, agricultural, contractual, market and opportunity contexts rather than one exhaustive occupation schedule. It does **not** establish one numerical ranking algorithm for medieval decisions.

## Evidence ledger

### 1. Rural people could combine several economic activities rather than execute one profession script

**Christopher Dyer, _Peasants Making History: Living in an English Region 1200–1540_, chapter 9, “Peasants and industry” (Oxford University Press, 2022).** Dyer describes widespread non-agricultural countryside activity and peasants combining farming with fishing, food trades, building work and crafts; part-time work and the participation of women and young people materially change the picture of rural employment.

- https://academic.oup.com/book/43934/chapter/370551541
- Supports: a person's feasible/candidate work can span several kinds of activity; occupation labels are insufficient as action generators.
- Supports: household economic circumstances and opportunities can make supplementary work relevant.
- Does **not** establish a universal probability of doing crafts, a fixed daily division of labour or one occupation mix for every household.

### 2. Work opportunities and constraints depended on household/family responsibilities and local economic context

**P. J. P. Goldberg, _Women, Work, and Life Cycle in a Medieval Economy: Women in York and Yorkshire c.1300–1520_, chapter 3, “Women and Work” (Oxford University Press, 1992).** Goldberg places women's work in the familial economy and wider urban/rural economy and examines how access to work was affected by wealth/training, marital status, local economic needs and household/family responsibilities.

- https://academic.oup.com/book/7906/chapter-abstract/153157934
- Supports: candidate work cannot be derived from sex/occupation alone; household responsibilities, capability/resources and local opportunity materially constrain what work is available or attractive.
- Does **not** establish a single gendered task table, fixed hour schedule or universal household allocation rule.

### 3. Household labour expectations existed, but this does not justify a universal command API

**Christopher Dyer, _Peasants Making History_, chapter 5, “Family and household” (Oxford University Press, 2022).** Dyer describes households containing kin and sometimes unrelated servants and notes expectations of household discipline/hierarchy intended in part to secure labour and succession.

- https://academic.oup.com/book/43934/chapter/370549926
- Supports: household membership can create real labour expectations/pressures that matter to decisions.
- Does **not** establish one universal `HeadOfHousehold` authority set, equal authority across households, or automatic power to commit every member's labour/resources.
- Simulation consequence: household expectation is a possible reason/input, while the authority/representation rule remains a separate bounded model task.

### 4. Paid labour and service create commitments outside a person's primary household

**Mark Bailey, “The regulation of the rural market in waged labour in fourteenth-century England,” _Continuity and Change_ 38(2) (2023).** Bailey reconstructs a substantial rural hired-labour market from manorial court evidence and shows that its organisation and regulation changed markedly across the Black Death.

- https://www.cambridge.org/core/journals/continuity-and-change/article/regulation-of-the-rural-market-in-waged-labour-in-fourteenthcentury-england/C7726EC8A2D0C628ACFF49428FDA95A7
- Supports: a person may face accepted work commitments/opportunities that compete with household or self-directed work; labour must not come only from a profession/workplace assignment.
- Supports: pre- and post-1348 labour conditions must not be collapsed into one timeless decision parameter.
- Does **not** establish one wage threshold, employment share or universal rule that a paid task always outranks household work.

### 5. Agricultural time creates windows and consequences, not personal motives by itself

**Helena Hamerow et al., _Feeding Medieval England: A Long ‘Agricultural Revolution’, 700–1300_, chapter “Crop Rotation and Seasonal Sowing” (Oxford University Press, 2025), reused from the accepted Agricultural Year contract.** The source supports seasonal sowing/rotation/fallow constraints and local variation.

- https://academic.oup.com/book/61548/chapter/537298429
- Supports: time/season can make a task feasible, urgent or impossible and can alter the consequence of delay.
- Does **not** support: `month/day/hour -> person performs task`, one universal crop calendar or one national work schedule.

The accepted Agricultural Year audit independently re-checked this premise; this contract relies only on the already-accepted structural distinction.

### 6. Cooperation and joint work create another source of candidate tasks

**Christopher Dyer, “Partnership among peasants: rural England, 1270–1520,” _Continuity and Change_ 37(3) (2022/2023 online).** Dyer examines partnerships as two or more people pursuing common objectives, including joint landholding, clearing and paid work.

- https://www.cambridge.org/core/journals/continuity-and-change/article/partnership-among-peasants-rural-england-12701520/F8DB6A2A76E46C44687718E4FDEA8CC8
- Supports: cooperation/partnership can create shared commitments or opportunities without converting all village residents into one collective actor.
- Does **not** establish one universal cooperation rate or authority to conscript neighbours.

### 7. People could deliberately pursue opportunities that changed work/residence

**Christopher Dyer, _Peasants Making History_, chapter 4, “Peasants changing society” (Oxford University Press, 2022).** Dyer describes people moving in pursuit of land, employment and marriage and frames such choices as attempts to improve circumstances, while noting that outcomes and mobility were limited/unequal.

- https://academic.oup.com/book/43934/chapter/370549741
- Supports: known future opportunities can legitimately become candidate intentions rather than every action being maintenance of the current role/place.
- Does **not** establish perfect information, guaranteed betterment, one migration threshold or unrestricted mobility.

### Evidence limits and disagreement

- Surviving sources reveal contracts, disputes, occupations and major economic behavior better than moment-to-moment private reasoning. This contract therefore does **not** claim to reconstruct medieval psychology.
- Historical evidence establishes multiple simultaneous pressures/opportunities and heterogeneous work, but not one universal ranking function between them.
- Household expectations were real but authority varied with gender, status, age, property, service and local social context. No universal household commander is accepted here.
- Occupation labels in records can describe status or predominant work without implying exclusive activity.
- Calendar/time could coordinate obligations and work, but the accepted model distinction remains that a clock value constrains or schedules an existing obligation/process; it is not sufficient cause for an unrelated person's motive.
- Pre- and post-Black Death labour/opportunity conditions differ materially. AI decision parameters that affect economic behaviour must remain context-sensitive rather than silently averaged.

## Causal model

Stable shape:

`person state + accepted personal/household pressures + active obligations/commitments + known opportunities + environmental/process windows + current task/progress + relationships/requests + capabilities/resources/rights + decision-relevant knowledge -> candidate tasks -> feasibility/authorization filtering -> controller selection -> task commitment -> destination/travel when needed -> action/process -> consequences -> updated pressures/obligations/opportunities/history`

### Cause precedes activity

`Working`, `Travelling`, `Eating`, `Trading`, `TendingLivestock` or similar presentation/activity labels are **derived state** from a selected task/action, not causes.

A schedule may represent a real appointment, contract window, market opening, ritual/calendar restriction or process deadline **only when an accepted world relationship/process creates that schedule**. The existence of `08:00` by itself never creates `Working`.

## Decision vocabulary

### Reason / pressure

A decision reason is an observable-to-the-actor world fact that can justify considering a task.

Possible reason sources include, when defined by an accepted model:

- personal health/maintenance pressure;
- responsibility for a dependant;
- household maintenance expectation with an accepted basis;
- active contract/service/tenure obligation;
- agricultural/resource process becoming feasible or time-sensitive;
- known shortage or threat of loss;
- known offer, exchange, employment or tenancy opportunity;
- request/coordination from another person under an accepted relationship;
- unfinished current task/project;
- expected migration/household/lifecycle opportunity.

This list is extensible. A reason kind must come from world state or an accepted model; it must not be invented merely to force an animation or route.

### Candidate task

A `TaskCandidate` is a possible intended course of action before controller choice.

Minimum conceptual fields/references:

- actor `Person`;
- reason/source provenance;
- intended outcome or process;
- target subject/person/place when known;
- required capability/skill;
- required rights/permission/authority;
- required tools/resources/inputs;
- earliest/latest/season/environment window when applicable;
- linked obligation/contract/holding/relationship when applicable;
- existing commitment/progress if this continues current work;
- expected material/social consequences known to the actor, including consequence of delay/non-performance where modeled;
- required destination if action cannot occur at current location;
- decision-relevant information provenance.

A candidate is not yet an action and reserves nothing merely by existing unless an accepted planning/commitment rule says so.

### Feasible task

A candidate can be physically feasible only when its hard requirements are satisfiable in the current/planned action window.

Checks may include:

- actor is alive/available/capable;
- necessary knowledge identifies the target/opportunity;
- required resource/tool/labour capacity exists or can be acquired as part of the task plan;
- required environmental/time window is open or reachable;
- required place can be reached under the travel model;
- incompatible current commitments are resolved, interrupted or explicitly breached;
- the requested action is physically possible.

Feasibility is separate from authorization.

### Authorized task

Authorization comes from the accepted Property/Tenure/Common Rights, Contract/Obligation, household-authority or other applicable model.

A physically feasible unauthorized act can remain a candidate when the world allows deliberate trespass, theft, breach or another violation. Such a candidate must be marked as apparently unauthorized/violating rather than silently converted into lawful behavior.

### Selected intention / task commitment

Controller choice turns one candidate into the current intended task or an explicit plan/commitment.

Selection must record enough provenance to answer at least:

- what task was selected;
- what reasons were considered material by the controller;
- which obligation/opportunity/process it was linked to;
- what target/destination it implies;
- whether selection interrupts or abandons another task;
- what known cost/risk/default/loss follows from alternatives left undone where material.

The trace is for deterministic replay/audit, not omniscient UI disclosure.

## Candidate generation rules

1. **World state generates reasons; clock/profession do not.** Time may activate a due condition/window already created by a process or agreement.
2. **Only accepted state can generate canonical reasons.** A prototype `Hunger` meter, affinity score or profession flag cannot become a task generator merely because it exists in current code.
3. **Knowledge matters.** An actor cannot select a specific unknown job, buyer, vacant holding or distant opportunity through global simulation omniscience unless an accepted information mechanism exposes it.
4. **Existing commitments remain candidates.** A task does not reset every hour; continuing work is an ordinary candidate while it remains relevant/feasible.
5. **Failure/neglect remains possible.** A due obligation or urgent process creates a strong reason and consequences; it does not become a supernatural forced action. A controller can choose breach, delay, abandonment or a competing task when physically possible.
6. **One fact may create several candidate responses.** A seed shortage might generate buy, borrow, seek credit, reduce sowing, sell another asset or accept failure depending on available rights/opportunities.
7. **One task may answer several reasons.** Paid harvest work may supply income while satisfying a service obligation; caring for livestock may protect household assets and meet a contract duty.
8. **No exhaustive profession switch.** Candidate generation comes from active world relationships/processes and capabilities, not `switch (Profession)`.

## Controller selection

### Shared world rules

HumanController and AIController receive the same authoritative candidate facts, feasibility checks, rights/authorization state and consequences subject to their modeled knowledge.

Controller type may change **which feasible option is chosen**. It may not change what is physically possible, what the person owns/owes, where the person is, what resources exist or what happens after the action.

### AIController

An AI policy must be deterministic for the same authoritative state/seed and must be explainable as a choice among generated candidates.

Permitted decision inputs include world-state facts such as:

- urgency/deadline/window;
- severity of modeled consequence if delayed/ignored;
- accepted obligation strength/status;
- expected resource benefit/cost;
- current task progress and interruption cost;
- travel/time cost from the accepted travel model;
- relationship/household expectations with an accepted basis;
- risk/uncertainty represented in actor knowledge;
- capability and success likelihood when modeled;
- controller-specific stable preferences/personality only after such traits have an accepted model.

The AI policy must **not** use hidden bonuses of the form:

- `profession == Farmer -> choose farm`;
- `hour == 8 -> choose work`;
- `playerNearby -> choose interaction` unless an ordinary percept/social reason exists;
- `settlementNeedsFood -> every resident may access settlement stock`;
- omniscient access to all future prices/jobs/people/rights.

This contract intentionally does not claim one historically true numeric weighting of the permitted inputs. Policy parameters are explicit controller approximations and must not be described as universal medieval psychology. If a parameter materially changes settlement economy/demography, it becomes part of later calibration/long-horizon evidence rather than hidden design tuning.

### HumanController

HumanController chooses from the same world-feasible possibilities, subject to what the controlled person can know/attempt.

The UI may expose recognized reasons and feasible candidate actions, but must not grant:

- unknown opportunities;
- player-only authorization;
- free cancellation of debts/commitments;
- teleport destinations;
- special resource access;
- automatic success.

A human-controlled person may deliberately choose an economically poor, socially costly, illegal or defaulting action when ordinary world rules make the physical attempt possible.

## Commitment, interruption and switching

A selected task may persist across multiple simulation steps.

Rules:

1. **No hourly reset.** Time advancement alone does not cancel the current intention or return a person to a profession schedule.
2. **Progress persists.** Multi-step work/travel/process state survives until completion, cancellation, interruption or invalidation.
3. **New reasons may trigger reconsideration.** A deadline, new information, process failure, threat, request or completed subtask can cause the controller to select again.
4. **Interruption has consequences.** Abandoning travel/work may consume elapsed time/resources or cause obligation/process loss when applicable.
5. **Commitments constrain future feasibility.** Labour contracted to one task cannot be simultaneously used elsewhere.
6. **Cancellation is an event.** It records cause/time and does not silently erase linked obligations.

Exact reconsideration cadence is an implementation design question. It must be event/state driven enough to avoid recreating `every hour -> rebuild schedule` as the canonical behavior law.

## Destination derivation

A destination exists because the selected task requires a target place/person/resource.

Conceptually:

`selected task + target + current location -> required semantic destination -> travel plan`

Examples:

- selected task is `perform harvest work under Contract C` -> target parcel/work place comes from C/process -> travel model plans movement there;
- selected task is `buy seed from known Seller S` -> destination derives from S/meeting/market context;
- selected task is `care for dependant D` -> destination derives from D's known location/residence when physical presence is required;
- selected task continues work already co-located -> no travel is generated;
- no selected task requires leaving home -> the clock does not invent a commute.

Travel itself remains governed by P3 and later spatial models.

## Rights, obligations and violations

The accepted rights and exchange contracts remain authoritative.

Important distinctions:

- an obligation is a **reason** and potential consequence source, not guaranteed performance;
- a right makes an action authorized, not necessarily desirable;
- physical access can make an unauthorized action possible, but the task must preserve violation semantics;
- non-selection of an obligation does not delete it;
- household membership can contribute reasons/expectations only through accepted household rules, not universal ownership/command;
- employment/service agreement can create a work reason without changing person species or permanent profession;
- a resource reservation can create a reason not to consume/sell it but does not necessarily make consumption physically impossible.

## Knowledge and perception boundary

Decision candidates are limited by modeled knowledge.

Minimum direction:

- a person knows their own current obligations/commitments unless an accepted exception exists;
- knowledge of opportunities, prices, people, locations and rights must have provenance when it is not inherently personal/current;
- stale or wrong beliefs may later be modeled separately; this contract does not require omniscient truth access;
- UI/AI must not query world-global hidden state simply to manufacture the best task.

A full social-information/perception model is deferred. Until it exists, production mechanics should use explicit known references/events rather than invisible omniscient discovery.

## Player/NPC symmetry

The authoritative state path is:

`World state -> Person candidate set -> Controller choice -> Person intention/task -> world action/consequence`

for either:

`AIController -> Person`

or:

`HumanController -> Person`.

Changing controller must not change:

- identity;
- rights/claims;
- household membership;
- obligations/contracts/debts;
- resources;
- task requirements;
- location/travel physics;
- action consequences.

The newly attached controller may select differently from the existing controller, but it cannot receive a different ontology or bypass the candidate/feasibility/authorization layers.

## Rules

1. **Cause before task.** Every canonical task traces to world-state reasons, commitments, process state or known opportunity.
2. **Task before destination.** A destination is derived from intended action; location schedules do not fabricate intentions.
3. **Candidate generation and choice are separate.** The world exposes reasons/options; the controller chooses.
4. **Feasibility and authorization are separate.** Unauthorized but physically possible actions can remain representable violations.
5. **Knowledge bounds choice.** No omniscient global task discovery by default.
6. **Current work persists.** Intentions/tasks are durable state, not recreated from each clock tick.
7. **Obligations can be breached.** Due does not mean forced; consequences persist when performance is skipped.
8. **Labour/time is finite.** One person cannot satisfy incompatible simultaneous commitments.
9. **No profession scheduler.** Profession/occupation may summarize history/status but never select the next task by itself.
10. **No controller privilege.** Human/AI differences are selection-policy differences, not world-rule differences.
11. **Decision trace is persistent enough for replay/audit.** Important selections, cancellations and linked reasons survive save/load where they affect state.
12. **No hidden historical constants.** AI scoring/weights must be explicit implementation policy and cannot be described as historical fact without separate evidence/calibration.

## Long-horizon behavior

This foundation changes how actions are selected but does not by itself define demographic/economic parameters. It therefore cannot independently prove historical economic balance.

When connected to Agriculture, Exchange or later food/demographic mechanics, the Reality Modeling Policy's **>=10 simulated-year proof** must include decision behavior because task selection can materially change labour, production, debt and migration trajectories.

Long-run checks should eventually show that the controller layer does not create impossible state by:

- duplicating labour across simultaneous tasks/contracts;
- dropping obligations because they were not selected;
- creating resources/opportunities through selection itself;
- repeatedly resetting tasks on clock boundaries;
- using omniscient hidden buyers/jobs/resources;
- granting player-controlled persons different feasible actions or consequences;
- trapping all people forever in one occupation despite accepted changing obligations/opportunities;
- generating systematic economic survival only through hidden priority bonuses or guaranteed task success.

A poor controller may produce plausible distress or collapse. Failure is valid if it follows from explicit choices/constraints rather than scheduler corruption.

## Assumptions and uncertainty

- Exact quantitative ranking/utility weights between obligations, household needs, risk, income and opportunities: **not claimed as historical constants**; controller-policy calibration remains later validation work.
- Exact reconsideration cadence and interruption thresholds: implementation design, subject to causal/event-driven constraints above.
- Personality, temperament, altruism, risk tolerance and social preference distributions: **MODEL_UNDERDEFINED** until separately modeled if materially needed.
- Personal health/food/sleep need equations: outside this contract; only accepted need/pressure models may generate canonical reasons.
- Household authority/representation and who may bind member labour/resources: **MODEL_UNDERDEFINED** and a separate immediate blocker.
- Detailed information propagation, rumor, market knowledge and belief accuracy: **MODEL_UNDERDEFINED**.
- Travel duration/routing/cost and routine movement: blocked on accepted P3 travel model.
- Social/religious calendar obligations and institutional office schedules: require their own accepted basis before becoming task generators.
- Quantitative opportunity frequency, wages, prices, migration and agricultural parameters remain under the corresponding accepted/deferred contracts.

These limits mean this contract is a **causal selection foundation**, not a claim that one AI heuristic reproduces medieval private cognition.

## Fixture boundary

The following current/prototype patterns are explicitly noncanonical:

- `07:00 -> commute`;
- `08:00 -> Working`;
- `17:00 -> Home`;
- `Profession.Farmer/Cook/Forager -> one fixed task`;
- one permanent `WorkplaceId` acting as motive;
- reconstructing activity/location from clock hour during save/load;
- selecting `ShareRation`, `AskAboutWork` or `Encourage` because they are the only hard-coded interaction verbs;
- using `_settlementOwnerId`/settlement inventory as an automatic answer to shortages;
- AI knowing every market offer, resident need, vacancy or resource globally;
- clearing current work simply because a simulation hour advanced;
- changing feasible options because the actor is player-controlled.

Existing regression tests may temporarily assert these fixtures only as noncanonical pipeline seams and must be changed when the accepted model replaces them.

## Falsifiers

Revise this model if evidence or implementation shows that:

- ordinary rural action can be represented causally by one exhaustive profession schedule without losing documented multi-activity household/labour behavior;
- candidate generation and controller choice cannot be separated without introducing hidden authority/resource creation;
- tasks can be derived from location/time alone while still preserving accepted obligations, opportunities and seasonal process causality;
- persistent current tasks/commitments are unnecessary for multi-step work/travel and save/load;
- AI/Human controllers require different rights, action requirements or consequence rules to function;
- long-horizon trajectories require hidden omniscience or fixed occupation bonuses rather than explicit world-state reasons.

## Feedback and observability

Debug/audit projections should be able to expose, for authorized development tooling:

- current selected intention/task;
- reason/source references that generated it;
- target/destination requirement;
- linked obligation/contract/process;
- why obvious alternatives were infeasible or unavailable where practical;
- interruption/cancellation reason;
- important consequence of missed obligation/window.

Player UI should expose only what the controlled person can know and what is useful for decision-making. It must not reveal every hidden candidate, score, future outcome or private counterparty state.

## Persistence

Persist enough authoritative state to reconstruct exactly:

- current intention/task and stable identity/reference;
- linked obligation/process/target references;
- task progress/commitment state;
- relevant selected-reason provenance where needed for replay/audit;
- interruption/cancellation/completion history that changed world state;
- controller-independent Person identity;
- decision-relevant known references that must survive save/load.

Do **not** reconstruct current task, destination or activity from profession, workplace, settlement membership or clock hour.

## Acceptance scenario

A future structural implementation should be able to demonstrate:

1. Person P has several simultaneous reasons: an unfinished local task, a known paid work opportunity, an accepted obligation approaching its due window, and an agricultural task whose environmental window is narrowing.
2. Candidate generation produces only tasks that follow from those explicit world facts; it does not add a profession-default task because the clock reaches 08:00.
3. Feasibility checks reject candidates whose required tool/resource/capability cannot be obtained; authorization is evaluated separately.
4. AIController deterministically chooses one feasible candidate using explicit decision inputs and records the chosen reason/target. The same state/seed replays the same choice.
5. Choosing one task commits finite person time. Conflicting labour commitments cannot execute simultaneously.
6. If the selected task requires another semantic place, that task supplies the destination request to P3; travel is not created by a commute schedule.
7. If P ignores an obligation or urgent process in favour of another feasible task, the skipped obligation/window remains in world state and ordinary breach/loss consequences can occur.
8. The task persists across hour advancement until completion, interruption, invalidation or a controller reconsideration event; no hourly scheduler silently replaces it.
9. HumanController may take over P and sees/chooses from the same physically/socially possible world actions subject to P's knowledge, while rights/resources/consequences remain unchanged.
10. Save/load/replay preserves P's current task, commitment/progress and resulting destination/action chain.

This scenario proves the causal bridge. It does not prove historically calibrated AI preference weights, household authority, travel duration or long-run economic viability.

## Deferred complexity

Separate bounded work is still required for:

- household authority/representation and labour/resource commitment rules;
- P3 semantic travel duration/routing and historically grounded routine movement constraints;
- richer information/perception/social knowledge;
- personality/preference models if they become causally necessary;
- calibrated AI decision policy where its aggregate effects materially determine economy/demography;
- health/food/rest need models beyond any accepted structural seams;
- institution/religion/social-calendar obligations if implemented;
- >=10-year integrated validation for P5/P6 economic/demographic behavior.

Completing this contract does not authorize production code to fill any of those gaps with convenient constants.