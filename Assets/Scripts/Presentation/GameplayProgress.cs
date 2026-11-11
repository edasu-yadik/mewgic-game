using L=Mewgic.Localization.L;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System;using System.Linq;using Mewgic.Core;using UnityEngine;
namespace Mewgic
{
    public static class GameplayProgress
    {
        public const string StorageKey="Mewgic.Progress.v1";
        static ProgressLedger current;
        public static string LoadError {get;private set;}
#if UNITY_EDITOR
        public static bool IsTestProfile {get;private set;}
#else
        public static bool IsTestProfile => false;
#endif
        public static ProgressLedger Current
        {
            get
            {
                if(current!=null)return current;
                try{current=Open(StorageKey);LoadError=null;if(!current.Presentation.motionPreferenceSet&&PlayerPrefs.GetInt("Mewgic.Map.ReducedMotion",0)==1)current.SetReducedMotion(true);}
                catch(Exception e){LoadError=e.Message;current=new ProgressLedger(new ProgressData{undo=0,hint=0,extra=0,shuffle=0},_=>throw new InvalidOperationException(L.V("error.load")));}
                return current;
            }
        }
        public static ProgressLedger Open(string key,Func<DateTimeOffset> clock=null,string directory=null)
        {
            var slots=new ProgressSlots(key,directory??Path.Combine(Application.persistentDataPath,"Progress"));
            string primary=slots.Read(key);
            string pendingKey=key+".pending", backupKey=key+".backup", previousKey=key+".previous";
            string pending=slots.Read(pendingKey);
            var candidates=new[]{primary,pending,slots.Read(backupKey),slots.Read(previousKey)};
            ProgressData data=null;
            string head="";
            bool foundAny=false, primaryValid=false;
            var seen=new System.Collections.Generic.HashSet<string>(StringComparer.Ordinal);
            for(int i=0;i<candidates.Length;i++)
            {
                string candidate=candidates[i];
                string slot=i==0?key:i==1?pendingKey:i==2?backupKey:previousKey;
                if(!slots.Exists(slot))continue;
                foundAny=true;
                if(!seen.Add(candidate))continue;
                var parsed=ReadCandidate(candidate);
                if(i==0)primaryValid=parsed!=null;
                if(parsed==null||(data!=null&&parsed.storageRevision<=data.storageRevision))continue;
                data=parsed;head=candidate;
            }

            if(data==null)
            {
                if(foundAny)throw new InvalidOperationException(L.V("error.load"));
                data=new ProgressData();
            }
            else if(primary!=head||slots.Exists(pendingKey))
            {
                if(primaryValid&&primary!=head)slots.Write(previousKey,primary);
                else if(!primaryValid&&slots.Exists(key))slots.Write(key+".corrupt",primary);
                Promote(slots,key,pendingKey,backupKey,head);
                if(key==StorageKey)RecoveryNotice="Doğrulanmış son kayıt kopyasından kurtarıldı.";
            }

            long revision=data.storageRevision;
            string expectedPrimary=slots.Read(key), expectedPending=slots.Read(pendingKey);
            return new ProgressLedger(data,copy=>
            {
                if(slots.Read(key)!=expectedPrimary||slots.Read(pendingKey)!=expectedPending)
                    throw new InvalidOperationException("Kayıt başka bir oturumda değişti; yeniden yüklenmeli.");

                copy.storageRevision=checked(revision+1);
                string next=JsonUtility.ToJson(copy);
                if(ReadCandidate(next)==null)throw new InvalidOperationException("Kayıt doğrulanamadı.");

                // The flushed journal is the commit point. Recovery may finish promotion after a crash.
                slots.BeginCommit(pendingKey,next,expectedPending);

                Promote(slots,key,pendingKey,backupKey,next,previousKey,head);
                revision=copy.storageRevision;head=next;
                expectedPrimary=slots.Read(key);expectedPending=slots.Read(pendingKey);
            },clock);
        }

        static void Promote(ProgressSlots slots,string key,string pendingKey,string backupKey,string document,string previousKey=null,string old=null)
        {
            try
            {
                if(!string.IsNullOrEmpty(old))slots.Write(previousKey,old);
                slots.Write(key,document);
                slots.Write(backupKey,document);
                slots.ClearJournal(pendingKey);
                slots.Flush();
            }
            catch(Exception e)
            {
                // A failed mirror flush cannot undo a committed journal.
                try{slots.Write(pendingKey,document);}catch(Exception){}
                Debug.LogWarning("Kayıt aynası henüz yazılamadı: "+e.GetType().Name);
            }
        }

        sealed class ProgressSlots
        {
            const int FileThreshold=128*1024;
            readonly string key, prefix;
            bool fileBacked;

            internal ProgressSlots(string key,string directory)
            {
                this.key=key;
                using var hash=SHA256.Create();
                string id=BitConverter.ToString(hash.ComputeHash(Encoding.UTF8.GetBytes(key))).Replace("-","");
                prefix=Path.Combine(directory,id);
                fileBacked=File.Exists(PathFor(key))||File.Exists(PathFor(key+".pending"))||
                    File.Exists(PathFor(key+".backup"))||File.Exists(PathFor(key+".previous"));
            }

            string PathFor(string slot)=>prefix+slot.Substring(key.Length)+".json";
            internal bool Exists(string slot)=>File.Exists(PathFor(slot))||PlayerPrefs.HasKey(slot);
            internal string Read(string slot)
            {
                string path=PathFor(slot);
                return File.Exists(path)?File.ReadAllText(path):PlayerPrefs.GetString(slot,"");
            }

