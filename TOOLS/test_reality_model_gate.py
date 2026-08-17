#!/usr/bin/env python3
from __future__ import annotations

import copy
import importlib.util
from pathlib import Path

MODULE_PATH = Path(__file__).with_name("validate_reality_model.py")
spec = importlib.util.spec_from_file_location("reality_gate", MODULE_PATH)
assert spec is not None and spec.loader is not None
gate = importlib.util.module_from_spec(spec)
spec.loader.exec_module(gate)

P3_CONTRACT = "DESIGN/MODELS/P3_SEMANTIC_LOCATION_AND_TRAVEL.md"


def must_fail(fn, label: str) -> None:
    try:
        fn()
    except gate.RealityGateError:
        return
    raise AssertionError(f"expected RealityGateError: {label}")


def dimension(verdict: str = "PASS") -> dict:
    return {
        "verdict": verdict,
        "summary": "Reviewed against the declared world model.",
        "evidence": ["Independent model-review evidence."],
    }


def historical(verdict: str = "PASS") -> dict:
    value = dimension(verdict)
    if verdict == "PASS":
        value["model_context"] = {
            "region": "reference region",
            "period": "reference period",
        }
        value["sources"] = [
            {"citation": "Source A", "supports": "Claim A"},
            {"citation": "Source B", "supports": "Claim B"},
        ]
    return value


def long_horizon(verdict: str = "NOT_APPLICABLE", years: int | None = None) -> dict:
    value = dimension(verdict)
    if years is not None:
        value["horizon_years"] = years
    return value


def audit_for(phase_id: str) -> dict:
    return {
        "schema_version": 1,
        "program": "playable-prototype-v1",
        "phase_id": phase_id,
        "code_review": "PASS",
        "systems_audit": "PASS",
        "overall": "PASS",
        "model_contracts": [P3_CONTRACT],
        "model_review": {
            "causal_logic": dimension(),
            "historical_grounding": historical(),
            "player_npc_symmetry": dimension(),
            "long_horizon": long_horizon(),
        },
    }


def as_failed(audit: dict) -> dict:
    value = copy.deepcopy(audit)
    value["systems_audit"] = "FAIL"
    value["overall"] = "FAIL"
    return value


def main() -> int:
    # P0-P2 remain backward compatible: reality review starts at P3.
    gate.validate_model_review({"program": gate.PROGRAM}, "P2_AUTHORITATIVE_PLAYER_ACTOR")

    p3 = audit_for("P3_SEMANTIC_LOCATION_AND_TRAVEL")

    # The current P3 contract is intentionally MODEL_UNDERDEFINED, so PASS is blocked.
    must_fail(lambda: gate.validate_model_review(p3, p3["phase_id"]), "underdefined P3 contract cannot pass")

    missing = copy.deepcopy(p3)
    del missing["model_review"]
    must_fail(lambda: gate.validate_model_review(missing, p3["phase_id"]), "P3 missing model review")

    no_history = copy.deepcopy(p3)
    no_history["model_review"]["historical_grounding"] = historical("NOT_APPLICABLE")
    must_fail(lambda: gate.validate_model_review(no_history, p3["phase_id"]), "P3 historical waiver")

    failed_history = as_failed(p3)
    failed_history["model_review"]["historical_grounding"] = historical("FAIL")
    gate.validate_model_review(failed_history, failed_history["phase_id"])

    one_source = as_failed(p3)
    one_source["model_review"]["historical_grounding"]["sources"] = [
        {"citation": "Source A", "supports": "Claim A"}
    ]
    must_fail(lambda: gate.validate_model_review(one_source, p3["phase_id"]), "P3 insufficient historical sources")

    asymmetric = as_failed(p3)
    asymmetric["model_review"]["player_npc_symmetry"] = dimension("FAIL")
    gate.validate_model_review(asymmetric, asymmetric["phase_id"])

    p4 = audit_for("P4_REMOVE_HOT_PATH_FULL_STATE_CLONE")
    p4["model_contracts"] = []
    for name in gate.DIMENSIONS:
        p4["model_review"][name] = dimension("NOT_APPLICABLE")
    gate.validate_model_review(p4, p4["phase_id"])

    p5 = audit_for("P5_FOOD_SHORTAGE_GAMEPLAY_LOOP")
    p5["model_review"]["long_horizon"] = long_horizon("PASS", 9)
    must_fail(lambda: gate.validate_model_review(p5, p5["phase_id"]), "P5 horizon below ten years")

    p5_fail = as_failed(p5)
    p5_fail["model_review"]["long_horizon"] = long_horizon("FAIL", 9)
    gate.validate_model_review(p5_fail, p5_fail["phase_id"])

    bad_causal = copy.deepcopy(p4)
    bad_causal["model_review"]["causal_logic"] = dimension("FAIL")
    must_fail(lambda: gate.validate_model_review(bad_causal, p4["phase_id"]), "model FAIL must fail systems audit")
    bad_causal["systems_audit"] = "FAIL"
    bad_causal["overall"] = "FAIL"
    gate.validate_model_review(bad_causal, p4["phase_id"])

    readme = as_failed(p3)
    readme["model_contracts"] = ["DESIGN/MODELS/README.md"]
    must_fail(lambda: gate.validate_model_review(readme, p3["phase_id"]), "README cannot satisfy model contract")

    print("MWS_REALITY_MODEL_GATE_SELF_TEST_OK")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
