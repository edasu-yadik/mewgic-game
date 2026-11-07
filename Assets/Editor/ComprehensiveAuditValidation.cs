using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Mewgic;
using Mewgic.Map;
using Newtonsoft.Json;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static class ComprehensiveAuditValidation
{
    const string Output = "docs/ProductionReports/ComprehensiveAudit";

    [MenuItem("Mewgic/Comprehensive Audit/Run Crystal Map QA")]
    public static void RunMap() => MapFlowProbe.Begin(Output + "/Map", captureMotion: true);

    [MenuItem("Mewgic/Comprehensive Audit/Run Gameplay and Daily QA")]
    public static void RunGameplay() => GameplayPolishProbe.Begin(output: Output + "/Gameplay");

    [MenuItem("Mewgic/Comprehensive Audit/Validate Bindings and Save Rules")]
    public static void RunModels()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Önce Play Mode durdurulmalı");
        Directory.CreateDirectory(Output);
        ValidateBindings();
        MapFlowValidation.Models(Output + "/Map");
        GameplayPolishValidation.Models(Output + "/Gameplay");
        GameplayPolishValidation.LegacyModels(Output + "/Gameplay");
    }

    static void ValidateBindings()
    {
        var checks = new List<string>();
        var errors = new List<string>();
        void Check(bool condition, string label) => (condition ? checks : errors).Add(label);
        void ValidateHierarchy(GameObject root)
        {
            foreach (var child in root.GetComponentsInChildren<Transform>(true))
            {
                Check(GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(child.gameObject) == 0,
                    child.gameObject.scene.name + "/" + child.name + " script bağlantısı");
                foreach (var component in child.GetComponents<Component>())
                {
                    if (component == null) continue;
                    var property = new SerializedObject(component).GetIterator();
                    while (property.NextVisible(true))
                    {
                        if (property.propertyType == SerializedPropertyType.ObjectReference &&
                            property.objectReferenceValue == null && property.objectReferenceInstanceIDValue != 0)
                            errors.Add(child.name + "/" + component.GetType().Name + "/" + property.propertyPath);
                    }
                }
            }
        }

        foreach (var name in new[] { "Boot", "Home", "Gameplay", "LevelMap" })
        {
            string path = "Assets/Scenes/" + name + ".unity";
            Check(EditorBuildSettings.scenes.Any(entry => entry.enabled && entry.path == path), name + " derleme sahnesi");
            var scene = EditorSceneManager.OpenPreviewScene(path);
            try
            {
                var roots = scene.GetRootGameObjects();
                foreach (var root in roots) ValidateHierarchy(root);
                if (name == "LevelMap")
                {
                    var map = roots.SelectMany(root => root.GetComponentsInChildren<MapWorld>(true)).Single();
                    Check(map.sky && map.node && map.chest && map.fog && map.waterLightShader &&
                        map.terrain.Length >= 4 && map.terrain.Take(4).All(art => art != null), "LevelMap görsel ve shader bağları");
                    var serialized = new SerializedObject(map);
                    Check(serialized.FindProperty("journeyCatPoses").objectReferenceValue != null, "LevelMap yürüyüş atlası");
                    var terraces = serialized.FindProperty("crystalTerraces");
                    Check(terraces.arraySize == 5 && Enumerable.Range(0, 5).All(i => terraces.GetArrayElementAtIndex(i).objectReferenceValue),
                        "Kristal'in beş teras referansı");
                }
                if (name == "Gameplay")
                {
                    var gameplay = roots.SelectMany(root => root.GetComponentsInChildren<GameplayPresentation>(true)).Single();
                    var serialized = new SerializedObject(gameplay);
                    foreach (var field in new[] { "wizardCatPrefab", "backdrop", "foreground", "shelf", "waterfallShader", "waterfallMask" })
                        Check(serialized.FindProperty(field)?.objectReferenceValue != null, "Gameplay." + field);
                }
            }
            finally { EditorSceneManager.ClosePreviewScene(scene); }
        }

        foreach (string guid in AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Map", "Assets/Prefabs/Characters" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Check(prefab != null, path);
            if (prefab != null) ValidateHierarchy(prefab);
        }

        var crystal = Resources.Load<MapJourneyTheme>("Map/JourneyThemes/crystal");
        Check(crystal && crystal.referenceTerrace && crystal.mascotRestAtlas, "Kristal referans çizimi ve uyku atlası");
        Check(Resources.Load<Sprite>("Daily/QuestIcon_v001") != null, "Günlük görev parşömen sprite'ı");
        Check(Resources.Load<Sprite>("Gameplay/Gameplay_GlassShell_v001") != null, "Şişe cam sprite'ı");
        var shader = Shader.Find("Mewgic/MapWaterLight");
        Check(shader && !ShaderUtil.ShaderHasError(shader), "Harita su shader'ı");
        File.WriteAllText(Output + "/BINDINGS.json", JsonConvert.SerializeObject(new
        {
            success = errors.Count == 0, editor = Application.unityVersion, count = checks.Count, checks, errors
        }, Formatting.Indented));
        if (errors.Count > 0) throw new InvalidOperationException(string.Join(Environment.NewLine, errors));
    }
}
