CREATE TABLE IF NOT EXISTS league_seasons(id INTEGER PRIMARY KEY, start BIGINT NOT NULL, "end" BIGINT NOT NULL, closed INTEGER NOT NULL DEFAULT 0);
CREATE TABLE IF NOT EXISTS league_divisions(scope INTEGER NOT NULL, entity TEXT NOT NULL, tier INTEGER NOT NULL,
  PRIMARY KEY(scope,entity));
CREATE TABLE IF NOT EXISTS league_entries(season INTEGER NOT NULL REFERENCES league_seasons(id), scope INTEGER NOT NULL,
  entity TEXT NOT NULL, name TEXT NOT NULL, tier INTEGER NOT NULL, points BIGINT NOT NULL, wins INTEGER NOT NULL,
  rank INTEGER NOT NULL DEFAULT 0, next_tier INTEGER NOT NULL DEFAULT 0, PRIMARY KEY(season,scope,entity));
CREATE INDEX IF NOT EXISTS league_rank ON league_entries(season,scope,points DESC,entity);
CREATE TABLE IF NOT EXISTS league_event_bonuses(season INTEGER NOT NULL, player TEXT NOT NULL, week TEXT NOT NULL,
  PRIMARY KEY(season,player,week));
CREATE TABLE IF NOT EXISTS league_rewards(season INTEGER NOT NULL, player TEXT NOT NULL REFERENCES players(id),
  tier INTEGER NOT NULL, rank INTEGER NOT NULL, next_tier INTEGER NOT NULL, gold INTEGER NOT NULL, cosmetic TEXT NOT NULL,
  wallet TEXT, PRIMARY KEY(season,player));
CREATE TABLE IF NOT EXISTS league_wallet_owners(wallet TEXT PRIMARY KEY, player TEXT NOT NULL REFERENCES players(id));
CREATE TABLE IF NOT EXISTS league_clan_emblems(clan TEXT PRIMARY KEY, tier INTEGER NOT NULL);
