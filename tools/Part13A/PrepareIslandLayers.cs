using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

// Üretim hazırlığı; mevcut sahnelere otomatik bağlanmaz, sanat içermez.
public static class Part13AEditor
{
    public static readonly string[] Layers = {
        "Background", "IslandBase", "Architecture", "Vegetation",
        "WaterOrSpecialEffects", "Decorations", "LevelNodes", "Rewards", "ForegroundEffects"
    };
    const string Template = "Assets/Prefabs/Map/IslandLayerTemplate.prefab";
    public static void Prepare()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Önce Play Mode durdurulmalı.");
        Directory.CreateDirectory("Assets/Prefabs/Map");
        AssetDatabase.Refresh();
        if (File.Exists(Template)) { Validate(); return; }
        var root = new GameObject("IslandLayerTemplate", typeof(RectTransform));
        try
        {
            var rect = root.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(620, 620);
            rect.pivot = new Vector2(.5f, 0);
            foreach (var layer in Layers)
            {
                var child = new GameObject(layer, typeof(RectTransform)).GetComponent<RectTransform>();
                child.SetParent(rect, false);
                child.anchorMin = Vector2.zero; child.anchorMax = Vector2.one;
                child.offsetMin = child.offsetMax = Vector2.zero;
            }
            PrefabUtility.SaveAsPrefabAsset(root, Template);
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
        Validate();
    }
    public static void Validate()
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Template);
        if (prefab == null || prefab.transform.childCount != Layers.Length)
            throw new InvalidOperationException("Katman şablonu eksik.");
        for (int i = 0; i < Layers.Length; i++)
            if (prefab.transform.GetChild(i).name != Layers[i])
                throw new InvalidOperationException("Katman sırası yanlış: " + Layers[i]);
        if (prefab.GetComponentsInChildren<Renderer>(true).Length != 0 ||
            prefab.GetComponentsInChildren<UnityEngine.UI.Graphic>(true).Length != 0)
            throw new InvalidOperationException("Hazırlık şablonunda yer tutucu sanat olmamalı.");
        var data = Mewgic.Map.MapProgressionCatalog.Definition();
        if (data.regions.Length != 12 || data.regions.Where((r, i) => r.firstLevel != i * 50 + 1 || r.levelCount != 50 || r.artReady != (i < 4)).Any())
            throw new InvalidOperationException("12 x 50 katalog veya korunan sanat durumu değişmiş.");
        File.WriteAllText("docs/ProductionReports/Part13/STRUCTURE_TESTS.json",
            "{\"success\":true,\"layers\":9,\"islands\":12,\"levelsPerIsland\":50,\"placeholderRenderers\":0}");
    }
}
