#!/usr/bin/env python3
"""Validate the sequential playable-prototype phase gate."""

from __future__ import annotations

import json
import re
import shutil
import subprocess
import sys
from pathlib import Path
from typing import Any

ROOT = Path(__file__).resolve().parents[1]
STATE_PATH = ROOT / "MACHINE/playable-prototype.json"
STATE_REPO_PATH = "MACHINE/playable-prototype.json"
ALLOWED_STATUSES = {"LOCKED", "IMPLEMENTING", "AUDIT_REQUIRED", "FAILED", "PASSED"}
ACTIVE_STATUSES = {"IMPLEMENTING", "AUDIT_REQUIRED", "FAILED"}
ALLOWED_VERDICTS = {"PASS", "FAIL"}
ALLOWED_REVIEW_MODES = {"independent-post-commit", "human-owner"}
ALLOWED_TRANSITIONS = {
    "LOCKED": {"LOCKED", "IMPLEMENTING"},
    "IMPLEMENTING": {"IMPLEMENTING", "AUDIT_REQUIRED"},
    "AUDIT_REQUIRED": {"AUDIT_REQUIRED", "FAILED", "PASSED"},
    "FAILED": {"FAILED", "IMPLEMENTING"},
    "PASSED": {"PASSED"},
}
PROTECTED_PREFIXES = (
    "src/",
    "tests/",
    "benchmarks/",
    "TOOLS/",
    "DESIGN/",
    ".github/workflows/",
)
PROTECTED_FILES = {
    "AGENTS.md",
    "Directory.Build.props",
    "global.json",
    ".editorconfig",
    "WorldSimulate.sln",
    "WorldSimulate.Core.slnf",
}
SHA40 = re.compile(r"^[0-9a-f]{40}$")


class GateError(RuntimeError):
    pass


def require(condition: bool, message: str) -> None:
    if not condition:
        raise GateError(message)


def load_json(path: Path) -> Any:
    try:
        return json.loads(path.read_text(encoding="utf-8"))
    except FileNotFoundError as exc:
        raise GateError(f"missing required file: {path}") from exc
    except json.JSONDecodeError as exc:
        raise GateError(f"invalid JSON in {path}: {exc}") from exc


def git_available() -> bool:
    return (ROOT / ".git").exists() and shutil.which("git") is not None


def git(args: list[str], *, check: bool = True) -> str | None:
    if not git_available():
        return None
    completed = subprocess.run(
        ["git", *args],
        cwd=ROOT,
        text=True,
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
    )
    if completed.returncode != 0:
        if check:
            detail = completed.stderr.strip() or completed.stdout.strip() or "git command failed"
            raise GateError(f"git {' '.join(args)}: {detail}")
        return None
    return completed.stdout


def git_json_at(revision: str, repo_path: str) -> dict[str, Any] | None:
    content = git(["show", f"{revision}:{repo_path}"], check=False)
    if content is None:
        return None
    try:
        value = json.loads(content)
    except json.JSONDecodeError as exc:
        raise GateError(f"invalid JSON at {revision}:{repo_path}: {exc}") from exc
    require(isinstance(value, dict), f"{revision}:{repo_path} must contain a JSON object")
    return value


def phase_map(state: dict[str, Any]) -> tuple[list[str], dict[str, dict[str, Any]]]:
    phases = state.get("phases")
    require(isinstance(phases, list) and phases, "phases must be a non-empty list")
    order: list[str] = []
    by_id: dict[str, dict[str, Any]] = {}
    for phase in phases:
        require(isinstance(phase, dict), "each phase must be an object")
        phase_id = phase.get("id")
        require(isinstance(phase_id, str) and phase_id, "phase id is required")
        require(phase_id not in by_id, f"duplicate phase id: {phase_id}")
        by_id[phase_id] = phase
        order.append(phase_id)
    return order, by_id


def active_phase_status(state: dict[str, Any]) -> tuple[str | None, str | None]:
    order, by_id = phase_map(state)
    active = [phase_id for phase_id in order if by_id[phase_id].get("status") in ACTIVE_STATUSES]
    require(len(active) <= 1, f"multiple active phases: {active}")
    if not active:
        return None, None
    phase_id = active[0]
    return phase_id, by_id[phase_id]["status"]


