# Village Vertical Slice v0.11

This milestone turns the authoritative settlement simulation into a physically playable village. Greybox dimensions, simulation identity, residence ownership, population density, interaction seams, time progression and debug observability are intended to survive later art replacement.

## Spatial contract

- One Godot world unit is one meter.
- Playable footprint is roughly 260 x 220 meters.
- Main road is 7 meters wide; side lanes are 5 meters wide.
- The greybox contains 14 buildings, including 10 residential homes.
- Homes have playable footprints and open doorways; the player enters interiors without a loading transition.
- Work areas and fields sit outside the residential spine so travel distance matters.

`VillageLayout` is source-controlled and validated by the Godot headless smoke. Final assets must fit these anchors rather than shrinking the world around the art.

## Playtest population contract

The village playtest baseline is 12 authoritative residents. Three residents were only the original simulation prototype and are not the intended village density.

- Residents have stable simulation `EntityId`, identity, profession, workplace and household assignments.
- The baseline population is split evenly across Farmer, Cook and Forager: four residents per profession.
- Six households contain two residents each and occupy the first six authoritative homes.
- Four of the ten homes remain vacant so later housing-allocation gameplay still has meaningful capacity.
- Initial ration and grain stock is scaled for the 12-resident baseline rather than the original three-person prototype.
- `VillageLayout` provides at least 12 resident spawn anchors and 12 social anchors so the baseline population is spatially distributed instead of stacked.

This is a playtest content baseline, not a hard population ceiling. Population growth, migration and future settlement sizes remain simulation concerns.

## Third-person player contract

The feel target is authored third-person exploration in the broad family of The Witcher 3: character embodiment, camera-relative locomotion, trailing collision-aware camera, walk/run distinction, jump, smooth acceleration and smooth facing. This is a feel reference only; assets, animation, UI and gameplay content remain original.

Player control tuning lives in `DESIGN/PLAYER_CONTROL_SYSTEM.md`; feature code does not own locomotion constants.

## Physical interaction contract

- NPCs, projected item stacks and building entrances expose `Area3D` hit zones.
- The camera interaction ray sees both world collision and interaction areas, so walls occlude targets.
- World targets are accepted only inside the 3.6 meter player interaction bound.
- `F` / gamepad `A` requests interaction with the physical target.
- Resident targets carry authoritative `EntityId`; item inspection re-resolves the authoritative stack through `GameSession`.
- Building entrances remain physical/contextual until door state becomes authoritative gameplay.

Godot emits intent and presents facts; it does not mutate needs, inventory, jobs, affinity or authoritative time directly.

## Simulation-driven resident movement

Authoritative hourly `ResidentActivity` drives presentation destinations:

- `Working` -> assigned `WorkplaceId`;
- `Resting` -> authoritative residence;
- `Eating` -> food/common area;
- `Idle` -> deterministic social anchors.

A source-controlled route graph moves resident views along roads and work tracks. Building destinations route through the actual doorway before the interior. Render coordinates are presentation-only and are never persisted or replay-authoritative.

## Playtest time contract

The authoritative settlement runtime still advances only on canonical whole-hour boundaries and never reads ambient wall-clock time. The playable client owns only the cadence that decides when to request those deterministic hour advances.

- Prototype residents start in `Resting`, which is consistent with authoritative time `00:00` before the first scheduler step.
- A new playable `GameSession` bootstraps the deterministic runtime through hour 08:00 before the world is shown, so resident activity is already scheduler-derived at play start.
- At the current content baseline, all 12 residents are `Working` at 08:00; a core test protects this schedule contract.
- `PlaytestTimeProfile` is the single tuning store for client time cadence. Current baseline is one game minute per real second, or one authoritative game hour every 60 real seconds.
- `PlaytestClock` lives on the Godot/client boundary and calls the existing `GameSession.AdvanceHours()` API; it never mutates resident state directly.
- Manual `T / Y` hour advance remains available. External/manual time changes reset the real-time clock phase so a manual skip is not immediately followed by an accidental automatic extra hour.
- Opening HUD or the F3 observer does not pause authoritative playtest time.

The 60-second cadence is a playtest value, not final game balance. Future pause/time-scale gameplay can replace the cadence without changing deterministic settlement rules.

## Full-screen village observer

`F3` toggles a removable, presentation-only observer workspace while the world continues running.

- The observer fills the viewport with a shared DesignSystem `Window` surface.
- The village map consumes the main available area instead of living in a small floating card.
- A fixed-width diagnostic column lists each rendered resident, activity, needs, destination, distance and remaining route points.
- The map draws the actual remaining presentation route for every rendered resident plus destination and player markers.
- The summary compares authoritative resident count with rendered resident-view count and groups residents by activity.
- The observer remains read-only: it has no `GameSession`, simulation-runtime or command dependency.

The observer is deliberately easy to remove: its implementation remains under `Debug/VillageMonitor/`, with one scene instance and one debug input binding at composition boundaries.

## Authoritative residence contract

Settlement schema v5 introduces persisted housing without duplicating membership truth.

- `HomeState` owns stable identity, display name, `SpatialKey` and capacity.
- `HouseholdState` owns stable identity and exactly one `HomeId`.
- `ResidentState` stores exactly one optional `HouseholdId`; resident membership is not duplicated as a household member list.
- A home cannot be assigned to multiple households.
- Assigned residents must reference an existing household and household occupancy cannot exceed home capacity.
- `HomeId` is derived through the household and exposed in read-only projections.
- `SpatialKey` maps authoritative homes to source-controlled physical buildings; display-name changes do not break routing.
- All 10 visible residential buildings have authoritative `HomeState`, including currently vacant homes.

World-global entity identity includes homes and households. Their IDs live inside the existing settlement entity-ID block.

Resident migration deliberately clears `WorkplaceId` and `HouseholdId`. The source household/home remains in the source settlement; the migrant arrives unassigned and can be allocated housing by a future gameplay rule.

### Persistence compatibility

Settlement schema remains v5. `SettlementStateJson` explicitly migrates both v4 and v3 snapshots. Legacy residents load with `HouseholdId = 0` and no homes/households are fabricated. Changing the default playtest population does not rewrite existing persisted settlements: resident/home/household state is already stored explicitly in snapshots.

## NPC and item identity

NPC presentation remains derived from authoritative resident identity/profession and currently uses modular greybox variation. Stockpile presentation remains keyed by authoritative item IDs (`grain`, `ration`, `herb`, fallback). These are replacement seams for real art, not alternate gameplay identity.

## Immediate follow-up slices

1. playtest and tune third-person control feel;
2. inspect the full-day 12-resident activity cycle and movement/crowding through the full-screen observer;
3. household gameplay: housing allocation, household consumption and relationship/family semantics;
4. real building/NPC/item asset archetype pipeline;
5. idle/walk/run/work/interact animation state machine;
6. time-of-day lighting and final time-scale/pause UX;
7. NPC collision/crowd avoidance as population grows.

This is still a greybox gameplay milestone, not an art-complete milestone.
