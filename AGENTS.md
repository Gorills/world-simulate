# FAST SOLO DEV protocol

Optimize for a solo developer using coding agents. Quality gates exist to shorten debugging, not to create ceremony.

## Git
- Keep `main` plus one active milestone/feature branch. Do not create a branch for each task, fix, test, or agent.
- Batch coherent edits into one commit. Do not push intermediate placeholders.
- Do not merge without the owner explicitly asking for a merge.
- Do not rewrite shared history unless explicitly requested.

## Daily validation ladder
- During implementation: `python TOOLS/dev.py fast`.
- Before push: `python TOOLS/dev.py check`.
- Run `python TOOLS/dev.py godot` only for Godot/client/adapter integration or when explicitly requested.
- Run `python TOOLS/dev.py full` only at a milestone/checkpoint or when explicitly requested.
- Run `python TOOLS/dev.py bench` only for explicit performance evidence. Never as part of the normal edit loop.

`fast` must remain the default agent loop. Do not substitute the whole solution, Godot, benchmark, export, or release validation for it.

## CI anti-stall
- One change batch -> one CI cycle.
- After push, inspect at most one status snapshot unless the owner asks for another.
- On failure, inspect only the failed job/step logs. Never reread successful logs by default.
- Fix all errors visible in that failed step in one batch; prefer a new commit over repeated reruns.
- Rerun a job only for a clearly transient infrastructure failure.
- Never poll CI in a loop. Do not keep the chat occupied waiting for CI.

## Godot scope
Routine Proof A/gameplay kernel changes do not require Godot headless CI unless they touch the actual Godot dependency boundary. If the client starts depending on a new runtime/domain file, update the CI Godot-scope matcher in the same change.

## Agent task sizing
Prefer small-to-medium vertical edits that a weaker local agent can understand from nearby code and tests. Escalate architecture/persistence/LOD/public-boundary changes to a stronger agent or deliberate review.
