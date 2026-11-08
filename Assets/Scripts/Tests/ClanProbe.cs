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
    public sealed class ClanProbe : MonoBehaviour
    {
        const string Output = "docs/ProductionReports/Clans/";
        static readonly string[] Suffixes = { "", ".pending", ".backup", ".previous", ".corrupt" };
        static readonly FieldInfo Profile = typeof(GameplayProgress).GetField("current",BindingFlags.Static | BindingFlags.NonPublic);
        static readonly FieldInfo PuzzleField = typeof(PrototypeApp).GetField("puzzle",BindingFlags.Instance | BindingFlags.NonPublic);
        static ClanProbe active;
        readonly string key = "Mewgic.QA.Clans." + Guid.NewGuid().ToString("N");
        readonly List<string> checks = new(), errors = new();
        Dictionary<string,string> original;
        bool background;
        Puzzle Board => (Puzzle)PuzzleField.GetValue(FindFirstObjectByType<PrototypeApp>());

        [MenuItem("Mewgic/QA/Run Clans")]
        public static void Begin()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            for (int i = 0; i < SceneManager.sceneCount; i++)
                if (SceneManager.GetSceneAt(i).isDirty) throw new InvalidOperationException("Önce açık sahneyi kaydet.");
            EditorSceneManager.OpenScene("Assets/Scenes/Home.unity");
            new GameObject("Clan QA").AddComponent<ClanProbe>();
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
            var gameView = EditorWindow.GetWindow(typeof(Editor).Assembly.GetType("UnityEditor.GameView"));
            gameView.Show();
            gameView.maximized = true;
            gameView.Focus();
            Size(1600);
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
            Check(condition(),description + (condition() ? "" : " · " + Text("ClanNotice")));
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
            Check(receiver == button.gameObject,name + " gerçek raycast" + (hits.Count == 0 ? $" · isabet yok ({point.position} / {Screen.width}×{Screen.height})" : " → " + hits[0].gameObject.name));
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

        IEnumerator Run()
        {
            string address = File.ReadAllText("Library/ClanQaEndpoint.txt").Trim();
            ClanClient.TestEndpoint(address);
            string id = Guid.NewGuid().ToString("N")[..7];
            string leader = "lider_" + id, member = "uye_" + id, password = Guid.NewGuid().ToString("N");
            yield return Until(() => L.Ready && Find("NavTeam") != null,"Home ve Türkçe tablo hazır");
            yield return new WaitForSecondsRealtime(.5f);
            if (Find("ArrivalDismiss") != null) { Click("ArrivalDismiss"); yield return null; }
            if (Find("ClanClose") != null)
            {
                Click("ClanClose");
                yield return new WaitForSecondsRealtime(.35f);
            }
            Click("NavTeam");
            yield return Shot("01-Login-9x16");
            yield return Sign(true,leader,password);
            Click("ClanCreate");
            yield return null;
            Enter("ClanNameInput","Kristal Patiler");
            Enter("ClanDescriptionInput","Kristal Şelaleler'in büyücü kedileri.");
            Enter("ClanAnnouncementInput","Hafta sonu büyüsü için salonda buluşalım!");
            Click("ClanMode");
            Click("ClanEmblem_1");
            yield return Shot("02-Create-9x16");
            Click("ClanSave");
            yield return Until(() => Find("ClanBanner") != null,"Klan kuruldu ve sunucu lideri atadı");
            var getLeader = ClanClient.Me();
            yield return Await(getLeader,"Liderin kalıcı üyeliği okundu");
            string clanId = getLeader.Result.clanId;
            Check(!string.IsNullOrWhiteSpace(clanId),"Klan sunucu kimliği atadı");
            yield return new WaitForSecondsRealtime(.35f);
            Click("ClanLogout");
            yield return Sign(true,member,password);
            Click("ClanBrowse");
            yield return Until(() => Find("ClanQuery") != null,"Arama paneli açıldı");
            yield return new WaitForSecondsRealtime(.20f);
            Enter("ClanQuery","Kristal");
            Click("ClanSearch");
            yield return Until(() => Find("ClanOpen_" + clanId) != null,"Gerçek klan aramada bulundu");
            yield return Shot("03-Search-9x16");
            Click("ClanOpen_" + clanId);
            yield return Until(() => Find("ClanJoin") != null,"Klan ayrıntısı sunucudan açıldı");
            yield return new WaitForSecondsRealtime(.35f);
            Click("ClanJoin");
            yield return Until(() => Find("ClanJoin")?.GetComponent<Button>().IsInteractable() == false &&
                Text("ClanNotice") == L.Resolve(L.V("clan.request_sent")),"İkinci hesabın katılım isteği kaydedildi");
            yield return new WaitForSecondsRealtime(.35f);
            Click("ClanLogout");
            yield return Sign(false,leader,password);
            var view = ClanClient.View(clanId);
            yield return Await(view,"Liderin istek listesi sunucudan geldi");
            var request = view.Result.requests.Single(p => p.handle == member);
            var scroll = Find("ClanViewport").GetComponent<ScrollRect>();
            Canvas.ForceUpdateCanvases();
            var button = Find("ClanApprove_" + request.id).transform;
            var row = (RectTransform)button.parent;
            var local = (Vector2)scroll.content.InverseTransformPoint(row.position);
            float range = scroll.content.rect.height - scroll.viewport.rect.height;
            scroll.verticalNormalizedPosition = 1 - Mathf.Clamp01((-local.y-scroll.viewport.rect.height*.52f)/range);
            yield return null;
            Click("ClanApprove_" + request.id);
            yield return Until(() => Find("ClanMember_" + request.id) != null,"Lider gerçek istek düğmesiyle üyeyi aldı");
            yield return new WaitForSecondsRealtime(.35f);
            Click("ClanLogout");
            yield return Sign(false,member,password);
            yield return Until(() => Find("ClanBanner") != null,"Üye kendi klan salonunu gördü");
            yield return new WaitForSecondsRealtime(.35f);
            Click("ClanClose");
            yield return null;
            Click("Play");
            yield return Win(1);
            var profile = ClanClient.Me();
            yield return Await(profile,"Gerçek ilk kazanış sunucudan okundu");
            Check(profile.Result.wins == 1 && GameplayProgress.Current.Presentation.coins > 0,
                "Oyun altını ve sunucu kazanışı ayrı kaydedildi");
            Click("Replay");
            yield return Win(1);
            profile = ClanClient.Me();
            yield return Await(profile,"Tekrar sonucu sunucudan okundu");
            Check(profile.Result.wins == 1,"Aynı bölümün gerçek tekrarı yeni klan puanı üretmedi");
            Click("NextLevel");
            yield return Win(2);
            Click("NextLevel");
            yield return Win(3);
            profile = ClanClient.Me();
            yield return Await(profile,"Üretilmiş üçüncü bölüm sunucudan okundu");
            Check(profile.Result.wins == 3,"Sunucu sabit ve deterministik üretilmiş bölümleri doğruladı");
            var score = ClanClient.View(clanId);
            yield return Await(score,"Sunucunun ortak klan toplamı okundu");
            long expected = 3L * profile.Result.winPoints + profile.Result.dailyPoints +
                (score.Result.weekendEvent ? profile.Result.weekendPoints : 0);
            Check(score.Result.clan.points == expected && score.Result.clan.weeklyPoints == expected,
                "Ortak haftalık ve toplam puan yalnız benzersiz kazanışlardan hesaplandı");
            Click("ResultHome");
            yield return Until(() => Find("MapHome") != null,"Sonuçtan haritaya dönüldü");
            yield return new WaitForSecondsRealtime(.30f);
            if (Find("ChapterClose") != null)
            {
                Click("ChapterClose");
                yield return new WaitForSecondsRealtime(.30f);
            }
            Click("MapHome");
            yield return Until(() => Find("NavTeam") != null,"Gameplay sonrası ana menüye dönüldü");
            yield return new WaitForSecondsRealtime(.5f);
            if (Find("ArrivalDismiss") != null) { Click("ArrivalDismiss"); yield return null; }
            if (Find("ClanClose") != null)
            {
                Click("ClanClose");
                yield return new WaitForSecondsRealtime(.35f);
            }
            Click("NavTeam");
            yield return Until(() => Text("ClanWeekly") == L.Resolve(L.V("clan.weekly",expected,score.Result.clan.weeklyGoal)),"Ortak hedef güncellendi");
            Size(1950);
            yield return Until(() => Screen.height == 1950,"9:19.5 Game View");
            yield return Shot("04-Hall-9x19_5");
            Size(2000);
            yield return Until(() => Screen.height == 2000,"9:20 Game View");
            yield return null;
            var banner = Find("ClanPanel").GetComponent<RectTransform>();
            var safe = (RectTransform)banner.parent;
            safe.anchorMin = new Vector2(.03f,.055f);
            safe.anchorMax = new Vector2(.97f,.945f);
            yield return null;
            Check(banner.anchorMin == Vector2.zero && banner.anchorMax == Vector2.one &&
                safe.anchorMin == new Vector2(.03f,.055f) && Text("ClanName") == "Kristal Patiler","Klan salonu güvenli alana bağlı ve metin doğru");
            Canvas.ForceUpdateCanvases();
            foreach (string label in new[] { "ClanTitle", "ClanName", "ClanStats", "ClanWeekly" })
            {
                var caption = Find(label).GetComponent<TMP_Text>();
                caption.ForceMeshUpdate();
                Check(!caption.isTextOverflowing,label + " guvenli alanda tasmiyor");
            }
            yield return Shot("05-Hall-9x20");
            ClanClient.TestEndpoint("http://127.0.0.1:1");
            Click("ClanHall");
            yield return Until(() => Text("ClanNotice") == L.Resolve(L.V("clan.error_offline")),"Ulaşılamayan socket ağ hatasını gösterdi");
            Check(Find("ClanBanner") != null,"Ağ kesintisinde son okunan salon ekranda kaldı");
            yield return Shot("06-Offline-9x20");
            ClanClient.TestEndpoint(address);
            Click("ClanHall");
            yield return Until(() => Text("ClanNotice") == "" && Find("ClanBanner") != null,"Ağ geri gelince salon yenilendi");
            ClanClient.TestExpireAccess();
            var renewed = ClanClient.Me();
            yield return Await(renewed,"Unity süresi geçen erişimi yeniledi");
            Check(renewed.Result.wins == 3 && renewed.Result.clanId == clanId,"Yenilemede aynı hesap ve puan korundu");
            string proofKey = "Mewgic.QA.ClanOutbox."+Guid.NewGuid().ToString("N");
            var prepared = GameplayLevelCatalog.PrepareAsync(4);
            yield return Await(prepared,"Çevrimdışı sıra için dördüncü bölüm hazır");
            var proof = PuzzleSolver.Solve(prepared.Result.Bottles).Moves.SelectMany(m => new[] { m.Source,m.Target }).ToArray();
            ClanClient.TestOutbox(proofKey);
            try
            {
                ClanClient.TestEndpoint("http://127.0.0.1:1");
                ClanClient.Submit(4,proof);
                yield return Await(ClanClient.Flush(),"Kesintide gönderim tamamlanamadı");
                Check(ClanClient.Pending == 1 && PlayerPrefs.HasKey(proofKey),"Bekleyen kanıt kalıcı kuyruğa yazıldı");
                var exit = ClanClient.SignOut();
                yield return Until(() => exit.IsCompleted,"Kesintide oturum belleği temizlendi");
                _ = exit.Exception;
                Check(!ClanClient.SignedIn && ClanClient.Pending == 0 && PlayerPrefs.HasKey(proofKey),"Oturum kapanınca kalıcı kanıt korundu");
                ClanClient.TestEndpoint(address);
                yield return Await(ClanClient.SignIn(false,member,password),"Aynı hesap yeniden giriş yaptı");
                Check(ClanClient.Pending == 1,"Bekleyen kanıt cihaz kaydından geri okundu");
                yield return Await(ClanClient.Flush(),"Yeniden bağlanınca kanıt teslim edildi");
                var updated = ClanClient.Me();
                yield return Await(updated,"Geri yüklenen kanıtın sunucu puanı okundu");
                Check(updated.Result.wins == 4 && ClanClient.Pending == 0 && !PlayerPrefs.HasKey(proofKey),
                    "Sunucu bir kazanış yazdı ve gönderilen kanıt kuyruktan çıkarıldı");
            }
            finally
            {
                ClanClient.TestEndpoint(address);
                ClanClient.TestOutbox(null);
                PlayerPrefs.DeleteKey(proofKey);
                PlayerPrefs.Save();
            }
            yield return new WaitForSecondsRealtime(.25f);
            Click("ClanAccountSettings");
            yield return Shot("07-Account-9x20");
            string clipboard = GUIUtility.systemCopyBuffer;
            try
            {
                Click("ClanExportAccount");
                yield return Until(() => Text("ClanNotice") == L.Resolve(L.V("clan.account_exported")),"Hesap verisi gerçek API'den kopyalandı");
                Check(GUIUtility.systemCopyBuffer.Contains(member) && !GUIUtility.systemCopyBuffer.Contains(password),"Dışa aktarım oyuncuyu içerir ve parola içermez");
            }
            finally { GUIUtility.systemCopyBuffer = clipboard; }
            string saveBeforeDeletion = JsonUtility.ToJson(GameplayProgress.Current.Snapshot);
            Enter("ClanDeletionPassword",password);
            Click("ClanDeleteAccount");
            yield return Until(() => Find("ClanSignIn") != null && !ClanClient.SignedIn,"Hesap silme oturumu kapattı");
            Check(JsonUtility.ToJson(GameplayProgress.Current.Snapshot) == saveBeforeDeletion,"Silme cihazdaki ilerlemeyi değiştirmedi");
        }
    }
}
#endif
