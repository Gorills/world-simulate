# Settlement Agricultural Year, Seed, Labour and Livestock — Model Contract

Status: **ACCEPTED**

This contract defines the minimum causal structure for arable production, seed continuity, seasonal work, labour allocation, draft power, livestock maintenance and pasture dependence. It does **not** define universal medieval English crop calendars, crop yields, seed rates, labour coefficients, herd structures, fodder requirements or grazing capacities; those remain local/calibration questions and are `MODEL_UNDERDEFINED` where material.

## Mechanic

Represent agriculture as a chain of physical and social processes acting on explicit land, seed, labour, tools/animals, rights and environmental conditions rather than as `profession + clock -> output`.

For a sown arable crop, harvest must arise from a parcel that was legitimately available for cultivation, prepared and sown with an actual seed resource, tended through the relevant environmental interval, harvested with sufficient labour/capability and converted into stored resource lots subject to losses and competing claims such as food, seed, fodder, rent or sale. Other agricultural outputs such as meadow hay require their own causal land/resource process and must not inherit a seed requirement merely because they are harvested.

## Intended feeling

Agriculture should feel like a coupled household/land/resource problem rather than a background animation. A household may have land but insufficient labour, seed or draft power; may have grain but be unable to consume all of it safely because next year's seed must be retained; may own animals but lack pasture/fodder rights; or may have labour available but no legitimate access to a parcel.

The player and AI should face the same feasible-task constraints and the same consequences of bad timing, missing inputs, weather, labour scarcity or resource diversion.

## Dependencies

This contract depends on the accepted foundations:

- `DESIGN/MODELS/SETTLEMENT_PERSON_HOUSEHOLD_LIFECYCLE.md` — ordinary `Person`, household labour/maintenance pressures and controller symmetry;
- `DESIGN/MODELS/SETTLEMENT_PROPERTY_TENURE_COMMON_RIGHTS.md` — explicit cultivation/use/common rights, holdings and obligations.

Neither household membership nor physical presence grants cultivation, grazing, harvesting or removal rights.

## Reference context

Baseline: rural lowland England, especially arable and mixed husbandry, approximately 1270–1348 for first calibration, with 1350–1450 retained as a separate stress/validation regime because labour supply, land use and bargaining conditions changed sharply after the Black Death.

Evidence from earlier medieval England may be used where it establishes durable physical husbandry processes such as foddering or draft-animal dependence. Evidence from later periods may clarify a process only when the contract does not back-project later numerical values or institutional rules into the baseline.

This is not a universal model of every English region. Upland, strongly pastoral, fenland, woodland and specialized commercial regimes require different parameterization and may require additional process variants.

## Evidence ledger

### 1. Crop rotation and seasonal sowing are structured constraints, not a personal schedule

**Helena Hamerow et al., _Feeding Medieval England: A Long ‘Agricultural Revolution’, 700–1300_, chapter “Crop Rotation and Seasonal Sowing” (Oxford University Press, 2025).** The synthesis describes regular open-field systems using two- or three-course rotations, including autumn-sown and spring-sown courses plus fallow. Walter of Henley is cited for a three-part winter-seed / spring-seed / fallow arrangement.

- https://academic.oup.com/book/61548/chapter/537298429
- Supports: parcels/courses can have crop-state and seasonal sowing constraints; rotation/fallow are meaningful state transitions.
- Does **not** support: every settlement using one three-field layout, identical crop mixes, or `date -> resident activity` as a behavioral law.

### 2. Soil fertility, manure, pasture and livestock are coupled to arable production

**Hamerow et al., chapter “The Intensity of Cultivation: Soil Fertility and the Expansion of Arable.”** The chapter discusses low-input cereal production, soil fertility, manuring and the need to feed/graze animals, including working animals kept near settlements and supplied with hay/fodder.

- https://academic.oup.com/book/61548/chapter/537294899
- Supports: parcel fertility/condition and manure/fodder flows can affect production; livestock and arable systems cannot be modeled as entirely independent stock generators.
- Does **not** establish one manure-response function, stall-feeding ratio or fertility-decay constant for 1270–1450.

