# P3 Semantic Location and Travel — Model Contract

Status: **REVIEW_REQUIRED**

This contract defines the structural model for authoritative semantic presence and travel for ordinary people. It replaces the prototype assumption that clock time directly creates commuting/work/home movement.

It does **not** claim one universal medieval travel speed, one complete road graph, one daily routine, one fixed workplace/home commute, exact terrain/weather modifiers, universal access to every path, or complete historical wayfinding psychology. Those remain calibration/content questions where material.

## Mechanic

Represent where an ordinary person is, why a destination exists, how a physically and socially feasible route is selected, and how travel consumes time before presence-dependent action can occur, without making render coordinates authoritative world state.

The canonical seam is:

`world state -> selected task/intention -> required target/place -> known candidate destinations/routes -> route/access/mode feasibility -> travel plan -> departure -> persistent travel progress -> arrival/presence -> action/process -> consequences`

There is no canonical transition of the form:

`clock hour -> commute -> workplace`

or:

`Profession -> fixed WorkplaceId -> travel`.

## Dependencies

This contract reuses accepted settlement foundations:

- `DESIGN/MODELS/SETTLEMENT_PERSON_HOUSEHOLD_LIFECYCLE.md` — persistent controller-neutral `Person`, residence as an ordinary relationship rather than a teleport anchor;
- `DESIGN/MODELS/SETTLEMENT_PROPERTY_TENURE_COMMON_RIGHTS.md` — co-location is separate from access/use/transfer authority;
- `DESIGN/MODELS/SETTLEMENT_EXCHANGE_CONTRACTS_MIGRATION.md` — migration, market/service opportunities and contracts can create destinations but arrival does not auto-create residence/work/rights;
- `DESIGN/MODELS/SETTLEMENT_INTENTION_TASK_SELECTION.md` — accepted upstream causal bridge from world-state reasons to a selected task;
- `DESIGN/MODELS/SETTLEMENT_HOUSEHOLD_AUTHORITY_REPRESENTATION.md` — household requests/representation do not create destination or access powers without accepted authority.

The upstream accepted seam is:

`selected task -> required target/place -> destination request`.

P3 owns what happens after that request becomes a movement requirement.

## Reference context

First historical calibration context: **rural lowland England, approximately 1270–1348**, with **1350–1450** retained as a separate stress/validation regime for changed labour/mobility conditions.

The historical evidence here establishes only the structural propositions needed for P3:

- ordinary people and goods moved regularly rather than being immobile fixtures;
- local and longer movement used roads/tracks and sometimes waterways;
- movement had purposes such as work, trade, service, migration and other obligations/opportunities;
- route availability was a physical, customary and institutional fact rather than a magic straight-line connection;
- journey speed varied with traveller/group/mode/purpose and physical conditions;
- familiar local landscapes mattered to mobility and knowledge;
- no evidence supports one universal `07:00 -> commute` rule.

## Evidence ledger

### 1. Movement in medieval England was ordinary and used functioning transport connections

**Wendy R. Childs, “Moving around,” in _A Social History of England, 1200–1500_ (Cambridge University Press, 2006).**

- https://www.cambridge.org/core/books/abs/social-history-of-england-12001500/moving-around/24EE4EF1DCA0918DA159BEA6E9378CAE
- Childs emphasizes that the extent/ease of medieval English travel is often underestimated and describes roads used by peasants, workmen, traders and many other travellers.
- The chapter connects regular movement to England's trade, markets, towns and government and notes use of roads and rivers.
- It states that speed depended on factors including group size/purpose and the fitness of people/horses.
- Supports: travel is ordinary world process; mode/group/purpose matter; one universal duration is unsafe.
- Does **not** establish one peasant walking speed, one route choice algorithm or one daily commute schedule.

### 2. Medieval English trade/towns required a transport network, but the exact road graph is evidentially incomplete

**Paul Hindle, “Sources for the English medieval road system,” in _Roadworks: Medieval Britain, Medieval Roads_ (Manchester University Press, 2016).**

