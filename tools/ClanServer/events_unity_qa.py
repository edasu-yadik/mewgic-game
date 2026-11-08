"""Drive the Unity Home and Gameplay scenes against disposable real event accounts."""
import json
from pathlib import Path
import subprocess
import tempfile
import time
from test_league import Arena
from test_http import REPO, SERVER

FIXTURE = REPO / "Library" / "EventsQaFixture.json"
REPORT = REPO / "docs" / "ProductionReports" / "Events" / "PLAY_MODE.json"


def main():
    if not SERVER.exists() or FIXTURE.exists():
        raise SystemExit("Build ClanServer Release and close any active Events QA.")
    with tempfile.TemporaryDirectory(prefix="mewgic-events-unity-") as folder:
        arena = Arena(Path(folder))
        created = False
        try:
            arena.set_time("2026-11-02T12:00:00Z")
            arena.start()
            token, password = arena.register("event_qa")
            for level in range(1, 5):
                arena.win(token, level)
            with FIXTURE.open("x", encoding="utf-8") as stream:
                created = True
                json.dump({"endpoint": arena.base, "handle": "event_qa", "password": password}, stream)
            previous = REPORT.stat().st_mtime_ns if REPORT.exists() else 0
            subprocess.run(["unity", "command", "editor", "invoke_menu", "Mewgic/QA/Run Magical Events", "--json"],
                           cwd=REPO, check=True, timeout=45)
            for _ in range(300):
                if REPORT.exists() and REPORT.stat().st_mtime_ns > previous:
                    try:
                        result = json.loads(REPORT.read_text(encoding="utf-8"))
                    except json.JSONDecodeError:
                        time.sleep(.5)
                        continue
                    print(f"{result['count']} Play Mode kontrolü. {REPORT}")
                    if not result["success"]:
                        raise SystemExit(result["failure"] or result["errors"])
                    return
                time.sleep(.5)
            raise TimeoutError("Events Play Mode timed out.")
        finally:
            arena.stop()
            if created:
                FIXTURE.unlink(missing_ok=True)


if __name__ == "__main__":
    main()
