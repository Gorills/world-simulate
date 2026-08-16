# world-simulate

Executable implementation workspace for Mythic World Simulation.

The authoritative simulation core is engine-independent C#/.NET. Godot is a client adapter, not the source of truth for world state.

## Current checkpoint

P0.6 Proof A scaffold: solution graph, deterministic core smoke path, architecture tests, and bounded CI/Godot headless validation.

## CI policy

- Core CI runs only for code/build changes relevant to the .NET solution.
- Godot smoke runs only for Godot adapter/integration changes.
- Expensive Proof A benchmark runs are manual until a measured reason justifies promotion.
- CI uses concurrency cancellation and hard timeouts to avoid stale or runaway runs.
- Failed runs are diagnosed from the failed job/step first; successful logs are not re-read by default.

## Stack checkpoint

- C# 12 / .NET 8 compatibility floor for Proof A.
- Godot 4.7.1 .NET for the client adapter and headless smoke validation.
- `Mws.Domain`, `Mws.Simulation.Api`, and `Mws.Simulation.Runtime` must remain Godot-free.
