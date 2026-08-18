# World Simulate UI Design System v0.1

## Purpose

The UI must scale from the village vertical slice to city, region, country and world-management screens without every feature inventing its own colors, spacing or control states.

The visual target is a restrained dark systems-game interface: clear information hierarchy, warm focus accents, readable dense data, and enough polish that debug tools and production screens feel intentionally designed.

## Ownership

`src/Mws.Client.Godot/UI/Theme/` owns all visual tokens and Godot theme implementation.

Feature code chooses **semantic roles** through `DesignSystem`. It does not choose RGB values, create `StyleBox` resources, or write `theme_override_*` values in `.tscn` scenes.

Scene files own structure and layout constraints. The design system owns visual semantics and spacing rhythm.

## Palette roles

The palette is intentionally semantic rather than feature-specific:

- Canvas: application background.
- Window: highest-level persistent surface.
- Card: grouped content or a secondary panel.
- Inset: dense data, maps, lists and scroll regions.
- Floating: prompts, popovers and debug windows above gameplay.
- Accent: focus, selected state and primary actions.
- Info / Positive / Warning / Danger: state communication only.
- TextPrimary / TextSecondary / TextMuted / TextDisabled: information hierarchy.

Do not add colors named after a feature such as `VillageGreen` or `TradeBlue`. Add a semantic role if a real reusable role is missing.

## Spacing and shape

The system follows a small spacing scale:

- Tight: 4 px
- Small: 8 px
- Medium: 14 px
- Large: 22 px
- Page margin: 24 px

Radii are 6 / 10 / 14 px for small controls, normal controls/cards and windows.

New UI should prefer these values over one-off spacing. Layout structure may still define minimum sizes and anchors in scenes.

## Typography

Current roles:

- Display: major screen title.
- Heading: window title.
- SectionHeading: card/section title.
- Body: normal readable text.
- Muted: secondary context.
- Caption: dense metadata and debug information.
- Metric: time, counts and other high-value numeric state.

Identity data such as resident names is content, not localization or typography metadata.

## Surfaces

Use `DesignSystem.ApplySurface` with:

- `UiSurface.Window` for a primary screen region.
- `UiSurface.Card` for grouped controls or a sidebar block.
- `UiSurface.Inset` for dense data/map/scroll content.
- `UiSurface.Floating` for contextual overlays and debug windows.

A feature should not create its own panel background or border style.

## Buttons and focus

Use `UiButtonRole`:

- Primary: the main action in a local context.
- Secondary: ordinary explicit action.
- Ghost: low-emphasis utility action.
- Row: selectable data row.
- SelectedRow: selected data row.

Every interactive control must expose keyboard/gamepad focus. Focus is a bright warm ring, not a color-only text change.

Hover, pressed, disabled and focus visuals are part of the design system and must not be rebuilt in feature code.

Immutable surface/control style resources are cached by semantic role and shared across controls; dynamic lists must not allocate a fresh style resource per row.

## Status and badges

`UiTone` provides Neutral / Info / Positive / Warning / Danger.

Badges are for compact state labels, not decoration. Warning and Danger are reserved for conditions that require attention.

## Data visualization

Maps and diagnostics use `UiDataColor` rather than hardcoded colors.

The debug observer deliberately uses the same window/card/inset primitives as production UI. Debug tools may be denser, but they must not become a second visual system.

## Localization

Scenes contain no player-facing `text` values. Visible language comes from `GameLocalization` / `LocalizedContent`.

Layout must tolerate Russian text expansion. Prefer wrapping and flexible containers over fixed label widths.

## Extension workflow

When a new screen needs a visual treatment:

1. Try an existing semantic role.
2. If the role is reusable but missing, add it to `UiSemantics` and implement it in `DesignSystem`.
3. Add/adjust tokens only when the semantic role cannot be expressed with existing tokens.
4. Use the role from feature code.
5. Add architecture coverage if the new primitive introduces a new styling boundary.

Do not add a one-off theme override to ship faster. That cost compounds as the world-management UI grows.

## Current reference composition

The village HUD is the production reference:
- settlement = Window,
- resident and interaction blocks = Cards,
- bottom action/status bar = Card,
- resident rows = Row / SelectedRow.

The F3 village observer is the debug reference:
- observer chrome = Floating,
- map and scrolling data = Inset,
- key marker = Info badge,
- dense metadata = Caption.

These references are not frozen layouts. They demonstrate how semantic primitives compose.