- https://academic.oup.com/manchester-scholarship-online/book/16180/chapter-abstract/171231165
- Hindle links growth of towns/trade to a functioning transport system in which roads were the backbone, supplemented by river and sea transport.
- The chapter also stresses the limitations of evidence for reconstructing the exact network and uses itineraries, maps, place-names and archaeology cautiously.
- Supports: simulation needs explicit transport connections/modes rather than teleporting between arbitrary semantic places.
- Supports: source-controlled route content must remain evidence/content, not a claim that historians know every medieval path exactly.

### 3. A medieval road is partly a recognized right of passage, not merely geometry

**Alan Cooper, “Once a highway, always a highway: roads and English law, c. 1150–1300,” in _Roadworks: Medieval Britain, Medieval Roads_ (2016).**

- https://academic.oup.com/manchester-scholarship-online/book/16180/chapter-abstract/171231557
- Cooper traces legal doctrines protecting highway utility, clearing obstructions, limiting illegal tolls and requiring maintenance.
- The wider volume explicitly cautions against treating the medieval road as a modern homogeneous road system and discusses roads as rights of passage.
- Supports: route availability may have a recognized passage/custom/legal basis in addition to physical passability.
- Supports: an edge/path can become obstructed, disputed, maintained or unavailable without changing destination identity.
- Does **not** imply every local track is a royal highway or universally open.

### 4. Rural people moved for land, employment and marriage, often over short/familiar distances

**Christopher Dyer, _Peasants Making History: Living in an English Region 1200–1540_, chapter 4, “Peasants changing society” (Oxford University Press, 2022).**

- https://academic.oup.com/book/43934/chapter-abstract/370549741
- Dyer describes peasants migrating across their region, often over short distances, while pursuing land, employment and marriage.
- Supports: movement/destination can arise from concrete opportunities and social/economic intentions rather than schedule state.
- Supports: local/familiar landscape knowledge is a plausible constraint on destination/route discovery.
- Does **not** establish perfect geographical knowledge, unrestricted mobility or one migration distance threshold.

### 5. Labour/service arrangements create real movement requirements without a permanent profession destination

**Mark Bailey, “The regulation of the rural market in waged labour in fourteenth-century England,” _Continuity and Change_ 38(2) (2023).**

- https://www.cambridge.org/core/journals/continuity-and-change/article/regulation-of-the-rural-market-in-waged-labour-in-fourteenthcentury-england/C7726EC8A2D0C628ACFF49428FDA95A7
- Bailey documents substantial casual/seasonal wage labour and longer live-in service arrangements, with materially changed conditions after the Black Death.
- Supports: an accepted work/service task can require presence somewhere other than current residence.
- Does **not** support one fixed daily workplace, one universal commute or one immutable mobility restriction.

### 6. Landscape/property changes can block or reroute roads/tracks

**S. A. Mileson, _Parks in Medieval England_, chapter 7, “Parks and the Community” (Oxford University Press, 2009).**

- https://academic.oup.com/book/32963/chapter-abstract/278100906
- Mileson documents park-making restricting access to woods/common resources and blocking or rerouting roads and tracks.
- Supports: route availability is stateful/contextual; a formerly useful route can be blocked/rerouted.
- Supports: route access and land/resource rights must remain explicit instead of assuming straight-line movement through every parcel.
- Does **not** establish that all enclosure/parks affected every route equally.

### 7. Physical walking speed is not a historical profession constant

Modern human locomotion evidence is used only to constrain physically plausible on-foot movement, not to reconstruct medieval daily itineraries.

**Browning et al., “Effects of obesity and sex on the energetic cost and preferred speed of walking,” _Journal of Applied Physiology_ 100(2) (2006).**

- https://journals.physiology.org/doi/full/10.1152/japplphysiol.00767.2005
- Healthy adults in the study preferred level walking near approximately 1.4 m/s, close to the speed minimizing energy cost per distance.
- Supports: ordinary walking has a physically meaningful distance/time relationship rather than `one route = one hour`.
- Does **not** justify applying 1.4 m/s universally across age, illness, load, mud, gradient, weather or long-duration travel.

Modern load-carriage research also shows increasing load can reduce self-selected walking speed and change gait/effort:

- https://pmc.ncbi.nlm.nih.gov/articles/PMC8997774/

This evidence is a physics/biomechanics bound only. Historical daily journey duration remains mode/context dependent.

## Evidence limits

