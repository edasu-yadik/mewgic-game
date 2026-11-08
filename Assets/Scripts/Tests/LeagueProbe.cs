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
    public sealed class LeagueProbe : MonoBehaviour
    {
        const string Output = "docs/ProductionReports/League/";
        static readonly string[] Suffixes = { "", ".pending", ".backup", ".previous", ".corrupt" };
        static readonly FieldInfo Profile = typeof(GameplayProgress).GetField("current",BindingFlags.Static | BindingFlags.NonPublic);
        static readonly FieldInfo PuzzleField = typeof(PrototypeApp).GetField("puzzle",BindingFlags.Instance | BindingFlags.NonPublic);
        static LeagueProbe active;
        readonly string key = "Mewgic.QA.League." + Guid.NewGuid().ToString("N");
        readonly List<string> checks = new(), errors = new();
        Dictionary<string,string> original;
        bool background;
        Puzzle Board => (Puzzle)PuzzleField.GetValue(FindFirstObjectByType<PrototypeApp>());

        [MenuItem("Mewgic/QA/Run League")]
        public static void Begin()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Önce açık sahneyi kaydet.");
            EditorSceneManager.OpenScene("Assets/Scenes/Home.unity");
            new GameObject("League QA").AddComponent<LeagueProbe>();
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
            data.levels.Add(new LevelRecord { level=1,bestStars=3,rewardClaimed=true });
            data.presentation.reduceMotion = true;
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
            Check(condition(),description + (condition() ? "" : " · " + Text("LeagueNotice")));
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
            var fixture = JsonUtility.FromJson<Fixture>(File.ReadAllText("Library/LeagueQaFixture.json"));
            ClanClient.TestEndpoint(fixture.endpoint);
            yield return Until(() => L.Ready && Find("NavTeam") != null,"Türkçe ana menü hazır");
            yield return new WaitForSecondsRealtime(.45f);
            if (Find("ArrivalDismiss") != null) { Click("ArrivalDismiss"); yield return null; }
            Click("NavTeam");
            yield return Sign(false,fixture.handle,fixture.password);
            Click("ClanLeague");
            yield return Until(() => Text("LeagueSeason")?.Contains("Sezon 2") == true,"Arena etkin sunucu sezonunu açtı");
            Check(Text("LeagueTimer").Contains("gün") && Text("LeagueTierGold_5").Contains("650"),
                "Sunucu sayacı ve ödül kademeleri gösterildi");
            for (int i=0;i<6;i++) Check(Find("LeagueTier_" + i)?.GetComponent<LeagueBadge>() != null,"Rozet önizlemesi " + i);
            Check(Find("LeagueUnranked") != null,"Kazanış öncesi hesap sırasız");
            yield return Shot("01-Arena-9x16");
            Click("LeagueHistory");
            yield return Until(() => Find("LeagueClaim") != null && Text("LeagueTimer") == L.Resolve(L.V("league.closed")),
                "Kapalı sezon ve ödül makbuzu görüntülendi");
            yield return Shot("02-History-9x16");
            var past = ClanClient.League(false,1);
            yield return Await(past,"Arşiv kaydı gerçek API'den okundu");
            var ledger = GameplayProgress.Current;
            long gold = ledger.Presentation.coins;
            Click("LeagueClaim");
            yield return Until(() => ledger.HasLeagueReward(past.Result.reward.id) &&
                Find("LeagueClaim")?.GetComponent<Button>().IsInteractable() == false,"Ödül gerçek oyun kaydına aktarıldı");
            Check(ledger.Presentation.coins == gold+100,"Sezon ödülü mağaza altınına bir kez eklendi");

            var reopened = GameplayProgress.Open(key);
            Check(reopened.HasLeagueReward(past.Result.reward.id) && reopened.Presentation.coins == gold+100,
                "Yeniden açılan oyun kaydında altın ve makbuz korunuyor");
            var same = ClanClient.Claim(1,reopened.GetOrCreateLeagueWallet());
            yield return Await(same,"Sunucu aynı oyun kaydına aynı makbuzu döndürdü");
            Check(reopened.ApplyLeagueReward(reopened.GetOrCreateLeagueWallet(),same.Result.id,same.Result.gold,same.Result.cosmetic) &&
                reopened.Presentation.coins == gold+100,"Aynı makbuz ikinci kez altın vermedi");
            var invalid = ClanClient.Claim(1,Guid.NewGuid().ToString("N"));
            yield return Until(() => invalid.IsCompleted,"Başka kayıt ile teslim isteği sonuçlandı");
            Check(invalid.Exception?.GetBaseException() is ClanClientException denied && denied.Code == "league_other_wallet",
                "Sunucu ikinci bir oyun kaydına aktarımı engelledi");

            bool fail = false;
            var model = new ProgressLedger(new ProgressData(),_=>{ if(fail) throw new IOException("fixture"); });
            string id = model.GetOrCreateLeagueWallet();
            string receipt = "league-1-" + Guid.NewGuid().ToString("N");
            long oldGold = model.Presentation.coins;
            fail = true;
            Check(!model.ApplyLeagueReward(id,receipt,200,"league_cat") && !model.HasLeagueReward(receipt) &&
                !model.Owns(ShopItem.LeagueCat) && model.Presentation.coins == oldGold,"Yerel yazma hatası tüm ödülü atomik olarak geri bıraktı");
            fail = false;
            Check(model.ApplyLeagueReward(id,receipt,200,"league_cat") && model.Owns(ShopItem.LeagueCat) &&
                model.Presentation.coins == oldGold+200 && model.Equip(ShopItem.LeagueCat),"Arena kostümü teslim edildi ve kuşanılabildi");

            yield return Shot("03-Claimed-9x16");
            Click("LeagueClose");
            yield return new WaitForSecondsRealtime(.20f);
            Click("ClanClose");
            yield return null;
            Click("Play");
            yield return Win(2);
            var score = ClanClient.League(false,null);
            yield return Await(score,"Lig puanı gerçek döküşten sonra okundu");
            Check(score.Result.self?.points == 20 && score.Result.self.wins == 1,
                "Doğrulanmış bölüm 2, zorluk ve üç yıldızla 20 LP yazdı");
            var teams = ClanClient.League(true,null);
            yield return Await(teams,"Klan lig tablosu okundu");
            Check(teams.Result.self?.points == 20 && teams.Result.clanEmblem == 0,
                "Ortak klan puanı ve önceki sezon amblemi korundu");

            // Reopen the normal Home entry point with the persisted game profile.
            Profile.SetValue(null,GameplayProgress.Open(key));
            SceneManager.LoadScene("Home");
            yield return null;
            yield return Until(() => Find("NavTeam") != null,"Oyun ekranı yeniden açıldı");
            yield return new WaitForSecondsRealtime(.45f);
            if (Find("ArrivalDismiss") != null) { Click("ArrivalDismiss"); yield return null; }
            Click("NavTeam");
            yield return Until(() => Find("ClanLeague") != null,"Oturumla klan salonu yeniden açıldı");
            yield return new WaitForSecondsRealtime(.20f);
            Click("ClanLeague");
            yield return Until(() => Find("LeagueSelf") != null,"Aynı sıra ve puan yeniden açılışta geldi");
            Size(1950);
            yield return Until(() => Screen.height == 1950,"9:19.5 Game View");
            yield return Shot("04-Players-9x19_5");
            Click("LeagueClans");
            yield return Until(() => Find("LeagueSection_your_clan") != null && Find("LeagueSelf") != null,
                "Klan sıralaması gerçek düğmeyle açıldı");
            Size(2000);
            yield return Until(() => Screen.height == 2000,"9:20 Game View");
            yield return null;
            var root = (RectTransform)Find("LeagueArena").transform;
            var safe = (RectTransform)root.parent;
            safe.anchorMin = new Vector2(.03f,.055f);
            safe.anchorMax = new Vector2(.97f,.945f);
            yield return null;
            Check(root.anchorMin == Vector2.zero && root.anchorMax == Vector2.one &&
                safe.anchorMin == new Vector2(.03f,.055f),"Arena güvenli alanı koruyor");
            foreach (string name in new[] { "LeagueTitle","LeagueSeason","LeagueTimer","LeagueTierName_5" })
            {
                var caption = Find(name).GetComponent<TMP_Text>();
                caption.ForceMeshUpdate();
                Check(!caption.isTextOverflowing,name + " güvenli alanda taşmıyor");
            }
            yield return Shot("05-Clans-9x20");
        }
    }
}
#endif