### 3. Seed is a production input and yield must not be confused with disposable surplus

**Hamerow et al., concluding synthesis “A Long ‘Agricultural Revolution’.”** The synthesis reports thirteenth-century crop yields commonly discussed as grain returned per grain sown, broadly around 3:1–5:1 in the cited literature, while emphasizing low-input/low-fertility conditions.

- https://academic.oup.com/book/61548/chapter/537305187
- Supports: sown seed is a material input and gross harvest is not equivalent to net household-consumable surplus.
- Simulation consequence: harvest accounting must preserve the possibility of reserving seed for a future sowing cycle before all grain is consumed/sold.
- The 3:1–5:1 range is **not accepted as a global simulation constant**; yield calibration remains separate and context-specific.

### 4. Draft animals are productive capital with continuing maintenance costs

**J. Michael Jefferson, _The Templar Estates in Lincolnshire, 1185–1565_, chapter “Livestock, Excluding Sheep, on the Former Templar Estates, 1308–13” (Boydell & Brewer/Cambridge Core edition).** The account evidence describes draught animals as essential to arable farming: oxen and/or horses prepared land for sowing, while carthorses and oxen hauled crops; livestock also depended heavily on the harvest for provender, and cattle reproduction supplied replacement oxen.

- https://www.cambridge.org/core/books/abs/templar-estates-in-lincolnshire-11851565/livestock-excluding-sheep-on-the-former-templar-estates-130813/9DBFCFCF9A1985BF298DD8673558C0BF
- Supports: draft capacity is a real production constraint and animals are linked to feed/harvest continuity rather than decorative inventory.
- Does **not** establish peasant household ownership rates, a universal ox/horse mix, fixed team size or national feed quantities; this is a particular estate complex.

**Jordan Claridge and Spike Gibbs, “Waifs and Strays: Property Rights in Late Medieval England,” Appendix A.** Their draft-horse maintenance discussion separates food/fodder, shoeing/harnessing/stabling and depreciation.

- https://www.cambridge.org/core/journals/journal-of-british-studies/article/waifs-and-strays-property-rights-in-late-medieval-england/148ADDD32647806A4793D0AB2933F888
- Supports: a working animal consumes resources and requires maintenance over time; draft power is not free.
- Exact maintenance quantities/prices remain calibration data, not canonical constants here.

### 5. Pasture/common access matters to livestock, but regional husbandry differs

**Angus J. L. Winchester, “Shielings and Common Pastures,” in _Northern England and Southern Scotland in the Central Middle Ages_.** The chapter stresses the dependence of livestock husbandry on pasture and contrasts more pastoral northern regimes with southern/Midland patterns.

- https://www.cambridge.org/core/books/northern-england-and-southern-scotland-in-the-central-middle-ages/shielings-and-common-pastures/AF4ECC96C7790140E94358918CB03BFD
- Supports: livestock viability depends on access to grazing/fodder resources and those rights/landscapes vary regionally.
- Does **not** justify importing northern transhumance or herd structures into the lowland calibration baseline.

The accepted Property/Tenure/Common Rights contract remains authoritative for who may use pasture/common resources.

### 6. Village livestock quantities are especially uncertain

**M. M. Postan, “Village livestock in the thirteenth century.”** Postan emphasizes that evidence for villagers' livestock is much weaker than evidence for demesne flocks/herds because manorial sources record peasant animals selectively.

- https://www.cambridge.org/core/books/abs/essays-on-medieval-agriculture-and-general-problems-of-the-medieval-economy/village-livestock-in-the-thirteenth-century/F4A56149B662C254646462978431E6B2
- Supports: do not infer a universal peasant herd-size distribution from demesne accounts or later husbandry.
- V1 structural livestock entities are acceptable; spawn ratios/herd distributions remain `MODEL_UNDERDEFINED`.

### 7. Labour availability and labour institutions changed sharply across 1348

**Mark Bailey, “The regulation of the rural market in waged labour in fourteenth-century England,” _Continuity and Change_ (2023).** Bailey reconstructs a substantial hired rural labour market before the Black Death and a major post-plague transformation in labour scarcity, wages, mobility and regulation. His re-assessment also rejects older over-generalizations that pre-plague villages universally fixed harvest wages or compelled all residents to reap locally.

