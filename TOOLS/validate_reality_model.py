#!/usr/bin/env python3
"""Validate blocking reality/model review evidence for playable-prototype phases."""
from __future__ import annotations

import json
import re
import sys
from pathlib import Path
from typing import Any

ROOT = Path(__file__).resolve().parents[1]
STATE_FILE = ROOT / "MACHINE/playable-prototype.json"
POLICY_FILE = ROOT / "DESIGN/REALITY_MODELING_POLICY.md"
MODELS_DIR = ROOT / "DESIGN/MODELS"
AUDIT_ROOT = "AUDIT_RESULTS/PLAYABLE_PROTOTYPE/"
MODEL_ROOT = "DESIGN/MODELS/"
PROGRAM = "playable-prototype-v1"

DIMENSIONS = (
    "causal_logic",
    "historical_grounding",
    "player_npc_symmetry",
    "long_horizon",
)
VERDICTS = {"PASS", "FAIL", "NOT_APPLICABLE"}
MODEL_STATUSES = {"MODEL_UNDERDEFINED", "REVIEW_REQUIRED", "ACCEPTED"}
MODEL_STATUS_RE = re.compile(
    r"^Status:\s*(?:\*\*)?(MODEL_UNDERDEFINED|REVIEW_REQUIRED|ACCEPTED)(?:\*\*)?\s*$"
)
PHASE_RULES = {
    "P3_SEMANTIC_LOCATION_AND_TRAVEL": {
        "required_review": {"causal_logic", "historical_grounding", "player_npc_symmetry"},
        "require_contracts": True,
    },
    "P4_REMOVE_HOT_PATH_FULL_STATE_CLONE": {
        "required_review": set(),
        "require_contracts": False,
    },
    "P5_FOOD_SHORTAGE_GAMEPLAY_LOOP": {
        "required_review": set(DIMENSIONS),
        "require_contracts": True,
    },
    "P6_MEASURABLE_VERTICAL_SLICE": {
        "required_review": set(DIMENSIONS),
        "require_contracts": True,
    },
}


class RealityGateError(RuntimeError):
    pass


def need(condition: bool, message: str) -> None:
    if not condition:
        raise RealityGateError(message)


def read_json(path: Path) -> dict[str, Any]:
    try:
        value = json.loads(path.read_text(encoding="utf-8"))
    except FileNotFoundError as exc:
        raise RealityGateError(f"missing required file: {path}") from exc
    except json.JSONDecodeError as exc:
        raise RealityGateError(f"invalid JSON in {path}: {exc}") from exc
    need(isinstance(value, dict), f"{path} must contain a JSON object")
    return value


def nonempty_text(value: Any) -> bool:
    return isinstance(value, str) and bool(value.strip())


def evidence_list(entry: dict[str, Any], label: str) -> list[Any]:
    evidence = entry.get("evidence")
    need(isinstance(evidence, list) and bool(evidence), f"{label}: evidence must be a non-empty list")
    need(all(nonempty_text(item) for item in evidence), f"{label}: evidence entries must be non-empty strings")
    return evidence


def validate_dimension(entry: Any, label: str, required_review: bool) -> str:
    need(isinstance(entry, dict), f"{label}: review entry must be an object")
    verdict = entry.get("verdict")
    need(verdict in VERDICTS, f"{label}: verdict must be PASS, FAIL or NOT_APPLICABLE")
    need(nonempty_text(entry.get("summary")), f"{label}: summary is required")
    evidence_list(entry, label)
    if required_review:
        need(verdict != "NOT_APPLICABLE", f"{label}: this phase may not waive this review dimension")
    return verdict


def validate_historical(entry: dict[str, Any], label: str, required_review: bool) -> str:
    verdict = validate_dimension(entry, label, required_review)
    if verdict != "PASS":
        return verdict

    context = entry.get("model_context")
    need(isinstance(context, dict), f"{label}: PASS requires model_context")
    need(nonempty_text(context.get("region")), f"{label}: model_context.region is required")
    need(nonempty_text(context.get("period")), f"{label}: model_context.period is required")

    sources = entry.get("sources")
    need(isinstance(sources, list) and len(sources) >= 2, f"{label}: PASS requires at least two credible sources")
    citations: set[str] = set()
    for index, source in enumerate(sources):
        source_label = f"{label}.sources[{index}]"
        need(isinstance(source, dict), f"{source_label}: source must be an object")
        citation = source.get("citation")
        supports = source.get("supports")
        need(nonempty_text(citation), f"{source_label}: citation is required")
        need(nonempty_text(supports), f"{source_label}: supports is required")
        citations.add(citation.strip())
    need(len(citations) >= 2, f"{label}: sources must contain at least two distinct citations")
    return verdict


def validate_long_horizon(entry: dict[str, Any], label: str, required_review: bool) -> str:
    verdict = validate_dimension(entry, label, required_review)
    if verdict != "PASS":
        return verdict
    years = entry.get("horizon_years")
    need(isinstance(years, int) and not isinstance(years, bool), f"{label}: PASS requires integer horizon_years")
    need(years >= 10, f"{label}: PASS requires horizon_years >= 10, got {years}")
    return verdict