def validate_transition(old: dict[str, Any], new: dict[str, Any], label: str) -> None:
    old_order, old_by_id = phase_map(old)
    new_order, new_by_id = phase_map(new)
    require(old_order == new_order, f"{label}: phase ids/order changed; amend the gate deliberately")

    changed: list[str] = []
    for phase_id in old_order:
        old_status = old_by_id[phase_id].get("status")
        new_status = new_by_id[phase_id].get("status")
        require(old_status in ALLOWED_TRANSITIONS, f"{label}: invalid old status for {phase_id}")
        require(new_status in ALLOWED_TRANSITIONS[old_status],
                f"{label}: illegal transition {phase_id} {old_status} -> {new_status}")
        if old_status != new_status:
            changed.append(phase_id)

    require(len(changed) <= 1,
            f"{label}: change at most one phase status per transition, got {changed}")


def is_protected(path: str) -> bool:
    normalized = path.replace("\\", "/").lstrip("./")
    return normalized in PROTECTED_FILES or any(normalized.startswith(prefix) for prefix in PROTECTED_PREFIXES)


def git_name_set(args: list[str]) -> set[str]:
    output = git(args)
    if output is None:
        return set()
    return {line.strip() for line in output.splitlines() if line.strip()}


def worktree_protected_paths() -> set[str]:
    if not git_available():
        return set()
    paths = set()
    paths |= git_name_set(["diff", "--name-only"])
    paths |= git_name_set(["diff", "--cached", "--name-only"])
    paths |= git_name_set(["ls-files", "--others", "--exclude-standard"])
    return {path for path in paths if is_protected(path)}


def head_parents() -> list[str]:
    value = git(["rev-list", "--parents", "-n", "1", "HEAD"])
    if value is None:
        return []
    parts = value.strip().split()
    return parts[1:]


def select_head_parent(head_state: dict[str, Any] | None) -> str | None:
    parents = head_parents()
    if not parents:
        return None
    if head_state is not None:
        for parent in parents:
            if git_json_at(parent, STATE_REPO_PATH) == head_state:
                return parent
    return parents[0]


def head_protected_paths(parent: str) -> set[str]:
    return {
        path for path in git_name_set(["diff", "--name-only", parent, "HEAD"])
        if is_protected(path)
    }


def validate_protected_scope(state: dict[str, Any]) -> None:
    if not git_available():
        return

    phase_id, status = active_phase_status(state)
    committed_state = git_json_at("HEAD", STATE_REPO_PATH)
    dirty = worktree_protected_paths()
    if dirty and status != "IMPLEMENTING":
        if status == "AUDIT_REQUIRED" and committed_state is not None:
            committed_phase_id, committed_status = active_phase_status(committed_state)
            require(
                committed_phase_id == phase_id and committed_status == "IMPLEMENTING",
                "protected working-tree changes in AUDIT_REQUIRED are allowed only while closing "
                "that same committed IMPLEMENTING phase",
            )
        else:
            raise GateError(
                "protected working-tree changes require a phase in IMPLEMENTING; "
                f"active={phase_id or 'none'} status={status or 'none'} paths={sorted(dirty)}"
            )

    parent = select_head_parent(committed_state)
    if parent is None:
        raise GateError("git parent commit is unavailable; the phase gate requires repository history")

    changed = head_protected_paths(parent)
    if not changed:
        return

    require(committed_state is not None, "HEAD has protected changes but no committed phase state")
    head_phase_id, head_status = active_phase_status(committed_state)

    if head_status == "IMPLEMENTING":
        return

    if head_status == "AUDIT_REQUIRED":
        parent_state = git_json_at(parent, STATE_REPO_PATH)
        require(parent_state is not None,
                "protected completion commit requires a parent playable-prototype state")
        parent_phase_id, parent_status = active_phase_status(parent_state)
        require(parent_phase_id == head_phase_id and parent_status == "IMPLEMENTING",
                "protected changes in an AUDIT_REQUIRED commit are allowed only when that same "
                "phase was IMPLEMENTING in the parent commit")
        return

    raise GateError(
        "protected HEAD changes are not allowed unless a phase is IMPLEMENTING or the commit "
        f"closes that same phase into AUDIT_REQUIRED; active={head_phase_id or 'none'} "
        f"status={head_status or 'none'} paths={sorted(changed)}"
    )


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
    require(bool(audit["validation"]), f"{phase_id}: audit validation evidence must not be empty")
    require(bool(audit["systems_examined"]), f"{phase_id}: systems_examined must not be empty")

    if overall == "FAIL":
        require(bool(audit["findings"]), f"{phase_id}: failed audit must record findings")

    if git_available():
        exists = git(["cat-file", "-e", f"{subject_sha}^{{commit}}"], check=False)
        require(exists is not None, f"{phase_id}: audit subject commit does not exist locally")
        ancestor = subprocess.run(
            ["git", "merge-base", "--is-ancestor", subject_sha, "HEAD"],
            cwd=ROOT,
            stdout=subprocess.DEVNULL,
            stderr=subprocess.DEVNULL,
        )
        require(ancestor.returncode == 0, f"{phase_id}: audit subject must be an ancestor of HEAD")
        subject_state = git_json_at(subject_sha, STATE_REPO_PATH)
        require(subject_state is not None, f"{phase_id}: audit subject has no playable-prototype state")
        _, subject_by_id = phase_map(subject_state)
        require(phase_id in subject_by_id, f"{phase_id}: audit subject does not contain the phase")
        require(subject_by_id[phase_id].get("status") == "AUDIT_REQUIRED",
                f"{phase_id}: audit subject must have phase status AUDIT_REQUIRED")

    return audit


