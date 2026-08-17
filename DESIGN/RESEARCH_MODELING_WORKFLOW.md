# Bounded Research and Modeling Workflow

This process is required for historical/causal simulation research governed by `DESIGN/REALITY_MODELING_POLICY.md`. Its purpose is to preserve evidence, keep model work reviewable, and prevent both design drift and endless research loops.

## Unit of work

One pass handles one bounded research/modeling task that can be completed and reviewed as a coherent unit.

For each task:

1. state the narrow model question and explicit deferred scope;
2. inspect existing `ACCEPTED` contracts first and reuse their accepted findings instead of restarting research from zero;
3. research only the evidence needed for the current question;
4. create or update one coherent model contract/evidence set;
5. use `MODEL_UNDERDEFINED` when a material causal or evidence gap remains, otherwise `REVIEW_REQUIRED` until audit;
6. commit/push the coherent task result;
7. report the result and stop before starting another task.

Production implementation must not be used to fill an unresolved model gap. A model task being complete does not itself authorize production implementation.

## Evidence ledger

A model contract is the durable research record for later work, not merely a conclusion.

For every source that supports historical human/economic/social behavior, preserve enough information to avoid repeating the same research later:

- source identity and stable citation/link;
- reference region, period and institutional context;
- the specific claim(s) the source supports;
- what the source does **not** establish;
- important regional/temporal variation or scholarly disagreement;
- any simplification adopted by the simulation;
- unresolved quantitative or causal questions.

Do not cite a source merely because it is about the topic. The contract must say what evidence is being taken from it.

An `ACCEPTED` contract may be used by later contracts as a reviewed baseline. Later work should not redo its whole literature search unless new evidence, a contradiction, a changed reference context or a load-bearing dependency requires reopening it.

## Audit pass

The audit is a separate pass over the exact committed task. Do not start the next research or implementation task during that pass.

Audit must check all of the following that apply:

### 1. Exact repository state

- verify the intended branch and exact HEAD SHA;
- inspect the committed scope/diff;
- confirm no unrelated production or fixture change was smuggled into the research task.

### 2. Independent fact re-check

Identify the task's **load-bearing claims**: facts which, if false or materially narrower than stated, would change an entity, causal link, right/obligation, decision rule, acceptance criterion or model status.

For those claims, independently reopen/check the underlying evidence rather than trusting the previous research summary. Verify:

- the source actually supports the stated claim;
- region, period, population and institutional context match the wording;
- a conditional or local observation has not been promoted to a universal rule;
- disagreements, source-selection problems and material counterevidence have not been omitted;
- quantitative values are not inferred beyond what the evidence can support;
- citations are independent enough to satisfy the Reality Modeling Policy rather than merely repeating one another.

Do **not** automatically repeat the entire literature search. Non-load-bearing facts are reopened only when the audit finds a contradiction, ambiguity, weak citation or changed dependency.

If a previously `ACCEPTED` contract supplies a load-bearing premise for the new task, the audit may rely on its evidence ledger but must re-check the specific premise when it is critical to the new model or has become disputed.

### 3. Causal model

Check that cause precedes state and that time/calendar only constrains behavior rather than fabricating motives. Confirm that entities and transitions are sufficient to explain the claimed behavior without hidden game-only magic.

### 4. Player/NPC symmetry

Check that HumanController and AIController operate on the same ordinary world actor and rules. Any difference in capability must come from ordinary state such as rights, office, permission, contract, skill, knowledge or physical access.

### 5. Ownership, rights and obligations

Check that possession, use, transfer, residence, work and authority are not granted merely by co-location, player status or convenient aggregate ownership.

### 6. Uncertainty and fixture boundary

Check that unresolved historical/quantitative questions remain explicit, `MODEL_UNDERDEFINED` where material, and that prototype fixtures/regression expectations are not being treated as evidence.

### 7. Long-horizon requirement

Where the model changes settlement economy, demography or resource balance, require the policy's long-horizon evidence before acceptance. Plausible failure is allowed; unexplained failure is not.

### 8. CI on the exact SHA

Audit includes GitHub Actions/required checks for the exact commit under review.

- If relevant workflows are running/queued, report the audit as CI-pending and do not claim the step fully closed.
- If a required check fails, inspect the failed job/step, repair the same task and stop; do not advance to the next task.
- Do not poll in a loop. Inspect a bounded status snapshot, and re-check on the next owner-directed continuation when necessary.
- After an audit-status/fix commit, CI must be checked on that final SHA as well.

## Status transitions

- `MODEL_UNDERDEFINED`: material research/causal gaps remain. Stop rather than inventing rules.
- `REVIEW_REQUIRED`: concrete model and evidence exist; independent audit has not yet passed.
- `ACCEPTED`: load-bearing evidence, causal structure, symmetry, rights/obligations, uncertainty and required validation have passed audit.

Only the audit pass should promote a contract from `REVIEW_REQUIRED` to `ACCEPTED`. `ACCEPTED` means the model is an approved baseline in its declared context; it does not make deferred questions universal and does not automatically authorize a later task.

## Blockers and repair

A blocker includes, at minimum:

- a load-bearing source does not support the claim;
- material regional/temporal variation was hidden;
- a causal transition depends on an invented constant or fixture;
- player/NPC symmetry or rights are violated;
- required long-horizon evidence is missing;
- required CI fails;
- a material dependency remains `MODEL_UNDERDEFINED`.

When a blocker is found, repair only the blocked task in a bounded pass, commit/push the repair, report and stop. Do not begin the next task until the owner explicitly continues and the repaired task passes audit.

## Anti-loop rule

The normal rhythm is:

`bounded task -> commit/push -> report/stop -> owner continues -> audit -> fix if blocked OR accept -> report/stop`

Do not polish the same accepted task indefinitely. New research after audit must be justified by a concrete blocker, contradiction, new dependency or changed reference context. Otherwise preserve the accepted result and move on only when the owner requests it.
