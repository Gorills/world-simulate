# Village Vertical Slice v0.2

This milestone turns the existing authoritative settlement simulation into a physically playable village. It is deliberately a greybox milestone: dimensions, ownership and interaction seams are intended to survive art replacement even though meshes/materials are temporary.

## Spatial contract

- One Godot world unit is treated as one meter.
- Playable village footprint: roughly 260 x 220 meters.
- Main road: 7 meters wide; side lanes: 5 meters wide.
- Buildings are placed with meaningful yards/setbacks rather than packed into a compact city block.
- Greybox contains at least 12 buildings; v0.2 contains 14.
- Homes have footprints of at least 7 x 8 meters and door openings at least 1.4 meters wide.
- Buildings are physical shells with collision on walls and open doorways. The player can walk into the interior without a loading transition.
- Work areas and fields sit outside the residential spine so future travel time has real spatial meaning.

The spatial layout is source-controlled data (`VillageLayout`) and validates its minimum scale/spacing in the Godot headless smoke. Final art assets must conform to these anchors instead of silently shrinking the village around the models.

## Player feel target

The target is an authored third-person exploration feel in the same broad family as The Witcher 3: character embodiment, camera-relative locomotion, a trailing collision-aware camera, walk/run speed distinction and smooth facing changes. This is an interaction/feel reference only; assets, animations, UI and content remain original.

The current slice includes:

- WASD / left-stick camera-relative movement;
- Shift / L3 sprint;
- mouse / right-stick camera control;
- independent character facing and camera yaw;
- `SpringArm3D` camera collision for tight exterior/interior spaces;
- Tab / Start toggles the existing simulation HUD and releases/captures the pointer.

Combat, lock-on, horse movement and animation-state matching are outside this slice.

## Physical interaction contract

World interaction is camera-directed but proximity bounded.

- NPC, projected item stacks and building entrances expose `Area3D` hit zones on a dedicated interaction collision layer.
- The player camera owns a ray that collides with both world geometry and interaction areas. A wall therefore blocks a target behind it.
- A ray hit is accepted only when the interaction area is within 3.6 meters of the player body.
- `F` / gamepad `A` requests interaction with the current physical target.
- Resident targets carry the authoritative `EntityId`; item targets carry the authoritative stack/item IDs and projected quantity.
- Resident interaction selects that resident through `GameSession` and then uses the existing authoritative settlement command flow.
- Item inspection re-resolves the stack through `GameSession` instead of trusting stale presentation state.
- Building entrances are spatial context only for now. Doorways remain physically open and traversable; this slice does not invent non-authoritative locked/open door gameplay before buildings become authoritative settlement state.

The Godot world emits intent and presentation facts only. It never mutates resident needs, inventory, affinity or jobs directly.

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

1. Navigation and resident daily movement between home, workplace and village destinations.
2. Household/home ownership in authoritative settlement state.
3. Real building/NPC/item asset pipeline with stable visual archetype IDs.
4. Animation state machine for idle/walk/run/work/interact.
5. Time-of-day lighting and village ambience.

The slice is not considered an art milestone. Its purpose is to make spatial scale, player embodiment, enterable buildings, physical interaction and presentation identity real before deeper village-life rules are added.