- https://www.cambridge.org/core/journals/continuity-and-change/article/regulation-of-the-rural-market-in-waged-labour-in-fourteenthcentury-england/C7726EC8A2D0C628ACFF49428FDA95A7
- Supports: household labour, hired/day/piece labour, service and obligations can all participate in agricultural work; labour scarcity/opportunity is historically contingent.
- Supports keeping pre-1348 and post-1348 labour regimes separate.
- Does **not** justify one fixed share of household/hired/service labour for every settlement.

### 8. Weather and labour shocks can jointly reduce realized production

**“Surviving the Black Death in medieval England: recovering from illness at Warboys, Huntingdonshire,” _Historical Research_ (2026), drawing on contemporary chronicles and Campbell's yield work.** The article describes acute labour shortage, uncultivated land, livestock-management failure and very poor harvest outcomes in the early 1350s, compounded by adverse weather.

- https://academic.oup.com/histres/article/99/285/376/8676553
- Supports: realized agricultural outcomes need causal sensitivity to labour disruption and environmental conditions; acreage alone does not guarantee output.
- Does not justify turning the exceptional 1349–1352 crisis into the normal baseline.

### Evidence limits and disagreement

- Demesne accounts are much richer than peasant-household records. Demesne yields, herds and labour organizations cannot be copied directly into every household.
- Medieval rotations varied by locality, soil and field system. `three-field` is a possible regime, not universal world law.
- Yield per seed is context-sensitive to crop, soil, weather, husbandry and record methodology. No global yield multiplier is accepted here.
- Peasant livestock abundance/distribution is poorly observed; no fixed livestock-per-household spawn table is accepted.
- Pre- and post-Black Death labour conditions must not be averaged into one timeless coefficient.
- Exact sowing/harvest dates varied by crop, weather and locality; seasons constrain task feasibility but do not generate intentions automatically.

## Causal model

Stable shape:

`household/person pressures + recognized land/resource rights + parcel state + crop/rotation state + season/weather + seed/input stocks + available labour + skills + tools/draft capacity + obligations/opportunities -> feasible agricultural tasks -> controller choice/allocation -> travel/action/process -> parcel/crop/livestock/resource consequences -> storage/reservations/obligations -> next-cycle options`

There is no canonical transition of the form:

`hour/day/month -> Farmer.Work()`

Calendar/environment instead changes the feasible/urgent task set.

Examples:

- A household holds cultivation rights to Parcel P, autumn sowing is agronomically feasible, seed is reserved, draft/tool capacity and labour are available -> preparing/sowing P can become a high-value feasible task.
- The same date arrives but the household lacks seed -> sowing is infeasible unless seed is acquired, borrowed or otherwise legitimately obtained.
- A crop reaches a harvestable state and poor weather threatens losses -> harvest urgency rises, but actual harvest still requires labour, access and tools.
- A household lacks enough internal labour during harvest -> it may hire labour, call on an accepted service obligation, cooperate with others, reduce harvested acreage or suffer losses depending on later contract/economic rules.
- Grain enters storage after harvest -> quantities may be reserved for consumption, future seed, fodder, dues/obligations or exchange -> consuming/selling the seed reservation can increase present welfare while reducing next-cycle capability.
- Draft animals increase feasible work/transport capacity -> they simultaneously create pasture/fodder and maintenance requirements.
- Livestock have lawful common/pasture access but forage availability is insufficient -> condition/productivity can decline; a right to graze is not a guarantee of unlimited forage.

## Core entities

### LandParcel

Minimum conceptual state:

- stable identity and place/spatial reference;
- area/extent in whatever spatial abstraction later becomes canonical;
- rights/holding references from the accepted property model;
- land-use/cultivation state;
- soil/fertility/condition representation sufficient for the accepted production model;
- current crop/fallow/pasture state where applicable;
- process/history references.

Exact soil chemistry is deferred. The model only requires that persistent land condition can matter and is not recreated from the clock.

### CropCycle / CropStand

Minimum conceptual state:

