"""Run real PostgreSQL integration using a disposable local/CI database."""
from concurrent.futures import ThreadPoolExecutor
from contextlib import closing, contextmanager
import hashlib
import os
import shutil
import sqlite3
from pathlib import Path
import subprocess
import sys
import tempfile
import uuid

from test_http import REPO, SERVER, check
from test_league import Arena
import test_events
import test_league

CONNECTION = os.environ.get("MEWGIC_TEST_POSTGRES")
if not CONNECTION:
    raise SystemExit("Set MEWGIC_TEST_POSTGRES for a disposable PostgreSQL database.")


@contextmanager
def database(connection=None):
    keys = {"Postgres__ConnectionString":connection, "Postgres__Schema":"qa_"+uuid.uuid4().hex[:16],
            "ASPNETCORE_ENVIRONMENT":"Development"}
    previous = {key:os.environ.get(key) for key in keys}
    for key,value in keys.items():
        if value is None:
            os.environ.pop(key,None)
        else:
            os.environ[key]=value
    try:
        yield
    finally:
        for key,value in previous.items():
            if value is None:
                os.environ.pop(key,None)
            else:
                os.environ[key]=value


@contextmanager
def server(root, options=()):
    root.mkdir(parents=True,exist_ok=True)
    arena = Arena(root,options)
    arena.set_time("2026-11-02T12:00:00Z")
    try:
        arena.start()
        yield arena
    except Exception:
        print((root/"service.log").read_text(encoding="utf-8",errors="replace")[-6000:],flush=True)
        raise
    finally:
        arena.stop()


def migration(root):
    with database(), server(root/"sqlite") as legacy:
        one,password = legacy.register("migration_one")
        two,_ = legacy.register("migration_two")
        clan = legacy.make_clan(one,"Transfer_Moon")
        legacy.ok("POST",f"/v1/clans/{clan}/join",two)
        legacy.win(one,1,clan)
        legacy.win(two,1,clan)
        event = test_events.active(legacy,one,"daily_spell")
        wallet = uuid.uuid4().hex
        status,grant = test_events.claim(legacy,one,event,0,wallet)
        check(status == 200,"SQLite fixture contains committed reward")
        sqlite = legacy.root/"arena.db"
        backup = root/"backup.db"
        subprocess.run(["dotnet",str(SERVER),"backup",str(sqlite),str(backup)],check=True)
    malformed = root/"malformed.db"
    shutil.copyfile(backup,malformed)
    with closing(sqlite3.connect(malformed)) as db:
        db.execute("UPDATE clans SET open='not an integer'")
        db.commit()
    with database(CONNECTION):
        failed = subprocess.run(["dotnet",str(SERVER),"import-sqlite",str(malformed)],capture_output=True)
        repaired = subprocess.run(["dotnet",str(SERVER),"import-sqlite",str(backup)],capture_output=True,text=True)
        check(failed.returncode != 0 and repaired.returncode == 0 and "players: 2" in repaired.stdout,
              "Mid-import failure rolls back every row and allows a clean retry")
    original = hashlib.sha256(backup.read_bytes()).digest()
    with database(CONNECTION):
        result = subprocess.run(["dotnet",str(SERVER),"import-sqlite",str(backup)],check=True,capture_output=True,text=True)
        check("players: 2" in result.stdout and "wins: 2" in result.stdout and "event_claims: 1" in result.stdout and
              hashlib.sha256(backup.read_bytes()).digest() == original,
              "Import checks row counts and leaves SQLite source byte-for-byte intact")
        repeat = subprocess.run(["dotnet",str(SERVER),"import-sqlite",str(backup)],capture_output=True,text=True)
        check(repeat.returncode != 0 and "empty PostgreSQL destination" in repeat.stderr,"Import refuses a populated destination")
        with server(root/"pg-a",("--Render:EdgeTls","true")) as api, server(root/"pg-b",("--Render:EdgeTls","true")) as second:
            check(api.ok("GET","/ready")["status"] == "ready","PostgreSQL readiness is checked separately")
            a = api.ok("GET","/v1/account/progress",one)
            login = second.ok("POST","/v1/login",data={"handle":"migration_one","password":password})
            check(a["levels"] == [1] and a["playerId"] == login["playerId"],"Password, identity, session and verified wins survived import")
            detail = api.ok("GET",f"/v1/clans/{clan}",two)
            check(len(detail["members"]) == 2 and api.view(two,clans=True)["self"]["points"] == 32,
                  "Clan roster and league points survived import")
            check(len(api.ok("GET","/v1/clans?q=_",one)["clans"]) == 1 and
                  api.ok("GET","/v1/clans?q=%25",one)["clans"] == [],"Search metacharacters are literal in PostgreSQL")
            check(test_events.claim(api,one,event,0,wallet) == (200,grant),
                  "SQLite reward receipt and original draw survived PostgreSQL import")
            with ThreadPoolExecutor(max_workers=2) as workers:
                wins = [job.result() for job in [workers.submit(instance.win,one,2,clan) for instance in (api,second)]]
            check(sum(win["duplicate"] for win in wins) == 1 and api.view(one)["self"]["wins"] == 2,
                  "Overlapping API instances commit one win and score only once")
            with ThreadPoolExecutor(max_workers=2) as workers:
                grants = [job.result() for job in [workers.submit(test_events.claim,instance,one,event,0,wallet) for instance in (api,second)]]
            check(grants == [(200,grant),(200,grant)],"Overlapping API instances return the same persisted receipt")
            api.stop()
            api.start()
            check(api.ok("GET","/v1/account/progress",one)["levels"] == [1,2] and test_events.claim(api,one,event,0,wallet) == (200,grant),
                  "PostgreSQL keeps writes and reward receipts after API restart")


def run():
    with tempfile.TemporaryDirectory() as folder:
        root = Path(folder)
        migration(root)
        with database(CONNECTION):
            subprocess.run([sys.executable,str(REPO/"tools/ClanServer/test_http.py")],check=True)
        with database(CONNECTION), server(root/"league") as arena:
            test_league.test(arena)
        with database(CONNECTION), server(root/"events") as arena:
            test_events.test(arena)
        with database(CONNECTION):
            (root/"ranked").mkdir()
            test_events.test_ranked_configuration(root/"ranked")
        with database("Host=127.0.0.1;Port=1;Database=postgres;Username=qa;SSL Mode=Disable"):
            with server(root/"unavailable",("--Render:EdgeTls","true")) as arena:
                check(arena.ok("GET","/health")["status"] == "ok","API process stays up when database is unavailable")
                check(arena.call("GET","/ready") == (503,{"error":"retry"}),"Database outage returns retryable readiness response")
                check(arena.call("GET","/v1/event-calendar") == (503,{"error":"retry"}),"Database outage does not produce fake calendar state")


if __name__ == "__main__":
    run()
