# Model audit records

This directory stores append-only durable evidence for independent audits of reality/model contracts.

A model contract may be `ACCEPTED` only when an applicable audit record exists here. The record complements the contract's evidence ledger: the contract says what the model claims and why; the audit record says what an independent review actually re-checked on a specific committed SHA.

Each audit record should identify:

- model contract/path and exact reviewed SHA;
- verdict and audit date;
- load-bearing claims re-checked;
- underlying sources reopened and what the re-check established;
- causal logic, player/NPC symmetry, ownership/rights/obligations, uncertainty/fixture and long-horizon verdicts as applicable;
- deferred or still-`MODEL_UNDERDEFINED` areas;
- relevant CI/check outcomes on the reviewed SHA;
- final acceptance/status-change SHA when one exists.

Records are append-only. Do not edit an older PASS or FAIL to match later understanding. Materially revised models receive a new audit record so the historical verification trail remains visible.
