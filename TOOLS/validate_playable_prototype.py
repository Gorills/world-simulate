#!/usr/bin/env python3
"""Validate playable-prototype phase state, audit evidence and Git scope."""

from __future__ import annotations

import json
import re
import shutil
import subprocess
import sys
from pathlib import Path
from typing import Any

ROOT = Path(__file__).resolve().parents[1]
STATE_FILE = ROOT / "MACHINE/playable-prototype.json"
STATE_REPO_PATH = "MACHINE/playable-prototype.json"
PROGRAM = "playable-prototype-v1"

STATUSES = {"LOCKED", "IMPLEMENTING", "AUDIT_REQUIRED", "FAILED", "PASSED"}
ACTIVE = {"IMPLEMENTING", "AUDIT_REQUIRED", "FAILED"}
VERDICTS = {"PASS", "FAIL"}
REVIEW_MODES = {"independent-post-commit", "human-owner"}
TRANSITIONS = {
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
    "AUDIT_RESULTS/PLAYABLE_PROTOTYPE/",
)
PROTECTED_FILES = {
    "AGENTS.md",
    ".editorconfig",
    ".gitignore",
    "Directory.Build.props",
    "global.json",
    "WorldSimulate.sln",
    "WorldSimulate.Core.slnf",
}
SHA40 = re.compile(r"^[0-9a-f]{40}$")


class GateError(RuntimeError):
    pass


def need(condition: bool, message: str) -> None:
    if not condition:
        raise GateError(message)


def read_json(path: Path) -> dict[str, Any]:
    try:
        value = json.loads(path.read_text(encoding="utf-8"))
    except FileNotFoundError as exc:
        raise GateError(f"missing required file: {path}") from exc
    except json.JSONDecodeError as exc:
        raise GateError(f"invalid JSON in {path}: {exc}") from exc
    need(isinstance(value, dict), f"{path} must contain a JSON object")
    return value


def has_git() -> bool:
    return (ROOT / ".git").exists() and shutil.which("git") is not None


def git(args: list[str], *, required: bool = True) -> str | None:
    if not has_git():
        return None
    result = subprocess.run(
        ["git", *args],
        cwd=ROOT,
        text=True,
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
    )
    if result.returncode != 0:
        if required:
            detail = result.stderr.strip() or result.stdout.strip() or "git command failed"
            raise GateError(f"git {' '.join(args)}: {detail}")
        return None
    return result.stdout


def commit_sha(revision: str) -> str:
    value = git(["rev-parse", f"{revision}^{{commit}}"])
    need(value is not None and SHA40.fullmatch(value.strip()) is not None,
         f"cannot resolve commit revision: {revision}")
    return value.strip()


def json_at(revision: str, repo_path: str = STATE_REPO_PATH) -> dict[str, Any] | None:
    content = git(["show", f"{revision}:{repo_path}"], required=False)
    if content is None:
        return None
    try:
        value = json.loads(content)
    except json.JSONDecodeError as exc:
        raise GateError(f"invalid JSON at {revision}:{repo_path}: {exc}") from exc
    need(isinstance(value, dict), f"{revision}:{repo_path} must contain a JSON object")
    return value


def phase_map(state: dict[str, Any]) -> tuple[list[str], dict[str, dict[str, Any]]]:
    phases = state.get("phases")
    need(isinstance(phases, list) and phases, "phases must be a non-empty list")
    order: list[str] = []
    by_id: dict[str, dict[str, Any]] = {}
    for phase in phases:
        need(isinstance(phase, dict), "each phase must be an object")
        phase_id = phase.get("id")
        need(isinstance(phase_id, str) and phase_id, "phase id is required")
        need(phase_id not in by_id, f"duplicate phase id: {phase_id}")
        order.append(phase_id)
        by_id[phase_id] = phase
    return order, by_id


