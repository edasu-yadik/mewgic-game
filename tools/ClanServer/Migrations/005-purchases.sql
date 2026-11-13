CREATE TABLE IF NOT EXISTS store_purchases(
  platform TEXT NOT NULL CHECK(platform IN ('apple','google')),
  environment TEXT NOT NULL CHECK(environment IN ('sandbox','production')),
  transaction_id TEXT NOT NULL,
  product_id TEXT NOT NULL,
  player TEXT REFERENCES players(id) ON DELETE SET NULL,
  quantity INTEGER NOT NULL CHECK(quantity BETWEEN 1 AND 100),
  purchase_state TEXT NOT NULL CHECK(purchase_state='verified'),
  reward_state TEXT NOT NULL CHECK(reward_state='delivered'),
  confirmation_state TEXT NOT NULL CHECK(confirmation_state IN ('waiting','retry','confirmed')),
  created_utc BIGINT NOT NULL,
  updated_utc BIGINT NOT NULL,
  PRIMARY KEY(platform,environment,transaction_id)
);
CREATE INDEX IF NOT EXISTS store_purchases_player ON store_purchases(player);

-- The immutable account grant is committed together with the deduplication record.
-- Account deletion removes the grant but leaves a minimal replay barrier.
CREATE TABLE IF NOT EXISTS store_reward_grants(
  platform TEXT NOT NULL,
  environment TEXT NOT NULL,
  transaction_id TEXT NOT NULL,
  player TEXT NOT NULL REFERENCES players(id) ON DELETE CASCADE,
  gold BIGINT NOT NULL CHECK(gold>=0),
  extra_bottle INTEGER NOT NULL CHECK(extra_bottle>=0),
  bottle_hammer INTEGER NOT NULL CHECK(bottle_hammer>=0),
  shuffle INTEGER NOT NULL CHECK(shuffle>=0),
  color_swap INTEGER NOT NULL CHECK(color_swap>=0),
  PRIMARY KEY(platform,environment,transaction_id),
  FOREIGN KEY(platform,environment,transaction_id) REFERENCES store_purchases(platform,environment,transaction_id)
);
CREATE INDEX IF NOT EXISTS store_reward_grants_player ON store_reward_grants(player,environment);
