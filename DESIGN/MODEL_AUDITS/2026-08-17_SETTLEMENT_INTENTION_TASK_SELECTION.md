# Audit — Settlement Intention and Task Selection

Audit date: **2026-08-17**

Verdict: **PASS**

Model contract: `DESIGN/MODELS/SETTLEMENT_INTENTION_TASK_SELECTION.md`

Reviewed research/model SHA: `da6fe92fbd9f0e6adceeb2caaee2a459eeaab44d`

## Scope

Independent audit of the structural intention/task-selection foundation only. No production simulation code was reviewed or changed. This audit does not accept one universal medieval preference function, numerical AI utility weights, personality distributions, household command authority, travel duration/routing, demographic rates or economic calibration.

The accepted Person/Household, Property/Tenure/Common Rights, Agricultural Year and Exchange/Contracts/Migration contracts are treated as dependencies. Household authority/representation remains a separate material model gap. `DESIGN/MODELS/P3_SEMANTIC_LOCATION_AND_TRAVEL.md` remains `MODEL_UNDERDEFINED`; this audit accepts only the upstream causal seam from selected task to destination request, not current prototype travel durations or schedules.

## Repository and CI

The exact reviewed SHA was branch HEAD `da6fe92fbd9f0e6adceeb2caaee2a459eeaab44d` at audit time. The commit added only `DESIGN/MODELS/SETTLEMENT_INTENTION_TASK_SELECTION.md`.

Required GitHub Actions on the reviewed SHA all passed:

- `ci #139` — success;
- `playable-prototype-gate #160` — success;
- `proof-a-measure #134` — success.

## Load-bearing fact re-check

### Rural work was not one exhaustive profession script

Re-checked Christopher Dyer, _Peasants Making History: Living in an English Region 1200–1540_, chapter 9, “Peasants and industry” (Oxford University Press, 2022):

https://academic.oup.com/book/43934/chapter/370551541

The chapter describes widespread non-agricultural activity in the countryside and peasants combining farming with fishing, food trades, building work and crafts. It explicitly notes the significance of part-time work and the participation of women and young people.

Audit conclusion: it is supported to reject `Profession -> one next task` as a sufficient causal rule and to permit several kinds of candidate work when world relationships/resources/opportunities justify them. The source does not establish a universal occupation mix, daily schedule or task probability.

### Household/family context can constrain available work without supplying a universal task table

Re-checked P. J. P. Goldberg, _Women, Work, and Life Cycle in a Medieval Economy: Women in York and Yorkshire c.1300–1520_, chapter 3, “Women and Work” (Oxford University Press, 1992):

https://academic.oup.com/book/7906/chapter-abstract/153157934

Goldberg situates work in both familial and wider urban/rural economies and explicitly considers wealth/training, marital status, local economic needs and household/family responsibilities as constraints on access to work.

Audit conclusion: household responsibilities and local opportunity are justified as possible decision inputs, but the source does not support a fixed gender/occupation schedule or one household allocation algorithm.

### Household labour expectations are real, but command authority remains underdefined

Re-checked Christopher Dyer, _Peasants Making History_, chapter 5, “Family and household”:

https://academic.oup.com/book/43934/chapter/370549926

The chapter describes households that could include unrelated servants and states that expectations of discipline and hierarchy were intended in part to secure household labour and orderly succession.

Audit conclusion: household expectations can create reasons/pressures affecting a person's choices. This does **not** establish one universal `HeadOfHousehold` authority set or automatic power to bind every member's labour/resources. The contract correctly keeps household authority/representation as a separate blocker.

### Hired work and service create commitments outside one household/profession

Re-checked Mark Bailey, “The regulation of the rural market in waged labour in fourteenth-century England,” _Continuity and Change_ 38(2) (2023):

https://www.cambridge.org/core/journals/continuity-and-change/article/regulation-of-the-rural-market-in-waged-labour-in-fourteenthcentury-england/C7726EC8A2D0C628ACFF49428FDA95A7

Bailey's systematic analysis of 1,445 manorial court sessions supports a sizeable rural hired-labour market. The article distinguishes irregular/discontinuous day or piece work from servants under longer contracts and shows that the legal/regulatory environment changed materially after the Black Death.

Audit conclusion: accepted labour/service commitments and opportunities can compete with household/self-directed tasks, and pre-/post-1348 conditions must not be collapsed into one timeless parameter. Bailey does not justify a universal wage threshold or priority rule.

### Season/calendar can change agricultural feasibility without selecting a person's action

Re-checked Helena Hamerow et al., _Feeding Medieval England: A Long ‘Agricultural Revolution’, 700–1300_, chapter “Crop Rotation and Seasonal Sowing” (Oxford University Press, 2025):