- parcel and crop/crop-family identity;
- sowing/input event and seed quantity/provenance;
- development/seasonal state;
- environmental exposure relevant to later yield modeling;
- labour/process events already performed;
- harvestable/failed/harvested state;
- output provenance.

A crop is not a timer that materializes yield after N hours regardless of inputs or weather.

### ResourceLot and reservations

Existing/future resource lots must be able to distinguish grain by owner/holder, quantity, storage and relevant quality/state.

Agricultural continuity additionally requires explicit reservations/claims such as:

- food/maintenance;
- **seed** for a future sowing;
- fodder/feed;
- rent/tithe/other accepted obligation;
- exchange/sale allocation.

A reservation does not necessarily make grain physically untouchable. It represents planned/recognized use so consumption of reserved seed can have explicit consequences rather than being silently replenished.

### AgriculturalTask / ProductionProcess

Canonical work is task/process based, not profession based.

Examples include:

- manure/prepare parcel;
- plough/harrow or other accepted soil preparation;
- sow;
- weed/tend where modeled;
- mow/haymaking;
- reap/cut;
- bind/gather/cart;
- thresh/winnow/process;
- feed/water/tend livestock;
- move livestock to/from lawful pasture;
- maintain tools/harness/fences where later modeled.

Each task may require combinations of:

`person labour + skill/capability + right/permission + place + season/environment window + tool + draft animal + material input + time`

Missing requirements make the task infeasible or change its outcome; profession labels do not conjure them.

### Labour allocation

Agricultural labour may come from:

- household members who choose/are expected to contribute under the household/life-cycle model;
- hired day/piece workers;
- servants/employees under an accepted service/employment arrangement;
- labour-service obligations where an applicable holding/custom creates them;
- cooperation/partnership under a later contract.

No source is universally preferred. The controller/economic model later chooses among feasible sources based on obligations, availability, cost, relationships and urgency.

A person can perform multiple kinds of work over time. `Farmer` is not a fundamental simulation species.

### LivestockGroup

V1 may aggregate similar animals into cohorts where individual identity is not causally required.

Minimum conceptual state for a group:

- species/type and count;
- holder/right references;
- location;
- condition/health sufficient for production and survival;
- age/reproductive structure only to the degree supported by a later livestock-demography model;
- feed/fodder/pasture requirements;
- productive roles/capabilities such as draft, milk, wool, manure or meat where accepted;
- history of major changes.

Exact reproduction/mortality/productivity rates remain `MODEL_UNDERDEFINED` until separately calibrated.

### Draft capacity

Draft capacity is a capability produced by available animals + condition + harness/tool + handler + time, not a boolean property of a farm.

Using draft animals consumes labour/time and contributes to animal maintenance requirements. Lack of draft capacity may reduce the acreage/timeliness of feasible preparation/transport rather than triggering free substitute power.

## Calendar, season and weather

Time is an environmental coordinate and deadline system, not an actor motivation.

The model may define crop/process windows such as:

- preparation/sowing windows;
- growing periods;
- harvest windows;
- haymaking windows;
- winter fodder pressure;
- pasture availability changes.

Within those windows, household/person decisions still depend on resources, rights, competing tasks and expectations.

Weather/environment may affect:

- task feasibility/timeliness;
- crop development and damage;
- haymaking/storage quality;
- pasture/fodder availability;
- realized harvest quantity/quality;
- transport/access where later modeled.

Exact stochastic weather distributions and crop response functions are outside this contract.

## Player/NPC symmetry

HumanController and AIController operate on the same `Person`, tasks and agricultural process rules.

Controller type cannot grant:

- free seed;
- free access to a parcel/common;
- free labour;
- free tools/draft power;
- guaranteed crop success;
- exemption from livestock maintenance;
- instantaneous harvest/production.

A player may choose to consume seed, neglect livestock, plant late, hire labour or abandon a parcel if those actions are physically/socially feasible. AI should face the same consequence topology even if its decision policy differs.

## Rights and authorization

The accepted Property/Tenure/Common Rights contract governs authorization.

Agricultural tasks may require different rights over the same subject:

