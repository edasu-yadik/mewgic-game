#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mewgic.Core;
using TMPro;
using UnityEngine;
using UnityEngine.Profiling;
using Unity.Profiling;

namespace Mewgic
{
    public sealed partial class FullGamePreview
    {
        const string ReviewOutput = "Library/MewgicVisualQA/FinalReview/";
        static readonly string[] ShopTabs = { "Offers", "Gold", "Powers", "Costumes" };
        readonly List<object> reviewPerformance = new();
        readonly List<object> reviewOverflow = new();

        public void NavigateShopTab(string tab)
        {
            if (!Busy && ShopTabs.Contains(tab)) StartCoroutine(Drive(OpenShopTab(tab), false));
        }

        IEnumerator OpenShopTab(string tab)
        {
            if (GameObject.Find("MagicShop") == null) yield return OpenPage(4, 1, 3);
            Press("ShopTab_" + tab);
            yield return Until(() => GameObject.Find("ShopPage_" + tab) != null &&
                GameObject.Find("ShopOutgoingPage") == null, "Mağaza " + tab);
            yield return null;
        }

        IEnumerator Review()
        {
            Directory.CreateDirectory(ReviewOutput);
            var screens = new[]
            {
                (0, 1, "Home"), (1, 5, "Gameplay"), (2, 7, "Victory"), (3, 1, "World"),
                (11, 1, "Village"), (9, 100, "Crystal"),
                (5, 1, "Clans"), (6, 1, "League"), (7, 1, "Events"), (8, 1, "Daily")
            };
            foreach (int height in new[] { 1600, 1950, 2000 })
            {
                Size(height);
                yield return Until(() => Screen.width == 900 && Screen.height == height, "900×" + height);
                foreach (var (page, level, name) in screens)
                {
                    yield return OpenPage(page, level, 3);
                    if (page == 2) yield return new WaitForSecondsRealtime(4.2f);
                    if (page is >= 5 and <= 8) yield return new WaitForSecondsRealtime(.55f);
                    yield return ReviewShot($"{name}-{height}");
                    if (height == 1600 && page is 0 or 1 or 2 or 3 or 9) yield return Measure(name);
                }

                foreach (string tab in ShopTabs)
                {
                    yield return OpenShopTab(tab);
                    var page = GameObject.Find("ShopPage_" + tab);
                    var labels = page.GetComponentsInChildren<TMP_Text>();
                    foreach (var label in labels)
                    {
                        label.ForceMeshUpdate();
                        if (label.isTextOverflowing)
                            throw new InvalidOperationException($"Mağaza {tab}/{height}: {label.name} taştı.");
                    }
                    yield return ReviewShot($"{tab}-{height}", false);
                    checks.Add($"{tab} {Screen.width}×{Screen.height}: {labels.Length} metin etiketi");
                    if (height == 1600) yield return Measure(tab);
                }
            }

            Size(1600);
            yield return Until(() => Screen.width == 900 && Screen.height == 1600, "Mağaza test oranı");
            yield return OpenShopTab("Powers");
            var ledger = GameplayProgress.Current;
            var config = EconomyConfig.Current;
            long gold = ledger.Presentation.coins, gems = ledger.Presentation.gems;
            int stock = ledger.Remaining(BoosterKind.ExtraBottle);
            Press("BuyExtraBottle", true);
            yield return null;
            Press("ShopConfirm", true);
            yield return Until(() => GameObject.Find("ShopMotion") == null, "Güç alımı animasyonu");
            if (ledger.Presentation.coins != gold - config.extraPrice || ledger.Presentation.gems != gems ||
                ledger.Remaining(BoosterKind.ExtraBottle) != stock + config.boosterQuantity)
                throw new InvalidOperationException("Ek Şişe alış kaydı uyuşmuyor.");
            checks.Add("İzole profilde Ek Şişe satın alındı. Altın ve stok doğrulandı.");
            yield return ReviewShot("PowerPurchased", false);

            yield return OpenShopTab("Costumes");
            gold = ledger.Presentation.coins;
            Press("ShopAction_ExplorerCat", true);
            yield return null;
            Press("ShopConfirm", true);
            yield return Until(() => GameObject.Find("ShopMotion") == null, "Kostüm alımı animasyonu");
            if (!ledger.Owns(ShopItem.ExplorerCat) || ledger.Presentation.coins != gold ||
                ledger.Presentation.gems != gems - config.explorerCatPrice)
                throw new InvalidOperationException("Kaşif Kedi alış kaydı uyuşmuyor.");
            Press("ShopAction_ExplorerCat", true);
            yield return Until(() => GameObject.Find("ShopMotion") == null, "Kuşanma animasyonu");
            if (ledger.Equipped(ShopCategory.Cats) != ShopItem.ExplorerCat)
                throw new InvalidOperationException("Kaşif Kedi kuşanılmadı.");
            checks.Add("Kaşif Kedi elmasla alındı ve izole profilde kuşanıldı.");
            yield return ReviewShot("CostumeEquipped", false);
            yield return OpenShopTab("Offers");
            yield return ReviewShot("LeftOpen", false);
        }

        IEnumerator ReviewShot(string name, bool inspectText = true)
        {
            yield return new WaitForEndOfFrame();
            string file = ReviewOutput + name + ".png";
            var previous = File.GetLastWriteTimeUtc(file);
            ScreenCapture.CaptureScreenshot(file);
            yield return Until(() => File.Exists(file) && new FileInfo(file).Length > 0 &&
                File.GetLastWriteTimeUtc(file) != previous, name + " Game View");
            if (!inspectText) yield break;
            Canvas.ForceUpdateCanvases();
            var corners = new Vector3[4];
            foreach (var label in FindObjectsByType<TMP_Text>(FindObjectsSortMode.None))
            {
                if (!label.isActiveAndEnabled || label.alpha < .02f) continue;
                label.rectTransform.GetWorldCorners(corners);
                var min = RectTransformUtility.WorldToScreenPoint(null, corners[0]);
                var max = RectTransformUtility.WorldToScreenPoint(null, corners[2]);
                if (max.x < 0 || max.y < 0 || min.x > Screen.width || min.y > Screen.height) continue;
                label.ForceMeshUpdate();
                if (label.isTextOverflowing)
                    reviewOverflow.Add(new { screen = name, label = label.transform.parent.name + "/" + label.name,
                        text = label.text });
            }
            checks.Add(name + " gerçek Game View");
        }

        IEnumerator Measure(string screen)
        {
            const int samples = 45;
            using var cpu = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 1);
            using var gpuDraws = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count", 1);
            using var batches = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Batches Count", 1);
            var frameMs = new double[samples];
            var draws = new long[samples];
            var batch = new long[samples];
            for (int i = 0; i < samples; i++)
            {
                yield return null;
                frameMs[i] = cpu.LastValue / 1_000_000d;
                draws[i] = gpuDraws.LastValue;
                batch[i] = batches.LastValue;
            }
            Array.Sort(frameMs);
            Array.Sort(draws);
            Array.Sort(batch);
            reviewPerformance.Add(new { screen, samples, mainThreadP95Ms = cpu.Valid ? frameMs[42] : (double?)null,
                drawCallsMedian = gpuDraws.Valid ? draws[22] : (long?)null,
                batchesMedian = batches.Valid ? batch[22] : (long?)null,
                usedBytes = Profiler.GetTotalAllocatedMemoryLong(), reservedBytes = Profiler.GetTotalReservedMemoryLong(),
                loadedTextureBytes = Texture.currentTextureMemory });
        }
    }
}
#endif
