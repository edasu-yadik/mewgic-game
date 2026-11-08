using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Mewgic.Clans
{
    [Serializable] internal sealed class ClanRefresh { public string refreshToken; }
    [Serializable] internal sealed class ClanDelete { public string password; }
    [Serializable] internal sealed class ClanOutbox { public ClanWin[] wins; }

    internal static partial class ClanClient
    {
        static string refreshToken, outboxKey;
        static object sessionIdentity;
        static DateTimeOffset expiresAt;
        static Task refreshing;

        static void SetSession(ClanSession session)
        {
            if (string.IsNullOrEmpty(session.token) || string.IsNullOrEmpty(session.refreshToken) ||
                !Guid.TryParseExact(session.playerId,"N",out _) || session.expiresIn < 1)
                throw new ClanClientException("server");
            token = session.token;
            refreshToken = session.refreshToken;
            expiresAt = DateTimeOffset.UtcNow.AddSeconds(session.expiresIn);
        }

#if UNITY_EDITOR
        static string testOutboxKey;
        internal static void TestExpireAccess() => expiresAt = DateTimeOffset.MinValue;
        internal static void TestOutbox(string key) => testOutboxKey = outboxKey = key;
#endif

        static Task RefreshSession(object owner)
        {
            if (refreshing != null && !refreshing.IsCompleted) return refreshing;
            return refreshing = RenewSession(owner);
        }

        static async Task RenewSession(object owner)
        {
            try
            {
                var session = await SendRequest<ClanSession>("POST","/session/refresh",
                    new ClanRefresh { refreshToken=refreshToken },null);
                if (sessionIdentity != owner) throw new ClanClientException("unauthorized");
                SetSession(session);
            }
            catch (ClanClientException error) when (error.Code == "unauthorized")
            {
                if (sessionIdentity == owner) Forget();
                throw;
            }
        }

        [Serializable] sealed class ExportedAccount
        {
            public string playerId,handle,clanId;
            public ExportedWin[] wins;
            public ExportedLeague[] league;
            public ExportedEvent[] events;
            public string[] eventReceipts;
        }
        [Serializable] sealed class ExportedWin { public int level; public string day; }
        [Serializable] sealed class ExportedLeague { public int season,rank; public long points; }
        [Serializable] sealed class ExportedEvent { public string @event; public long points; }

        public static async Task<string> ExportAccount() =>
            JsonUtility.ToJson(await Send<ExportedAccount>("GET","/account/export"),true);

        public static async Task DeleteAccount(string password)
        {
            string key = outboxKey;
            await Send<ClanAction>("POST","/account/delete",new ClanDelete { password=password });
            Forget();
            if (key == null) return;
            PlayerPrefs.DeleteKey(key);
            PlayerPrefs.Save();
        }

        static void RestoreOutbox(string player)
        {
#if UNITY_EDITOR
            if (GameplayProgress.IsTestProfile) outboxKey = testOutboxKey;
            else
#endif
            {
                using var sha = SHA256.Create();
                string scope = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(BaseUrl+"|"+player))).Replace("-","");
                outboxKey = "Mewgic.Online.Outbox."+scope;
            }
            if (outboxKey == null) return;
            string json = PlayerPrefs.GetString(outboxKey,"");
            if (json.Length == 0 || json.Length > 180000) return;
            try
            {
                var record = JsonUtility.FromJson<ClanOutbox>(json);
                foreach (var win in (record?.wins ?? Array.Empty<ClanWin>()).Take(32))
                    if (win != null && win.level > 0 && win.moves is { Length: > 0 and <= 1024 } && win.moves.Length%2 == 0)
                        queue.TryAdd(win.level,win);
            }
            catch (ArgumentException) { }
        }

        static void SaveOutbox()
        {
            if (outboxKey == null) return;
            try
            {
                if (queue.Count == 0) PlayerPrefs.DeleteKey(outboxKey);
                else PlayerPrefs.SetString(outboxKey,JsonUtility.ToJson(new ClanOutbox { wins=queue.Values.ToArray() }));
                PlayerPrefs.Save();
            }
            catch (PlayerPrefsException) { /* Keep the in-memory queue and allow normal gameplay. */ }
        }
    }
}
