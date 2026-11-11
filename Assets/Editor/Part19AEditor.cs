using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Mewgic;
using Mewgic.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class Part19AEditor
{
    const string Output="docs/ProductionReports/Part19A";

    public static void Audit() => Audit(Output);

    public static void Audit(string output)
    {
        var references=new List<string>();
        var scenePaths=AssetDatabase.FindAssets("t:Scene",new[]{"Assets"})
            .Select(AssetDatabase.GUIDToAssetPath).OrderBy(path=>path).ToArray();
        var build=new HashSet<string>(EditorBuildSettings.scenes.Where(s=>s.enabled).Select(s=>s.path));
        var sceneResults=new List<object>();
        var missingGuid=new Regex(@"\{fileID: (-?\d+), guid: ([0-9a-f]{32}), type: \d+\}");
        foreach(string path in scenePaths)
        {
            var scene=EditorSceneManager.OpenPreviewScene(path);
            int objects=0,components=0;
            try
            {
                foreach(var root in scene.GetRootGameObjects())
                foreach(var transform in root.GetComponentsInChildren<Transform>(true))
                {
                    objects++;
                    foreach(var component in transform.GetComponents<Component>())
                    {
                        if(component==null){references.Add(path+" / "+transform.name+" / Missing Script");continue;}
                        components++;
                        var property=new SerializedObject(component).GetIterator();
                        while(property.Next(true))
                            if(property.propertyType==SerializedPropertyType.ObjectReference&&
                                property.objectReferenceValue==null&&property.objectReferenceInstanceIDValue!=0)
                                references.Add(path+" / "+transform.name+" / "+property.propertyPath);
                    }
                }
            }
            finally{EditorSceneManager.ClosePreviewScene(scene);}
            foreach(Match match in missingGuid.Matches(File.ReadAllText(path)))
            {
                string id=match.Groups[2].Value;
                if(!id.StartsWith("0000000000000000")&&string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(id)))
                    references.Add(path+" / çözülemeyen GUID "+id);
            }
            sceneResults.Add(new{path,build=build.Contains(path),objects,components,dependencies=AssetDatabase.GetDependencies(path).Length});
        }
        var packageNames=JObject.Parse(File.ReadAllText("Packages/manifest.json"))["dependencies"];
        var locked=JObject.Parse(File.ReadAllText("Packages/packages-lock.json"))["dependencies"];
        var packages=((JObject)packageNames).Properties().Select(p=>new{name=p.Name,declared=p.Value.ToString(),resolved=(string)locked[p.Name]?["version"]}).ToArray();
        var atlases=AssetDatabase.FindAssets("t:SpriteAtlas",new[]{"Assets"}).Select(AssetDatabase.GUIDToAssetPath).ToArray();
        var files=AssetDatabase.FindAssets("t:Texture2D",new[]{"Assets"}).Select(AssetDatabase.GUIDToAssetPath)
            .Where(p=>File.Exists(p)&&AssetImporter.GetAtPath(p) is TextureImporter)
            .OrderByDescending(p=>new FileInfo(p).Length).Take(18);
        var textures=files.Select(p=>
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath(p);
            var android=importer.GetPlatformTextureSettings("Android");
            var ios=importer.GetPlatformTextureSettings("iPhone");
            importer.GetSourceTextureWidthAndHeight(out int width,out int height);
            return new{path=p,width,height,bytes=new FileInfo(p).Length,readable=importer.isReadable,mips=importer.mipmapEnabled,
                android=android.overridden?android.format.ToString():"Default",ios=ios.overridden?ios.format.ToString():"Default"};
        }).ToArray();
        var addressableSettings = UnityEditor.AddressableAssets.AddressableAssetSettingsDefaultObject.Settings;
        var addressableRoots = addressableSettings == null ? Array.Empty<string>() : addressableSettings.groups
            .Where(group => group != null).SelectMany(group => group.entries)
            .Select(entry => entry.AssetPath).Where(path => !string.IsNullOrEmpty(path)).Distinct().ToArray();
        var allPaths = AssetDatabase.GetAllAssetPaths().Where(path => path.StartsWith("Assets/") && File.Exists(path)).ToArray();
        var possibleRuntimeRoots = allPaths.Where(path => path.Contains("/Resources/") ||
            path.StartsWith("Assets/StreamingAssets/") || addressableRoots.Any(root =>
                path == root || path.StartsWith(root + "/")));
        var roots = build.Concat(possibleRuntimeRoots).Concat(atlases)
            .Concat(PlayerSettings.GetPreloadedAssets().Where(asset => asset != null).Select(AssetDatabase.GetAssetPath))
            .Distinct().ToArray();
        var reachable = new HashSet<string>(AssetDatabase.GetDependencies(roots, true));
        foreach (string path in reachable.Where(path => path.StartsWith("Assets/") && path.EndsWith(".prefab") && File.Exists(path)))
            foreach (Match match in missingGuid.Matches(File.ReadAllText(path)))
            {
                string id = match.Groups[2].Value;
                if (!id.StartsWith("0000000000000000") && string.IsNullOrEmpty(AssetDatabase.GUIDToAssetPath(id)))
                    references.Add(path + " / çözülemeyen GUID " + id);
            }
        var heavyCandidates = allPaths.Where(path => !reachable.Contains(path) &&
                (AssetImporter.GetAtPath(path) is TextureImporter || path.EndsWith(".psd", StringComparison.OrdinalIgnoreCase)))
            .Select(path => new { path, bytes = new FileInfo(path).Length }).Where(file => file.bytes >= 1_000_000)
            .OrderByDescending(file => file.bytes).Take(25).ToArray();
        Directory.CreateDirectory(output);
        File.WriteAllText(Path.Combine(output, "PROJECT_AUDIT.json"), JsonConvert.SerializeObject(new
        {
            unity = Application.unityVersion, scenes = sceneResults, references = references.Distinct(),
            packages, atlases, textures, runtimeRoots = roots.Length, resolvedDependencies = reachable.Count, heavyCandidates
        }, Formatting.Indented));
    }

    public static void StorageChecks()
    {
        var results=new List<string>();
        string key="Mewgic.Tests.Part19A."+Guid.NewGuid().ToString("N");
        string dir=Path.Combine(Path.GetTempPath(),key);
        var clock=new DateTimeOffset(2026,11,4,10,0,0,TimeSpan.Zero);
        ProgressLedger Open()=>GameplayProgress.Open(key,()=>clock);
        void Check(bool pass,string label){if(!pass)throw new InvalidOperationException(label);results.Add(label);}
        const string Legacy="{\"version\":1,\"undo\":3,\"hint\":7,\"extra\":1,\"shuffle\":1,\"levels\":[{\"level\":1,\"bestStars\":2,\"rewardClaimed\":true}],\"presentation\":{\"version\":1,\"coins\":123}}";
        try
        {
            PlayerPrefs.SetString(key,Legacy);
            var ledger=Open();
            Check(ledger.Completed(1)&&ledger.BestStars(1)==2&&ledger.Presentation.coins==123&&ledger.Remaining(BoosterKind.Hint)==7,"Eski kayıt yükseltmede yıldız, coin ve güç stokunu korudu");
            var stale=Open();
            Check(ledger.Claim(1,3,true,out _)&&ledger.LastCoinReward==5,"Eski kayıt yıldız farkını doğru işledi");
            Check(!stale.BuyBooster(BoosterKind.Hint)&&Open().Presentation.coins==128,"Eski açık oturum yeni kaydı ezemedi");
            string previous=PlayerPrefs.GetString(key+".previous");
            Check(ledger.ClaimDailyGift(),"Çevrimdışı sandık kaydı");
            string newer=PlayerPrefs.GetString(key);
            var generation=JObject.Parse(newer);
            Check(generation.Value<long>("storageRevision")==2,"Kayıt nesli artıyor");
            PlayerPrefs.SetString(key+".pending",newer);PlayerPrefs.SetString(key,previous);PlayerPrefs.SetString(key+".backup",previous);
            var recovered=Open();
            Check(recovered.Presentation.coins==228&&!recovered.ClaimDailyGift(),"Yarım ayna yazımından sonra güncel coin ve sandık işareti kurtarıldı");
            PlayerPrefs.SetString(key,"{broken");
            Check(Open().Presentation.coins==228&&PlayerPrefs.GetString(key+".corrupt")=="{broken","Bozuk ana kayıt aynadan döndü ve bozuk veri saklandı");
            PlayerPrefs.SetString(key,"{}");PlayerPrefs.SetString(key+".backup","{}");
            Check(Open().Presentation.coins==123,"Her iki son kopya bozukken önceki geçerli nesil kullanıldı");
            PlayerPrefs.SetString(key,Legacy.Replace("123","\"123\""));
            PlayerPrefs.SetString(key+".backup","{}");PlayerPrefs.SetString(key+".previous","{}");
            bool blocked=false;try{Open();}catch(InvalidOperationException){blocked=true;}
            Check(blocked,"Yanlış JSON tipleri boş kayıt olarak kabul edilmedi");
            PlayerPrefs.SetString(key,Legacy.Replace("\"version\":1","\"version\":2"));
            PlayerPrefs.SetString(key+".backup",Legacy);
            blocked=false;try{Open();}catch(NotSupportedException){blocked=true;}
            Check(blocked&&PlayerPrefs.GetString(key).Contains("\"version\":2"),"Gelecek sürüm eski yedekle ezilmedi");
            var limit=new ProgressLedger(new ProgressData(),_=>{},()=>clock);
            Check(limit.SetResources(long.MaxValue,0,5)&&!limit.ClaimDailyGift()&&!limit.Claim(1,3,true,out _)&&limit.Presentation.coins==long.MaxValue,"Coin üst sınırında ödül ve sandık atomik kaldı");
            Check(limit.BuyBooster(BoosterKind.Hint)&&limit.Presentation.coins==long.MaxValue-40,"Üst sınırdan harcama tam sayı hassasiyetini korudu");

            var request=WorldMapRules.Request(int.MaxValue);request.Validate();
            var pool=JsonConvert.DeserializeObject<GeneratedLevel[]>(Resources.Load<TextAsset>("Levels/GeneratedFallbacksV2").text);
            var high=LevelGenerator.Resolve(request,null,pool).level;
            Check(high!=null&&high.level==int.MaxValue,"int.MaxValue bölüm kimliği üretildi");
            high.Validate();
            var archive=new FileLevelArchive(dir);
            archive.Write(high);
            string path=Path.Combine(dir,"level-"+int.MaxValue+".json");
            string exact=File.ReadAllText(path);
            File.WriteAllText(path,"{}");
            Check(archive.TryRead(int.MaxValue,out var archived)&&archived.signature==high.signature,"Bozuk bölüm dosyası yedekten özgün imzayı korudu");
            File.Move(path+".backup",path+".tmp");
            Check(archive.TryRead(int.MaxValue,out archived)&&archived.signature==high.signature,"Yarım bölüm yayımlaması doğrulanmış geçici kopyadan okunuyor");
            File.WriteAllText(path+".tmp","{}");
            blocked=false;try{archive.TryRead(int.MaxValue,out _);}catch(InvalidDataException){blocked=true;}
            Check(blocked&&File.ReadAllText(path)=="{}"&&exact.Length>10,"Yayımlanmış bölüm kopyaları bozuksa yeniden üretim engelleniyor");
            Write("STORAGE_TESTS.json",new{success=true,results});
        }
        catch(Exception e){Write("STORAGE_TESTS.json",new{success=false,results,failure=e.ToString()});throw;}
        finally
        {
            foreach(var suffix in new[]{"",".pending",".backup",".previous",".corrupt"})PlayerPrefs.DeleteKey(key+suffix);
            PlayerPrefs.Save();
            if(Directory.Exists(dir))Directory.Delete(dir,true);
        }
    }

    static void Write(string name,object result)
    {
        Directory.CreateDirectory(Output);
        File.WriteAllText(Path.Combine(Output,name),JsonConvert.SerializeObject(result,Formatting.Indented));
    }
}