def active_phase(state: dict[str, Any]) -> tuple[str | None, str | None]:
    order, by_id = phase_map(state)
    found = [phase_id for phase_id in order if by_id[phase_id].get("status") in ACTIVE]
    need(len(found) <= 1, f"multiple active phases: {found}")
    if not found:
        return None, None
    phase_id = found[0]
    return phase_id, by_id[phase_id]["status"]


def stable_topology(state: dict[str, Any]) -> dict[str, Any]:
    return {key: value for key, value in state.items() if key not in {"active_phase", "phases"}}


def stable_phase_metadata(phase: dict[str, Any]) -> dict[str, Any]:
    return {key: value for key, value in phase.items() if key not in {"status", "latest_audit"}}


def status_changes(old: dict[str, Any], new: dict[str, Any]) -> list[tuple[str, str, str]]:
    old_order, old_by_id = phase_map(old)
    new_order, new_by_id = phase_map(new)
    need(old_order == new_order, "phase ids/order changed; amend the program version deliberately")
    changed: list[tuple[str, str, str]] = []
    for phase_id in old_order:
        old_status = old_by_id[phase_id].get("status")
        new_status = new_by_id[phase_id].get("status")
        if old_status != new_status:
            changed.append((phase_id, old_status, new_status))
    return changed


def validate_evidence_transition(
    phase_id: str,
    old_phase: dict[str, Any],
    new_phase: dict[str, Any],
    label: str,
) -> None:
    old_status = old_phase.get("status")
    new_status = new_phase.get("status")
    old_audit = old_phase.get("latest_audit")
    new_audit = new_phase.get("latest_audit")

    if old_status == new_status:
        need(old_audit == new_audit,
             f"{label}: {phase_id} latest_audit changed without a status transition")
        return

    if (old_status, new_status) in {
        ("LOCKED", "IMPLEMENTING"),
        ("IMPLEMENTING", "AUDIT_REQUIRED"),
    }:
        need(old_audit is None and new_audit is None,
             f"{label}: {phase_id} must not carry audit evidence into {new_status}")
        return

    if old_status == "AUDIT_REQUIRED" and new_status in {"PASSED", "FAILED"}:
        need(old_audit is None,
             f"{label}: {phase_id} AUDIT_REQUIRED checkpoint must not already carry verdict evidence")
        need(isinstance(new_audit, str) and new_audit,
             f"{label}: {phase_id} {new_status} requires new audit evidence")
        return

    if old_status == "FAILED" and new_status == "IMPLEMENTING":
        need(isinstance(old_audit, str) and old_audit,
             f"{label}: {phase_id} FAILED must reference the failed audit")
        need(new_audit is None,
             f"{label}: {phase_id} repair must clear latest_audit before implementation")
        return

    need(old_audit == new_audit,
         f"{label}: unexpected audit evidence change for {phase_id}")


def validate_transition(old: dict[str, Any], new: dict[str, Any], label: str) -> None:
    need(stable_topology(old) == stable_topology(new),
         f"{label}: program metadata changed; create a new program version instead")

    changed = status_changes(old, new)
    old_order, old_by_id = phase_map(old)
    _, new_by_id = phase_map(new)

    for phase_id in old_order:
        old_phase = old_by_id[phase_id]
        new_phase = new_by_id[phase_id]
        need(stable_phase_metadata(old_phase) == stable_phase_metadata(new_phase),
             f"{label}: immutable metadata changed for {phase_id}")

        old_status = old_phase.get("status")
        new_status = new_phase.get("status")
        need(old_status in TRANSITIONS, f"{label}: invalid old status for {phase_id}")
        need(new_status in TRANSITIONS[old_status],
             f"{label}: illegal transition {phase_id} {old_status} -> {new_status}")
        validate_evidence_transition(phase_id, old_phase, new_phase, label)

    need(len(changed) <= 1, f"{label}: change at most one phase status, got {changed}")


def normalize(path: str) -> str:
    value = path.replace("\\", "/")
    while value.startswith("./"):
        value = value[2:]
    return value.lstrip("/")


