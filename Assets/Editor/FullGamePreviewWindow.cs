using System;
using Mewgic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public sealed class FullGamePreviewWindow : EditorWindow
{
    const string StartSceneKey = "Mewgic.VisualQA.PreviousScene";
    int level = 1;
    int selected;
    static readonly string[] Screens = { "Ana Menü", "Oyun", "Bölüm Sonu", "Harita", "Özel Teklifler", "Klan", "Lig", "Etkinlikler", "Günlük Görevler" };

    [MenuItem("Mewgic/Visual QA/Open Full Game Preview")]
    public static void Open()
    {
        GetWindow<FullGamePreviewWindow>("MeWGic Visual QA").Show();
        StartPreview(false);
    }

    [MenuItem("Mewgic/Visual QA/Verify Full Game Preview")]
    static void Verify() => StartPreview(true);

    [MenuItem("Mewgic/Visual QA/Run Final Visual Review")]
    public static void Review()
    {
        GetWindow<FullGamePreviewWindow>("MeWGic Visual QA").Show();
        StartPreview(false, true);
    }

    [MenuItem("Mewgic/Visual QA/Verify Continuous Map")]
    static void VerifyMap() => StartPreview(false, map: true);

    [InitializeOnLoadMethod]
    static void RegisterRestore() => EditorApplication.playModeStateChanged += change =>
    {
        if (change != PlayModeStateChange.EnteredEditMode || !SessionState.GetBool(FullGamePreview.SessionKey,false)) return;
        EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>(SessionState.GetString(StartSceneKey,""));
        SessionState.EraseString(StartSceneKey);
        SessionState.EraseBool(FullGamePreview.SessionKey);
        SessionState.EraseBool(FullGamePreview.VerifyKey);
        SessionState.EraseBool(FullGamePreview.ReviewKey);
        SessionState.EraseBool(FullGamePreview.SingleMapKey);
    };

    static void StartPreview(bool verify, bool review = false, bool map = false)
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) return;
        for (int i=0;i<UnityEngine.SceneManagement.SceneManager.sceneCount;i++)
            if (UnityEngine.SceneManagement.SceneManager.GetSceneAt(i).isDirty)
                throw new InvalidOperationException("Önizleme için açık sahne değişikliklerini önce kaydet.");
        EditorSceneManager.OpenScene("Assets/Scenes/Home.unity");
        SessionState.SetString(StartSceneKey,AssetDatabase.GetAssetPath(EditorSceneManager.playModeStartScene));
        SessionState.SetBool(FullGamePreview.SessionKey,true);
        SessionState.SetBool(FullGamePreview.VerifyKey,verify);
        SessionState.SetBool(FullGamePreview.ReviewKey,review);
        SessionState.SetBool(FullGamePreview.SingleMapKey,map);
        EditorSceneManager.playModeStartScene = AssetDatabase.LoadAssetAtPath<SceneAsset>("Assets/Scenes/Home.unity");
        EditorApplication.isPlaying = true;
    }

    void OnInspectorUpdate() => Repaint();

    void OnGUI()
    {
        EditorGUILayout.LabelField("MeWGic Görsel Önizleme",EditorStyles.boldLabel);
        EditorGUILayout.HelpBox("İzole oyuncu kaydı · Yerel klan, lig ve etkinlik verisi · Örnek reklam",MessageType.Info);
        var preview = FullGamePreview.Active;
        if (preview == null)
        {
            using (new EditorGUI.DisabledScope(EditorApplication.isPlayingOrWillChangePlaymode))
                if (GUILayout.Button("Önizlemeyi başlat")) StartPreview(false);
            return;
        }
        EditorGUILayout.LabelField(preview.Status,EditorStyles.wordWrappedLabel);
        level = EditorGUILayout.IntSlider("Test bölümü",level,1,120);
        using (new EditorGUI.DisabledScope(preview.Busy))
        {
            for (int i=0;i<Screens.Length;i++)
            {
                int page=i;
                if (GUILayout.Button($"{i+1}. {Screens[i]}")) Select(page);
            }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Önceki ekran")) Select((selected+8)%9);
                if (GUILayout.Button("Sonraki ekran")) Select((selected+1)%9);
            }
            using (new EditorGUILayout.HorizontalScope())
                for (int stars=1;stars<=3;stars++)
                    if (GUILayout.Button($"{stars} yıldız")) { selected=2; preview.Navigate(2,level,stars); }
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Altın Paketleri")) preview.NavigateShopTab("Gold");
                if (GUILayout.Button("Oyun Güçleri")) preview.NavigateShopTab("Powers");
                if (GUILayout.Button("Kostümler")) preview.NavigateShopTab("Costumes");
            }
            if (GUILayout.Button("Büyülü Köy · 1–10")) preview.Navigate(11,1,3);
            if (GUILayout.Button("Kristal Şelaleler · 91–100")) preview.Navigate(9,100,3);
            if (GUILayout.Button("Kedi yürüyüşü · 99 → 100")) preview.Navigate(10,100,3);
            if (GUILayout.Button("Örnek bakiye ve ilerlemeyi sıfırla")) preview.ResetPreview();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("9:16")) FullGamePreview.Size(1600);
                if (GUILayout.Button("9:19.5")) FullGamePreview.Size(1950);
                if (GUILayout.Button("9:20")) FullGamePreview.Size(2000);
            }
        }
        if (GUILayout.Button("Önizlemeyi durdur")) EditorApplication.isPlaying = false;

        void Select(int page)
        {
            selected=page;
            preview.Navigate(page,level,3);
        }
    }
}
