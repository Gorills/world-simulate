# Village Vertical Slice v0.1

This milestone turns the existing authoritative settlement simulation into a physically playable village. It is deliberately a greybox milestone: dimensions, ownership and interaction seams are intended to survive art replacement even though meshes/materials are temporary.

## Spatial contract

- One Godot world unit is treated as one meter.
- Playable village footprint: roughly 260 x 220 meters.
- Main road: 7 meters wide; side lanes: 5 meters wide.
- Buildings are placed with meaningful yards/setbacks rather than packed into a compact city block.
- Greybox contains at least 12 buildings; v0.1 contains 14.
- Homes have footprints of at least 7 x 8 meters and door openings at least 1.4 meters wide.
- Buildings are physical shells with collision on walls and open doorways. The player can walk into the interior without a loading transition.
- Work areas and fields sit outside the residential spine so future travel time has real spatial meaning.

The spatial layout is source-controlled data (`VillageLayout`) and validates its minimum scale/spacing in the Godot headless smoke. Final art assets must conform to these anchors instead of silently shrinking the village around the models.

## Player feel target

The target is an authored third-person exploration feel in the same broad family as The Witcher 3: character embodiment, camera-relative locomotion, a trailing collision-aware camera, walk/run speed distinction and smooth facing changes. This is an interaction/feel reference only; assets, animations, UI and content remain original.

v0.1 includes:

- WASD / left-stick camera-relative movement;
- Shift / L3 sprint;
- mouse / right-stick camera control;
- independent character facing and camera yaw;
- `SpringArm3D` camera collision for tight exterior/interior spaces;
- Tab / Start toggles the existing simulation HUD and releases/captures the pointer.

Combat, lock-on, horse movement and animation-state matching are outside this slice.

## NPC visual identity

Authoritative identity remains `EntityId`; Godot does not invent NPC gameplay identity.

The greybox resident renderer derives stable presentation variants from the projected resident:

- height/body variation;
- multiple skin tones;
- profession-biased clothing palettes;
- profession marker;
- selected-resident marker.

These are placeholders for real modular character assets. Replacing them must preserve the mapping from authoritative resident identity/profession to presentation profile.

## Item visual identity

Authoritative item identity remains the stable simulation `ItemId`. The world renderer maps current projected stockpile stacks to visibly distinct temporary forms:

- `grain` -> grouped grain sacks;
- `ration` -> provision crate with banding;
- `herb` -> tied herb bundle;
- unknown future item IDs -> neutral fallback block.

Visual replacement therefore happens behind the item-ID mapping instead of changing simulation rules or save data.

## Immediate follow-up slices

1. Physical interaction targeting: player proximity/raycast -> resident/door/item intent -> `GameSession`.
2. Navigation and resident daily movement between home, workplace and village destinations.
3. Household/home ownership in authoritative settlement state.
4. Real building/NPC/item asset pipeline with stable visual archetype IDs.
5. Animation state machine for idle/walk/run/work/interact.
6. Time-of-day lighting and village ambience.

The slice is not considered an art milestone. Its purpose is to make spatial scale, player embodiment, enterable buildings and presentation identity real before deeper village-life rules are added.