- Medieval journey evidence is biased toward documented travellers, itineraries, courts and institutions; ordinary intra-village trips are much less directly recorded.
- Royal/courier travel speeds are not accepted as ordinary peasant speeds.
- Exact medieval road/path maps are incomplete; content graphs must distinguish reconstruction/fixture from historical fact.
- Human walking biomechanics provide a physical baseline but not a universal daily travel speed.
- Weather, mud, darkness, fatigue, load, animal availability and route maintenance can matter, but P3 does not invent universal modifiers before they are required/calibrated.
- The model accepts a structural transport network and travel-progress process, not one historically complete route atlas.

## Causal model

Stable shape:

`selected task + target requirement + actor knowledge + current semantic presence + route/passages known + access/physical constraints + available travel modes + carried load/companions + environment -> feasible destination/route/mode plan -> departure -> travel progress consuming time/capacity -> interruption/reroute/completion -> arrival/presence -> action eligibility -> consequence`

### Task before destination

A semantic destination is derived from the selected task or an external displacement event.

Examples:

- `perform harvest task for Parcel P` -> destination derives from P or its accepted work-access place;
- `buy seed from known seller S at Market M` -> destination derives from the accepted meeting/market context;
- `care for dependant D` -> destination derives from D/current accepted meeting place when physical presence is required;
- `continue task already at current place` -> no travel;
- `migrate under selected intention` -> destination is the intended settlement/place, but arrival does not automatically create household/residence/work rights.

No clock value creates a destination by itself.

## Core concepts

### Semantic Place

A `Place` is an authoritative meaningful world location or spatial scope that can support presence-dependent actions.

Possible place kinds include, when content/mechanics require them:

- residence/building;
- settlement/neighbourhood;
- land parcel/field/meadow/pasture;
- yard/farmstead/work site;
- market/meeting venue;
- mill/church/court/institutional site;
- road/bridge/ferry/pass point where traversal itself matters;
- another accepted spatial subject.

`Home` and `Workplace` may remain convenience/projection categories, but they are not an exhaustive ontology and do not create movement motives.

A place needs stable identity independent of render coordinates.

### Presence

Authoritative person location has at least these conceptual states:

- `AtPlace(placeId)`;
- `Travelling(travelStateId)`;
- later extensions for displacement/custody/unknown location only if needed.

A person cannot be authoritatively present at both origin and destination during ordinary travel.

Presence is necessary for actions that physically require co-location, but presence does not grant permission, ownership or resource access.

### Destination requirement

The selected task identifies the semantic target/place requirement before travel planning.

A task may identify:

- one required place;
- one target person/resource whose known location determines a place;
- several acceptable places;
- a venue/window chosen by an accepted market/contract/institution rule.

If no valid/known destination can be derived, travel must not be fabricated merely to keep an NPC moving.

### Route / passage connection

A `RouteConnection` represents an authoritative traversable relation between semantic places/waypoints, not necessarily a modern engineered road.

Minimum conceptual attributes/references where material:

- stable connection identity;
- endpoints/connected spatial scopes;
- supported travel modes;
- physical distance or other measurable traversal extent when available;
- current passability/state;
- applicable recognized passage/access basis where access is restricted/contested;
- terrain/surface/gradient/environment class only where used by the duration model;
- known obstruction/closure/toll/ferry/bridge conditions where modeled;
- provenance/content source or explicit reconstruction/fixture marker for historically specific route content.

A route graph is simulation authority data. Godot render geometry may visualize it but must not silently become the only source of authoritative route feasibility.

### Route knowledge

An actor/controller may only plan through routes/destinations available through accepted knowledge.

V1 may use explicitly known local connections initialized from content for ordinary residents. It must not query a world-global omniscient graph to discover unknown distant employment, markets or people.

Detailed wayfinding, mistaken beliefs and asking directions are deferred. The knowledge boundary is canonical; the psychology is not.

### Travel mode

Travel mode affects physical feasibility and duration.

At minimum the ontology must allow distinction between:

- on foot;
- mounted/animal-assisted;
- cart/wagon/pack transport;
- water/ferry/boat where later supported.

P3 implementation may initially support only **on-foot ordinary-person travel** if the scenario is explicitly scoped that way. That is an implementation boundary, not a historical claim that horses/carts/water transport did not exist.

