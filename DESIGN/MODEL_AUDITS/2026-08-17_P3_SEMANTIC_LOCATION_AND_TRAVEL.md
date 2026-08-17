# Audit — P3 Semantic Location and Travel

Audit date: **2026-08-17**

Verdict: **PASS**

Model contract: `DESIGN/MODELS/P3_SEMANTIC_LOCATION_AND_TRAVEL.md`

Reviewed research/model SHA: `cfb97cc85dd6cbff82a52fbc454f9b4ccc4a0085`

## Scope

Independent audit of the structural P3 semantic-location/travel model only. No production simulation code was reviewed or changed in this audit. This audit accepts the causal semantic-place/travel topology and its authority boundaries; it does **not** accept one universal medieval walking speed, a complete medieval road atlas, fixed daily commuting schedules, exact terrain/weather/load coefficients, complete wayfinding psychology, or all mounted/cart/water transport rules.

The accepted Person/Household, Property/Tenure/Common Rights, Exchange/Contracts/Migration, Intention/Task Selection, and Household Authority/Representation contracts are treated as dependencies.

## Repository and CI

The exact reviewed SHA was branch HEAD `cfb97cc85dd6cbff82a52fbc454f9b4ccc4a0085` at audit time. That commit modified only `DESIGN/MODELS/P3_SEMANTIC_LOCATION_AND_TRAVEL.md`.

Required GitHub Actions on the reviewed SHA all passed:

- `ci #143` — success;
- `playable-prototype-gate #168` — success;
- `proof-a-measure #138` — success.

## Load-bearing fact re-check

### Movement was ordinary and journey speed was context-dependent

Re-checked Wendy R. Childs, “Moving around,” in _A Social History of England, 1200–1500_ (Cambridge University Press, 2006):

https://www.cambridge.org/core/books/abs/social-history-of-england-12001500/moving-around/24EE4EF1DCA0918DA159BEA6E9378CAE

Childs explicitly describes England's roads and rivers as busy and states that medieval travel speed depended on the size and purpose of the travelling group and the fitness of people and horses.

Audit conclusion: regular movement and context-dependent journey time are supported. The source does **not** establish one peasant walking speed, one route-choice algorithm, or a universal home/work commute schedule.

### A functioning transport network existed, but its exact graph is incompletely reconstructable

Re-checked Paul Hindle, “Sources for the English medieval road system,” in _Roadworks: Medieval Britain, Medieval Roads_ (Manchester University Press, 2016):

https://academic.oup.com/manchester-scholarship-online/book/16180/chapter-abstract/171231165

Hindle links medieval growth of towns and trade to a functioning transport system in which roads formed the backbone, supplemented by river and sea transport. The chapter simultaneously emphasizes the limits of documentary/place-name evidence and the need to use itineraries, maps and archaeology carefully.

Audit conclusion: explicit transport connections are justified as simulation authority; claiming a complete historically attested route graph is not. The contract correctly marks exact route content as reconstruction/content uncertainty.

### Roads/passages had legal and institutional state beyond geometry

Re-checked Alan Cooper, “Once a highway, always a highway: roads and English law, c. 1150–1300,” _Roadworks_:

https://academic.oup.com/manchester-scholarship-online/book/16180/chapter-abstract/171231557

Cooper documents doctrines and actions concerning road obstructions, illegal tolls and maintenance. The volume introduction by Valerie Allen and Ruth Evans also argues that the medieval road should often be understood as a right of passage/function rather than only a modern-style physical road object:

https://academic.oup.com/manchester-scholarship-online/book/16180/chapter-abstract/171229140

Audit conclusion: it is appropriate for route state to distinguish physical passability from recognized passage/access context. This does not make every local track a public highway or grant universal access.

### Routes could be blocked or rerouted by changing land use

Re-checked S. A. Mileson, _Parks in Medieval England_, chapter 7, “Parks and the Community” (Oxford University Press, 2009):

https://academic.oup.com/book/32963/chapter-abstract/278100906

Mileson's abstract explicitly states that park-making reduced access to resources and led to blocking and rerouting of roads and tracks.

Audit conclusion: route blockage/rerouting are legitimate world-state transitions; route geometry/passability must not be assumed immutable.

### Rural movement could follow land, employment and marriage opportunities

Re-checked Christopher Dyer, _Peasants Making History_, chapter 4, “Peasants changing society” (Oxford University Press, 2022):

https://academic.oup.com/book/43934/chapter-abstract/370549741

Dyer describes peasants migrating across the region, often over short distances, in pursuit of land, employment and marriage.

Audit conclusion: concrete social/economic opportunities can create destinations; no fixed clock/profession destination law is required. The source does not support unrestricted mobility or perfect destination knowledge.

### Hired labour provides real non-household movement requirements

Re-checked Mark Bailey, “The regulation of the rural market in waged labour in fourteenth-century England,” _Continuity and Change_ 38(2) (2023):

https://www.cambridge.org/core/journals/continuity-and-change/article/regulation-of-the-rural-market-in-waged-labour-in-fourteenthcentury-england/C7726EC8A2D0C628ACFF49428FDA95A7

Bailey's quantitative analysis of 1,445 manorial court sessions supports a substantial rural hired-labour market and materially different regulation after the Black Death.

