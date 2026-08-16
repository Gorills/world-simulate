# world-simulate

Executable implementation workspace for Mythic World Simulation.

The authoritative simulation core is engine-independent C#/.NET. Godot is a client adapter, not the source of truth for world state.

## Fast solo-dev loop

The default edit loop is intentionally small:

```bash
python TOOLS/dev.py doctor   # toolchain check
python TOOLS/dev.py fast     # core tests only; use repeatedly while editing
python TOOLS/dev.py check    # core build + core/architecture tests + headless smoke; use before push
python TOOLS/dev.py run      # run the headless simulation
```

Explicit/expensive commands:

```bash
python TOOLS/dev.py godot    # Godot C# build + local headless smoke when Godot is installed
python TOOLS/dev.py full     # milestone validation, not the normal edit loop
python TOOLS/dev.py bench    # explicit Proof A performance run only
```

`fast` and `check` use `WorldSimulate.Core.slnf`, so routine simulation work does not build the Godot client or benchmark project. Restore is stamp-cached locally and only reruns when project/build inputs change.

## Git policy

Keep `main` plus one active milestone/feature branch. Do not create task/fix/test branches for every small change. Batch coherent edits, push once, and let stale CI cancel automatically. Merge only when explicitly requested.

## CI policy

- One `ci` workflow owns routine PR validation.
- Core compiles first; Godot cannot start on a broken shared C# build.
- Core CI builds only `WorldSimulate.Core.slnf`, not the whole solution.
- Godot is skipped unless the PR touches the actual client/adapter dependency boundary.
- NuGet packages and the verified Godot editor are cached.
- Stale runs are cancelled; core/Godot jobs have hard 6/7 minute timeouts.
- Expensive Proof A benchmark runs remain `workflow_dispatch` only.
- On failure inspect only the failed job/step; do not poll or reread successful logs.

See `AGENTS.md` for the mandatory fast-loop policy used by coding agents.

## Current checkpoint

P0.6 Proof A scaffold: deterministic core semantics, persistence probes, architecture tests, bounded CI, and Godot headless validation. P0.6 is not formally complete until the required measured workloads/evidence are accepted.

## Stack checkpoint

- C# 12 / .NET 8 compatibility floor for Proof A.
- Godot 4.7.1 .NET for the client adapter and headless smoke validation.
- `Mws.Domain`, `Mws.Simulation.Api`, and `Mws.Simulation.Runtime` must remain Godot-free.
