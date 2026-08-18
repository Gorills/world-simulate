#!/usr/bin/env python3
"""Strict local pre-push gate for the GitHub PR checks."""
from __future__ import annotations

import argparse
import json
import os
import platform
import re
import shutil
import subprocess
import sys
import time
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
CACHE = ROOT / ".cache/prepush"
DOTNET_PIN = json.loads((ROOT / "global.json").read_text())["sdk"]["version"]
GODOT_PIN = "4.7.1"

CORE = ROOT / "WorldSimulate.Core.slnf"
CORE_TEST = ROOT / "tests/Mws.Core.Tests/Mws.Core.Tests.csproj"
ARCH_TEST = ROOT / "tests/Mws.Architecture.Tests/Mws.Architecture.Tests.csproj"
HEADLESS = ROOT / "src/Mws.Headless/Mws.Headless.csproj"
SETTLEMENT_BENCH = ROOT / "benchmarks/Mws.Settlement.Benchmarks/Mws.Settlement.Benchmarks.csproj"
PROOF_SMOKE = ROOT / "benchmarks/Mws.Benchmarks/Mws.Benchmarks.csproj"
GODOT_PROJECT = ROOT / "src/Mws.Client.Godot/Mws.Client.Godot.csproj"
PROOF_MEASURE = ROOT / "BENCHMARKS/Mws.ProofA.Measurements/Mws.ProofA.Measurements.csproj"

CORE_SCOPE = re.compile(
    r"^(src/|tests/|benchmarks/|WorldSimulate\.sln$|WorldSimulate\.Core\.slnf$|"
    r"Directory\.Build\.props$|\.editorconfig$|global\.json$|TOOLS/dev\.py$|\.github/workflows/ci\.yml$)"
)
GODOT_SCOPE = re.compile(
    r"^(src/Mws\.Client\.Godot/|src/Mws\.Simulation\.Api/|src/Mws\.Simulation\.Runtime/Settlement/|"
    r"src/Mws\.Simulation\.Runtime/World/WorldRuntime(\.Advance|\.Commands|\.Inputs|\.Player)?\.cs$|"
    r"src/Mws\.Simulation\.Runtime/Mws\.Simulation\.Runtime\.csproj$|"
    r"src/Mws\.Domain/(WorldSeed|EntityId|SimulationTime|SimulationScopeId)\.cs$|"
    r"src/Mws\.Domain/Mws\.Domain\.csproj$|Directory\.Build\.props$|\.editorconfig$|global\.json$|"
    r"\.github/workflows/ci\.yml$)"
)
PROOF_SMOKE_SCOPE = re.compile(
    r"^(benchmarks/|src/Mws\.Persistence\.Json/|src/Mws\.Simulation\.Runtime/Foundation/|"
    r"src/Mws\.Simulation\.Runtime/Verification/ProofA/|src/Mws\.Simulation\.Api/ProofA|"
    r"src/Mws\.Domain/(CommandId|EntityId|SimulationTime|SimulationScopeId)\.cs$)"
)
PROOF_MEASURE_SCOPE = re.compile(
    r"^(BENCHMARKS/Mws\.ProofA\.Measurements/|BENCHMARKS/WORKLOADS/|src/Mws\.Domain/|"
    r"src/Mws\.Simulation\.Runtime/Mws\.Simulation\.Runtime\.csproj$|src/Mws\.Simulation\.Runtime/Foundation/|"
    r"src/Mws\.Simulation\.Runtime/Verification/ProofA/|src/Mws\.Simulation\.Api/ProofAKernelContracts\.cs$|"
    r"src/Mws\.Persistence\.Json/ProofAKernelJson\.cs$|MACHINE/benchmark-result\.schema\.json$|"
    r"\.github/workflows/proof-a-measure\.yml$)"
)


def run(cmd: list[str], label: str, timeout: int = 300, marker: str | None = None,
        env: dict[str, str] | None = None) -> str:
    print(f"\n== {label} ==\n$ {' '.join(cmd)}")
    started = time.monotonic()
    try:
        result = subprocess.run(
            cmd, cwd=ROOT, env=env or os.environ.copy(), text=True,
            stdout=subprocess.PIPE, stderr=subprocess.STDOUT, timeout=timeout)
    except subprocess.TimeoutExpired as exc:
        raise SystemExit(f"TIMEOUT after {timeout}s: {label}") from exc
    output = result.stdout or ""
    if output:
        print(output, end="" if output.endswith("\n") else "\n")
    if result.returncode:
        raise SystemExit(result.returncode)
    if marker and marker not in output:
        raise SystemExit(f"{label}: missing required marker: {marker}")
    print(f"OK {label} ({time.monotonic() - started:.2f}s)")
    return output


