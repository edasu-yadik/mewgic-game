"""Run independent HTTP players against a real disposable SQLite-backed ClanServer."""
import concurrent.futures
import datetime
import json
import os
from pathlib import Path
import socket
import subprocess
import sys
import tempfile
import time
from urllib.error import HTTPError, URLError
from urllib.request import Request, urlopen

SERVER = Path(__file__).resolve().parent / "bin" / "Release" / "net10.0" / "ClanServer.dll"
REPO = Path(__file__).resolve().parents[2]


def check(condition, description):
    if not condition:
        raise AssertionError(description)
    print("PASS " + description, flush=True)


def solved(level):
    data = json.loads((REPO / "Assets" / "Resources" / "Levels" / f"Level{level:02}.json").read_text(encoding="utf-8"))
    origin = tuple(tuple(bottle["layers"]) for bottle in data["bottles"])
    visited = set()

    def search(board):
        if all(not b or len(b) == 4 and len(set(b)) == 1 for b in board):
            return []
        if board in visited:
            return None
        visited.add(board)
        for i, source in enumerate(board):
            if not source:
                continue
            color = source[-1]
            run = next((n for n, x in enumerate(reversed(source)) if x != color), len(source))
            for j, target in enumerate(board):
                if i == j or len(target) == 4 or target and target[-1] != color:
                    continue
                amount = min(run, 4 - len(target))
                copy = list(board)
                copy[i], copy[j] = source[:-amount], target + (color,) * amount
                rest = search(tuple(copy))
                if rest is not None:
                    return [i, j] + rest
        return None

    return search(origin)


