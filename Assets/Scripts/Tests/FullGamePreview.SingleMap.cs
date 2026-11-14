#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mewgic.Core;
using Mewgic.Map;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Mewgic
{
    public sealed partial class FullGamePreview
    {
        public const string SingleMapKey = "Mewgic.VisualQA.SingleMap";
        const string SingleMapOutput = "Library/MewgicVisualQA/SingleMap/";

        IEnumerator VerifySingleMap()
        {
            yield return Until(() => Screen.width == 900 && Screen.height == 1600, "Harita Game View 900×1600");
            yield return OpenPage(3, 1, 3);
            var map = FindFirstObjectByType<MapWorld>();
            RequireSingleMap(map);
            RequireMap(Node(1).Status == MapLevelStatus.Current && Node(2).Status == MapLevelStatus.Locked, "İlk açık ve kilitli taşlar");
            Press("MapLevel_2", true);
            RequireMap(map.InformationVisible && SceneManager.GetActiveScene().name == "LevelMap", "Kilitli taş oyuna girmedi");
            yield return null;
            Press("MapInfoClose", true);
            yield return null;
            var backdrop = GameObject.Find("FixedMapEnvironment").GetComponent<RawImage>();
            Vector3 world = backdrop.transform.position;
            Vector2 track = map.Content.anchoredPosition;
            DragMap(map);
            yield return null;
            RequireMap((map.Content.anchoredPosition - track).sqrMagnitude > 1 &&
                backdrop.transform.position == world &&
                FindFirstObjectByType<JourneyPawTrail>().transform.IsChildOf(map.Content), "Kaydırmada dış çevre sabit ve pati katmanı rotaya bağlı");
            map.Scroll.StopMovement();
            Press("FocusCurrent", true);
            yield return Until(() => (map.Content.anchoredPosition - track).sqrMagnitude < 1, "Bulunduğum Bölüm");
            yield return MapShot("Home-to-Map");
            checks.Add("Ana Menü → Harita, kilitli taş, kaydırma ve Bulunduğum Bölüm");
            Press("MapLevel_1", true);
            yield return Until(() => SceneManager.GetActiveScene().name == "Gameplay", "Bölüm taşından gerçek oyun");
            checks.Add("Aktif bölüm taşı Gameplay sahnesini açtı");

            yield return OpenPage(2, 5, 3, 99);
            yield return new WaitForSecondsRealtime(4.2f);
            Press("ResultHome", true);
            yield return Until(() => FindFirstObjectByType<MapWorld>()?.Mounted == true, "Bölüm sonundan doğrudan harita");
            map = FindFirstObjectByType<MapWorld>();
            RequireSingleMap(map);
            RequireMap(map.Progression.CurrentId == 100 && !map.JourneyRunning && Node(5).Status == MapLevelStatus.Completed,
                "Eski bölüm dönüşünde açık bölüm ve kayıt korundu");
            checks.Add("Bölüm sonu → aynı harita, eski bölüm tekrarında ilerleme korunuyor");

            yield return OpenPage(9, 100, 3);
            map = FindFirstObjectByType<MapWorld>();
            map.FocusLevel(40);
            yield return null;
            backdrop = GameObject.Find("FixedMapEnvironment").GetComponent<RawImage>();
            var tintBefore = backdrop.color;
            yield return MapShot("Theme-Village");
            map.FocusLevel(50);
            ScrollMap(map, -6);
            yield return null;
            var tintMid = backdrop.color;
            yield return MapShot("Theme-In-Scroll");
            map.FocusLevel(61);
            yield return null;
            var tintAfter = backdrop.color;
            RequireMap(tintBefore != tintMid && tintMid != tintAfter && FindFirstObjectByType<MapWorld>() == map,
                $"50 → 51 renk karışımı aynı sahnede ({tintBefore}, {tintMid}, {tintAfter})");
            yield return MapShot("Theme-51");
            checks.Add("50 → 51 renk karışımı aynı sahnede");

            yield return OpenPage(10, 100, 3);
            map = FindFirstObjectByType<MapWorld>();
            yield return Until(() => map.JourneyRunning, "99 → 100 pati yürüyüşü");
            var paws = FindFirstObjectByType<JourneyPawTrail>();
            RequireMap(!Node(100).transform.Find("StoneLight").gameObject.activeSelf,
                "Varış ışıması adımlar bitmeden görünmedi");
            yield return Until(() => paws.VisibleSteps >= 5, "Patiler sırayla oluştu");
            RequireSingleMap(map);
            yield return MapShot("Golden-Paw-Trail");
            yield return Until(() => !map.JourneyRunning, "99 → 100 varış");
            var light = Node(100).GetComponentInChildren<JourneyStoneRing>();
            RequireMap(light != null && light.ArrivalStrength > .5f &&
                Node(100).Status == MapLevelStatus.Current &&
                !GameplayProgress.Current.PeekMapJourney(out _, out _), "Altın varış ışıması ve kalıcı varış kaydı");
            yield return MapShot("Golden-Arrival");
            yield return Until(() => light.ArrivalStrength == 0, "Varış parlaması söndü");
            RequireMap(light.gameObject.activeSelf && paws.VisibleSteps > 0, "Aktif taşın aurası ve geçiş izleri kaldı");
            checks.Add("99 → 100 tek sıra altın patiler, varış ışığı ve kalıcı kayıt");

            GameplayProgress.UseTestProfile(Fixture(49));
            RequireMap(GameplayProgress.Current.Claim(50, 3, true, out _), "50 → 51 test kaydı");
            yield return OpenPage(3, 1, 3);
            map = FindFirstObjectByType<MapWorld>();
            yield return Until(() => map.JourneyRunning, "50 → 51 köprü bağlantısı");
            yield return Until(() => !map.JourneyRunning, "50 → 51 varış");
            RequireSingleMap(map);
            RequireMap(map.Progression.CurrentId == 51 && Node(51).Status == MapLevelStatus.Current &&
                !GameplayProgress.Current.PeekMapJourney(out _, out _), "Pati akışı tek rotada tema sınırını geçti");
            checks.Add("50 → 51 aynı rotada tema sınırını geçti");

            yield return VerifyDistantRoad();

            foreach (int height in new[] { 1600, 1950, 2000 })
            {
                Size(height);
                yield return Until(() => Screen.width == 900 && Screen.height == height, "Harita ekran oranı " + height);
                yield return OpenPage(9, 100, 3);
                map = FindFirstObjectByType<MapWorld>();
                RequireSingleMap(map);
                RequireMap(Node(100).Status == MapLevelStatus.Current && map.ActivePointCount <= 45, "Sanal taş havuzu ve aktif taş");
                yield return MapShot("Crystal-" + height);
                checks.Add("900×" + height + " Kristal Game View, etkin bölüm 100");
            }
            Size(1600);
            yield return Until(() => Screen.height == 1600, "Son Game View boyutu");
            Press("MapHome", true);
            yield return Until(() => SceneManager.GetActiveScene().name == "Home", "Haritadan Ana Menü");
            checks.Add("Harita → Ana Menü");
            yield return OpenPage(9, 100, 3);
        }

        IEnumerator VerifyDistantRoad()
        {
            foreach (int start in new[] { 1, 9, 49, 99, 999, 9999, 99999, 999999 })
            {
                GameplayProgress.UseTestProfile(Fixture(start - 1));
                int last = start == 49 ? 52 : start == 1 ? 2 : start + 2;
                for (int level = start; level < last; level++)
                {
                    RequireMap(GameplayProgress.Current.Claim(level, 2, true, out bool granted) && granted,
                        $"İzole kayıt {level} kazanışını işledi");
                    SceneManager.LoadScene("LevelMap");
                    yield return null;
                    yield return Until(() =>
                    {
                        var currentMap = FindFirstObjectByType<MapWorld>();
                        return currentMap != null && currentMap.Mounted && currentMap.Progression.CurrentId == level + 1;
                    }, "Test haritası açıldı");
                    var map = FindFirstObjectByType<MapWorld>();
                    yield return Until(() => !map.JourneyRunning && !GameplayProgress.Current.PeekMapJourney(out _, out _),
                        $"{level} → {level + 1} varış");
                    RequireMap(Node(level + 1).Status == MapLevelStatus.Current && map.ActivePointCount <= 55,
                        $"{level + 1} doğru aktif taş ve sınırlı havuz");
                }
                if (last < 100000) continue;
                var highMap = FindFirstObjectByType<MapWorld>();
                yield return MapShot("Level-" + last);
                var backdrop = GameObject.Find("FixedMapEnvironment").transform;
                var backdropPosition = backdrop.position;
                var finger = new PointerEventData(EventSystem.current) {
                    button = PointerEventData.InputButton.Left, pointerId = 1,
                    position = RectTransformUtility.WorldToScreenPoint(null, highMap.Viewport.TransformPoint(highMap.Viewport.rect.center))
                };
                Vector2 origin = finger.position;
                highMap.Scroll.OnBeginDrag(finger);
                int initial = highMap.SelectedChapter;
                int prior = initial;
                for (int step = 1; step <= 32; step++)
                {
                    finger.position = origin + Vector2.down * (step * 190);
                    highMap.Scroll.OnDrag(finger);
                    yield return null;
                    RequireMap(highMap.ActivePointCount <= 55 && backdrop.position == backdropPosition &&
                        highMap.SelectedChapter <= prior && prior - highMap.SelectedChapter <= 1,
                        "Uzun dokunma sürüklemesinde havuz ve sabit manzara");
                    prior = highMap.SelectedChapter;
                }
                RequireMap(initial - prior >= 4, "Sürükleme yerel kaydırma sınırını geçti");
                yield return MapShot("Scroll-" + last);
                for (int step = 31; step >= 24; step--)
                {
                    finger.position = origin + Vector2.down * (step * 190);
                    highMap.Scroll.OnDrag(finger);
                    yield return null;
                    RequireMap(highMap.SelectedChapter >= prior && highMap.SelectedChapter - prior <= 1,
                        "Aynı dokunuşta yön değişimi");
                    prior = highMap.SelectedChapter;
                }
                highMap.Scroll.OnEndDrag(finger);
                highMap.FocusLevel(2);
                Press("FocusCurrent", true);
                yield return Until(() => !highMap.ReturningToCurrent, "Uzak dönüş tamamlandı");
                yield return Until(() => Node(last)?.GetComponentInChildren<JourneyStoneRing>()?.ArrivalStrength > .2f,
                    "Uzak dönüş ve aktif taş ışığı");
                var position = Node(last).transform.position;
                highMap.FocusLevel(2);
                highMap.FocusLevel(last);
                yield return null;
                RequireMap(Vector2.Distance(Node(last).transform.position, position) < 2,
                    "Yüksek numaralı rota ikinci bağlamada aynı konumda");
            }
            var request = WorldMapRules.Request(1000001);
            var prepared = GameplayLevelCatalog.PrepareAsync(request.level);
            yield return Until(() => prepared.IsCompleted, "1.000.001 bulmacası hazırlandı");
            RequireMap(prepared.IsCompletedSuccessfully && prepared.Result.Info.id == 1000001,
                "Yüksek kimlikli bulmaca görünür haritadan bağımsız hazırlandı");
            checks.Add("1 → 2 ile 999.999 → 1.000.000 → 1.000.001 arası istenen geçişler, dokunma sürüklemesi ve uzak dönüş");
        }

        static void RequireSingleMap(MapWorld map)
        {
            RequireMap(map != null && SceneManager.GetActiveScene().name == "LevelMap" &&
                GameObject.Find("ContinuousRoadContent") != null &&
                new[] { "PreviousIsland", "NextIsland", "ChapterClose", "DetailNext", "WorldCostumeMascot", "WizardCatGuide", "JourneyCatContact" }
                    .All(name => GameObject.Find(name) == null), "Tek LevelMap, ada seçimi görünmüyor");
        }

        static MapNodeBadge Node(int level) => GameObject.Find("MapLevel_" + level)?.GetComponent<MapNodeBadge>();

        static void RequireMap(bool valid, string message)
        {
            if (!valid) throw new InvalidOperationException(message);
        }

        static void DragMap(MapWorld map)
        {
            var viewport = map.Viewport;
            var pointer = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left,
                position = RectTransformUtility.WorldToScreenPoint(null, viewport.TransformPoint(viewport.rect.center))
            };
            ExecuteEvents.Execute(map.Scroll.gameObject, pointer, ExecuteEvents.beginDragHandler);
            pointer.position += Vector2.up * 320;
            ExecuteEvents.Execute(map.Scroll.gameObject, pointer, ExecuteEvents.dragHandler);
            ExecuteEvents.Execute(map.Scroll.gameObject, pointer, ExecuteEvents.endDragHandler);
        }

        static void ScrollMap(MapWorld map, float delta) =>
            ExecuteEvents.Execute(map.Scroll.gameObject, new PointerEventData(EventSystem.current)
            {
                scrollDelta = new Vector2(0, delta)
            }, ExecuteEvents.scrollHandler);

        static IEnumerator MapShot(string name)
        {
            string file = SingleMapOutput + name + ".png";
            Directory.CreateDirectory(SingleMapOutput);
            var previous = File.GetLastWriteTimeUtc(file);
            yield return new WaitForEndOfFrame();
            ScreenCapture.CaptureScreenshot(file);
            yield return Until(() => File.Exists(file) && new FileInfo(file).Length > 0 &&
                File.GetLastWriteTimeUtc(file) != previous, name + " Game View");
        }
    }
}
#endif
