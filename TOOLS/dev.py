#!/usr/bin/env python3
"""Fast solo-developer entry point for world-simulate.

Daily loop:
  python TOOLS/dev.py fast
Before push:
  python TOOLS/dev.py check
Explicit/expensive:
  python TOOLS/dev.py godot | full | bench
"""
from __future__ import annotations

import argparse
import os
import shutil
import subprocess
import sys
import time
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
CORE_FILTER = ROOT / "WorldSimulate.Core.slnf"
CORE_TEST = ROOT / "tests/Mws.Core.Tests/Mws.Core.Tests.csproj"
ARCH_TEST = ROOT / "tests/Mws.Architecture.Tests/Mws.Architecture.Tests.csproj"
HEADLESS = ROOT / "src/Mws.Headless/Mws.Headless.csproj"
GODOT_PROJECT = ROOT / "src/Mws.Client.Godot/Mws.Client.Godot.csproj"
BENCHMARK = ROOT / "benchmarks/Mws.Benchmarks/Mws.Benchmarks.csproj"
CACHE_DIR = ROOT / ".cache/dev"
CORE_STAMP = CACHE_DIR / "core-restore.stamp"
GODOT_STAMP = CACHE_DIR / "godot-restore.stamp"
PLAYABLE_GATE = ROOT / "TOOLS/validate_playable_prototype.py"
REALITY_GATE = ROOT / "TOOLS/validate_reality_model.py"


def run(cmd: list[str], *, timeout: int, label: str) -> None:
    started = time.monotonic()
    print(f"\n== {label} ==")
    print("$", " ".join(cmd))
    try:
        completed = subprocess.run(cmd, cwd=ROOT, env=os.environ.copy(), timeout=timeout)
    except subprocess.TimeoutExpired as exc:
        raise SystemExit(f"TIMEOUT after {timeout}s: {label}") from exc
    elapsed = time.monotonic() - started
    if completed.returncode != 0:
        raise SystemExit(completed.returncode)
    print(f"OK {label} ({elapsed:.2f}s)")


def dotnet() -> str:
    executable = shutil.which("dotnet")
    if executable is None:
        raise SystemExit("dotnet not found. Install the SDK pinned by global.json.")
    return executable


def restore_inputs() -> list[Path]:
    inputs = [ROOT / "global.json", ROOT / "Directory.Build.props", CORE_FILTER]
    inputs.extend(ROOT.glob("**/*.csproj"))
    return [path for path in inputs if path.exists()]


def restore_needed(stamp: Path) -> bool:
    if not stamp.exists():
        return True
    stamp_time = stamp.stat().st_mtime_ns
    return any(path.stat().st_mtime_ns > stamp_time for path in restore_inputs())


def restore_core() -> None:
    if not restore_needed(CORE_STAMP):
        return
    CACHE_DIR.mkdir(parents=True, exist_ok=True)
    run([dotnet(), "restore", str(CORE_FILTER), "--nologo"], timeout=180, label="restore core")
    CORE_STAMP.touch()


def restore_godot() -> None:
    if not restore_needed(GODOT_STAMP):
        return
    CACHE_DIR.mkdir(parents=True, exist_ok=True)
    run([dotnet(), "restore", str(GODOT_PROJECT), "--nologo"], timeout=180, label="restore godot")
    GODOT_STAMP.touch()


def find_godot() -> str | None:
    explicit = os.environ.get("GODOT_BIN")
    if explicit and Path(explicit).is_file():
        return explicit
    for name in ("godot", "godot4"):
        executable = shutil.which(name)
        if executable:
            return executable
    return None


def doctor() -> None:
    print(f"python={sys.version.split()[0]}")
    executable = dotnet()
    version = subprocess.check_output([executable, "--version"], cwd=ROOT, text=True).strip()
    print(f"dotnet={version}")
    print(f"godot={find_godot() or 'optional/not-found'}")
    if shutil.which("git") and (ROOT / ".git").exists():
        branch = subprocess.check_output(["git", "branch", "--show-current"], cwd=ROOT, text=True).strip()
        print(f"branch={branch or 'detached'}")
    print("mode=FAST_SOLO_DEV")


def validate_playable_program() -> None:
    run(
        [sys.executable, str(PLAYABLE_GATE)],
        timeout=5,
        label="playable prototype phase gate",
    )


def validate_reality_model() -> None:
    run(
        [sys.executable, str(REALITY_GATE)],
        timeout=5,
        label="reality/model gate",
    )


def validate_policy_gates() -> None:
    validate_playable_program()
    validate_reality_model()


def fast() -> None:
    validate_policy_gates()
    restore_core()
    run(
        [dotnet(), "test", str(CORE_TEST), "-c", "Debug", "--no-restore", "--nologo", "--verbosity", "minimal"],
        timeout=90,
        label="fast core tests",
    )


def check(configuration: str = "Debug") -> None:
    validate_policy_gates()
    restore_core()
    run(
        [dotnet(), "build", str(CORE_FILTER), "-c", configuration, "--no-restore", "--nologo", "--verbosity", "minimal"],
        timeout=150,
        label=f"core build {configuration}",
    )
    for project, label in ((CORE_TEST, "core tests"), (ARCH_TEST, "architecture tests")):
        run(
            [dotnet(), "test", str(project), "-c", configuration, "--no-build", "--no-restore", "--nologo", "--verbosity", "minimal"],
            timeout=90,
            label=label,
        )
    run(
        [dotnet(), "run", "--project", str(HEADLESS), "-c", configuration, "--no-build", "--no-restore", "--", "42", "100"],
        timeout=30,
        label="headless smoke",
    )


def run_headless(extra: list[str]) -> None:
    restore_core()
    args = extra or ["42", "100"]
    run(
        [dotnet(), "run", "--project", str(HEADLESS), "-c", "Debug", "--no-restore", "--", *args],
        timeout=60,
        label="headless run",
    )


def godot_smoke() -> None:
    restore_godot()
    run(
        [dotnet(), "build", str(GODOT_PROJECT), "-c", "Debug", "--no-restore", "--nologo", "--verbosity", "minimal"],
        timeout=150,
        label="godot csharp build",
    )
    executable = find_godot()
    if executable is None:
        print("Godot executable not found; C# adapter build passed. Set GODOT_BIN or install godot/godot4 for headless smoke.")
        return
    run(
        [executable, "--headless", "--path", str(ROOT / "src/Mws.Client.Godot"), "--quit-after", "120"],
        timeout=150,
        label="godot headless smoke",
    )


def full() -> None:
    check("Release")
    godot_smoke()


def bench(steps: str) -> None:
    run(
        [dotnet(), "run", "-c", "Release", "--project", str(BENCHMARK), "--", steps],
        timeout=480,
        label="explicit Proof A benchmark",
    )


def main() -> int:
    parser = argparse.ArgumentParser(description="Fast solo-dev commands")
    sub = parser.add_subparsers(dest="command", required=True)
    sub.add_parser("doctor")
    sub.add_parser("fast")
    sub.add_parser("check")
    run_parser = sub.add_parser("run")
    run_parser.add_argument("args", nargs=argparse.REMAINDER)
    sub.add_parser("godot")
    sub.add_parser("full")
    bench_parser = sub.add_parser("bench")
    bench_parser.add_argument("--steps", default="1000000")
    args = parser.parse_args()

    if args.command == "doctor":
        doctor()
    elif args.command == "fast":
        fast()
    elif args.command == "check":
        check()
    elif args.command == "run":
        run_headless(args.args)
    elif args.command == "godot":
        godot_smoke()
    elif args.command == "full":
        full()
    elif args.command == "bench":
        bench(args.steps)
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