def main():
    with socket.socket() as listener:
        listener.bind(("127.0.0.1", 0))
        port = listener.getsockname()[1]
    base = f"http://127.0.0.1:{port}"

    def call(method, path, data=None, token=None, status=200):
        headers = {"Content-Type": "application/json"} if data is not None else {}
        if token:
            headers["Authorization"] = "Bearer " + token
        req = Request(base + path, method=method, headers=headers,
                      data=None if data is None else json.dumps(data).encode())
        try:
            response = urlopen(req, timeout=15)
        except HTTPError as exc:
            response = exc
        with response:
            raw = response.read()
            payload = json.loads(raw) if raw.startswith(b"{") else {}
            if response.code != status:
                raise AssertionError(f"{method} {path}: expected {status}, got {response.code} {payload}")
            return payload

    def start(root, output):
        flags = getattr(subprocess, "CREATE_NO_WINDOW", 0)
        proc = subprocess.Popen(["dotnet", str(SERVER), "--urls", base, "--Database", str(root / "clans.db"),
                                 "--Clans:Capacity", "3", "--Clans:WeeklyGoal", "30"],
                                stdout=output, stderr=subprocess.STDOUT, creationflags=flags)
        try:
            for _ in range(50):
                try:
                    if call("GET", "/health").get("status") == "ok":
                        return proc
                except URLError:
                    time.sleep(.15)
                if proc.poll() is not None:
                    break
            raise RuntimeError((root / "server.log").read_text(encoding="utf-8"))
        except BaseException:
            if proc.poll() is None:
                proc.terminate()
                proc.wait(timeout=10)
            raise

    def register(handle):
        password = os.urandom(12).hex()
        token = call("POST", "/v1/register", {"handle": handle, "password": password})["token"]
        return token, password

    def draft(name, opened=True, minimum=0):
        return {"name":name,"description":"A real clan","announcement":"Gather at the hall",
                "open":opened,"emblem":1,"minimumWins":minimum}

    with tempfile.TemporaryDirectory(prefix="mewgic-clans-") as path:
        root = Path(path)
        with (root / "server.log").open("wb") as output:
            proc = start(root, output)
            try:
                leader, secret = register("leader")
                deputy, _ = register("deputy")
                member, _ = register("member")
                fourth, _ = register("fourth")
                outsider, _ = register("outsider")
                check(call("GET", "/v1/me", status=401)["error"] == "unauthorized", "Anonymous API access denied")
                check(call("POST", "/v1/login", {"handle":"leader","password":"bad password"}, status=401)["error"] == "unauthorized",
                      "Wrong password denied")
                main_clan = call("POST","/v1/clans",draft("Crystal Paws",False),leader)["clan"]
                cid = main_clan["id"]
                endpoint = "/v1/clans/" + cid
                check(main_clan["capacity"] == 3 and main_clan["members"] == 1, "Leader assigned and configured capacity returned")
                check(call("GET","/v1/clans?q=CRYSTAL",token=member)["clans"][0]["id"] == cid, "Search is case-insensitive")
                check(call("POST",endpoint+"/join",token=deputy)["state"] == "requested", "Approval clan creates request")
                pending = call("GET",endpoint,token=leader)["requests"]
                target = pending[0]["id"]
                check(call("GET",endpoint,token=outsider)["requests"] == [], "Pending requests hidden from outsiders")
                check(call("POST",endpoint+f"/requests/{target}/accept",token=outsider,status=403)["error"] == "forbidden",
                      "Outsider cannot approve request")
                call("POST",endpoint+f"/requests/{target}/accept",token=leader)
                call("POST",endpoint+f"/members/{target}/promote",token=leader)
                check(call("GET",endpoint,token=deputy)["role"] == 1, "Leader appoints deputy")
                check(call("PUT",endpoint,draft("Stolen Clan"),deputy,status=403)["error"] == "forbidden", "Deputy cannot edit clan settings")
                call("POST",endpoint+"/invite",{"handle":"member"},deputy)
                check(call("GET","/v1/me",token=member)["invitations"][0]["id"] == cid, "Real invite visible to recipient")
                call("POST",endpoint+"/invitations/accept",token=member)
                info = call("GET",endpoint,token=leader)
                mid = next(m["id"] for m in info["members"] if m["handle"] == "member")
                lid = next(m["id"] for m in info["members"] if m["handle"] == "leader")
                check(info["clan"]["members"] == 3 and info["announcement"] == "Gather at the hall", "Invite accepted and announcement visible")
                check(call("POST",endpoint+f"/members/{lid}/kick",token=deputy,status=403)["error"] == "forbidden",
                      "Deputy cannot kick leader")
                check(call("POST",endpoint+f"/members/{target}/kick",token=member,status=403)["error"] == "forbidden",
                      "Member cannot kick deputy")

                solution = solved(1)
                payload = {"level":1,"moves":solution,"clanId":cid}
                check(call("POST","/v1/wins",{"level":1,"moves":[0,1],"clanId":cid},member,status=400)["error"] == "proof",
                      "Unsolved or illegal move proof rejected")
                check(call("POST","/v1/wins",{**payload,"clanId":"forged"},member,status=409)["error"] == "membership_changed",
                      "Proof cannot credit another clan")
                with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:
                    wins = list(pool.map(lambda token: call("POST","/v1/wins",payload,token), [member,member,deputy,leader]))
                weekend = datetime.datetime.now(datetime.timezone.utc).weekday() >= 5
                first = 15 + (15 if weekend else 0)
                check(sum(win["points"] for win in wins) == first * 3 and sum(win["duplicate"] for win in wins) == 1,
                      "Concurrent valid solves credit three accounts exactly once")
                check(call("POST","/v1/wins",{"level":2,"moves":solved(2),"clanId":cid},member)["points"] == 10,
                      "Second new level on same day gives base points only")
                info = call("GET",endpoint,token=member)
                check(info["clan"]["weeklyPoints"] == first*3+10 and info["weeklyAchievement"] and info["firstWin"],
                      "Weekly pool and achievements derived from committed wins")

                call("POST",endpoint+f"/members/{mid}/kick",token=deputy)
                check(call("GET","/v1/me",token=member)["clanId"] == "", "Deputy can remove ordinary member")
                call("PUT",endpoint,draft("Crystal Paws"),leader)
                with concurrent.futures.ThreadPoolExecutor(max_workers=2) as pool:
                    def race(token):
                        try:
                            return call("POST",endpoint+"/join",token=token)["state"]
                        except AssertionError as error:
                            if "409" in str(error) and "full" in str(error):
                                return "full"
                            raise
                    arrivals = list(pool.map(race,[member,fourth]))
                check(sorted(arrivals) == ["full","joined"], "Concurrent joins cannot overfill last slot")
                call("POST",endpoint+"/leave",token=leader)
                check(call("GET",endpoint,token=deputy)["role"] == 2, "Leader departure promotes deputy atomically")
                check(call("POST","/v1/wins",{**payload,"clanId":""},leader)["duplicate"], "Replay cannot create points after leaving")
                gated = call("POST","/v1/clans",draft("Moon Guard",True,2),leader)["clan"]["id"]
                check(call("POST",f"/v1/clans/{gated}/join",token=outsider,status=409)["error"] == "requirements", "Server enforces joining prerequisites")
                call("POST",f"/v1/clans/{gated}/leave",token=leader)
                check(call("GET",f"/v1/clans/{gated}",token=leader,status=404)["error"] == "not_found", "Last leader leaving closes the empty clan")
                replacement = call("POST","/v1/login",{"handle":"leader","password":secret})["token"]
                check(replacement != leader, "New login creates a distinct opaque session")
                call("POST","/v1/logout",token=leader)
                check(call("GET","/v1/me",token=leader,status=401)["error"] == "unauthorized", "Logout revokes bearer session")

                proc.terminate()
                proc.wait(timeout=10)
                try:
                    call("GET","/health")
                    raise AssertionError("Socket still reachable after shutdown")
                except (URLError, ConnectionError):
                    check(True, "Real network interruption observed")
                proc = start(root, output)
                restored = call("GET",endpoint,token=deputy)
                check(restored["role"] == 2 and restored["clan"]["points"] == first*3+10,
                      "Membership, succession and score persist across process restart")
                check(call("POST","/v1/wins",payload,deputy)["duplicate"], "Replay still rejected after server restart")
            finally:
                if proc.poll() is None:
                    proc.terminate()
                    proc.wait(timeout=10)


if __name__ == "__main__":
    main()