A mode cannot be selected unless the actor has the ordinary capability/access/resources required for it.

### Travel plan

A travel plan records enough authority to explain movement:

- actor;
- source selected task/intention or displacement event;
- origin;
- destination;
- route/connection sequence or equivalent deterministic path reference;
- selected travel mode;
- departure time;
- planned traversal cost/duration inputs;
- current progress;
- carried load/companions/transport subject where materially relevant;
- interruptions/reroutes;
- status: planned/travelling/arrived/cancelled/blocked as implementation requires.

The plan is not reconstructed from current clock hour.

## Travel duration and progress

Travel duration must derive from world/route state rather than a universal one-hour transition.

Conceptually:

`route extent + mode capability + actor capability/condition + load/group + terrain/surface + environment + stops/constraints -> traversal time`

For a first on-foot implementation, a physically plausible level/unloaded walking speed may be calibrated from human locomotion evidence and then modified only by modeled conditions. Historical evidence must constrain any claims about full-day travel, seasonal roads or loaded journeys.

Rules:

1. **Distance/time is causal.** Longer equivalent routes cannot cost the same time merely because the simulation ticks hourly.
2. **Simulation resolution is not world law.** If authoritative ticks are coarse, progress may be integrated/quantized while preserving a sub-tick remaining duration/progress value.
3. **One-hour travel remains a fixture.** It may survive temporarily in regression compatibility but cannot be the accepted P3 duration model.
4. **Load/group/mode may matter when represented.** Their effects must be explicit, not hidden in profession/controller bonuses.
5. **Exact historical calibration is scoped.** Unsupported universal speed/terrain/weather coefficients are not accepted.

## Route feasibility and authorization

Physical route feasibility and action authorization are separate.

A route may be:

- physically passable and publicly/customarily traversable;
- physically passable but access-restricted/contested;
- physically blocked;
- temporarily unavailable because a bridge/ferry/route condition fails;
- unknown to the actor;
- passable only by a particular mode.

Travelling to a place does not authorize entering a restricted building, harvesting a parcel, taking a resource or performing a contract action.

Likewise, owning/holding a resource does not imply the actor can instantaneously reach it.

## Departure, progress, interruption and reroute

1. A selected task requiring another place creates a travel requirement only after a feasible destination/route is found.
2. Departure changes authoritative state from `AtPlace(origin)` to `Travelling(plan)`.
3. Travel consumes authoritative time/capacity; the person cannot simultaneously perform incompatible labour elsewhere.
4. Progress persists across save/load and across ordinary time steps.
5. A new urgent reason, route closure, task cancellation, target movement or invalidation may trigger reconsideration/rerouting through the accepted task-selection controller.
6. Cancelling travel does not teleport the person back to origin or forward to destination. Current authoritative progress/location-on-route representation determines the resulting state.
7. Arrival changes state to `AtPlace(destination)` only when traversal completes.
8. Arrival makes presence-dependent actions physically eligible; rights/authorization/task conditions are checked separately.

Exact mid-route place representation can be a route-progress state rather than a render coordinate.

## Moving targets

If the target is another person or mobile subject, destination must come from known/planned meeting state rather than perfect pursuit.

V1 may require a stable meeting/place target at task selection time. Dynamic interception, pursuit and continuous target prediction are deferred unless needed by the active phase.

A target moving away may invalidate/replan the task/travel; it must not silently teleport the traveller.

## External displacement

Not all movement is voluntary task choice. A future accepted mechanic may create displacement through custody, eviction, expulsion, transport, disaster or similar world events.

Such movement must still have:

- causal source/event;
- origin/destination or displacement process;
- travel/transport consequences;
- controller-neutral world rules.

P3 does not invent those mechanics; it preserves the seam.

## Player/NPC symmetry

The authoritative path is identical for either controller:

`Person + selected task -> destination requirement -> route/mode feasibility -> TravelState -> presence -> action`

Changing AIController to HumanController must not change:

- person identity;
- location state;
- route graph/passability;
- walking/mode physics;
- ownership/access rights;
- travel duration rules;
- co-location requirements;
- action consequences.