https://academic.oup.com/book/61548/chapter/537298429

The chapter supports regular sowing/fallow sequences and autumn/spring sowing in relevant systems while also recognizing irregular rotations and local variation.

Audit conclusion: season/date may open, close or increase urgency of a real agricultural process window. The evidence does not establish `calendar value -> specific person performs work`, one universal crop calendar or a fixed daily routine.

### Cooperation can create shared commitments/opportunities without a collective village actor

Re-checked Christopher Dyer, “Partnership among peasants: rural England, 1270–1520,” _Continuity and Change_ 37(3):

https://www.cambridge.org/core/journals/continuity-and-change/article/partnership-among-peasants-rural-england-12701520/F8DB6A2A76E46C44687718E4FDEA8CC8

Dyer defines partnership as people pursuing common objectives in mutual cooperation and documents joint landholding/clearing, paid work in pairs, and other collaborative activities. The article also emphasizes incomplete/indirect evidence and regional limits.

Audit conclusion: collaboration can legitimately generate candidate work/commitments, but no universal cooperation rate, command relationship or settlement-wide collective actor is supported.

### Future opportunities can become intentions without perfect information or guaranteed betterment

Re-checked Christopher Dyer, _Peasants Making History_, chapter 4, “Peasants changing society”:

https://academic.oup.com/book/43934/chapter/370549741

The chapter describes peasants migrating, often over short distances, in pursuit of land, employment and marriage in attempts to improve circumstances, while noting limited and unequal mobility.

Audit conclusion: known opportunities may generate candidate intentions. The evidence does not support omniscient opportunity discovery, guaranteed improvement, a universal migration threshold or unrestricted movement.

## Causal model review

**PASS.**

The accepted structural topology is:

`world state -> reasons/pressures/obligations/opportunities -> candidate tasks -> feasibility + authorization -> controller selection -> committed task -> destination/travel when required -> action/process -> consequences -> updated world state`

The topology correctly separates cause from activity, candidate generation from selection, physical feasibility from authorization, and task selection from destination/travel. Existing work/commitments persist rather than being reconstructed every hour. Due obligations or process windows can be neglected or breached with consequences instead of becoming supernatural forced actions.

This audit does **not** accept one numerical ranking function as historical truth. AI weights/scoring remain explicit controller policy and become later calibration/long-horizon evidence if they materially determine settlement trajectories.

## Player/NPC symmetry review

**PASS.**

HumanController and AIController act on the same ordinary `Person`, candidate facts, feasibility checks, rights, obligations, resources and action consequences. Controller type may change the selected feasible option; it may not change world permissions, create resources, expose hidden opportunities or bypass travel/action requirements.

## Rights, obligations and household-authority review

**PASS for this contract's declared scope.**

The contract correctly relies on accepted rights/contracts for authorization and preserves physically possible unauthorized actions as possible violations. Obligations remain world state when ignored.

Household membership alone does not authorize assignment of tasks or commitment of household resources. Exact household authority/representation remains `MODEL_UNDERDEFINED` and is not silently filled by this contract.

## Knowledge/perception boundary review

**PASS for structural scope / richer information model deferred.**

The contract prevents AI/global systems from inventing known buyers, jobs, rights or opportunities through omniscient queries. It requires decision-relevant knowledge provenance when information is not inherently personal/current. Detailed belief error, rumor and information propagation remain deferred rather than invented.

## Uncertainty and fixture-boundary review

**PASS.**

The contract explicitly rejects the current `07:00 -> commute`, `08:00 -> Working`, `17:00 -> Home`, profession-driven task selection, hourly task reset, settlement-stock shortcuts and controller privilege as canonical law.

Numerical utility weights, personality/preferences, health/food/rest equations, household authority, perception/information propagation, travel routing/duration and economic/demographic calibration remain explicit deferred areas.

## Long-horizon review

**PASS for structural contract acceptance, not for implemented economic/demographic viability.**

Task selection can materially change labour, production, debt and migration outcomes. Therefore later integrated Agriculture/Exchange/P5/P6 behavior remains subject to the Reality Modeling Policy's >=10 simulated-year proof, including the effects of the controller policy. A structurally accepted decision seam does not itself establish balanced or historically calibrated trajectories.

## Final verdict

**PASS.**

No load-bearing evidence, causal, symmetry, rights, uncertainty or fixture blocker prevents the Intention/Task Selection foundation from becoming `ACCEPTED` in its declared reference context.

`ACCEPTED` approves the causal boundary and shared controller/world-rule topology only. It does not approve household command authority, P3 travel calibration, omniscient information, historically calibrated AI preference weights or long-run economy/demography. Those remain separate blockers where materially required.