- occupation/residence does not imply cultivation;
- cultivation does not automatically imply crop-removal/transfer rights for every actor;
- livestock ownership does not imply pasture rights;
- common-right eligibility does not imply unlimited stocking;
- employment at a place does not imply ownership of inputs/outputs.

Agricultural production must not route all outputs into a magical settlement stockpile. Output ownership/claims derive from the relevant holding, inputs, contracts, household rules and obligations defined by accepted models.

## Rules

1. **Cause before calendar state.** Season changes feasibility/urgency; it does not assign an activity directly.
2. **Every crop output has provenance.** Parcel + seed/input + process + environment + labour/capability -> output.
3. **Seed is conserved as a real resource.** Sowing consumes seed; future seed must come from retained/acquired stock, not automatic regeneration.
4. **Gross harvest is not disposable surplus.** Food, future seed, fodder, dues and other claims may compete for the output.
5. **Land rights are explicit.** A resident may not cultivate/harvest a parcel merely because it is near the settlement.
6. **Labour is finite and allocatable.** A person cannot simultaneously satisfy incompatible tasks; labour shortages can leave work undone.
7. **Draft power is finite and maintained.** Animals/tools do not provide free throughput.
8. **Livestock require resources.** Pasture/fodder/water/care and lawful access must exist where relevant.
9. **Rights do not guarantee ecology.** A valid common right does not create forage or erase overuse.
10. **No universal three-field law.** Rotation is an explicit local regime/configuration.
11. **No universal yield/labour/herd constants.** Quantitative calibration is separate and evidence-bound.
12. **Production failure is valid.** Poor weather, late work, missing seed, labour loss or exhausted resources may causally reduce/cancel output.

## Long-horizon requirement

Because this model directly changes settlement resources and productive capacity, it cannot PASS as an implemented economic system without a future >=10 simulated-year proof under the Reality Modeling Policy.

That proof must eventually track at least:

- parcel use/crop/fallow state;
- seed stocks and seed reservations;
- harvest/storage flows and losses;
- household consumption pressure;
- labour supply/allocation and major demographic interruptions;
- tools/draft capacity;
- livestock count/condition and feed/pasture pressure;
- cultivation/common rights and linked obligations;
- environment/weather shocks;
- exchanges/acquisitions required to fill shortages.

Required invariants include:

- no seed creation from nothing;
- no harvest without a causal crop/process history;
- no duplicate labour/draft capacity;
- no unrestricted consumption of common resources by non-right-holders;
- no livestock survival/productivity without the accepted maintenance path;
- save/load/replay preserving parcel/crop/seed/livestock/process state;
- both plausible survival and plausible collapse accepted when their causes are explainable.

This contract defines structure only; it does not claim that any current numerical setup is historically balanced for ten years.

## Assumptions and uncertainty

- Exact crop set and crop shares for the reference settlement: **MODEL_UNDERDEFINED**.
- Exact two-/three-course or other rotation configuration: **MODEL_UNDERDEFINED** until a concrete regional/local calibration is selected.
- Exact sowing/harvest windows by crop and locality: **MODEL_UNDERDEFINED**.
- Seed rates and yield distributions: **MODEL_UNDERDEFINED**.
- Soil fertility/manure response: **MODEL_UNDERDEFINED**.
- Labour-hours/days per agricultural operation and substitution between human/draft power: **MODEL_UNDERDEFINED**.
- Household vs hired vs servant vs labour-service shares: **MODEL_UNDERDEFINED** and period-dependent.
- Draft-animal prevalence/mix among peasant households: **MODEL_UNDERDEFINED**.
- Livestock herd-size distributions, fertility, mortality and production: **MODEL_UNDERDEFINED**.
- Pasture carrying capacity, fodder yields and common-right stocking limits: **MODEL_UNDERDEFINED**.
- Storage spoilage/loss rates: **MODEL_UNDERDEFINED**.

These are not permission to choose convenient gameplay constants. They are explicit future calibration blockers for any mechanic that depends materially on them.

## Fixture boundary

The following prototype patterns are explicitly noncanonical:

- `Profession.Farmer -> produce Grain` as the causal production law;
- a fixed workplace automatically making a person an agricultural worker;
- `08:00 -> Working` or another hour-driven agricultural activity;
- grain appearing in `_settlementOwnerId` inventory solely because a farmer worked;
- settlement stock automatically feeding everyone without rights/reservations;
- infinite or implicit seed supply;
- harvest output with no parcel/crop provenance;
- all households having identical crop access, labour capacity or livestock;
- common pasture available to every settlement resident;
- livestock as static inventory counts with no location/feed/maintenance consequences;
- migration resetting work assignment while leaving land/resource relations implicit.

Existing profession/workplace/settlement-inventory fixtures may be useful migration seams but must not constrain the canonical model.

## Falsifiers

Revise this model if evidence or implementation shows that:

- explicit seed continuity is unnecessary to explain multi-year cereal production;
- parcel/process provenance adds no causal distinction over `worker -> output`;
- labour availability/timing has no meaningful influence on realized agricultural work;
- draft animals can be represented without maintenance/resource coupling while preserving the selected context;
- common/pasture rights and forage availability never constrain livestock viability;
- the parcel/crop/task abstraction cannot represent accepted local rotations without excessive special cases;
- ten-year proofs systematically require hidden replenishment or arbitrary balancing to avoid collapse.

## Feedback

Presentation may expose, subject to later knowledge/visibility rules:

- parcel/crop state and known next feasible operations;
- shortages blocking a task, such as seed, labour, tool, draft animal or right;
- household grain reservations by intended use where known;
- livestock condition and feed/pasture pressure;
- harvest results and major loss causes;
- due agricultural obligations.

UI must not show omniscient future yields or hidden legal/ecological truth merely because the player opened a farm screen.

## Persistence

Persist enough authoritative state to reconstruct exactly:

- parcel identity, use/crop/fallow state and relevant condition;
- crop-cycle identity, seed/input provenance and completed process history;
- agricultural task/process state when interrupted across saves;
- resource lots and seed/fodder/obligation reservations;
- livestock groups, location, holder and condition;
- relevant tool/draft capability state;
- rights/holding references;
- major environment exposure required for deterministic replay.

Do not reconstruct crop state, agricultural activity, seed ownership, animal location or rights from profession, workplace, household, controller type or clock hour.

## Acceptance scenario

A future structural implementation should be able to demonstrate:

1. Household H has a recognized cultivation basis for Parcel P but only a finite grain stock.
2. H reserves part of that grain as seed; sowing P consumes the reserved seed and records provenance.
3. Preparing/sowing requires available people and, where the selected process requires it, tool/draft capacity; unavailable inputs delay or reduce the feasible operation rather than being synthesized.
4. Crop C develops through its environmental interval; a calendar window makes harvest feasible/urgent but does not directly set any person's `Activity`.
5. H allocates household labour or legitimately obtains external labour to harvest; insufficient/tardy labour can produce loss/unharvested crop.
6. Harvest creates resource lots linked to P/C and the relevant rights/obligations rather than a settlement-global free stock.
7. H may reserve part of harvest for next seed, consume part, owe part or exchange part. Consuming next year's seed changes future options.
8. A livestock group provides draft capability only while its condition and maintenance inputs permit; lawful pasture access does not produce infinite forage.
9. HumanController can take over any participating `Person` and faces exactly the same rights, task requirements and resource consequences.
10. Save/load/replay reproduces the same parcel/crop/seed/livestock/task state.

This scenario proves topology, not calibrated historical yields or herd economics.

## Deferred complexity

Separate bounded work is still required for:

- crop-specific calibrated agronomy and local rotation selection;
- detailed soil/fertility/manure ecology;
- livestock reproduction, disease, slaughter and product yields;
- commons carrying capacity and grazing ecology;
- tools, maintenance and capital replacement;
- exchange/credit/contracts for acquiring seed, animals, tools and labour;
- rent/tithe/service quantities and enforcement;
- weather generation and crop-response calibration;
- household food allocation/nutrition;
- detailed demographic labour supply.

Deferring those does not invalidate this structural contract, but any production mechanic that needs their numbers must remain blocked until the relevant model/calibration is accepted.