def validate_state(state: dict[str, Any]) -> None:
    require(state.get("schema_version") == 1, "unsupported playable-prototype state schema")
    require(state.get("program") == "playable-prototype-v1", "unexpected playable-prototype program id")

    policy = state.get("policy")
    require(isinstance(policy, str) and (ROOT / policy).is_file(), "program policy file is missing")

    order, by_id = phase_map(state)
    for index, phase_id in enumerate(order):
        phase = by_id[phase_id]
        require(phase.get("status") in ALLOWED_STATUSES,
                f"{phase_id}: status must be one of {sorted(ALLOWED_STATUSES)}")
        require(isinstance(phase.get("depends_on"), list), f"{phase_id}: depends_on must be a list")

        for dependency in phase["depends_on"]:
            require(dependency in by_id, f"{phase_id}: unknown dependency {dependency}")
            require(order.index(dependency) < index, f"{phase_id}: dependency {dependency} must be earlier")

        if phase["status"] != "LOCKED":
            for dependency in phase["depends_on"]:
                require(by_id[dependency]["status"] == "PASSED",
                        f"{phase_id}: cannot leave LOCKED before {dependency} is PASSED")

    phase_id, _ = active_phase_status(state)
    require(state.get("active_phase") == phase_id,
            f"active_phase must be {phase_id!r}, got {state.get('active_phase')!r}")

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


def validate_git_transition(state: dict[str, Any]) -> None:
    if not git_available():
        return

    committed_state = git_json_at("HEAD", STATE_REPO_PATH)
    if committed_state is not None and committed_state != state:
        validate_transition(committed_state, state, "working tree")

    parent = select_head_parent(committed_state)
    if parent is None:
        raise GateError("git parent commit is unavailable; the phase gate requires repository history")
    parent_state = git_json_at(parent, STATE_REPO_PATH)
    head_state = committed_state
    if parent_state is not None and head_state is not None:
        validate_transition(parent_state, head_state, "HEAD")


def main() -> int:
    state = load_json(STATE_PATH)
    require(isinstance(state, dict), "playable-prototype state must be a JSON object")
    validate_state(state)
    validate_git_transition(state)
    validate_protected_scope(state)
    print(f"MWS_PLAYABLE_GATE_OK program={state['program']} active={state.get('active_phase') or 'none'}")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except GateError as exc:
        print(f"MWS_PLAYABLE_GATE_FAIL {exc}", file=sys.stderr)
        raise SystemExit(2)
