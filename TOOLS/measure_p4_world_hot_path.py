#!/usr/bin/env python3
"""Measure the P4 public WorldRuntime hot paths against one exact baseline commit."""

from __future__ import annotations

import argparse
import json
import os
import re
import shutil
import subprocess
import tempfile
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
PROJECT = ROOT / "benchmarks/Mws.WorldHotPath.Benchmarks/Mws.WorldHotPath.Benchmarks.csproj"
SHA_RE = re.compile(r"^[0-9a-f]{40}$")
DOTNET_PIN = json.loads((ROOT / "global.json").read_text(encoding="utf-8"))["sdk"]["version"]


def run(cmd: list[str], *, cwd: Path = ROOT) -> str:
    print(f"$ {' '.join(cmd)}")
    result = subprocess.run(
        cmd,
        cwd=cwd,
        text=True,
        stdout=subprocess.PIPE,
        stderr=subprocess.STDOUT,
        check=False,
    )
    output = result.stdout or ""
    if output:
        print(output, end="" if output.endswith("\n") else "\n")
    if result.returncode:
        raise SystemExit(result.returncode)
    return output


def git(*args: str) -> str:
    result = subprocess.run(
        ["git", *args],
        cwd=ROOT,
        text=True,
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
        check=False,
    )
    if result.returncode:
        raise RuntimeError(result.stderr.strip() or f"git {' '.join(args)} failed")
    return result.stdout.strip()


def require_clean() -> None:
    dirty = git("status", "--porcelain", "--untracked-files=all")
    if dirty:
        print(dirty)
        raise SystemExit("P4 measurement requires a clean worktree so evidence maps to one exact commit.")


def ensure_commit(sha: str) -> None:
    result = subprocess.run(
        ["git", "cat-file", "-e", f"{sha}^{{commit}}"],
        cwd=ROOT,
        stdout=subprocess.DEVNULL,
        stderr=subprocess.DEVNULL,
        check=False,
    )
    if result.returncode == 0:
        return
    run(["git", "fetch", "--no-tags", "--depth=1", "origin", sha])


def require_dotnet(dotnet: str) -> None:
    result = subprocess.run(
        [dotnet, "--version"],
        cwd=ROOT,
        text=True,
        stdout=subprocess.PIPE,
        stderr=subprocess.STDOUT,
        check=False,
    )
    actual = (result.stdout or "").strip()
    if result.returncode or actual != DOTNET_PIN:
        raise SystemExit(f"dotnet SDK mismatch: actual={actual!r} expected={DOTNET_PIN!r}")


def clear_benchmark_build() -> None:
    project_dir = PROJECT.parent
    for name in ("bin", "obj"):
        path = project_dir / name
        if path.exists():
            shutil.rmtree(path)


def benchmark(
    dotnet: str,
    label: str,
    source_root: Path,
    output: Path,
    partitions: int,
    advance_hours: int,
    commands: int,
    samples: int,
) -> dict:
    clear_benchmark_build()
    source_root_arg = str(source_root.resolve()) + os.sep
    marker = f"MWS_P4_WORLD_HOT_PATH_OK label={label}"
    stdout = run(
        [
            dotnet,
            "run",
            "--configuration",
            "Release",
            "--project",
            str(PROJECT),
            f"-p:MwsSourceRoot={source_root_arg}",
            "--",
            "--label",
            label,
            "--partitions",
            str(partitions),
            "--advance-hours",
            str(advance_hours),
            "--commands",
            str(commands),
            "--samples",
            str(samples),
            "--output",
            str(output),
        ]
    )
    if marker not in stdout:
        raise SystemExit(f"{label}: missing benchmark marker {marker}")
    return json.loads(output.read_text(encoding="utf-8-sig"))


def percent_delta(before: float | int, after: float | int) -> float:
    if before == 0:
        raise SystemExit("Cannot calculate P4 delta from a zero baseline metric.")
    return ((float(after) - float(before)) / float(before)) * 100.0


def metric(report: dict, section: str, field: str) -> float | int:
    try:
        return report[section][field]
    except (KeyError, TypeError) as exc:
        raise SystemExit(f"Malformed P4 benchmark report: missing {section}.{field}") from exc


