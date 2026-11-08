"""Exercise season rollover and concurrent reward transfer against the actual server."""
import concurrent.futures
import json
import os
from pathlib import Path
import socket
import subprocess
import tempfile
import time
from urllib.error import HTTPError, URLError
from urllib.request import Request, urlopen
import uuid
from test_http import SERVER, REPO, check

PROOFS = {
    1: [0,2,1,0,1,2], 2: [0,3,2,0,1,2,1,3],
    3: [0,3,1,3,2,1,2,3,0,2], 4: [1,3,2,1,2,3],
    5: [2,3,0,2,1,0,1,3,1,2], 6: [2,3,0,2,0,3,0,2],
}


class Arena:
    def __init__(self, root, options=()):
        self.root = root
        self.options = options
        with socket.socket() as listener:
            listener.bind(("127.0.0.1",0))
            self.base = f"http://127.0.0.1:{listener.getsockname()[1]}"
        self.clock = root / "utc.txt"
        self.set_time("2026-11-03T12:00:00Z")
        self.proc = None

    def set_time(self, value):
        temp = self.root / "utc.next"
        temp.write_text(value, encoding="utf-8")
        # Atomic replacement. Reopen if the worker was reading at that instant.
        for _ in range(20):
            try:
                temp.replace(self.clock)
                return
            except PermissionError:
                time.sleep(.01)
        temp.replace(self.clock)

    def start(self):
        with (self.root / "service.log").open("ab") as log:
            self.proc = subprocess.Popen(["dotnet",str(SERVER),"--urls",self.base,"--environment","Development",
                "--Database",str(self.root/"arena.db"),"--LeagueQaClockFile",str(self.clock),"--League:MovePercent","40",*self.options],
                cwd=REPO,stdout=log,stderr=subprocess.STDOUT,creationflags=getattr(subprocess,"CREATE_NO_WINDOW",0))
        for _ in range(60):
            try:
                if self.call("GET","/health")[0] == 200:
                    return
            except (URLError, TimeoutError):
                time.sleep(.15)
            if self.proc.poll() is not None:
                break
        self.stop()
        raise RuntimeError((self.root/"service.log").read_text(encoding="utf-8"))

    def stop(self):
        if self.proc is not None and self.proc.poll() is None:
            self.proc.terminate()
            self.proc.wait(timeout=10)

    def call(self, method, path, token=None, data=None):
        headers = {"Content-Type":"application/json"} if data is not None else {}
        if token:
            headers["Authorization"] = "Bearer " + token
        request = Request(self.base+path,method=method,headers=headers,data=None if data is None else json.dumps(data).encode())
        try:
            response = urlopen(request,timeout=15)
        except HTTPError as exc:
            response = exc
        with response:
            raw = response.read()
            return response.code, json.loads(raw) if raw.startswith(b"{") else {}

    def ok(self, method, path, token=None, data=None):
        status, payload = self.call(method,path,token,data)
        if status != 200:
            raise AssertionError((method,path,status,payload))
        return payload

    def register(self, handle):
        password = os.urandom(16).hex()
        token = self.ok("POST","/v1/register",data={"handle":handle,"password":password})["token"]
        return token,password

    def make_clan(self, token, name):
        return self.ok("POST","/v1/clans",token,
            {"name":name,"description":"Season arena","announcement":"","emblem":0,"open":True,"minimumWins":0})["clan"]["id"]

    def win(self, token, level, clan=""):
        return self.ok("POST","/v1/wins",token,{"level":level,"moves":PROOFS[level],"clanId":clan,"points":999999})

    def view(self, token, season=None, clans=False):
        return self.ok("GET","/v1/league?scope="+("clans" if clans else "players")+
                       (f"&season={season}" if season is not None else ""),token)


