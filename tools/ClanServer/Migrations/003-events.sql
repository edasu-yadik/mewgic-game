CREATE TABLE IF NOT EXISTS event_clock(id INTEGER PRIMARY KEY CHECK(id=1), utc BIGINT NOT NULL);
CREATE TABLE IF NOT EXISTS event_windows(id TEXT PRIMARY KEY, kind TEXT NOT NULL, lane INTEGER NOT NULL,
  start BIGINT NOT NULL, "end" BIGINT NOT NULL, config TEXT NOT NULL);
CREATE INDEX IF NOT EXISTS event_calendar ON event_windows("end",start);
CREATE TABLE IF NOT EXISTS event_players(event TEXT NOT NULL REFERENCES event_windows(id), player TEXT NOT NULL REFERENCES players(id),
  clan TEXT NOT NULL, points BIGINT NOT NULL, last_level INTEGER NOT NULL, streak INTEGER NOT NULL,
  PRIMARY KEY(event,player));
CREATE TABLE IF NOT EXISTS event_scores(event TEXT NOT NULL REFERENCES event_windows(id), entity TEXT NOT NULL,
  name TEXT NOT NULL, points BIGINT NOT NULL, PRIMARY KEY(event,entity));
CREATE INDEX IF NOT EXISTS event_rank ON event_scores(event,points DESC,entity);
CREATE TABLE IF NOT EXISTS event_claims(event TEXT NOT NULL REFERENCES event_windows(id), player TEXT NOT NULL REFERENCES players(id),
  tier INTEGER NOT NULL, wallet TEXT NOT NULL, receipt TEXT NOT NULL UNIQUE, "grant" TEXT NOT NULL, PRIMARY KEY(event,player,tier));
