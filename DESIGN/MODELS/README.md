# Simulation model contracts

Put short reality/model contracts here before human/economic/social/world behavior becomes canonical simulation law.

Each contract follows `DESIGN/MECHANIC_CONTRACT_TEMPLATE.md`, the blocking rules in `DESIGN/REALITY_MODELING_POLICY.md`, and the bounded research/audit process in `DESIGN/RESEARCH_MODELING_WORKFLOW.md`.

Model status must be explicit near the top of each contract:

- `MODEL_UNDERDEFINED` — research/causal model is incomplete; implementation may explore infrastructure but the phase must not PASS.
- `REVIEW_REQUIRED` — a concrete model and evidence exist but have not yet passed independent model audit.
- `ACCEPTED` — the current reference context, load-bearing evidence, causal structure and simplifications have passed the required independent audit **and an applicable append-only audit record exists under `DESIGN/MODEL_AUDITS/`**.

A contract is both a model proposal and an evidence ledger. Record what each source supports, its context/limits, material disagreement and unresolved questions so later tasks can reuse accepted research instead of repeating the whole investigation.

`ACCEPTED` is reusable but not infallible. If a later task critically depends on one of its claims, that load-bearing premise is eligible for targeted re-check during audit. New evidence, contradiction or a changed reference context can force an accepted contract back into review.

The audit record is the durable verification ledger: it records the exact SHA reviewed, load-bearing facts re-checked, reopened sources, audit verdicts, remaining deferred areas and CI outcomes. Audit records are append-only; do not rewrite an older verdict after later model changes.

A contract is evidence for review; it does not make an unsupported assumption true. Research gaps should remain blockers rather than being filled with convenient constants.
