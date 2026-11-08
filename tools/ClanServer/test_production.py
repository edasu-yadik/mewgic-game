"""Use real HTTP clients, durable SQLite and online backup against the production store."""
from concurrent.futures import ThreadPoolExecutor
from contextlib import closing
import sqlite3
import subprocess
import tempfile
from pathlib import Path
import uuid
from urllib.error import HTTPError
from urllib.request import Request, urlopen

from test_http import SERVER, check
from test_league import Arena


def login(arena, handle, password):
    return arena.ok("POST","/v1/login",data={"handle":handle,"password":password})


def test(folder):
    arena = Arena(folder)
    arena.set_time("2026-11-02T12:00:00Z")
    arena.start()
    try:
        forwarded = {"X-Forwarded-For":"203.0.113.9","X-Forwarded-Proto":"http"}
        try:
            urlopen(Request(arena.base+"/health",headers=forwarded),timeout=5).close()
            raise AssertionError("Cleartext forwarded client was accepted")
        except HTTPError as error:
            check(error.code == 426, "Cleartext external requests are rejected behind loopback proxy")
        forwarded["X-Forwarded-Proto"] = "https"
        with urlopen(Request(arena.base+"/account/delete",headers=forwarded),timeout=5) as page:
            check(page.status == 200 and "frame-ancestors 'none'" in page.headers["Content-Security-Policy"] and
                  b'"/account/delete.js"' in page.read(), "Web account deletion route applies a restrictive CSP")
        with urlopen(arena.base+"/account/delete.js",timeout=5) as script:
            check(script.status == 200 and script.headers.get_content_type() == "application/javascript",
                  "Web account deletion script is shipped with API")
        leader, password = arena.register("production_leader")
        member, member_password = arena.register("production_member")
        clan = arena.make_clan(leader,"Production Moon")
        arena.ok("POST",f"/v1/clans/{clan}/join",member)
        device_two = login(arena,"production_leader",password)
        check(device_two["token"] != leader and device_two["expiresIn"] == 900,
              "Independent device session uses short-lived access")
        check(arena.call("GET","/v1/account/export")[0] == 401, "Account export requires authentication")
        check(arena.call("GET","/v1/me",device_two["refreshToken"])[0] == 401, "Refresh token cannot access application APIs")
        with ThreadPoolExecutor(max_workers=2) as workers:
            tasks = [workers.submit(arena.win,user,1,clan) for user in (leader,member)]
            for task in tasks:
                task.result()
        for user in (device_two["token"],member):
            detail = arena.ok("GET",f"/v1/clans/{clan}",user)
            check(len(detail["members"]) == 2 and detail["clan"]["points"] >= 20,
                  "Independent device reads the same clan roster and committed contribution")
            check(arena.view(user,clans=True)["self"]["points"] == 32, "Independent device reads the same clan ranking")
        progress = arena.ok("GET","/v1/account/progress",device_two["token"])
        check(progress["playerId"] == device_two["playerId"] and progress["levels"] == [1] and progress["next"] == 0,
              "Second device sees the same authenticated verified progress")
        check(arena.ok("GET","/v1/account/progress?after=1",leader)["levels"] == [], "Progress cursor excludes earlier levels")
        check(arena.call("GET","/v1/account/progress?after=-1",leader)[0] == 400, "Invalid progress cursor rejected")

        board = arena.ok("GET","/v1/events",leader)
        event = next(c for c in board["cards"] if c["kind"] == "daily_spell" and c["status"] == 1)
        route = f"/v1/events/{event['id']}/claim"
        wallet = uuid.uuid4().hex
        data = {"milestone":0,"walletId":wallet}
        first = arena.ok("POST",route,leader,data)
        check(arena.ok("POST",route,device_two["token"],data) == first, "Two device sessions return the same reward receipt")
        check(arena.call("POST",route,device_two["token"],dict(data,walletId=uuid.uuid4().hex))[0] == 409,
              "Another local wallet cannot duplicate payout")
        mine = arena.ok("GET","/v1/account/export",device_two["token"])
        check(mine["handle"] == "production_leader" and mine["eventReceipts"] == [first["id"]] and
              "password" not in mine and "token" not in mine,"Export contains only the authenticated player's records")
        refresh_data = {"refreshToken":device_two["refreshToken"]}
        with ThreadPoolExecutor(max_workers=3) as workers:
            rotations = list(workers.map(lambda _:arena.call("POST","/v1/session/refresh",data=refresh_data),range(3)))
        check(sorted(status for status,_ in rotations) == [200,401,401], "Concurrent refresh rotates once and rejects reuse")
        renewed = next(body for status,body in rotations if status==200)
        check(renewed["playerId"] == device_two["playerId"] and arena.call("GET","/v1/me",device_two["token"])[0] == 401,
              "Rotation keeps account identity and revokes old access")
        arena.ok("POST","/v1/session/revoke",data={"refreshToken":renewed["refreshToken"]})
        check(arena.call("GET","/v1/me",renewed["token"])[0] == 401 and
              arena.call("POST","/v1/session/refresh",data={"refreshToken":renewed["refreshToken"]})[0] == 401,
              "Logout revokes access and renewal together")

        database, backup = folder/"arena.db", folder/"backup.db"
        subprocess.run(["dotnet",str(SERVER),"backup",str(database),str(backup)],check=True)
        with closing(sqlite3.connect(backup)) as restore:
            check(restore.execute("PRAGMA integrity_check").fetchone()[0] == "ok" and
                  not restore.execute("PRAGMA foreign_key_check").fetchall(), "Online backup is consistent with WAL writes")
            check(restore.execute("PRAGMA user_version").fetchone()[0] == 4 and
                  restore.execute("SELECT receipt FROM event_claims").fetchone()[0] == first["id"],
                  "Backup preserves migrated schema and idempotency receipts")
        overwrite = subprocess.run(["dotnet",str(SERVER),"backup",str(database),str(backup)],capture_output=True)
        check(overwrite.returncode != 0, "Backup refuses to overwrite an existing recovery copy")

        check(arena.call("POST","/v1/account/delete",leader,{"password":"wrong-password"})[0] == 403,
              "Account deletion requires password confirmation")
        arena.ok("POST","/v1/account/delete",leader,{"password":password})
        check(arena.call("GET","/v1/me",leader)[0] == 401, "Deletion revokes access")
        detail = arena.ok("GET",f"/v1/clans/{clan}",member)
        check(len(detail["members"]) == 1 and detail["members"][0]["role"] == 2, "Deletion transfers clan leadership atomically")
        with closing(sqlite3.connect(database)) as db:
            check(db.execute("SELECT COUNT(*) FROM players WHERE id=?",(device_two["playerId"],)).fetchone()[0] == 0 and
                  db.execute("SELECT COUNT(*) FROM event_claims").fetchone()[0] == 0 and
                  not db.execute("PRAGMA foreign_key_check").fetchall(), "Personal rows and claims are erased without orphaned references")
        arena.stop()
        with closing(sqlite3.connect(database)) as db:
            db.execute("DROP TABLE session_refresh")
            db.execute("PRAGMA user_version=0")
            db.commit()
        arena.start()
        check(arena.ok("GET","/v1/account/progress",login(arena,"production_member",member_password)["token"])["levels"] == [1],
              "An existing unversioned database migrates without losing account or win")
        arena.stop()
        restore_dir = folder/"restored"
        restore_dir.mkdir()
        backup.replace(restore_dir/"arena.db")
        restored = Arena(restore_dir)
        restored.start()
        try:
            check(login(restored,"production_leader",password)["playerId"] == device_two["playerId"],
                  "Restored backup starts a real API and retains player identity")
        finally:
            restored.stop()
    finally:
        arena.stop()


if __name__ == "__main__":
    with tempfile.TemporaryDirectory() as folder:
        test(Path(folder))
