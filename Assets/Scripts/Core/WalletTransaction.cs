using System;
using System.Collections.Generic;

namespace Mewgic.Core
{
    [Serializable]
    public struct WalletTransaction
    {
        public const int RecentLimit = 32;
        public string id, source;
        public long goldDelta, gemsDelta;

        internal static void Record(ProgressData next, PlayerPresentationData previous, string source)
        {
            if (next.presentation.coins < 0 || next.presentation.gems < 0)
                throw new ArgumentException("Geçersiz bakiye.");

            long gold = next.presentation.coins - previous.coins;
            long gems = next.presentation.gems - previous.gems;
            if (gold == 0 && gems == 0) return;
            if (string.IsNullOrWhiteSpace(source))
                throw new InvalidOperationException("Bakiye işleminin kaynağı eksik.");

            var transactions = next.walletTransactions;
            if (transactions.Count == RecentLimit) transactions.RemoveAt(0);
            transactions.Add(new WalletTransaction
            {
                id = Guid.NewGuid().ToString("N"),
                source = source,
                goldDelta = gold,
                gemsDelta = gems
            });
        }

        internal static void Validate(List<WalletTransaction> transactions)
        {
            if (transactions == null) return; // Existing profiles predate the wallet history.
            if (transactions.Count > RecentLimit) throw new ArgumentException("İşlem geçmişi sınırı aşıldı.");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var transaction in transactions)
                if (!Guid.TryParseExact(transaction.id, "N", out _) || !ids.Add(transaction.id) ||
                    string.IsNullOrWhiteSpace(transaction.source) || transaction.source.Length > 128 ||
                    transaction.goldDelta == 0 && transaction.gemsDelta == 0)
                    throw new ArgumentException("Geçersiz bakiye işlemi.");
        }
    }
}