def is_protected(path: str) -> bool:
    value = normalize(path)
    return value in PROTECTED_FILES or any(value.startswith(prefix) for prefix in PROTECTED_PREFIXES)


def is_audit_json(path: str) -> bool:
    value = normalize(path)
    return value.startswith("AUDIT_RESULTS/PLAYABLE_PROTOTYPE/") and value.endswith(".json")


def git_names(args: list[str]) -> set[str]:
    output = git(args)
    return set() if output is None else {line.strip() for line in output.splitlines() if line.strip()}


def path_exists(revision: str, path: str) -> bool:
    return git(["cat-file", "-e", f"{revision}:{path}"], required=False) is not None


def dirty_protected() -> set[str]:
    paths = (
        git_names(["diff", "--no-renames", "--name-only"])
        | git_names(["diff", "--cached", "--no-renames", "--name-only"])
        | git_names(["ls-files", "--others", "--exclude-standard"])
    )
    return {path for path in paths if is_protected(path)}


def head_parents() -> list[str]:
    value = git(["rev-list", "--parents", "-n", "1", "HEAD"])
    if value is None:
        return []
    return value.strip().split()[1:]


def select_parent(head_state: dict[str, Any] | None) -> str | None:
    parents = head_parents()
    if not parents:
        return None
    if head_state is not None:
        for parent in parents:
            if json_at(parent) == head_state:
                return parent
    return parents[0]


def audit_transition(old: dict[str, Any], new: dict[str, Any]) -> tuple[str, str] | None:
    changed = status_changes(old, new)
    if (
        len(changed) == 1
        and changed[0][1] == "AUDIT_REQUIRED"
        and changed[0][2] in {"PASSED", "FAILED"}
    ):
        return changed[0][0], changed[0][2]
    return None


def expected_new_audit_path(old: dict[str, Any], new: dict[str, Any]) -> str | None:
    transition = audit_transition(old, new)
    if transition is None:
        return None
    phase_id, _ = transition
    _, new_by_id = phase_map(new)
    path = new_by_id[phase_id].get("latest_audit")
    return path if isinstance(path, str) and path else None


def validate_audit_path_changes(
    changed_audits: set[str],
    old: dict[str, Any] | None,
    new: dict[str, Any] | None,
    base_revision: str,
    label: str,
) -> None:
    if not changed_audits:
        return
    need(old is not None and new is not None,
         f"{label}: audit evidence changed without comparable program states")
    expected = expected_new_audit_path(old, new)
    need(expected is not None and changed_audits == {expected},
         f"{label}: audit evidence is append-only; only the new latest_audit may be added")
    need(not path_exists(base_revision, expected),
         f"{label}: audit evidence already exists and cannot be rewritten: {expected}")


def audit_record(path_text: str) -> dict[str, Any]:
    path = (ROOT / path_text).resolve()
    try:
        path.relative_to(ROOT.resolve())
    except ValueError as exc:
        raise GateError(f"audit path escapes repository: {path_text}") from exc
    return read_json(path)