def contract_status(path: Path) -> str:
    lines = path.read_text(encoding="utf-8").splitlines()[:16]
    for line in lines:
        match = MODEL_STATUS_RE.match(line.strip())
        if match:
            status = match.group(1)
            need(status in MODEL_STATUSES, f"{path}: invalid model status {status}")
            return status
    raise RealityGateError(f"{path}: model contract must declare Status near the top")


def validate_model_contracts(
    audit: dict[str, Any],
    phase_id: str,
    required: bool,
) -> list[tuple[str, str]]:
    contracts = audit.get("model_contracts")
    if not required and contracts is None:
        return []
    need(isinstance(contracts, list), f"{phase_id}: model_contracts must be a list")
    if required:
        need(bool(contracts), f"{phase_id}: at least one model contract is required")

    checked: list[tuple[str, str]] = []
    for item in contracts:
        need(nonempty_text(item), f"{phase_id}: model contract paths must be non-empty strings")
        path_text = item.replace("\\", "/").lstrip("./")
        need(path_text.startswith(MODEL_ROOT) and path_text.endswith(".md"),
             f"{phase_id}: model contract must be Markdown under {MODEL_ROOT}: {item}")
        need(path_text != f"{MODEL_ROOT}README.md",
             f"{phase_id}: README is not a model contract")
        path = ROOT / path_text
        need(path.is_file(), f"{phase_id}: model contract does not exist: {path_text}")
        checked.append((path_text, contract_status(path)))
    return checked


def validate_model_review(audit: dict[str, Any], phase_id: str) -> None:
    rules = PHASE_RULES.get(phase_id)
    if rules is None:
        return

    need(audit.get("program") == PROGRAM, f"{phase_id}: audit program mismatch")
    need(audit.get("phase_id") == phase_id, f"{phase_id}: audit phase mismatch")

    review = audit.get("model_review")
    need(isinstance(review, dict), f"{phase_id}: model_review is required starting at P3")
    required_review: set[str] = rules["required_review"]

    verdicts: dict[str, str] = {}
    for dimension in DIMENSIONS:
        label = f"{phase_id}.model_review.{dimension}"
        entry = review.get(dimension)
        if dimension == "historical_grounding":
            need(isinstance(entry, dict), f"{label}: review entry must be an object")
            verdicts[dimension] = validate_historical(entry, label, dimension in required_review)
        elif dimension == "long_horizon":
            need(isinstance(entry, dict), f"{label}: review entry must be an object")
            verdicts[dimension] = validate_long_horizon(entry, label, dimension in required_review)
        else:
            verdicts[dimension] = validate_dimension(entry, label, dimension in required_review)

    contracts = validate_model_contracts(audit, phase_id, bool(rules["require_contracts"]))

    any_model_fail = any(verdict == "FAIL" for verdict in verdicts.values())
    if any_model_fail:
        need(audit.get("systems_audit") == "FAIL",
             f"{phase_id}: reality/model FAIL must make systems_audit FAIL")
        need(audit.get("overall") == "FAIL",
             f"{phase_id}: reality/model FAIL must make overall FAIL")

    if audit.get("overall") == "PASS":
        for dimension in required_review:
            need(verdicts[dimension] == "PASS",
                 f"{phase_id}: overall PASS requires {dimension}=PASS")
        for path_text, status in contracts:
            need(status == "ACCEPTED",
                 f"{phase_id}: overall PASS requires accepted model contract, got {status}: {path_text}")


def validate_policy_files() -> None:
    need(POLICY_FILE.is_file(), f"missing blocking reality policy: {POLICY_FILE}")
    need(MODELS_DIR.is_dir(), f"missing model-contract directory: {MODELS_DIR}")


def validate_state(state: dict[str, Any]) -> None:
    need(state.get("program") == PROGRAM, "unexpected playable-prototype program")
    phases = state.get("phases")
    need(isinstance(phases, list), "phases must be a list")
    for phase in phases:
        need(isinstance(phase, dict), "phase entries must be objects")
        phase_id = phase.get("id")
        if phase_id not in PHASE_RULES:
            continue
        status = phase.get("status")
        audit_path = phase.get("latest_audit")
        if status not in {"PASSED", "FAILED"}:
            continue
        need(nonempty_text(audit_path), f"{phase_id}: {status} requires latest_audit")
        normalized = audit_path.replace("\\", "/").lstrip("./")
        need(normalized.startswith(AUDIT_ROOT) and normalized.endswith(".json"),
             f"{phase_id}: audit must be JSON under {AUDIT_ROOT}")
        audit = read_json(ROOT / normalized)
        validate_model_review(audit, phase_id)


def main() -> int:
    validate_policy_files()
    validate_state(read_json(STATE_FILE))
    print("MWS_REALITY_MODEL_GATE_OK")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except RealityGateError as exc:
        print(f"MWS_REALITY_MODEL_GATE_FAIL {exc}", file=sys.stderr)
        raise SystemExit(2)
