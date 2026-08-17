#!/usr/bin/env python3
"""Validate the sequential playable-prototype phase gate."""

from __future__ import annotations

import json
import re
import sys
from pathlib import Path
from typing import Any

ROOT = Path(__file__).resolve().parents[1]
STATE_PATH = ROOT / "MACHINE/playable-prototype.json"
ALLOWED_STATUSES = {"LOCKED", "IMPLEMENTING", "AUDIT_REQUIRED", "FAILED", "PASSED"}
ACTIVE_STATUSES = {"IMPLEMENTING", "AUDIT_REQUIRED", "FAILED"}
ALLOWED_VERDICTS = {"PASS", "FAIL"}
ALLOWED_REVIEW_MODES = {"independent-post-commit", "human-owner"}
SHA40 = re.compile(r"^[0-9a-f]{40}$")


class GateError(RuntimeError):
    pass


def load_json(path: Path) -> Any:
    try:
        return json.loads(path.read_text(encoding="utf-8"))
    except FileNotFoundError as exc:
        raise GateError(f"missing required file: {path.relative_to(ROOT)}") from exc
    except json.JSONDecodeError as exc:
        raise GateError(f"invalid JSON in {path.relative_to(ROOT)}: {exc}") from exc


def require(condition: bool, message: str) -> None:
    if not condition:
        raise GateError(message)


def validate_audit(path_text: str, phase_id: str, expected_overall: str) -> dict[str, Any]:
    require(
        path_text.startswith("AUDIT_RESULTS/PLAYABLE_PROTOTYPE/") and path_text.endswith(".json"),
        f"{phase_id}: audit path must be a JSON record under AUDIT_RESULTS/PLAYABLE_PROTOTYPE/",
    )
    path = (ROOT / path_text).resolve()
    root = ROOT.resolve()
    try:
        path.relative_to(root)
    except ValueError as exc:
        raise GateError(f"{phase_id}: audit path must stay inside repository") from exc
    require(path.is_file(), f"{phase_id}: audit file does not exist: {path_text}")
    audit = load_json(path)

    require(audit.get("schema_version") == 1, f"{phase_id}: unsupported audit schema")
    require(audit.get("program") == "playable-prototype-v1", f"{phase_id}: audit program mismatch")
    require(audit.get("phase_id") == phase_id, f"{phase_id}: audit phase_id mismatch")

    subject_sha = audit.get("subject_sha")
    require(isinstance(subject_sha, str) and SHA40.fullmatch(subject_sha) is not None,
            f"{phase_id}: audit subject_sha must be lowercase 40-hex")

    reviewer = audit.get("reviewer")
    require(isinstance(reviewer, str) and reviewer.strip(), f"{phase_id}: audit reviewer is required")
    require(audit.get("review_mode") in ALLOWED_REVIEW_MODES,
            f"{phase_id}: audit review_mode must be one of {sorted(ALLOWED_REVIEW_MODES)}")

    code_review = audit.get("code_review")
    systems_audit = audit.get("systems_audit")
    overall = audit.get("overall")
    require(code_review in ALLOWED_VERDICTS, f"{phase_id}: invalid code_review verdict")
    require(systems_audit in ALLOWED_VERDICTS, f"{phase_id}: invalid systems_audit verdict")
    require(overall in ALLOWED_VERDICTS, f"{phase_id}: invalid overall verdict")
    require(overall == ("PASS" if code_review == systems_audit == "PASS" else "FAIL"),
            f"{phase_id}: overall verdict does not match component verdicts")
    require(overall == expected_overall,
            f"{phase_id}: phase state expects audit overall {expected_overall}, got {overall}")

    for field in ("validation", "systems_examined", "findings", "residual_risks"):
        require(isinstance(audit.get(field), list), f"{phase_id}: audit {field} must be a list")

    if overall == "FAIL":
        require(bool(audit["findings"]), f"{phase_id}: failed audit must record findings")

    return audit


def main() -> int:
    state = load_json(STATE_PATH)
    require(state.get("schema_version") == 1, "unsupported playable-prototype state schema")
    require(state.get("program") == "playable-prototype-v1", "unexpected playable-prototype program id")

    policy = state.get("policy")
    require(isinstance(policy, str) and (ROOT / policy).is_file(), "program policy file is missing")

    phases = state.get("phases")
    require(isinstance(phases, list) and phases, "phases must be a non-empty list")

    by_id: dict[str, dict[str, Any]] = {}
    order: list[str] = []
    for phase in phases:
        require(isinstance(phase, dict), "each phase must be an object")
        phase_id = phase.get("id")
        require(isinstance(phase_id, str) and phase_id, "phase id is required")
        require(phase_id not in by_id, f"duplicate phase id: {phase_id}")
        require(phase.get("status") in ALLOWED_STATUSES,
                f"{phase_id}: status must be one of {sorted(ALLOWED_STATUSES)}")
        require(isinstance(phase.get("depends_on"), list), f"{phase_id}: depends_on must be a list")
        by_id[phase_id] = phase
        order.append(phase_id)

    for index, phase_id in enumerate(order):
        phase = by_id[phase_id]
        dependencies = phase["depends_on"]
        for dependency in dependencies:
            require(dependency in by_id, f"{phase_id}: unknown dependency {dependency}")
            require(order.index(dependency) < index, f"{phase_id}: dependency {dependency} must be earlier")

        if phase["status"] != "LOCKED":
            for dependency in dependencies:
                require(by_id[dependency]["status"] == "PASSED",
                        f"{phase_id}: cannot leave LOCKED before {dependency} is PASSED")

    active = [phase_id for phase_id in order if by_id[phase_id]["status"] in ACTIVE_STATUSES]
    require(len(active) <= 1, f"multiple active phases: {active}")
    expected_active = active[0] if active else None
    require(state.get("active_phase") == expected_active,
            f"active_phase must be {expected_active!r}, got {state.get('active_phase')!r}")

    first_nonpassed_seen = False
    for phase_id in order:
        phase = by_id[phase_id]
        status = phase["status"]
        if status == "PASSED":
            require(not first_nonpassed_seen,
                    f"{phase_id}: PASSED phase appears after an unfinished earlier phase")
            audit_path = phase.get("latest_audit")
            require(isinstance(audit_path, str) and audit_path,
                    f"{phase_id}: PASSED requires latest_audit")
            validate_audit(audit_path, phase_id, "PASS")
        else:
            first_nonpassed_seen = True
            if status == "FAILED":
                audit_path = phase.get("latest_audit")
                require(isinstance(audit_path, str) and audit_path,
                        f"{phase_id}: FAILED requires latest_audit")
                validate_audit(audit_path, phase_id, "FAIL")
            elif status in {"LOCKED", "IMPLEMENTING"}:
                require(phase.get("latest_audit") is None,
                        f"{phase_id}: {status} must not carry latest_audit")
            elif status == "AUDIT_REQUIRED":
                previous = phase.get("latest_audit")
                if previous is not None:
                    require(isinstance(previous, str) and previous,
                            f"{phase_id}: latest_audit must be null or a path")

    print(f"MWS_PLAYABLE_GATE_OK program={state['program']} active={state.get('active_phase') or 'none'}")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except GateError as exc:
        print(f"MWS_PLAYABLE_GATE_FAIL {exc}", file=sys.stderr)
        raise SystemExit(2)