            internal void BeginCommit(string slot,string next,string previous)
            {
                fileBacked|=next.Length>=FileThreshold;
                if(fileBacked)
                {
                    Write(slot,next);
                    return;
                }
                PlayerPrefs.SetString(slot,next);
                try{PlayerPrefs.Save();}
                catch
                {
                    if(previous.Length==0)PlayerPrefs.DeleteKey(slot);
                    else PlayerPrefs.SetString(slot,previous);
                    throw;
                }
            }

            internal void Write(string slot,string data)
            {
                if(!fileBacked)
                {
                    PlayerPrefs.SetString(slot,data);
                    return;
                }
                string path=PathFor(slot);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                string temporary=path+".tmp";
                using(var stream=new FileStream(temporary,FileMode.Create,FileAccess.Write,FileShare.None))
                {
                    using(var writer=new StreamWriter(stream,new UTF8Encoding(false),4096,true))
                        writer.Write(data);
                    stream.Flush(true);
                }
                if(File.Exists(path))File.Replace(temporary,path,null);
                else File.Move(temporary,path);
            }

            internal void ClearJournal(string slot)
            {
                if(fileBacked)File.Delete(PathFor(slot));
                if(PlayerPrefs.HasKey(slot))
                {
                    PlayerPrefs.DeleteKey(slot);
                    if(fileBacked)PlayerPrefs.Save();
                }
            }

            internal void Flush()
            {
                if(!fileBacked)PlayerPrefs.Save();
            }
        }

        public static string RecoveryNotice {get;private set;}

        static ProgressData ReadCandidate(string raw)
        {
            if(string.IsNullOrWhiteSpace(raw))return null;
            try
            {
                var json=Newtonsoft.Json.Linq.JObject.Parse(raw,new Newtonsoft.Json.Linq.JsonLoadSettings
                {DuplicatePropertyNameHandling=Newtonsoft.Json.Linq.DuplicatePropertyNameHandling.Error});
                CheckVersion(json["version"]);
                foreach(string field in new[]{"worldMap","presentation","daily","hub","shop","leagueWallet"})
                    if(json[field] is Newtonsoft.Json.Linq.JObject block)CheckVersion(block["version"]);
                foreach(string field in new[]{"version","undo","hint","extra","shuffle"})
                    if(!Integer(json[field]))return null;
                if(json["colorSwap"]!=null&&!Integer(json["colorSwap"]))return null;
                if(json["levels"] is not Newtonsoft.Json.Linq.JArray levels)return null;
                foreach(var level in levels)
                    if(!Integer(level["level"])||!Integer(level["bestStars"])||
                        level["rewardClaimed"]?.Type!=Newtonsoft.Json.Linq.JTokenType.Boolean)return null;
                if(json["storageRevision"]!=null&&!Integer(json["storageRevision"]))return null;
                if(json["presentation"] is Newtonsoft.Json.Linq.JObject presentation&&
                    ((presentation["coins"]!=null&&!Integer(presentation["coins"]))||
                    (presentation["gems"]!=null&&!Integer(presentation["gems"]))))return null;
                if(json["shop"]!=null && (json["shop"] is not Newtonsoft.Json.Linq.JObject shop ||
                    !Integer(shop["version"]) || !Integer(shop["purchaseRevision"]) ||
                    !long.TryParse(shop["purchaseRevision"].ToString(), out _) ||
                    !IntegerList(shop["owned"]) || !IntegerList(shop["equipped"])))return null;
                if(json["leagueWallet"]!=null && (json["leagueWallet"] is not Newtonsoft.Json.Linq.JObject wallet ||
                    !Integer(wallet["version"]) || (wallet["walletId"]!=null && wallet["walletId"].Type is not (Newtonsoft.Json.Linq.JTokenType.String or Newtonsoft.Json.Linq.JTokenType.Null)) || wallet["receipts"] is not Newtonsoft.Json.Linq.JArray receipts ||
                    receipts.Any(value=>value.Type!=Newtonsoft.Json.Linq.JTokenType.String)))return null;
                var data=JsonUtility.FromJson<ProgressData>(raw);
                if(data==null||data.storageRevision<0)return null;
                return new ProgressLedger(data,_=>{}).Snapshot;
            }
            catch(NotSupportedException){throw;}
            catch(Exception){return null;}
        }

        static bool IntegerList(Newtonsoft.Json.Linq.JToken value)
        {
            if(value is not Newtonsoft.Json.Linq.JArray array)return false;
            foreach(var entry in array)
                if(!Integer(entry)||!int.TryParse(entry.ToString(),out int id)||!ShopCatalog.Known((ShopItem)id))return false;
            return true;
        }

        static bool Integer(Newtonsoft.Json.Linq.JToken value)=>value?.Type==Newtonsoft.Json.Linq.JTokenType.Integer;
        static void CheckVersion(Newtonsoft.Json.Linq.JToken value)
        {
            if(Integer(value)&&decimal.TryParse(value.ToString(),out var version)&&version>1)
                throw new NotSupportedException("Yeni kayıt sürümü; daha güncel uygulama gerekli.");
        }

#if UNITY_EDITOR
        public static void UseTestProfile(ProgressData data=null,Func<DateTimeOffset> clock=null){current=new ProgressLedger(data??new ProgressData(),_=>{},clock);IsTestProfile=true;LoadError=null;}
        public static void RestorePlayerProfile(){current=null;IsTestProfile=false;LoadError=null;RecoveryNotice=null;}
#endif
    }
}