Audit conclusion: work/service tasks may legitimately require movement to places other than residence; the evidence does not establish one permanent workplace commute.

### Route knowledge should not be omniscient

As an independent supplemental check, reviewed Ruth Evans, “Getting there: wayfinding in the Middle Ages,” in _Roadworks_ (2016):

https://academic.oup.com/manchester-scholarship-online/book/16180/chapter-abstract/171233149

Evans reviews route planning, asking directions, guides, maps, landmarks and signage, and characterizes medieval wayfinding as situated/distributed cognition involving human cooperation and a hierarchy of spatial knowledge.

Audit conclusion: the contract's structural knowledge boundary is supported. A controller should not automatically discover every route/destination through a world-global graph. Exact knowledge initialization, learning, misinformation and asking directions remain correctly deferred.

### Modern walking evidence is only a physical calibration bound

Re-checked Browning, Baker, Herron and Kram, “Effects of obesity and sex on the energetic cost and preferred speed of walking,” _Journal of Applied Physiology_ 100(2) (2006):

https://journals.physiology.org/doi/10.1152/japplphysiol.00767.2005

The study measured preferred level walking speed near 1.42 m/s in its modern adult sample, with preferred speed close to the energy-cost minimum.

Audit conclusion: the source supports a physically meaningful distance/time relation for level human walking. It does **not** justify `1.42 m/s` as a universal medieval travel speed across age, health, load, terrain, weather or long journeys. The contract uses it only as a possible physical calibration bound, which is acceptable.

## Causal model review

**PASS.**

Accepted structural topology:

`selected task/intention -> required semantic target/place -> known candidate destinations/routes -> route/access/mode feasibility -> travel plan -> departure -> persistent progress consuming time/capacity -> interruption/reroute/completion -> arrival/presence -> separately authorized action/consequence`

The model correctly separates:

- reason/task from destination;
- destination from route planning;
- authoritative semantic place from render coordinates;
- physical passability from passage/access context;
- arrival/presence from permission to enter/use/transfer;
- simulation tick resolution from world travel duration;
- route knowledge from omniscient global discovery.

`07:00 -> commute`, `08:00 -> work`, `17:00 -> home`, `Profession -> permanent WorkplaceId`, and universal one-hour travel remain fixtures rather than causal law.

## Rights and access review

**PASS.**

The contract does not use location as authorization. Co-location may make an action physically possible, but accepted property/tenure/contract/household-authority rules remain authoritative for entering, using, harvesting, consuming or transferring subjects. Recognized passage/access context on a route does not create ownership of adjacent resources or destination property.

## Player/NPC symmetry review

**PASS.**

HumanController and AIController use the same `Person`, place graph, passability, travel modes, duration/progress rules, co-location requirements, rights and consequences. Controller choice may select a different ordinary task/route, but player status grants no teleportation, route bypass or special building/resource access.

Godot render coordinates remain presentation state and cannot directly overwrite authoritative semantic arrival.

## Persistence, determinism and coarse-time review

**PASS.**

The model requires persistent `AtPlace`/`Travelling` state, origin/destination, route reference, mode, departure/progress/remaining traversal state and materially relevant interruption/reroute state. It explicitly forbids reconstructing location from clock hour, profession, household membership or Godot transform.

Coarse simulation ticks are acceptable only if travel progress/remaining duration survives quantization. This avoids converting `one tick` into a universal historical duration.

## Uncertainty and fixture-boundary review

**PASS.**

The following remain explicit calibration/content/deferred questions rather than hidden constants:

- exact on-foot speed by age/health/load/season/surface/weather;
- exact prototype-region medieval route graph;
- mounted/cart/water performance and ownership/access;
- ferries/tolls/bridges and detailed maintenance rules;
- darkness/weather/fatigue/safety effects;
- detailed wayfinding, route learning, maps/directions and misinformation;
- dynamic moving-target pursuit/interception.

These gaps block only mechanics that materially require the missing rule. They do not invalidate the accepted structural place/travel topology.

## Long-horizon review

**PASS for structural P3 acceptance; integrated economic proof remains later.**

P3 itself does not set demographic or economic rates, so this audit does not claim a standalone ten-year viability result. However travel consumes finite time and can materially alter labour, agriculture, exchange, debt performance and migration. The required P5/P6 >=10-year integrated proof must therefore exercise travel costs and verify no duplicated labour, costless distant opportunity access, identity/debt reset on migration, or player-only travel exemptions.

## Remaining blockers outside this contract

- production P3 code still uses prototype schedule/travel fixtures until separately repaired and audited;
- prototype-region route/metric content must be selected/calibrated for the actual implementation;
- detailed duration modifiers become blockers if implementation makes them load-bearing;
- richer wayfinding/information remains deferred unless required by the active scenario;
- integrated economy/demography and >=10-year evidence remain P5/P6 work.

## Overall verdict

**PASS.**

The reviewed P3 model has sufficient causal structure, historical grounding, rights separation, controller symmetry, persistence requirements and uncertainty boundaries to be promoted from `REVIEW_REQUIRED` to `ACCEPTED` as a structural model contract.

Acceptance does **not** mean current production P3 implementation passes the phase. Production code must still be changed so movement follows accepted tasks/destinations and authoritative route/progress semantics instead of the prototype clock/profession commute fixtures; that implementation requires its own post-commit P3 phase audit.