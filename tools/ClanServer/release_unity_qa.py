"""Run a fresh profile and reopen it in a second Unity Play Mode session."""
import json
from pathlib import Path
import subprocess
import tempfile
import time
import uuid
from test_league import Arena
from test_http import REPO, SERVER

FIXTURE = REPO / "Library" / "ReleaseQaFixture.json"
OUTPUT = REPO / "docs" / "ProductionReports" / "ReleaseReadiness" / "Integrated"


def run_phase(fixture, phase):
    report = OUTPUT / f"Phase-{phase}.json"
    previous = report.stat().st_mtime_ns if report.exists() else 0
    FIXTURE.write_text(json.dumps(dict(fixture, phase=phase)), encoding="utf-8")
    subprocess.run(["unity", "command", "editor", "invoke_menu", "Mewgic/QA/Run Release Integration", "--json"],
                   cwd=REPO, check=True, timeout=45)
    for _ in range(360):
        if report.exists() and report.stat().st_mtime_ns > previous:
            try:
                data = json.loads(report.read_text(encoding="utf-8"))
            except json.JSONDecodeError:
                time.sleep(.5)
                continue
            if not data["success"]:
                raise RuntimeError(data["failure"] or data["errors"])
            print(f"PASS phase {phase}: {data['count']} checks", flush=True)
            time.sleep(1.25)
            subprocess.run(["unity", "status", "--until-ready", "--timeout", "30", "--project-path", str(REPO)],
                           cwd=REPO, check=True, timeout=35)
            return
        time.sleep(.5)
    raise TimeoutError(f"Release QA report missing: phase {phase}")


def main():
    if not SERVER.exists() or FIXTURE.exists():
        raise SystemExit("Build ClanServer Release and close any active Release QA.")
    with tempfile.TemporaryDirectory(prefix="mewgic-release-unity-") as folder:
        arena = Arena(Path(folder))
        with FIXTURE.open("x", encoding="utf-8"):
            pass
        try:
            arena.set_time("2026-11-02T12:00:00Z")
            arena.start()
            suffix = uuid.uuid4().hex
            fixture = {"endpoint":arena.base, "handle":"release_"+suffix[:8], "password":uuid.uuid4().hex,
                       "key":"Mewgic.QA.Release."+suffix}
            run_phase(fixture, 1)
            run_phase(fixture, 2)
        finally:
            arena.stop()
            FIXTURE.unlink(missing_ok=True)


if __name__ == "__main__":
    main()
