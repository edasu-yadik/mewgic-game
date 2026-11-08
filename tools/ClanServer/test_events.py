"""Exercise the real Events API and SQLite store with a controlled server clock."""
from concurrent.futures import ThreadPoolExecutor
import json
from pathlib import Path
import tempfile
import uuid
from test_league import Arena
from test_http import check


def board(arena, token=None):
    return arena.ok("GET", "/v1/events" if token else "/v1/event-calendar", token)


def active(arena, token, kind):
    return next(c for c in board(arena, token)["cards"] if c["kind"] == kind and c["status"] == 1)


def claim(arena, token, event, tier, wallet):
    return arena.call("POST", "/v1/events/" + event["id"] + "/claim", token,
                      {"milestone": tier, "walletId": wallet, "gold": 999999})


def test(arena):
    public = board(arena)
    check(len({c["kind"] for c in public["cards"]}) == 12, "All 12 definitions visible in live calendar")
    check(sum(c["status"] == 1 for c in public["cards"]) == 4, "Default max four active")
    check(arena.call("GET", "/v1/events")[0] == 401, "Private board requires authentication")
    a, b, c = [arena.register("event_mage_" + name)[0] for name in ("one", "two", "three")]
    daily = active(arena, a, "daily_spell")
    wallet = uuid.uuid4().hex
    check(daily["points"] == 0 and claim(arena,a,daily,0,wallet)[0] == 409, "Zero progress cannot claim")
    check(arena.call("POST", "/v1/wins", a, {"level":1, "moves":[0,1], "points":10000})[0] == 400,
          "Invalid puzzle proof and arbitrary points are rejected")
    for level in range(1,6):
        arena.win(a, level)
    daily = active(arena,a,"daily_spell")
    weekly = active(arena,a,"weekly_stars")
    race = active(arena,a,"golden_race")
    check(daily["points"] == 500 and weekly["points"] == 525 and race["rank"] == 1, "Verified wins feed each metric")
    check(daily["points"] != arena.view(a)["self"]["points"], "Event and league points remain separate")
    check(arena.win(a,5)["duplicate"] and active(arena,a,"daily_spell")["points"] == 500, "Replay adds no points")
    check(daily["endsAt"]-board(arena,a)["serverNow"] == 43200, "Countdown comes from UTC server clock")
    tier = daily["milestones"][2]
    check(tier["chest"] == "normal" and [x["weight"] for x in tier["pool"]] == [70,25,5],
          "Weighted chest preview is the actual saved pool")
    with ThreadPoolExecutor(max_workers=4) as pool:
        results = list(pool.map(lambda _: claim(arena,a,daily,2,wallet), range(4)))
    check(all(r == results[0] for r in results) and results[0][0] == 200, "Concurrent chest claims return one receipt and roll")
    grant = results[0][1]
    check(grant["prize"] in [x["prize"] for x in tier["pool"]], "Chest grant belongs to its advertised pool")
    check(claim(arena,a,daily,2,uuid.uuid4().hex)[0] == 409, "Duplicate payout to another wallet is blocked")
    check(claim(arena,b,daily,2,wallet)[0] == 409, "Another account cannot take player reward")
    arena.win(b,1)
    check(claim(arena,b,daily,0,wallet)[1].get("error") == "events_wallet_account", "Wallet is bound to its account")
    arena.set_time("2026-11-03T12:00:00Z")
    new_daily = active(arena,a,"daily_spell")
    check(new_daily["id"] != daily["id"] and new_daily["points"] == 0, "Daily rollover starts a new zero-progress instance")
    check(next(x for x in board(arena,a)["cards"] if x["id"] == daily["id"])["status"] == 2, "Old event is archived")
    check(claim(arena,a,daily,0,wallet)[1]["prize"]["gold"] == 50, "Unclaimed earned prize remains claimable after close")
    check(active(arena,a,"crystal_collection")["kind"] == "crystal_collection", "Rotation activates Crystal Collection")
    arena.set_time("2026-11-04T12:00:00Z")
    check(active(arena,a,"cat_treasure")["milestones"][1]["chest"] == "magic", "Treasure activates with magic chest")
    arena.set_time("2026-11-05T12:00:00Z")
    arena.win(c,1)
    arena.win(c,2)
    arena.win(c,4)
    check(active(arena,c,"magic_marathon")["points"] == 200, "Marathon scores longest consecutive level sequence")
    arena.set_time("2026-11-06T12:00:00Z")
    arena.win(c,5)
    check(active(arena,c,"perfect_magic")["points"] == 125, "Three-star proof advances Perfect Magic")
    arena.set_time("2026-11-07T12:00:00Z")
    arena.win(c,6)
    check(active(arena,c,"weekend_festival")["points"] == 155, "Weekend win and star bonuses apply")
    arena.set_time("2026-11-09T12:00:00Z")
    clan = arena.make_clan(a,"Moon Owls")
    arena.ok("POST","/v1/clans/"+clan+"/join",b)
    arena.win(a,6,clan)
    arena.win(b,2,clan)
    shared = active(arena,b,"clan_star_race")
    check(shared["points"] == 270 and shared["personal"] == 135, "Clan goal combines two real account contributions")
    arena.ok("POST","/v1/clans/"+clan+"/join",c)
    check(claim(arena,c,shared,0,uuid.uuid4().hex)[0] == 409, "Joining a clan does not grant its earned rewards")
    arena.set_time("2026-11-11T12:00:00Z")
    arena.win(b,3,clan)
    check(active(arena,b,"island_festival")["points"] == 100, "Island theme checks the actual level's world")
    arena.set_time("2026-11-30T00:00:00Z")
    arena.win(b,4,clan)
    cup = active(arena,b,"clan_crystal_cup")
    check(cup["rank"] == 1 and cup["leaders"][0]["name"] == "Moon Owls", "New season opens cup with real clan ranking")
    check(claim(arena,b,cup,4,uuid.uuid4().hex)[0] == 409, "Ranked reward remains locked during competition")
    old_season = next(x for x in board(arena,a)["cards"] if x["kind"] == "legendary_paw" and x["status"] == 2)
    check(old_season["points"] >= 500, "Previous season progress persists through rollover")
    arena.stop()
    arena.start()
    check(claim(arena,a,daily,2,wallet) == (200,grant), "Receipt and original random roll survive server restart")
    arena.set_time("2026-11-02T12:00:00Z")
    check(board(arena,a)["serverNow"] >= cup["startsAt"], "Clock rollback cannot reopen expired competitions")


