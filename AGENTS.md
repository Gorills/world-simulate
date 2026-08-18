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

## Simulation scope / anti-overmodeling gate

World Simulate targets a **playable, scalable magical-medieval life simulation**, not exhaustive microscopic reproduction and not museum-grade reenactment.

Before starting research, adding authoritative state, introducing a new model dependency, or requiring a review blocker for extra detail, state what the detail changes **now**. Detail is load-bearing only when omitting or coarsening it would materially affect at least one of:

- a player-observable choice, consequence, feedback loop or meaningful NPC behavior;
- causal correctness, ownership/rights, determinism, persistence or another authoritative invariant;
- scaling from village to town/region/world, including LOD, performance or state-ownership boundaries;
- long-horizon economic, demographic, institutional or social outcomes that the active milestone must validate;
- an explicit magic/world-law mechanic or a material downstream consequence of that magic;
- a stated acceptance criterion of the active playable-prototype phase.

If none applies, **do not research or model the detail now**. Record/defer it if useful and continue with the coarser model. “More realistic”, “historically interesting”, “could happen” and “we may need it someday” are not sufficient reasons.

Always ask whether a coarser causal category preserves the same gameplay and scaling behavior. Prefer that abstraction when it does. Do not model incidental bodily, locomotion, social or environmental minutiae by default: gait micro-variation, tiny pauses, exact bodily processes, fine-grained weather effects, individual path trivia and similar detail require a concrete load-bearing reason.

`MODEL_UNDERDEFINED` does not automatically mean “research this next”. An underdefined area may remain explicitly deferred when the active system can safely proceed without inventing a false rule.

Research depth must be proportional to decision impact. Quantify a value only when the number or threshold changes gameplay, scaling, long-horizon behavior or a required invariant. Reviews must challenge **unnecessary complexity** as well as incorrect models: a historically true and technically correct detail can still be rejected or deferred when it adds no load-bearing value.

### Magical-medieval target

Historical evidence supplies the mundane baseline for people, material life, institutions and constraints. Magic is an explicit counterfactual world law, not something that needs fake historical proof.

When magic is modeled, define its source/availability, costs, limits, knowledge/access and ordinary consequences. If magic materially changes labour, transport, medicine, warfare, religion, property, communication, agriculture or other systems, those systems must adapt instead of silently preserving an incompatible historical baseline. Historical research should then ground the **human/institutional response and useful analogies**, not attempt to prove that the magical phenomenon existed.

The target is believable lived experience in a coherent magical-medieval world: enough historical structure to make choices and consequences feel authentic, enough systemic magic to make the setting genuinely magical, and no simulation detail that exists only because it is possible to model.

## Bounded research/modeling workflow

Follow `DESIGN/RESEARCH_MODELING_WORKFLOW.md` for historical/causal model work.

- One bounded research/modeling task -> coherent commit/push -> report -> stop. Do not begin the next task in the same pass.
- On owner-directed continuation, audit the exact previous commit before starting anything new. A blocker keeps work on the same task.
- Preserve source-to-claim evidence and limits in model contracts so accepted research can be reused instead of repeated from zero.
- Audit must independently re-check load-bearing historical facts against their underlying sources; do not merely trust the previous research summary.
- Before promoting a contract to `ACCEPTED`, persist an append-only audit record under `DESIGN/MODEL_AUDITS/` with the exact reviewed SHA, load-bearing fact checks, reopened sources, verdicts, deferred gaps and CI outcomes.
- Audit includes relevant CI/required checks on the exact SHA. Running CI means pending, failed CI means repair the same task, and a later task must not start while a blocker remains.
- Do not turn audit into a full literature-search loop: reopen non-load-bearing evidence only for a concrete contradiction, ambiguity, weak citation or changed dependency.

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

`fast` must remain the default agent loop. Do not substitute the whole solution, Godot, benchmark, export or release validation for it.

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

- The unit of work is a **coherent causal capability or acceptance outcome**, not the smallest possible diff.
- “Minimal diff” means **no unrelated changes**. It does not mean the fewest files, lines, helpers or commits.
- Do not split an already-defined mechanic into separate commit/CI cycles merely because its production code, tests, API/state wiring or persistence support live in different files. When those edits together make one capability correct and reviewable, batch them.
- Before splitting an otherwise coherent change, name the concrete risk the split reduces. Valid reasons include an unresolved model decision, an independent acceptance criterion, a meaningful rollback boundary, a high-risk schema/persistence/architecture change, an external dependency, or a change that would otherwise become too large to review safely.
- If no concrete risk is reduced by splitting, keep the coherent change together. Conversely, do not combine independent capabilities merely to save a CI cycle.
- A bounded task may cross multiple closely related files and implementation seams, but it must still have one clear completion sentence. Run CI after that coherent batch, not after every helper or file.

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
