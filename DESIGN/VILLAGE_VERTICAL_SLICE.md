# Village Vertical Slice v0.3

This milestone turns the existing authoritative settlement simulation into a physically playable village. It is deliberately a greybox milestone: dimensions, ownership and interaction seams are intended to survive art replacement even though meshes/materials are temporary.

## Spatial contract

- One Godot world unit is treated as one meter.
- Playable village footprint: roughly 260 x 220 meters.
- Main road: 7 meters wide; side lanes: 5 meters wide.
- Buildings are placed with meaningful yards/setbacks rather than packed into a compact city block.
- Greybox contains 14 buildings with physical wall collision and open, traversable doorways.
- Work areas and fields sit outside the residential spine so travel time has real spatial meaning.
- Farm and herb-grove work areas now have visible connecting tracks and distinct work anchors.

The spatial layout is source-controlled data (`VillageLayout`) and validates its minimum scale/spacing in the Godot headless smoke. Final art assets must conform to these anchors instead of silently shrinking the village around the models.

## Player feel target

The target is an authored third-person exploration feel in the same broad family as The Witcher 3: character embodiment, camera-relative locomotion, a trailing collision-aware camera, walk/run speed distinction and smooth facing changes. This is an interaction/feel reference only; assets, animations, UI and content remain original.

The current slice includes WASD/left-stick movement, Shift/L3 sprint, mouse/right-stick camera control, independent character facing, `SpringArm3D` camera collision and Tab/Start world/menu switching.

## Physical interaction contract

World interaction is camera-directed but proximity bounded.

- NPC, projected item stacks and building entrances expose `Area3D` hit zones on a dedicated interaction collision layer.
- The player camera ray collides with world geometry and interaction areas, so walls can occlude targets.
- A ray hit is accepted only when the target is within 3.6 meters of the player.
- `F` / gamepad `A` requests interaction with the current physical target.
- Resident targets carry authoritative `EntityId`; item targets carry authoritative stack/item identity.
- Resident interaction goes through `GameSession` and the existing settlement command pipeline.
- Item inspection re-resolves the stack through `GameSession` instead of trusting stale presentation state.
- Building entrances are spatial context only; this slice does not invent lock/open gameplay before buildings exist in authoritative settlement state.

## Resident movement contract

Resident movement is driven by authoritative simulation state without making render coordinates authoritative.

- `ResidentProjection` now exposes the already-persisted authoritative `WorkplaceId`; this is projection-only and does not change save schema.
- `ResidentActivity.Working` routes the view toward the resident's projected workplace destination.
- `Resting` routes to a stable home placeholder, `Eating` to the inn/food area, and `Idle` to deterministic social anchors.
- Home placeholders are stable by `EntityId`, but they are explicitly **not** household ownership. Authoritative household/home assignment is the next domain slice.
- Building destinations include a doorway access point, so routes enter homes/food/work interiors through their physical opening rather than crossing a wall.
- A source-controlled village route graph follows the main road and work tracks. This avoids a runtime navmesh bake in the greybox while preserving intentional travel corridors.
- Views walk at human-scale presentation speed and rotate toward their current path segment. Interaction hit zones move with the NPC.
- Re-rendering after an interaction no longer resets residents to spawn positions.
- `Space` / gamepad `Y` can advance one simulation hour while the world is visible, making activity changes and resulting travel observable directly in the village.

The simulation owns activity, needs, work assignment and inventory. Godot owns only visual interpolation along the route. View coordinates are not serialized or replay-authoritative, so save/replay results do not depend on frame rate or whether a mesh finished walking before the next simulation command.

Current movement limitations are intentional: NPC bodies do not yet perform crowd avoidance, dynamic obstacle avoidance or animation-state matching. Those belong after authoritative home assignment and the first real character asset pipeline.

## NPC visual identity

Authoritative identity remains `EntityId`. The greybox renderer derives stable body/height, skin, profession-biased clothing, profession marker and selected marker variants from projected resident data. These are replacement seams for modular character assets, not final art.

## Item visual identity

Authoritative item identity remains the stable simulation `ItemId`. The world renderer maps projected stockpile stacks to distinct temporary forms: grain sacks, ration crates, herb bundles and a neutral fallback for future item IDs.

## Immediate follow-up slices

1. Authoritative households, home ownership and resident residence assignment.
2. Real building/NPC/item asset archetype pipeline with stable visual archetype IDs.
3. Idle/walk/run/work/interact animation state machine.
4. Continuous/time-scale village clock and time-of-day lighting.
5. NPC collision, crowd avoidance and richer route destinations once the population grows beyond the prototype residents.

The slice is not considered an art milestone. Its purpose is to make spatial scale, player embodiment, physical interaction and simulation-driven village life observable before deeper household and social rules are added.