The human controller may choose a different accepted task or route. It may not teleport, bypass inaccessible passages or gain special resource/building access.

Third-person render movement is a client presentation/control concern. It must reconcile with authoritative semantic travel/presence rather than becoming a second source of world location truth.

## Godot / simulation boundary

Simulation authority owns:

- stable place identities;
- authoritative `AtPlace`/`Travelling` state;
- task-linked destination;
- route/passability/mode constraints used by world rules;
- authoritative travel progress/duration;
- arrival/cancellation/reroute events;
- persistence/replay state.

Godot may own/present:

- render coordinates and navmesh path shape;
- interpolation along an authoritative semantic route;
- animation/camera/locomotion presentation;
- local collision/presentation details that do not create new world authority.

If client physical controls can materially alter authoritative travel, they must send ordinary movement/travel intent through a validated world boundary; client coordinates cannot directly overwrite simulation location.

## Knowledge and information boundary

A person does not automatically know:

- every place in the world;
- every route;
- every road closure;
- every person's current location;
- every market/employment opportunity.

P3 requires destination/route knowledge provenance where it is not ordinary initialized local knowledge or supplied by the selected task/relationship.

Detailed rumor/maps/directions/social information remain deferred.

## Persistence and replay

Persist enough authoritative state to reconstruct exactly:

- current `AtPlace` or `Travelling` state;
- origin/destination;
- source task/intention;
- route/path reference;
- travel mode;
- authoritative departure/progress/remaining traversal state;
- materially relevant load/transport references;
- interruption/reroute/cancellation state;
- knowledge/reference state required to preserve deterministic route choice where material.

Do **not** reconstruct location/activity from:

- clock hour;
- profession;
- permanent workplace assignment;
- current household id;
- Godot transform/render coordinates.

## Rules

1. **Cause before movement.** A task/intention or explicit displacement event creates destination need.
2. **Task before destination.** Clock/profession do not fabricate travel.
3. **Destination before route.** Route planning solves a real target requirement.
4. **Route is world state, not render decoration.** Traversability/access used by simulation must be authoritative.
5. **Presence is not permission.** Co-location never substitutes for ownership/access/contract authority.
6. **Travel consumes time.** No ordinary person can work simultaneously at origin/destination while travelling.
7. **Progress persists.** Travel is not recreated each hour.
8. **Duration derives from extent/mode/conditions.** One-hour travel is not canonical.
9. **Knowledge bounds planning.** No omniscient route/destination discovery by default.
10. **Routes can change.** Blockage/rerouting/access changes are valid world events.
11. **Modes require ordinary capability/resources.** Player status grants none.
12. **Controller symmetry.** AI/Human differences are choices, not physical/location powers.
13. **Render coordinates are non-authoritative.** Godot visual motion cannot overwrite semantic world state directly.
14. **Arrival is not social integration.** Migration arrival does not grant residence, household membership, job or inventory rights.
15. **No unsupported historical constants.** Exact speed/weather/terrain/group coefficients require calibration/evidence before they become load-bearing economy rules.

## Long-horizon behavior

P3 alone does not establish demographic/economic rates and therefore does not independently require a 10-year viability PASS.

However, travel consumes finite time and can materially affect labour, markets, agriculture, debt performance and migration. Any integrated P5/P6 >=10-year proof must therefore exercise travel-time costs rather than teleporting between economic opportunities.

Future long-horizon invariants include:

- no duplicated labour while travelling;
- route distance/time costs persist across save/load;
- migration does not reset identity/rights/debts;
- distant opportunities do not become costless because of global task selection;
- blocked routes can cause delay/failure without corrupting state;
- player-controlled people pay the same travel costs;
- economic viability does not depend on hidden one-hour universal movement.

## Assumptions and uncertainty

- Exact on-foot speed by age/health/load/season/surface/weather: **not yet a universal historical constant**; implementation calibration must be explicit.
- Exact medieval route graph for the prototype settlement/region: content/reconstruction work; every path must not be presented as historically attested.
- Exact toll/ferry/bridge rules and road-maintenance obligations: deferred until required.
- Mounted/cart/water mode performance and ownership/access: deferred unless implemented.
- Darkness, weather, fatigue and safety effects on route feasibility: deferred unless materially required.
- Detailed wayfinding, maps, asking directions, misinformation and route learning: **MODEL_UNDERDEFINED** beyond the structural knowledge boundary.
- Dynamic moving-target pursuit/interception: deferred.
- Exact local settlement geometry may use source-controlled measured world data, but Godot render coordinates remain non-authoritative.
- Historical routine schedules remain explicitly unsupported; accepted obligations/windows may contain times, but `time -> motive` remains invalid.