def validate_audit(path_text: str, phase_id: str, expected: str) -> dict[str, Any]:
    need(
        path_text.startswith("AUDIT_RESULTS/PLAYABLE_PROTOTYPE/") and path_text.endswith(".json"),
        f"{phase_id}: audit must be JSON under AUDIT_RESULTS/PLAYABLE_PROTOTYPE/",
    )
    audit = audit_record(path_text)
    need(audit.get("schema_version") == 1, f"{phase_id}: unsupported audit schema")
    need(audit.get("program") == PROGRAM, f"{phase_id}: audit program mismatch")
    need(audit.get("phase_id") == phase_id, f"{phase_id}: audit phase mismatch")

    subject = audit.get("subject_sha")
    need(isinstance(subject, str) and SHA40.fullmatch(subject) is not None,
         f"{phase_id}: subject_sha must be lowercase 40-hex")
    need(isinstance(audit.get("reviewer"), str) and audit["reviewer"].strip(),
         f"{phase_id}: reviewer is required")
    need(audit.get("review_mode") in REVIEW_MODES, f"{phase_id}: invalid review_mode")

    code = audit.get("code_review")
    systems = audit.get("systems_audit")
    overall = audit.get("overall")
    need(code in VERDICTS and systems in VERDICTS and overall in VERDICTS,
         f"{phase_id}: invalid audit verdict")
    need(overall == ("PASS" if code == systems == "PASS" else "FAIL"),
         f"{phase_id}: overall verdict does not match component verdicts")
    need(overall == expected, f"{phase_id}: expected audit {expected}, got {overall}")

    for field in ("validation", "systems_examined", "findings", "residual_risks"):
        need(isinstance(audit.get(field), list), f"{phase_id}: {field} must be a list")
    need(bool(audit["validation"]), f"{phase_id}: validation evidence must not be empty")
    need(bool(audit["systems_examined"]), f"{phase_id}: systems_examined must not be empty")
    if overall == "FAIL":
        need(bool(audit["findings"]), f"{phase_id}: failed audit must record findings")

    if not has_git():
        return audit

    need(git(["cat-file", "-e", f"{subject}^{{commit}}"], required=False) is not None,
         f"{phase_id}: audit subject commit is unavailable")
    ancestor = subprocess.run(
        ["git", "merge-base", "--is-ancestor", subject, "HEAD"],
        cwd=ROOT,
        stdout=subprocess.DEVNULL,
        stderr=subprocess.DEVNULL,
    )
    need(ancestor.returncode == 0, f"{phase_id}: audit subject must be an ancestor of HEAD")

    subject_state = json_at(subject)
    need(subject_state is not None, f"{phase_id}: audit subject has no program state")
    _, subject_by_id = phase_map(subject_state)
    need(phase_id in subject_by_id, f"{phase_id}: audit subject does not contain phase")
    need(subject_by_id[phase_id].get("status") == "AUDIT_REQUIRED",
         f"{phase_id}: audit subject must have status AUDIT_REQUIRED")
    return audit


def validate_audit_binding(
    old: dict[str, Any],
    new: dict[str, Any],
    subject_revision: str,
    label: str,
) -> None:
    transition = audit_transition(old, new)
    if transition is None:
        return

    phase_id, verdict_status = transition
    _, new_by_id = phase_map(new)
    audit_path = new_by_id[phase_id].get("latest_audit")
    need(isinstance(audit_path, str) and audit_path,
         f"{label}: {phase_id} verdict requires latest_audit")
    expected_subject = commit_sha(subject_revision)
    audit = audit_record(audit_path)
    need(audit.get("subject_sha") == expected_subject,
         f"{label}: {phase_id} audit must review exact checkpoint {expected_subject}")
    need(audit.get("overall") == ("PASS" if verdict_status == "PASSED" else "FAIL"),
         f"{label}: {phase_id} audit verdict does not match phase status")


