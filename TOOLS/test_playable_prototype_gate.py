#!/usr/bin/env python3
from __future__ import annotations

import copy
import importlib.util
from pathlib import Path

MODULE_PATH = Path(__file__).with_name("validate_playable_prototype.py")
spec = importlib.util.spec_from_file_location("playable_gate", MODULE_PATH)
assert spec is not None and spec.loader is not None
gate = importlib.util.module_from_spec(spec)
spec.loader.exec_module(gate)


def base_state() -> dict:
    return {
        "schema_version": 1,
        "program": "playable-prototype-v1",
        "policy": "DESIGN/PLAYABLE_PROTOTYPE_PROGRAM.md",
        "active_phase": "P0_PROCESS_GATE",
        "phases": [
            {
                "id": "P0_PROCESS_GATE",
                "status": "FAILED",
                "depends_on": [],
                "latest_audit": "AUDIT_RESULTS/PLAYABLE_PROTOTYPE/P0_R4.json",
            },
            {
                "id": "P1_PLAYABLE_USES_WORLD_RUNTIME",
                "status": "LOCKED",
                "depends_on": ["P0_PROCESS_GATE"],
                "latest_audit": None,
            },
            {
                "id": "P2_AUTHORITATIVE_PLAYER_ACTOR",
                "status": "LOCKED",
                "depends_on": ["P1_PLAYABLE_USES_WORLD_RUNTIME"],
                "latest_audit": None,
            },
        ],
    }


def must_fail(fn, label: str) -> None:
    try:
        fn()
    except gate.GateError:
        return
    raise AssertionError(f"expected GateError: {label}")


def main() -> int:
    assert gate.normalize(".github/workflows/x.yml") == ".github/workflows/x.yml"
    assert gate.is_protected(".github/workflows/x.yml")
    assert gate.is_protected(".editorconfig")
    assert gate.is_protected("AUDIT_RESULTS/PLAYABLE_PROTOTYPE/P0_R5.json")

    old = base_state()
    repair = copy.deepcopy(old)
    repair["phases"][0]["status"] = "IMPLEMENTING"
    repair["phases"][0]["latest_audit"] = None
    gate.validate_transition(old, repair, "test")

    illegal_topology = copy.deepcopy(repair)
    illegal_topology["phases"][1]["depends_on"] = []
    must_fail(lambda: gate.validate_transition(repair, illegal_topology, "test"), "dependency mutation")

    illegal_policy = copy.deepcopy(repair)
    illegal_policy["policy"] = "README.md"
    must_fail(lambda: gate.validate_transition(repair, illegal_policy, "test"), "policy mutation")

    close = copy.deepcopy(repair)
    close["phases"][0]["status"] = "AUDIT_REQUIRED"
    gate.validate_transition(repair, close, "test")

    verdict = copy.deepcopy(close)
    verdict["phases"][0]["status"] = "PASSED"
    verdict["phases"][0]["latest_audit"] = "AUDIT_RESULTS/PLAYABLE_PROTOTYPE/P0_R5.json"
    verdict["active_phase"] = None
    gate.validate_transition(close, verdict, "test")

    stale_evidence = copy.deepcopy(verdict)
    stale_evidence["phases"][0]["latest_audit"] = "AUDIT_RESULTS/PLAYABLE_PROTOTYPE/P0_OLD.json"
    must_fail(lambda: gate.validate_transition(verdict, stale_evidence, "test"), "passed evidence rewrite")

    two_changes = copy.deepcopy(close)
    two_changes["phases"][0]["status"] = "PASSED"
    two_changes["phases"][0]["latest_audit"] = "AUDIT_RESULTS/PLAYABLE_PROTOTYPE/P0_R5.json"
    two_changes["phases"][1]["status"] = "IMPLEMENTING"
    two_changes["active_phase"] = "P1_PLAYABLE_USES_WORLD_RUNTIME"
    must_fail(lambda: gate.validate_transition(close, two_changes, "test"), "two phase changes")

    wrong_evidence = copy.deepcopy(close)
    wrong_evidence["phases"][0]["status"] = "FAILED"
    wrong_evidence["phases"][0]["latest_audit"] = None
    must_fail(lambda: gate.validate_transition(close, wrong_evidence, "test"), "missing verdict audit")

    original_path_exists = gate.path_exists
    try:
        gate.path_exists = lambda revision, path: False
        gate.validate_audit_path_changes(
            {"AUDIT_RESULTS/PLAYABLE_PROTOTYPE/P0_R5.json"},
            close,
            verdict,
            "HEAD",
            "test",
        )
        must_fail(
            lambda: gate.validate_audit_path_changes(
                {"AUDIT_RESULTS/PLAYABLE_PROTOTYPE/P0_R4.json"},
                repair,
                repair,
                "HEAD",
                "test",
            ),
            "historical audit rewrite during implementation",
        )
        gate.path_exists = lambda revision, path: True
        must_fail(
            lambda: gate.validate_audit_path_changes(
                {"AUDIT_RESULTS/PLAYABLE_PROTOTYPE/P0_R5.json"},
                close,
                verdict,
                "HEAD",
                "test",
            ),
            "reuse existing audit path",
        )
    finally:
        gate.path_exists = original_path_exists

    print("MWS_PLAYABLE_GATE_SELF_TEST_OK")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