These uncertainties do not invalidate the structural travel topology. They become blockers only when a production mechanic materially relies on the missing parameter/rule.

## Fixture boundary

The following current/prototype behaviors are explicitly noncanonical:

- `07:00 -> commute`;
- `08:00 -> arrive/work`;
- `17:00 -> travel home`;
- every profession having one permanent destination;
- every resident taking exactly one simulation hour to travel regardless of distance;
- save/load reconstructing location from hour/profession;
- a route being considered authorized merely because a Godot path exists;
- player transform directly becoming authoritative world location without validated simulation intent/state;
- teleporting between settlements on migration;
- arrival automatically granting residence/work/household/inventory access;
- AI knowing every road, destination and moving person globally;
- co-location granting permission to consume/use/transfer a resource;
- regression tests treating the current commute schedule as historical evidence.

Temporary fixture tests may remain only as pipeline compatibility checks and must be revised when canonical implementation replaces them.

## Falsifiers

Revise this model if evidence or implementation shows that:

- semantic destination cannot be derived from selected tasks without reintroducing a clock/profession scheduler;
- route/passability state cannot be authoritative without persisting render coordinates;
- physically meaningful travel duration cannot coexist with coarse deterministic simulation ticks;
- distinguishing presence from access/authorization provides no meaningful protection against invalid world actions;
- a single universal travel duration remains necessary after route extent/mode are represented;
- HumanController requires teleportation or different route/access physics to remain playable;
- long-horizon economy requires ignoring travel costs to function.

## Feedback and observability

Authorized debug/audit projections should be able to expose:

- current semantic presence/travel state;
- selected task causing travel;
- destination and why it was selected;
- route/mode;
- authoritative progress/remaining traversal cost;
- route blockage/reroute reason;
- whether target/action is physically co-located;
- whether action remains unauthorized despite co-location.

Player-facing UI should expose only what the controlled person can know. Debug route/progress data is not automatically character knowledge.

## Acceptance scenario

A future P3 implementation/audit should demonstrate at minimum:

1. Person P is at Residence R with no clock-driven destination.
2. Accepted Intention/Task Selection produces a task requiring Place A from an explicit obligation/opportunity/process.
3. P3 derives A as destination and finds a route using only authoritative known/passable connections and an available mode.
4. Departure produces persistent `Travelling` state; P is no longer authoritatively at R or A.
5. Travel consumes time based on route extent/mode/accepted conditions rather than always exactly one hour.
6. While travelling, P cannot simultaneously perform incompatible work at either endpoint.
7. Save/load/replay preserves route, progress and resulting arrival exactly.
8. If a route becomes blocked, P remains in coherent travel state and can stop/reroute/reconsider without teleportation.
9. On arrival, P becomes `AtPlace(A)`; a presence-dependent action may proceed only if its separate rights/conditions pass.
10. HumanController controlling the same P uses identical route/passability/travel costs and can choose a different ordinary task/route without receiving special access.
11. Godot may interpolate visible movement, but changing render coordinates alone cannot create semantic arrival.
12. No 07/08/17 fixture is needed to explain why P travelled.

This scenario proves the structural P3 causal model. It does not prove a complete medieval road atlas, every transport mode or calibrated long-horizon economy.

## Deferred complexity

Separate bounded work may still be required for:

- prototype-region route/metric content calibration;
- detailed walking/load/terrain/weather duration calibration if it materially affects phase behavior;
- mounted/cart/water transport;
- ferries/tolls/bridges and specific access institutions;
- wayfinding, maps, directions and misinformation;
- dynamic moving-target pursuit;
- weather/darkness/safety travel decisions;
- integration with future demographic/economic >=10-year proof.

Until independently audited, this contract remains `REVIEW_REQUIRED` and does not authorize production changes that fill deferred parameters with convenient constants.