def main() -> int:
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--baseline", required=True)
    parser.add_argument("--output-dir", required=True)
    parser.add_argument("--dotnet", default=shutil.which("dotnet") or "dotnet")
    parser.add_argument("--partitions", type=int, default=8)
    parser.add_argument("--advance-hours", type=int, default=48)
    parser.add_argument("--commands", type=int, default=128)
    parser.add_argument("--samples", type=int, default=5)
    args = parser.parse_args()

    if not SHA_RE.fullmatch(args.baseline):
        raise SystemExit("--baseline must be an exact 40-character lowercase commit SHA.")

    require_clean()
    require_dotnet(args.dotnet)
    head = git("rev-parse", "HEAD")
    if not SHA_RE.fullmatch(head):
        raise SystemExit("HEAD did not resolve to an exact commit SHA.")

    ensure_commit(args.baseline)
    ancestor = subprocess.run(
        ["git", "merge-base", "--is-ancestor", args.baseline, head],
        cwd=ROOT,
        check=False,
    )
    if ancestor.returncode != 0:
        raise SystemExit(f"Baseline {args.baseline} is not an ancestor of subject {head}.")

    output_dir = Path(args.output_dir)
    if not output_dir.is_absolute():
        output_dir = ROOT / output_dir
    output_dir.mkdir(parents=True, exist_ok=True)
    baseline_output = output_dir / "baseline.json"
    subject_output = output_dir / "subject.json"
    comparison_output = output_dir / "comparison.json"

    with tempfile.TemporaryDirectory(prefix="mws-p4-baseline-") as temp_root:
        baseline_root = Path(temp_root) / "baseline"
        run(["git", "worktree", "add", "--detach", str(baseline_root), args.baseline])
        try:
            baseline = benchmark(
                args.dotnet,
                "baseline",
                baseline_root,
                baseline_output,
                args.partitions,
                args.advance_hours,
                args.commands,
                args.samples,
            )
        finally:
            run(["git", "worktree", "remove", "--force", str(baseline_root)])

    subject = benchmark(
        args.dotnet,
        "subject",
        ROOT,
        subject_output,
        args.partitions,
        args.advance_hours,
        args.commands,
        args.samples,
    )

    if baseline.get("Scenario") != subject.get("Scenario"):
        raise SystemExit("Baseline and subject benchmark scenarios differ.")
    if baseline.get("RuntimeVersion") != subject.get("RuntimeVersion"):
        raise SystemExit("Baseline and subject runtime versions differ.")

    advance_ms_before = metric(baseline, "Advance", "MedianMilliseconds")
    advance_ms_after = metric(subject, "Advance", "MedianMilliseconds")
    advance_alloc_before = metric(baseline, "Advance", "MedianAllocatedBytes")
    advance_alloc_after = metric(subject, "Advance", "MedianAllocatedBytes")
    command_ms_before = metric(baseline, "Commands", "MedianMilliseconds")
    command_ms_after = metric(subject, "Commands", "MedianMilliseconds")
    command_alloc_before = metric(baseline, "Commands", "MedianAllocatedBytes")
    command_alloc_after = metric(subject, "Commands", "MedianAllocatedBytes")

    comparison = {
        "schema_version": 1,
        "phase_id": "P4_REMOVE_HOT_PATH_FULL_STATE_CLONE",
        "baseline_sha": args.baseline,
        "subject_sha": head,
        "runtime_version": subject["RuntimeVersion"],
        "scenario": subject["Scenario"],
        "baseline": {
            "advance": {
                "median_milliseconds": advance_ms_before,
                "median_allocated_bytes": advance_alloc_before,
            },
            "commands": {
                "median_milliseconds": command_ms_before,
                "median_allocated_bytes": command_alloc_before,
            },
        },
        "subject": {
            "advance": {
                "median_milliseconds": advance_ms_after,
                "median_allocated_bytes": advance_alloc_after,
            },
            "commands": {
                "median_milliseconds": command_ms_after,
                "median_allocated_bytes": command_alloc_after,
            },
        },
        "delta_percent": {
            "advance_milliseconds": percent_delta(advance_ms_before, advance_ms_after),
            "advance_allocated_bytes": percent_delta(advance_alloc_before, advance_alloc_after),
            "command_milliseconds": percent_delta(command_ms_before, command_ms_after),
            "command_allocated_bytes": percent_delta(command_alloc_before, command_alloc_after),
        },
    }
    comparison_output.write_text(
        json.dumps(comparison, indent=2, sort_keys=True) + "\n",
        encoding="utf-8",
    )

    print(
        "MWS_P4_WORLD_HOT_PATH_COMPARE_OK "
        f"baseline={args.baseline} subject={head} "
        f"advance_ms_before={advance_ms_before:.4f} advance_ms_after={advance_ms_after:.4f} "
        f"advance_alloc_before={advance_alloc_before} advance_alloc_after={advance_alloc_after} "
        f"command_ms_before={command_ms_before:.4f} command_ms_after={command_ms_after:.4f} "
        f"command_alloc_before={command_alloc_before} command_alloc_after={command_alloc_after}"
    )
    require_clean()
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
