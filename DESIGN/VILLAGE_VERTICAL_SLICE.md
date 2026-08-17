# Village Vertical Slice v0.4

This milestone turns the authoritative settlement simulation into a physically playable village. Greybox dimensions, simulation identity, residence ownership and interaction seams are intended to survive later art replacement.

## Spatial contract

- One Godot world unit is one meter.
- Playable footprint is roughly 260 x 220 meters.
- Main road is 7 meters wide; side lanes are 5 meters wide.
- The greybox contains 14 buildings, including 10 residential homes.
- Homes have playable footprints and open doorways; the player enters interiors without a loading transition.
- Work areas and fields sit outside the residential spine so travel distance matters.

`VillageLayout` is source-controlled and validated by the Godot headless smoke. Final assets must fit these anchors rather than shrinking the world around the art.

## Third-person player contract

The feel target is authored third-person exploration in the broad family of The Witcher 3: character embodiment, camera-relative locomotion, trailing collision-aware camera, walk/run distinction and smooth facing. This is a feel reference only; assets, animation, UI and gameplay content remain original.

Current controls include WASD / left stick movement, Shift / L3 sprint, mouse / right-stick look, `SpringArm3D` camera collision and Tab / Start for the simulation HUD.

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
- The default village has two occupied households: Mira and Tor share one home; Ena occupies another.

World-global entity identity includes homes and households. Their IDs live inside the existing settlement entity-ID block.

Resident migration deliberately clears `WorkplaceId` and `HouseholdId`. The source household/home remains in the source settlement; the migrant arrives unassigned and can be allocated housing by a future gameplay rule.

### Persistence compatibility

Settlement schema advances from v4 to v5. `SettlementStateJson` explicitly migrates both v4 and v3 snapshots. Legacy residents load with `HouseholdId = 0` and no homes/households are fabricated. This preserves old state honestly instead of guessing historical residence assignments.

## NPC and item identity

NPC presentation remains derived from authoritative resident identity/profession and currently uses modular greybox variation. Stockpile presentation remains keyed by authoritative item IDs (`grain`, `ration`, `herb`, fallback). These are replacement seams for real art, not alternate gameplay identity.

## Immediate follow-up slices

1. household gameplay: housing allocation, household consumption and relationship/family semantics;
2. real building/NPC/item asset archetype pipeline;
3. idle/walk/run/work/interact animation state machine;
4. continuous/time-scale village clock and time-of-day lighting;
5. NPC collision/crowd avoidance as population grows.

This is still a greybox gameplay milestone, not an art-complete milestone.