def git(*args: str) -> str:
    result = subprocess.run(["git", *args], cwd=ROOT, text=True,
                            stdout=subprocess.PIPE, stderr=subprocess.PIPE, timeout=30)
    if result.returncode:
        raise SystemExit(result.stderr.strip() or f"git {' '.join(args)} failed")
    return result.stdout.strip()


def require_clean() -> None:
    dirty = git("status", "--porcelain", "--untracked-files=all")
    if dirty:
        print(dirty)
        raise SystemExit("Prepush requires a clean worktree so the result maps to one exact commit.")


def require_tools() -> tuple[str, str]:
    dotnet = shutil.which("dotnet")
    if not dotnet:
        raise SystemExit("dotnet not found")
    actual = subprocess.check_output([dotnet, "--version"], cwd=ROOT, text=True).strip()
    if actual != DOTNET_PIN:
        raise SystemExit(f"dotnet SDK mismatch: local={actual}, CI={DOTNET_PIN}")
    if platform.system() != "Linux":
        print(f"WARNING local OS={platform.system()}, CI OS=Linux")
    return dotnet, actual


def base_and_files(base: str) -> tuple[str, list[str]]:
    try:
        git("rev-parse", "--verify", base)
    except SystemExit:
        raise SystemExit(f"Missing {base}. Run `git fetch origin` first.")
    merge_base = git("merge-base", "HEAD", base)
    files = [line for line in git("diff", "--name-only", merge_base, "HEAD").splitlines() if line]
    return merge_base, files


def required(pattern: re.Pattern[str], files: list[str]) -> bool:
    return any(pattern.search(path) for path in files)


def policy_gates() -> None:
    tools = [
        "TOOLS/dev.py", "TOOLS/prepush.py", "TOOLS/validate_playable_prototype.py",
        "TOOLS/test_playable_prototype_gate.py", "TOOLS/validate_reality_model.py",
        "TOOLS/test_reality_model_gate.py",
    ]
    run([sys.executable, "-m", "py_compile", *tools], "tooling syntax", 30)
    for path, label in (
        ("TOOLS/test_playable_prototype_gate.py", "phase-gate negative self-tests"),
        ("TOOLS/test_reality_model_gate.py", "reality/model negative self-tests"),
        ("TOOLS/validate_playable_prototype.py", "playable prototype phase gate"),
        ("TOOLS/validate_reality_model.py", "reality/model gate"),
    ):
        run([sys.executable, path], label, 60)


def core_gate(dotnet: str, proof_smoke: bool) -> None:
    run([dotnet, "restore", str(CORE)], "restore core")
    run([dotnet, "build", str(CORE), "--configuration", "Release", "--no-restore", "--nologo",
         "--verbosity", "minimal"], "build core Release")
    for project, label in ((CORE_TEST, "core tests"), (ARCH_TEST, "architecture tests")):
        run([dotnet, "test", str(project), "--configuration", "Release", "--no-build", "--no-restore",
             "--nologo", "--verbosity", "minimal"], label)
    run([dotnet, "run", "--project", str(HEADLESS), "--configuration", "Release", "--no-build",
         "--no-restore", "--", "42", "100"], "headless core smoke", 90,
        "MWS_HEADLESS_OK seed=42 hours=100 day=4 hour=4 residents=12")

    CACHE.mkdir(parents=True, exist_ok=True)
    run([dotnet, "restore", str(SETTLEMENT_BENCH), "--nologo"], "restore settlement benchmark")
    run([dotnet, "run", "--configuration", "Release", "--project", str(SETTLEMENT_BENCH), "--no-restore",
         "--", "--residents", "512", "--days", "30", "--output", str(CACHE / "settlement-scale.json")],
        "production settlement scale smoke", 300, "MWS_SETTLEMENT_SCALE_OK residents=512 days=30")

    if proof_smoke:
        run([dotnet, "restore", str(PROOF_SMOKE), "--nologo"], "restore Proof A smoke")
        run([dotnet, "run", "--configuration", "Release", "--project", str(PROOF_SMOKE), "--no-restore",
             "--", "--scale", "500"], "Proof A workload smoke")


