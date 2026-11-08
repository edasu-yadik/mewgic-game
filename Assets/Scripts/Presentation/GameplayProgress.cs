using L=Mewgic.Localization.L;
using System;using Mewgic.Core;using UnityEngine;
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
        public static ProgressLedger Open(string key,Func<DateTimeOffset> clock=null)
        {
            string primary=PlayerPrefs.GetString(key,"");
            string pendingKey=key+".pending", backupKey=key+".backup", previousKey=key+".previous";
            string pending=PlayerPrefs.GetString(pendingKey,"");
            var candidates=new[]{primary,pending,PlayerPrefs.GetString(backupKey,""),PlayerPrefs.GetString(previousKey,"")};
            ProgressData data=null;
            string head="";
            bool foundAny=false, primaryValid=false;
            for(int i=0;i<candidates.Length;i++)
            {
                string candidate=candidates[i];
                string slot=i==0?key:i==1?pendingKey:i==2?backupKey:previousKey;
                if(!PlayerPrefs.HasKey(slot))continue;
                foundAny=true;
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
            else if(primary!=head||PlayerPrefs.HasKey(pendingKey))
            {
                if(primaryValid&&primary!=head)PlayerPrefs.SetString(previousKey,primary);
                else if(!primaryValid&&PlayerPrefs.HasKey(key))PlayerPrefs.SetString(key+".corrupt",primary);
                Promote(key,pendingKey,backupKey,head);
                if(key==StorageKey)RecoveryNotice="Doğrulanmış son kayıt kopyasından kurtarıldı.";
            }

            long revision=data.storageRevision;
            string expectedPrimary=PlayerPrefs.GetString(key,""), expectedPending=PlayerPrefs.GetString(pendingKey,"");
            return new ProgressLedger(data,copy=>
            {
                if(PlayerPrefs.GetString(key,"")!=expectedPrimary||PlayerPrefs.GetString(pendingKey,"")!=expectedPending)
                    throw new InvalidOperationException("Kayıt başka bir oturumda değişti; yeniden yüklenmeli.");

                copy.storageRevision=checked(revision+1);
                string next=JsonUtility.ToJson(copy);
                if(ReadCandidate(next)==null)throw new InvalidOperationException("Kayıt doğrulanamadı.");

                // The flushed journal is the commit point. Recovery may finish promotion after a crash.
                PlayerPrefs.SetString(pendingKey,next);
                try{PlayerPrefs.Save();}
                catch
                {
                    if(expectedPending.Length==0)PlayerPrefs.DeleteKey(pendingKey);
                    else PlayerPrefs.SetString(pendingKey,expectedPending);
                    throw;
                }

                Promote(key,pendingKey,backupKey,next,previousKey,head);
                revision=copy.storageRevision;head=next;
                expectedPrimary=PlayerPrefs.GetString(key,"");expectedPending=PlayerPrefs.GetString(pendingKey,"");
            },clock);
        }

        static void Promote(string key,string pendingKey,string backupKey,string document,string previousKey=null,string old=null)
        {
            try
            {
                if(!string.IsNullOrEmpty(old))PlayerPrefs.SetString(previousKey,old);
                PlayerPrefs.SetString(key,document);
                PlayerPrefs.SetString(backupKey,document);
                PlayerPrefs.DeleteKey(pendingKey);
                PlayerPrefs.Save();
            }
            catch(Exception e)
            {
                // A failed mirror flush cannot undo a committed journal.
                try{PlayerPrefs.SetString(pendingKey,document);}catch(PlayerPrefsException){}
                Debug.LogWarning("Kayıt aynası henüz yazılamadı: "+e.GetType().Name);
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
                foreach(string field in new[]{"worldMap","presentation","daily","hub","shop"})
                    if(json[field] is Newtonsoft.Json.Linq.JObject block)CheckVersion(block["version"]);
                foreach(string field in new[]{"version","undo","hint","extra","shuffle"})
                    if(!Integer(json[field]))return null;
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
