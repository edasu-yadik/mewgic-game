using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace Mewgic.Clans
{
    [Serializable] internal sealed class ClanConnection { public string baseUrl; }
    [Serializable] internal sealed class ClanSession { public string token; }
    [Serializable] internal sealed class ClanFailure { public string error; }
    [Serializable] internal sealed class ClanCredentials { public string handle, password; }
    [Serializable] internal sealed class ClanInvite { public string handle; }
    [Serializable] internal sealed class ClanAction { public string state; }
    [Serializable] internal sealed class ClanList { public ClanCard[] clans; }
    [Serializable] internal sealed class ClanAccount
    {
        public string handle, clanId;
        public int wins, weeklyGoal, winPoints, dailyPoints, weekendPoints;
        public ClanCard[] invitations;
    }
    [Serializable] internal sealed class ClanCard
    {
        public string id, name, description;
        public bool open;
        public int emblem, minimumWins, members, capacity, weeklyGoal;
        public long points, level, weeklyPoints;
        public int leagueEmblem = -1;
    }
    [Serializable] internal sealed class ClanPlayer { public string id, handle; public int role; public long points; }
    [Serializable] internal sealed class ClanDetail
    {
        public ClanCard clan;
        public string announcement, week;
        public int role;
        public bool requested, invited, firstWin, hundredPoints, weeklyAchievement, weekendEvent;
        public ClanPlayer[] members, requests;
    }
    [Serializable] internal sealed class ClanDraft
    {
        public string name, description, announcement;
        public bool open;
        public int emblem, minimumWins;
    }
    [Serializable] internal sealed class ClanWin { public int level; public int[] moves; public string clanId; }

    internal sealed class ClanClientException : Exception
    {
        public string Code { get; }
        public ClanClientException(string code) : base(code) => Code = code;
    }

    internal static partial class ClanClient
    {
        static string token, baseUrl, clanId;
        static readonly Dictionary<int, ClanWin> queue = new();
        static Task flushing;
        public static bool Configured => !string.IsNullOrEmpty(BaseUrl);
        public static bool SignedIn => !string.IsNullOrEmpty(token);
        public static int Pending => queue.Count;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            Forget();
            baseUrl = null;
            flushing = null;
        }

#if UNITY_EDITOR
        internal static void TestEndpoint(string localUrl)
        {
            if (!Uri.TryCreate(localUrl, UriKind.Absolute, out var uri) || !uri.IsLoopback)
                throw new ArgumentException("QA requires a loopback server.");
            baseUrl = uri.AbsoluteUri.TrimEnd('/');
        }
#endif

        static string BaseUrl
        {
            get
            {
                if (baseUrl != null) return baseUrl;
                var asset = Resources.Load<TextAsset>("Clans/Connection");
                string value = asset == null ? "" : JsonUtility.FromJson<ClanConnection>(asset.text)?.baseUrl;
#if UNITY_EDITOR
                if (string.IsNullOrWhiteSpace(value)) value = "http://127.0.0.1:18740";
#endif
                if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
                    !string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment))
                    return baseUrl = "";
                bool allowed = uri.Scheme == Uri.UriSchemeHttps;
#if UNITY_EDITOR
                allowed |= uri.IsLoopback && uri.Scheme == Uri.UriSchemeHttp;
#endif
                return baseUrl = allowed ? uri.AbsoluteUri.TrimEnd('/') : "";
            }
        }

        public static async Task SignIn(bool register, string handle, string password)
        {
            var session = await Send<ClanSession>("POST", register ? "/register" : "/login",
                new ClanCredentials { handle = handle, password = password }, false);
            if (string.IsNullOrEmpty(session.token)) throw new ClanClientException("server");
            Forget();
            token = session.token;
        }

        public static async Task SignOut()
        {
            try { await Send<ClanAction>("POST", "/logout"); }
            finally { Forget(); }
        }

        static void Forget()
        {
            token = clanId = null;
            queue.Clear();
        }

        public static async Task<ClanAccount> Me()
        {
            var account = await Send<ClanAccount>("GET", "/me");
            clanId = account.clanId;
            return account;
        }
        public static Task<ClanList> Search(string query) => Send<ClanList>("GET", "/clans?q=" + Uri.EscapeDataString(query ?? ""));
        public static Task<ClanDetail> View(string id) => Send<ClanDetail>("GET", "/clans/" + id);
        public static Task<ClanDetail> Save(string id, ClanDraft draft) => string.IsNullOrEmpty(id)
            ? Send<ClanDetail>("POST", "/clans", draft) : Send<ClanDetail>("PUT", "/clans/" + id, draft);
        public static Task<ClanAction> Act(string id, string action, object data = null) =>
            Send<ClanAction>("POST", "/clans/" + id + "/" + action, data);

        public static void Submit(int level, int[] moves)
        {
            if (!SignedIn || queue.Count >= 32) return;
            queue.TryAdd(level, new ClanWin { level = level, moves = moves, clanId = clanId });
            _ = Flush();
        }

        public static Task Flush()
        {
            if (flushing != null && !flushing.IsCompleted) return flushing;
            return flushing = FlushQueue();
        }

        static async Task FlushQueue()
        {
            while (SignedIn && queue.Count > 0)
            {
                var pending = new List<ClanWin>(queue.Values)[0];
                string owner = token;
                try { await Send<ClanAction>("POST", "/wins", pending); }
                catch (ClanClientException error)
                {
                    if (error.Code is not ("proof" or "level" or "membership_changed")) return;
                }
                catch (Exception) { return; }
                if (token != owner) return;
                queue.Remove(pending.level);
            }
        }

        static async Task<T> Send<T>(string method, string path, object data = null, bool authenticate = true) where T : class
        {
            if (string.IsNullOrEmpty(BaseUrl)) throw new ClanClientException("not_configured");
            if (Application.internetReachability == NetworkReachability.NotReachable && !BaseUrl.StartsWith("http://127.0.0.1:", StringComparison.Ordinal))
                throw new ClanClientException("offline");
            if (authenticate && !SignedIn) throw new ClanClientException("unauthorized");
            string owner = token;
            using var request = new UnityWebRequest(BaseUrl + "/v1" + path, method);
            request.timeout = 6;
            request.redirectLimit = 0;
            request.downloadHandler = new DownloadHandlerBuffer();
            if (data != null)
            {
                request.uploadHandler = new UploadHandlerRaw(Encoding.UTF8.GetBytes(JsonUtility.ToJson(data)));
                request.SetRequestHeader("Content-Type", "application/json");
            }
            if (authenticate) request.SetRequestHeader("Authorization", "Bearer " + owner);
            try { await request.SendWebRequest(); }
            catch (Exception) when (request.result == UnityWebRequest.Result.ConnectionError)
            { throw new ClanClientException("offline"); }
            if (request.responseCode == 401 && authenticate && owner == token) Forget();
            if (request.result == UnityWebRequest.Result.ConnectionError) throw new ClanClientException("offline");
            string response = request.downloadHandler.text;
            try
            {
                if (request.result == UnityWebRequest.Result.Success)
                    return JsonUtility.FromJson<T>(response) ?? throw new ClanClientException("server");
                var failure = JsonUtility.FromJson<ClanFailure>(response);
                throw new ClanClientException(failure?.error ?? StatusCode(request.responseCode));
            }
            catch (ArgumentException) { throw new ClanClientException(StatusCode(request.responseCode)); }
        }

        static string StatusCode(long status) => status switch
        {
            401 => "unauthorized", 403 => "forbidden", 429 => "rate_limit", 503 => "retry", _ => "server"
        };
    }

    internal sealed class ClanWinTrace
    {
        readonly List<int> moves = new();
        bool eligible = true;
        public void Pour(int source, int target)
        {
            if (!eligible) return;
            if (moves.Count >= 1024) { eligible = false; return; }
            moves.Add(source);
            moves.Add(target);
        }
        public void Booster(Mewgic.Core.BoosterKind kind)
        {
            if (kind == Mewgic.Core.BoosterKind.Undo && moves.Count >= 2)
                moves.RemoveRange(moves.Count - 2, 2);
            if (kind is Mewgic.Core.BoosterKind.ExtraBottle or Mewgic.Core.BoosterKind.Shuffle)
                eligible = false;
        }
        public void Reset() { moves.Clear(); eligible = true; }
        public void Submit(int level)
        {
            if (eligible && moves.Count > 0) ClanClient.Submit(level, moves.ToArray());
        }
    }
}
