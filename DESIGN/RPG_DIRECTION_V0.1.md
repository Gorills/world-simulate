# RPG Direction v0.1

Status: working design contract for the first playable vertical slice.

## North Star

**World Simulate is a systemic RPG where the player is an actor inside a world that keeps living without them.**
NPCs have persistent identity, needs, work, possessions and relationships. The player reads the situation,
chooses an RPG action, and the authoritative simulation resolves consequences that remain in the world.

The target feeling is **grounded, readable and consequential** rather than maximal mechanical density.
The player should feel that they are influencing an existing place, not operating a menu-driven spreadsheet
or triggering disconnected scripted scenes.

## Reference matrix

References are aspect references, not cloning targets.

| Reference | What we study | What we do not copy |
| --- | --- | --- |
| Kenshi | autonomous world, persistent actors, systemic consequences | UI density, grind, exact combat |
| Mount & Blade | player as one actor inside a larger simulation | exact campaign/combat structure |
| RimWorld | readable needs, jobs, resource chains and consequences | colony-director control model |
| Baldur's Gate 3 | controller-friendly layered RPG UI and explicit choices | cinematic scope, ruleset |
| Disco Elysium | consequences and character-facing social choices | dialogue-heavy structure |

Every new mechanic must name the reference aspect it is borrowing or state that it is an original experiment.

## Design pillars

1. **The world is not waiting for the player.** Simulation state advances and NPC identity survives time/save/load.
2. **Consequences persist.** Items, relationships, jobs, events and later injuries/reputation belong to authoritative state.
3. **NPCs are people in systems, not interaction kiosks.** Their needs, work and possessions exist before interaction.
4. **Complexity must be readable.** Important state is projected into clear UI; hidden complexity is not a virtue.
5. **Player verbs are explicit.** UI sends intent; the simulation owns rules and outcomes.
6. **Mouse/keyboard and gamepad are peers.** No required interaction may depend on pointer-only behavior.

## Core loop for the vertical slice

**Observe → choose a person/place → act → world resolves → read consequences → prepare/advance time.**

Minute-to-minute:
- inspect the settlement and residents;
- select a resident or useful place;
- perform a contextual RPG action;
- see immediate feedback and changed state.

Hour-to-hour:
- time advances;
- needs and work resolve;
- resource chains change;
- new interaction context emerges.

Day-to-day:
- relationships, resources and events accumulate;
- save/load must preserve the same causal state.

## RPG system shape

The stable *shape* is:

`Character → attributes/skills/needs → inventory/equipment → relationships → interactions/effects → progression`

Only needs, inventory, work and a minimal relationship value exist today. Stats, skills, equipment, checks,
progression and combat are intentionally not designed until their target feeling and reference contract are written.

Current `Farmer/Cook/Forager`, `grain/herb/ration` and simple interaction choices are **prototype fixtures**.
They prove the pipeline; they are not immutable game canon.

## Interaction rule

A real RPG interaction must have:
- a player-facing verb;
- a reason to choose it over another verb;
- immediate readable feedback;
- persistent or strategically relevant consequence;
- a controller flow that does not require a mouse.

`Affinity += 1` is acceptable as a fixture, not as final relationship design.

## Not decided yet

Do not invent these ad hoc:
- combat camera and combat pace;
- final attribute/skill list;
- XP/progression curve;
- dialogue check formula;
- equipment slots;
- loot rarity;
- crafting tree;
- player embodiment/camera beyond the settlement interaction prototype.

Each becomes a separate mechanic contract before implementation.

## Playable v0.1 acceptance

The first playable client foundation is successful when:
- one screen exposes the same authoritative settlement state as headless;
- a resident can be selected with mouse/keyboard or gamepad;
- interaction choices can be navigated and confirmed with gamepad;
- time can be advanced without a pointer;
- UI state never directly mutates simulation fields;
- the Godot composition root stays small and feature code has obvious homes.