def test(arena):
    a,b,c,d,e,f = [arena.register("wizard_"+str(i))[0] for i in range(6)]
    check(arena.call("GET","/v1/league")[0] == 401, "League requires an authenticated real account")
    opening = arena.view(a)
    check(opening["self"] is None and opening["top"] == [] and opening["previous"] == [],
          "Empty leaderboard has no synthetic participants")
    check(opening["season"]["endsAt"]-opening["season"]["startsAt"] == 28*86400,
          "Season has exactly 28 server UTC days")
    one = arena.make_clan(a,"Crystal Arena")
    two = arena.make_clan(d,"Rose Arena")
    for token,cid in ((b,one),(c,one),(e,two),(f,two)):
        arena.ok("POST",f"/v1/clans/{cid}/join",token)
    for token,cid in ((a,one),(b,one),(c,one),(d,two),(e,two),(f,two)):
        arena.win(token,1,cid)
    check(arena.view(a)["self"]["points"] == 16 and arena.view(a)["self"]["rank"] == 1,
          "Tutorial three-star formula ignores arbitrary client points and awards shared tie rank")
    check(arena.call("POST","/v1/wins",a,{"level":2,"moves":[0,1],"clanId":one})[0] == 400 and
          arena.view(a)["self"]["points"] == 16,"Invalid proof leaves league score unchanged")
    arena.set_time("2026-11-07T12:00:00Z")
    with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:
        results = list(pool.map(lambda token: arena.win(token,2,one),[a,a,b,c]))
    check(sum(result["duplicate"] for result in results) == 1 and arena.view(a)["self"]["points"] == 48,
          "Concurrent season submissions apply each verified level and weekend bonus once")
    for token in (a,b,c):
        arena.win(token,3,one)
    personal = arena.view(a)
    check(personal["self"]["points"] == 78 and personal["self"]["wins"] == 3 and len(personal["nearby"]) >= 3,
          "Difficulty, stars, weekend and season milestone produce expected score and nearby ranks")
    clan = arena.view(a,clans=True)
    check(clan["self"]["points"] == 234 and clan["self"]["rank"] == 1 and clan["top"][1]["points"] == 48,
          "Clan ranking aggregates real members without moving past points")
    wallet = uuid.uuid4().hex
    check(arena.call("POST","/v1/league/1/claim",a,{"walletId":wallet})[0] == 404, "Active season cannot pay rewards")
    arena.set_time("2026-11-30T00:00:00Z")
    with concurrent.futures.ThreadPoolExecutor(max_workers=4) as pool:
        boards = list(pool.map(arena.view,[a,b,c,d]))
    check(all(v["season"]["id"] == 2 and v["top"] == [] for v in boards), "Exact boundary closes once and creates a fresh season")
    check([v["currentTier"] for v in boards] == [1,1,1,0], "Top tied group is promoted together, bronze bottom stays bronze")
    past = arena.view(a,1)
    check(past["season"]["closed"] and past["reward"]["gold"] == 100 and past["self"]["rank"] == 1 and
          past["self"]["nextTier"] == 1,"Frozen standings include payout and new division")
    check(arena.view(a,clans=True)["clanEmblem"] == 0, "Clan earns its season emblem")
    check(arena.win(a,1,one)["duplicate"] and arena.view(a)["self"] is None,"Old level replay cannot farm new season")
    other = uuid.uuid4().hex
    with concurrent.futures.ThreadPoolExecutor(max_workers=3) as pool:
        claims = list(pool.map(lambda key: arena.call("POST","/v1/league/1/claim",a,{"walletId":key}), [wallet,wallet,other]))
    successful = [(key, grant) for key,(code,grant) in zip([wallet,wallet,other],claims) if code == 200]
    check(len({key for key,_ in successful}) == 1 and 409 in [code for code,_ in claims] and
          len({g["id"] for _,g in successful}) == 1, "Concurrent reward transfer binds to one wallet and one receipt")
    bound, receipt = successful[0][0], successful[0][1]["id"]
    check(arena.ok("POST","/v1/league/1/claim",a,{"walletId":bound})["id"] == receipt,
          "Same wallet can recover delivery using identical receipt")
    status,error = arena.call("POST","/v1/league/1/claim",b,{"walletId":bound})
    check(status == 409 and error["error"] == "league_wallet_account" and not arena.view(b,1)["reward"]["claimed"],
          "A shared device wallet cannot receive a second account's reward")
    for token,levels in ((a,(4,5,6)),(b,(4,5)),(c,(4,))):
        for level in levels:
            arena.win(token,level,one)
    arena.set_time("2026-12-28T00:00:00Z")
    check([arena.view(token)["currentTier"] for token in (a,b,c)] == [2,1,0],
          "Silver top promotes, middle stays and bottom demotes")
    check(arena.view(c,2)["reward"]["gold"] == 160,"Demotion pays completed tier reward")
    arena.stop()
    arena.start()
    check(arena.view(a)["season"]["id"] == 3 and arena.view(a)["currentTier"] == 2 and
          arena.ok("POST","/v1/league/1/claim",a,{"walletId":bound})["id"] == receipt,
          "Server restart preserves division, history and reward binding")
    check(arena.view(d,1)["reward"]["id"] != receipt, "Reward identity is scoped to the authenticated account")


def test_configuration():
    options = []
    for tier in range(6):
        options.extend([f"--League:Tiers:{tier}:Gold",str(77+tier),
                        f"--League:Tiers:{tier}:MinimumPromotion",str(80+tier),
                        f"--League:Tiers:{tier}:Cosmetic","league_cat" if tier == 0 else ""])
    with tempfile.TemporaryDirectory(prefix="mewgic-league-config-") as path:
        arena = Arena(Path(path),options)
        try:
            arena.start()
            player,_ = arena.register("solo_config")
            arena.win(player,1)
            board = arena.view(player)
            check(board["self"]["points"] == 16 and board["tiers"][0]["gold"] == 77 and
                  board["tiers"][0]["minimumPromotion"] == 80,"Solo play earns points and tier thresholds are configurable")
            arena.set_time("2026-11-30T00:00:00Z")
            reward = arena.view(player,1)["reward"]
            check(reward["gold"] == 77 and reward["cosmetic"] == "league_cat","Season closure uses configured gold and exclusive cosmetic")
        finally:
            arena.stop()


if __name__ == "__main__":
    with tempfile.TemporaryDirectory(prefix="mewgic-league-") as path:
        server = Arena(Path(path))
        try:
            server.start()
            test(server)
        finally:
            server.stop()
    test_configuration()