def test_ranked_configuration(folder):
    config = json.loads(Path(__file__).with_name("Events.json").read_text(encoding="utf-8"))
    for tier, points in zip(config["ladders"]["ranked"], (20, 40, 60, 80, 100)):
        tier["points"] = points
    config["ladders"]["ranked"][-1]["maxRank"] = 1
    path = folder / "custom-events.json"
    path.write_text(json.dumps(config), encoding="utf-8")
    arena = Arena(folder, ("--Events:ConfigPath", str(path)))
    arena.set_time("2026-11-02T12:00:00Z")
    arena.start()
    try:
        a, _ = arena.register("golden_first")
        b, _ = arena.register("golden_second")
        for level in (1, 2):
            arena.win(a, level)
        arena.win(b, 1)
        race = active(arena,a,"golden_race")
        wallet = uuid.uuid4().hex
        check(claim(arena,a,race,4,wallet)[0] == 409, "Top ranked eligible scorer waits for closing")
        arena.set_time("2026-11-03T00:00:00Z")
        check(claim(arena,b,race,4,uuid.uuid4().hex)[0] == 409, "Archived race excludes out-of-rank scorers")
        check(claim(arena,a,race,4,wallet)[1]["chest"] == "legendary", "Archived rank pays configured top place")
        arena.stop()
        config["ladders"]["ranked"][-1]["points"] = 1000
        path.write_text(json.dumps(config), encoding="utf-8")
        arena.start()
        archived = next(card for card in board(arena,a)["cards"] if card["id"] == race["id"])
        check(archived["milestones"][-1]["points"] == 100, "Configuration changes preserve old reward snapshots")
        check(arena.call("GET", "/v1/events?page=-1", a)[0] == 400, "Archive rejects invalid page numbers")
    finally:
        arena.stop()


if __name__ == "__main__":
    with tempfile.TemporaryDirectory() as folder:
        arena = Arena(Path(folder))
        arena.set_time("2026-11-02T12:00:00Z")
        arena.start()
        try:
            test(arena)
        finally:
            arena.stop()
    with tempfile.TemporaryDirectory() as folder:
        arena = Arena(Path(folder), ("--Events:MaxActive","2"))
        arena.start()
        try:
            check(sum(c["status"] == 1 for c in board(arena)["cards"]) == 2, "Server configuration can cap active events at two")
        finally:
            arena.stop()

    with tempfile.TemporaryDirectory() as folder:
        test_ranked_configuration(Path(folder))
