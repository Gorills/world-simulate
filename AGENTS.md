# FAST SOLO DEV protocol

Optimize for a solo developer using coding agents. Quality gates exist to shorten debugging, not to create ceremony.

## Reality/modeling gate
`DESIGN/REALITY_MODELING_POLICY.md` is a blocking global policy for authoritative simulation work.

- Do not preserve a prototype fixture or regression test when it conflicts with causal logic, historical evidence or player/NPC symmetry. Fix/delete the fixture or test instead.
- Before treating human, economic, social, institutional or physical-world behavior as canonical, create/update a model contract under `DESIGN/MODELS/` using `DESIGN/MECHANIC_CONTRACT_TEMPLATE.md`.
- Historical human behavior needs an explicit reference context and credible evidence; do not invent universal rules from intuition.
- The player is not a privileged simulation species. Player-only powers require ordinary world-state justification such as ownership, office, permission, contract, skill or physical access.
- Prefer `MODEL_UNDERDEFINED` and stop implementation over filling a research gap with a convenient constant.
- `python TOOLS/validate_reality_model.py` is part of routine validation. Starting at P3, a phase cannot PASS without the required model-review evidence.

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

## Playable prototype phase gate
The active playable-prototype program is defined by `DESIGN/PLAYABLE_PROTOTYPE_PROGRAM.md` and `MACHINE/playable-prototype.json`.

- Run `python TOOLS/validate_playable_prototype.py` before changing production scope. With Git available, it checks both phase state and actual protected working-tree/HEAD changes.
- Work only on the single phase marked `IMPLEMENTING`. Protected scope includes `src/`, tests, benchmarks, tooling, design, workflows, audit evidence and root build/agent/git-ignore contracts.
- A phase in `AUDIT_REQUIRED` is frozen. Review the exact committed implementation; do not keep coding in the audit pass.
- `FAILED` means repair the same phase: move that phase back to `IMPLEMENTING`, then repair it. Do not start a later phase.
- A later phase must remain `LOCKED` until all dependencies are `PASSED`.
- Passing tests is not enough to mark a phase `PASSED`; independent post-commit code review, systems audit and any required reality/model review must pass and be recorded under `AUDIT_RESULTS/PLAYABLE_PROTOTYPE/`.
- Audit JSON records are append-only evidence. Never rewrite a previous result; a new review creates a new JSON record.
- Passing a phase does not automatically authorize or start the next phase. Record `PASS` and start the next phase in separate state transitions.
- Respect the program scope freeze. Do not add polish or unrelated systems while authority, scaling and gameplay-causality phases are unresolved.
