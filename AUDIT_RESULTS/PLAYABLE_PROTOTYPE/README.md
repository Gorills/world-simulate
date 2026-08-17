# Playable Prototype Audit Results

This directory stores post-commit audit records for `DESIGN/PLAYABLE_PROTOTYPE_PROGRAM.md`.

Audit the exact implementation commit before writing a result. Do not modify that subject commit while reviewing it.

Minimal record shape:

```json
{
  "schema_version": 1,
  "program": "playable-prototype-v1",
  "phase_id": "P1_PLAYABLE_USES_WORLD_RUNTIME",
  "subject_sha": "40-hex commit sha",
  "reviewer": "reviewer identity",
  "review_mode": "independent-post-commit",
  "code_review": "PASS",
  "systems_audit": "PASS",
  "overall": "PASS",
  "validation": ["commands/checks actually inspected or run"],
  "systems_examined": ["authority", "persistence", "client/runtime", "performance"],
  "findings": [],
  "residual_risks": []
}
```

Rules:

- `overall` can be `PASS` only when both review verdicts are `PASS`.
- A blocking finding requires `overall: FAIL`.
- `subject_sha` is the immutable implementation commit being reviewed, not the later commit that stores this audit record.
- A failed phase is repaired in place and audited again with a new result file.
- Passing one phase does not automatically start the next phase.