def godot_gate(dotnet: str) -> None:
    godot = os.environ.get("GODOT_BIN") or shutil.which("godot") or shutil.which("godot4")
    if not godot:
        raise SystemExit("Godot required by scope; install Godot 4.7.1 .NET or set GODOT_BIN")
    version = subprocess.check_output([godot, "--version"], cwd=ROOT, text=True).strip()
    if not version.startswith(GODOT_PIN):
        raise SystemExit(f"Godot mismatch: local={version}, CI={GODOT_PIN}")
    run([dotnet, "restore", str(GODOT_PROJECT)], "restore Godot project")
    run([dotnet, "build", str(GODOT_PROJECT), "--configuration", "Debug", "--no-restore", "--nologo",
         "--verbosity", "minimal"], "build Godot C# project")
    run([godot, "--headless", "--path", str(ROOT / "src/Mws.Client.Godot"), "--quit-after", "120"],
        "Godot headless integration smoke", 180, "MWS_GODOT_SMOKE_OK")


def proof_measure_gate(dotnet: str, head: str) -> None:
    output = CACHE / "proof-a-measure"
    if output.exists():
        shutil.rmtree(output)
    output.mkdir(parents=True, exist_ok=True)
    env = os.environ.copy()
    env["MWS_SUBJECT_VERSION"] = head
    run([dotnet, "restore", str(PROOF_MEASURE), "--nologo"], "restore Proof A measurement harness")
    run([dotnet, "run", "--configuration", "Release", "--project", str(PROOF_MEASURE), "--no-restore",
         "--", str(output), "20000", "3"], "canonical Proof A RW-A through RW-D", 600, env=env)

    expected = {"RW-A_KERNEL_EVENTS", "RW-B_SAVE_REPLAY", "RW-C_TRACE", "RW-D_LOD_ROUNDTRIP"}
    seen: set[str] = set()
    for path in sorted(output.glob("RW-*_v1.json")):
        data = json.loads(path.read_text())
        seen.add(data["workload_id"])
        checks = (
            (data["budget_status_after"] == "MEASURED", "budget_status_after"),
            (data["repetitions"] == 3, "repetitions"),
            (data["subject_version"] == head, "subject_version"),
            (bool(data["target_hardware_class"]), "target_hardware_class"),
            (bool(data["safety_margin_rationale"]), "safety_margin_rationale"),
            (bool(data["regression_threshold"]), "regression_threshold"),
        )
        for ok, field in checks:
            if not ok:
                raise SystemExit(f"{path.name}: invalid {field}")
    if seen != expected:
        raise SystemExit(f"Proof A result set mismatch: seen={sorted(seen)} expected={sorted(expected)}")
    print("OK Proof A canonical result shape")


def main() -> int:
    parser = argparse.ArgumentParser(description="Strict local GitHub CI parity gate")
    parser.add_argument("--base", default="origin/main")
    args = parser.parse_args()

    require_clean()
    dotnet, dotnet_version = require_tools()
    merge_base, files = base_and_files(args.base)
    head = git("rev-parse", "HEAD")
    core = required(CORE_SCOPE, files)
    godot = required(GODOT_SCOPE, files)
    proof_smoke = required(PROOF_SMOKE_SCOPE, files)
    proof_measure = required(PROOF_MEASURE_SCOPE, files)

    print("MWS_PREPUSH_CONTEXT")
    print(f"  head={head}\n  base={args.base}\n  merge_base={merge_base}\n  dotnet={dotnet_version}")
    print(f"  changed_files={len(files)} core={core} godot={godot} proof_smoke={proof_smoke} proof_measure={proof_measure}")
    for path in files:
        print(f"    {path}")

    policy_gates()
    if core:
        core_gate(dotnet, proof_smoke)
    if godot:
        godot_gate(dotnet)
    if proof_measure:
        proof_measure_gate(dotnet, head)
    require_clean()
    print(f"\nMWS_PREPUSH_OK head={head} core={core} godot={godot} proof_measure={proof_measure}")
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
