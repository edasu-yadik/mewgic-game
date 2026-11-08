"""Run a disposable two-season arena against Unity Play Mode."""
import json
from pathlib import Path
import subprocess
import tempfile
import time

from test_league import Arena
from test_http import REPO, SERVER

FIXTURE = REPO / "Library" / "LeagueQaFixture.json"
REPORT = REPO / "docs" / "ProductionReports" / "League" / "PLAY_MODE.json"


def main():
    if not SERVER.exists():
        raise SystemExit("Önce dotnet build tools/ClanServer -c Release çalıştır.")
    if FIXTURE.exists():
        raise SystemExit("Önce açık lig QA koşusunu kapat.")
    with tempfile.TemporaryDirectory(prefix="mewgic-league-unity-") as path:
        arena = Arena(Path(path))
        created = False
        try:
            arena.start()
            a,password = arena.register("yildiz_kedi")
            b,_ = arena.register("mor_pati")
            one = arena.make_clan(a,"Kristal Arenası")
            two = arena.make_clan(b,"Ay Işığı")
            arena.win(a,1,one)
            arena.win(b,1,two)
            arena.set_time("2026-11-30T12:00:00Z")
            arena.win(b,2,two)
            with FIXTURE.open("x",encoding="utf-8") as fixture:
                created = True
                json.dump({"endpoint":arena.base,"handle":"yildiz_kedi","password":password},fixture)
            previous = REPORT.stat().st_mtime_ns if REPORT.exists() else 0
            subprocess.run(["unity","command","editor","invoke_menu","Mewgic/QA/Run League","--json"],
                           cwd=REPO,check=True,timeout=45)
            for _ in range(300):
                if REPORT.exists() and REPORT.stat().st_mtime_ns > previous:
                    try:
                        result=json.loads(REPORT.read_text(encoding="utf-8"))
                    except json.JSONDecodeError:
                        time.sleep(.5)
                        continue
                    print(f"{result['count']} Play Mode kontrolü. {REPORT}")
                    if not result["success"]:
                        raise SystemExit(result["failure"] or result["errors"])
                    return
                time.sleep(.5)
            raise TimeoutError("Lig Play Mode raporu zamanında yazılmadı.")
        finally:
            arena.stop()
            if created:
                FIXTURE.unlink(missing_ok=True)


if __name__ == "__main__":
    main()
