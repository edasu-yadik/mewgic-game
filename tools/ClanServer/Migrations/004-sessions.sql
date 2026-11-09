CREATE INDEX IF NOT EXISTS sessions_player ON sessions(player);
CREATE TABLE IF NOT EXISTS session_refresh(
  hash TEXT PRIMARY KEY, player TEXT NOT NULL REFERENCES players(id), access_hash TEXT NOT NULL UNIQUE,
  expires BIGINT NOT NULL);
CREATE INDEX IF NOT EXISTS session_refresh_expiry ON session_refresh(expires);
CREATE INDEX IF NOT EXISTS session_refresh_player ON session_refresh(player);
CREATE INDEX IF NOT EXISTS event_players_by_account ON event_players(player,event);
CREATE INDEX IF NOT EXISTS event_claims_by_account ON event_claims(player,event);
CREATE INDEX IF NOT EXISTS league_rewards_by_account ON league_rewards(player,season);
CREATE INDEX IF NOT EXISTS league_entries_by_entity ON league_entries(scope,entity,season);
CREATE INDEX IF NOT EXISTS event_scores_by_entity ON event_scores(entity,event);
CREATE INDEX IF NOT EXISTS league_wallet_owners_by_account ON league_wallet_owners(player);
CREATE INDEX IF NOT EXISTS league_event_bonuses_by_account ON league_event_bonuses(player);
