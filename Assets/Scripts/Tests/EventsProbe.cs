#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Mewgic.Clans;
using Mewgic.Core;
using Mewgic.Localization;
using Mewgic.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Mewgic
{
    [DefaultExecutionOrder(-20000)]
    public sealed class EventsProbe : MonoBehaviour
    {
        const string Output = "docs/ProductionReports/Events/";
        static readonly string[] Suffixes = { "", ".pending", ".backup", ".previous", ".corrupt" };
        static readonly FieldInfo Profile = typeof(GameplayProgress).GetField("current",BindingFlags.Static | BindingFlags.NonPublic);
        static readonly FieldInfo PuzzleField = typeof(PrototypeApp).GetField("puzzle",BindingFlags.Instance | BindingFlags.NonPublic);
        static EventsProbe active;
        readonly string key = "Mewgic.QA.Events." + Guid.NewGuid().ToString("N");
        readonly List<string> checks = new(), errors = new();
        Dictionary<string,string> original;
        bool background;
        Puzzle Board => (Puzzle)PuzzleField.GetValue(FindFirstObjectByType<PrototypeApp>());

        [MenuItem("Mewgic/QA/Run Magical Events")]
        public static void Begin()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Önce açık sahneyi kaydet.");
            EditorSceneManager.OpenScene("Assets/Scenes/Home.unity");
            new GameObject("Events QA").AddComponent<EventsProbe>();
            Size(1600);
            EditorApplication.isPlaying = true;
        }

        void Awake()
        {
            if (active != null) { Destroy(gameObject); return; }
            active = this;
            DontDestroyOnLoad(gameObject);
            original = PlayerSave();
            background = Application.runInBackground;
            Application.runInBackground = true;
            var data = new ProgressData();
            data.presentation.manualLocale = "tr";
            data.presentation.coins = 33;
            for (int level=1;level<=4;level++)
                data.levels.Add(new LevelRecord { level=level,bestStars=3,rewardClaimed=true });
            data.hub.owned.Add(RewardHubRules.StarBadge);
            data.presentation.reduceMotion = false;
            data.presentation.motionPreferenceSet = true;
            PlayerPrefs.SetString(key,JsonUtility.ToJson(data));
            PlayerPrefs.Save();
            GameplayProgress.UseTestProfile();
            Profile.SetValue(null,GameplayProgress.Open(key));
            GameplayLevelCatalog.ResetSelection();
            Application.logMessageReceived += Log;
        }

        void OnDestroy()
        {
            if (active != this) return;
            active = null;
            Application.runInBackground = background;
            Application.logMessageReceived -= Log;
            GameplayProgress.RestorePlayerProfile();
            GameplayLevelCatalog.ResetSelection();
            foreach (string suffix in Suffixes) PlayerPrefs.DeleteKey(key + suffix);
            PlayerPrefs.Save();
        }

        void Log(string message,string trace,LogType type)
        {
            if (type is LogType.Error or LogType.Assert or LogType.Exception) errors.Add(message);
        }

        static Dictionary<string,string> PlayerSave() => Suffixes.ToDictionary(s => s, s =>
            PlayerPrefs.HasKey(GameplayProgress.StorageKey+s) ? PlayerPrefs.GetString(GameplayProgress.StorageKey+s) : null);
        static void Size(int height) => Type.GetType("Part12BEditor, Assembly-CSharp-Editor").GetMethod("Size")
            .Invoke(null,new object[] { height });
        static GameObject Find(string name) => GameObject.Find(name);
        static string Text(string name) => Find(name)?.GetComponent<TMP_Text>()?.text;
        static void Enter(string name,string value) => Find(name).GetComponent<TMP_InputField>().text = value;

        void Check(bool condition,string text)
        {
            if (!condition) throw new InvalidOperationException(text);
            checks.Add(text);
        }

        IEnumerator Start()
        {
            Directory.CreateDirectory(Output);
            string failure = null;
            var stack = new Stack<IEnumerator>();
            stack.Push(Run());
            while (stack.Count > 0)
            {
                bool more; object item;
                try { more = stack.Peek().MoveNext(); item = more ? stack.Peek().Current : null; }
                catch (Exception error) { failure = error.ToString(); break; }
                if (!more) stack.Pop();
                else if (item is IEnumerator child && item is not CustomYieldInstruction) stack.Push(child);
                else yield return item;
            }
            var after = PlayerSave();
            bool intact = original.All(p => after[p.Key] == p.Value);
            File.WriteAllText(Output+"PLAY_MODE.json",Newtonsoft.Json.JsonConvert.SerializeObject(new
            {
                success = intact && failure == null && errors.Count == 0, editor = Application.unityVersion,
                transport = "UnityWebRequest → live HTTP API → SQLite", count = checks.Count, checks, errors, failure, playerSaveIntact = intact
            },Newtonsoft.Json.Formatting.Indented));
            EditorApplication.isPlaying = false;
        }

        IEnumerator Until(Func<bool> condition,string description)
        {
            float deadline = Time.realtimeSinceStartup + 24;
            while (!condition() && Time.realtimeSinceStartup < deadline) yield return null;
            Check(condition(),description + (condition() ? "" : " · " + Text("EventNotice")));
        }

        IEnumerator Await(Task task,string name)
        {
            yield return Until(() => task.IsCompleted,name);
            task.GetAwaiter().GetResult();
        }

        void Click(string name,bool raycast = true)
        {
            var button = Find(name)?.GetComponent<Button>();
            Check(button != null && button.IsInteractable(),name + " etkileşimli");
            if (!raycast) { button.onClick.Invoke(); return; }
            Canvas.ForceUpdateCanvases();
            var rect = (RectTransform)button.transform;
            var point = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left,
                position = RectTransformUtility.WorldToScreenPoint(null,rect.TransformPoint(rect.rect.center))
            };
            var hits = new List<RaycastResult>();
            EventSystem.current.RaycastAll(point,hits);
            var receiver = hits.Count == 0 ? null : ExecuteEvents.GetEventHandler<IPointerClickHandler>(hits[0].gameObject);
            Check(receiver == button.gameObject,name + " gerçek raycast" + (hits.Count == 0 ? " · isabet yok" : " → " + hits[0].gameObject.name));
            ExecuteEvents.Execute(receiver,point,ExecuteEvents.pointerClickHandler);
        }

        IEnumerator Shot(string file)
        {
            yield return new WaitForSecondsRealtime(.25f);
            var path = Output + file + ".png";
            ScreenCapture.CaptureScreenshot(path);
            yield return new WaitForSecondsRealtime(.35f);
            Check(File.Exists(path),file + " gerçek Game View");
        }

        IEnumerator Sign(bool register,string handle,string password)
        {
            yield return Until(() => Find("ClanHandle") != null,"Hesap alanları açıldı");
            yield return new WaitForSecondsRealtime(.20f);
            Enter("ClanHandle",handle);
            Enter("ClanPassword",password);
            Click(register ? "ClanRegister" : "ClanSignIn");
            yield return Until(() => Find("ClanHall") != null && ClanClient.SignedIn,"Gerçek HTTP oturumu açıldı");
            yield return new WaitForSecondsRealtime(.20f);
        }

        IEnumerator Win(int level)
        {
            yield return Until(() => FindFirstObjectByType<GameplayHud>()?.State.Level == level && Find("Bottle 1") != null,
                "Gameplay " + level + " açıldı");
            FindFirstObjectByType<PawGuide>()?.Close();
            var solution = PuzzleSolver.Solve(Board.Export());
            Check(solution.Status == SearchStatus.Solved && solution.Moves.Count > 0,"Bölüm " + level + " gerçek çözüm");
            foreach (var move in solution.Moves)
            {
                int next = Board.Moves + 1;
                Click("Bottle " + (move.Source + 1),false);
                yield return null;
                Click("Bottle " + (move.Target + 1),false);
                yield return Until(() => Board.Moves == next && (Board.Won || FindFirstObjectByType<PrototypeApp>().GuideCanInteract),"Döküş uygulandı");
            }
            yield return Until(() => Find("Win panel") != null,"Sonuç açıldı");
            yield return new WaitForSecondsRealtime(.25f);
            yield return Await(ClanClient.Flush(),"Bölüm kanıtı HTTP üzerinden gönderildi");
        }

        [Serializable] sealed class Fixture { public string endpoint,handle,password; }

        IEnumerator Run()
        {
            var fixture = JsonUtility.FromJson<Fixture>(File.ReadAllText("Library/EventsQaFixture.json"));
            ClanClient.TestEndpoint(fixture.endpoint);
            yield return Until(() => L.Ready && Find("NavEvents") != null,"Türkçe ana menü hazır");
            yield return new WaitForSecondsRealtime(.45f);
            if (Find("ArrivalDismiss") != null) { Click("ArrivalDismiss"); yield return null; }
            Click("NavEvents");
            yield return Until(() => Find("EventOpen_daily_spell") != null,"Herkese açık gerçek takvim açıldı");
            Check(Find("EventSignIn") != null,"Misafir için giriş düğmesi görünür");
            Click("EventSignIn");
            yield return Sign(false,fixture.handle,fixture.password);
            Click("ClanClose");
            yield return null;
            Click("NavEvents");
            yield return Until(() => Text("EventTimer_daily_spell")?.Contains("11:59") == true,"Sunucu geri sayımı ilerliyor");
            Check(Find("EventRows").transform.childCount == 4,"Dört aktif kart");
            yield return Shot("01-Active-9x16");
            Click("EventTab_0");
            Check(Find("EventCard_clan_crystal_cup") != null,"Yaklaşan klan sezonu listelenir");
            Size(1950);
            yield return Until(() => Screen.height == 1950,"9:19.5 Game View");
            yield return Shot("02-Upcoming-9x19_5");
            Click("EventLegacy");
            yield return Until(() => Find("RewardHubClose") != null,"Klasik ödül merkezi hâlâ erişilebilir");
            Check(GameplayProgress.Current.HubSnapshot.owned.Contains(RewardHubRules.StarBadge),"Önceki koleksiyon kaydı korundu");
            yield return new WaitForSecondsRealtime(.25f);
            Click("RewardHubClose");
            yield return null;
            Click("Play");
            yield return Win(5);
            var scores = ClanClient.Events();
            yield return Await(scores,"Doğrulanmış tamamlamadan sonra etkinlik verisi okundu");
            var daily = scores.Result.cards.First(c => c.kind == "daily_spell" && c.status == 1);
            Check(daily.points == 500,"Gerçek bölüm kazanışı 400 → 500 etkinlik puanı yazdı");

            SceneManager.LoadScene("Home");
            yield return null;
            yield return Until(() => Find("NavEvents") != null,"Ana ekran yeniden açıldı");
            yield return new WaitForSecondsRealtime(.45f);
            if (Find("ArrivalDismiss") != null) { Click("ArrivalDismiss"); yield return null; }
            Click("NavEvents");
            yield return Until(() => Find("EventOpen_daily_spell") != null,"İmzalı etkinlik tahtası yüklendi");
            Size(2000);
            yield return Until(() => Screen.height == 2000,"9:20 Game View");
            var root = (RectTransform)Find("MagicalEvents").transform;
            var safe = (RectTransform)root.parent;
            safe.anchorMin = new Vector2(.03f,.055f);
            safe.anchorMax = new Vector2(.97f,.945f);
            yield return null;
            Click("EventOpen_daily_spell");
            foreach (string name in new[] { "EventTitle","EventName","EventRule","EventDetailScore" })
            {
                var caption = Find(name).GetComponent<TMP_Text>();
                caption.ForceMeshUpdate();
                Check(!caption.isTextOverflowing,name+" güvenli alanda taşmıyor");
            }
            yield return Shot("03-Rewards-9x20");
            Click("EventOdds_2");
            yield return Shot("04-Chest-Odds-9x20");
            Click("EventModalClose");
            yield return null;
            var ledger = GameplayProgress.Current;
            long gold = ledger.Presentation.coins;
            int hint = ledger.Remaining(BoosterKind.Hint);
            Click("EventClaim_2");
            yield return Until(() => Find("EventChestPrize") != null,"Gerçek sandık teslimi animasyonu açıldı");
            var animated = Find("EventChestArt").GetComponent<RectTransform>();
            Check(animated.localScale.x != 1 || Find("EventChestPrize").GetComponent<TMP_Text>().alpha < 1,
                "Sandık açılma animasyonu oynuyor");
            yield return new WaitForSecondsRealtime(1);
            yield return Shot("05-Chest-Reveal-9x20");
            Click("EventModalClose");
            var wallet = ledger.GetOrCreateLeagueWallet();
            var same = ClanClient.ClaimEvent(daily.id,2,wallet);
            yield return Await(same,"Aynı sandık talebi aynı ödülü döndürdü");
            var grant = same.Result;
            Check(ledger.HasLeagueReward(grant.id) && ledger.Presentation.coins == gold+grant.prize.gold &&
                ledger.Remaining(BoosterKind.Hint) == hint+grant.prize.count,"Altın veya güçlendirici makbuzla bir kez eklendi");
            var reopened = GameplayProgress.Open(key);
            Check(reopened.HasLeagueReward(grant.id) && reopened.ApplyEventReward(wallet,grant.id,grant.prize.gold,
                grant.prize.booster,grant.prize.count,grant.prize.cosmetic) &&
                reopened.Presentation.coins == gold+grant.prize.gold && reopened.Remaining(BoosterKind.Hint) == hint+grant.prize.count,
                "Yeniden açılan kayıt tekrarlı teslimde değişmiyor");
            Check(reopened.HubSnapshot.owned.Contains(RewardHubRules.StarBadge),"Klasik koleksiyon yeni makbuzdan sonra da korunuyor");

            bool fail = false;
            var model = new ProgressLedger(new ProgressData(),_=>{ if(fail) throw new IOException("fixture"); });
            string id = model.GetOrCreateLeagueWallet();
            string receipt = "event-" + Guid.NewGuid().ToString("N");
            long oldGold = model.Presentation.coins;
            int oldUndo = model.Remaining(BoosterKind.Undo);
            fail = true;
            Check(!model.ApplyEventReward(id,receipt,50,"undo",2,"rose_effect") && !model.HasLeagueReward(receipt) &&
                !model.Owns(ShopItem.RoseEffect) && model.Presentation.coins == oldGold &&
                model.Remaining(BoosterKind.Undo) == oldUndo,"Yazma hatasında tüm kalemler ve makbuz atomik olarak korundu");
            fail = false;
            Check(model.ApplyEventReward(id,receipt,50,"undo",2,"rose_effect") && model.Owns(ShopItem.RoseEffect) &&
                model.Presentation.coins == oldGold+50 && model.Remaining(BoosterKind.Undo) == oldUndo+2,
                "Aynı teslim daha sonra tüm kalemleri bir kez kaydetti");

            ClanClient.TestEndpoint("http://127.0.0.1:1");
            Click("EventRefresh");
            yield return Until(() => Text("EventNotice") == L.Resolve(L.V("clan.error_offline")),"Kesinti anlaşılır biçimde gösterildi");
            Check(Find("EventClaim_2").GetComponent<Button>().interactable == false &&
                ledger.Presentation.coins == gold+grant.prize.gold,"Kesinti ödülü yinelemedi veya silmedi");
        }
    }
}
#endif
