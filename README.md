# world-simulate

Executable implementation workspace for Mythic World Simulation.

The authoritative simulation core is engine-independent C#/.NET. Godot is a client adapter, not the source of truth for world state.

## Current checkpoint

P0.6 Proof A scaffold: solution graph, deterministic core smoke path, architecture tests, and bounded CI/Godot headless validation.

## CI policy

- Core CI runs only for code/build changes relevant to the .NET solution.
- Godot smoke runs only for Godot adapter/integration changes.
- Expensive benchmarks and exports are manual until a measured reason justifies promotion.
- CI uses concurrency cancellation and hard timeouts to avoid stale or runaway runs.