def validate_state(state: dict[str, Any]) -> None:
    need(state.get("schema_version") == 1, "unsupported state schema")
    need(state.get("program") == PROGRAM, "unexpected program id")
    policy = state.get("policy")
    need(isinstance(policy, str) and (ROOT / policy).is_file(), "program policy file is missing")

    order, by_id = phase_map(state)
    for index, phase_id in enumerate(order):
        phase = by_id[phase_id]
        status = phase.get("status")
        need(status in STATUSES, f"{phase_id}: invalid status")
        dependencies = phase.get("depends_on")
        need(isinstance(dependencies, list), f"{phase_id}: depends_on must be a list")
        for dependency in dependencies:
            need(dependency in by_id, f"{phase_id}: unknown dependency {dependency}")
            need(order.index(dependency) < index, f"{phase_id}: dependency must be earlier")
            if status != "LOCKED":
                need(by_id[dependency]["status"] == "PASSED",
                     f"{phase_id}: cannot leave LOCKED before {dependency} PASSED")

    active_id, _ = active_phase(state)
    need(state.get("active_phase") == active_id,
         f"active_phase must be {active_id!r}, got {state.get('active_phase')!r}")

    unfinished = False
    for phase_id in order:
        phase = by_id[phase_id]
        status = phase["status"]
        audit_path = phase.get("latest_audit")
        if status == "PASSED":
            need(not unfinished, f"{phase_id}: PASSED after unfinished earlier phase")
            need(isinstance(audit_path, str) and audit_path, f"{phase_id}: PASSED requires audit")
            validate_audit(audit_path, phase_id, "PASS")
        else:
            unfinished = True
            if status == "FAILED":
                need(isinstance(audit_path, str) and audit_path, f"{phase_id}: FAILED requires audit")
                validate_audit(audit_path, phase_id, "FAIL")
            elif status in {"LOCKED", "IMPLEMENTING"}:
                need(audit_path is None, f"{phase_id}: {status} must not carry latest_audit")
            elif status == "AUDIT_REQUIRED":
                need(audit_path is None, f"{phase_id}: AUDIT_REQUIRED must not carry verdict evidence")


def validate_git_scope(state: dict[str, Any]) -> None:
    if not has_git():
        return

    committed = json_at("HEAD")
    if committed is not None and committed != state:
        validate_transition(committed, state, "working tree")
        validate_audit_binding(committed, state, "HEAD", "working tree")

    parent = select_parent(committed)
    need(parent is not None, "Git history unavailable; full repository history is required")
    parent_state = json_at(parent)
    if committed is not None and parent_state is not None:
        validate_transition(parent_state, committed, "HEAD")
        validate_audit_binding(parent_state, committed, parent, "HEAD")

    dirty = dirty_protected()
    dirty_audits = {path for path in dirty if is_audit_json(path)}
    validate_audit_path_changes(dirty_audits, committed, state, "HEAD", "working tree")
    ordinary_dirty = dirty - dirty_audits

    if ordinary_dirty:
        _, current_status = active_phase(state)
        allowed = current_status == "IMPLEMENTING"

        if not allowed and current_status == "AUDIT_REQUIRED" and committed is not None:
            current_id, _ = active_phase(state)
            committed_id, committed_status = active_phase(committed)
            allowed = current_id == committed_id and committed_status == "IMPLEMENTING"

        need(allowed,
             f"protected working-tree changes are outside allowed phase scope: {sorted(ordinary_dirty)}")

    changed = {
        path
        for path in git_names(["diff", "--no-renames", "--name-only", parent, "HEAD"])
        if is_protected(path)
    }
    changed_audits = {path for path in changed if is_audit_json(path)}
    validate_audit_path_changes(changed_audits, parent_state, committed, parent, "HEAD")
    ordinary_changed = changed - changed_audits
    if not ordinary_changed:
        return

    need(committed is not None, "HEAD has protected changes but no committed program state")
    head_id, head_status = active_phase(committed)
    if head_status == "IMPLEMENTING":
        return

    if head_status == "AUDIT_REQUIRED" and parent_state is not None:
        parent_id, parent_status = active_phase(parent_state)
        if parent_id == head_id and parent_status == "IMPLEMENTING":
            return

    raise GateError(
        f"protected HEAD changes are outside allowed phase scope: {sorted(ordinary_changed)}"
    )


def main() -> int:
    state = read_json(STATE_FILE)
    validate_state(state)
    validate_git_scope(state)
    print(f"MWS_PLAYABLE_GATE_OK program={PROGRAM} active={state.get('active_phase') or 'none'}")
    return 0


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except GateError as exc:
        print(f"MWS_PLAYABLE_GATE_FAIL {exc}", file=sys.stderr)
        raise SystemExit(2)
