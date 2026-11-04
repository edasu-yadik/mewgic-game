using L=Mewgic.Localization.L;
using System;using Mewgic.Core;using UnityEngine;
namespace Mewgic
{
    public static class GameplayProgress
    {
        public const string StorageKey="Mewgic.Progress.v1";
        static ProgressLedger current;
        public static string LoadError {get;private set;}
        public static bool IsTestProfile {get;private set;}
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
            string raw=PlayerPrefs.GetString(key,"");
            ProgressData data;
            if(!PlayerPrefs.HasKey(key)&&!PlayerPrefs.HasKey(key+".backup"))data=new ProgressData();
            else
            {
                try{data=ValidateJson(raw);}
                catch(Exception primary)
                {
                    // Never downgrade a future schema through an older backup.
                    try{var future=JsonUtility.FromJson<ProgressData>(raw);if(future!=null&&(future.version>1||(future.worldMap!=null&&future.worldMap.version>1)||(future.presentation!=null&&future.presentation.version>1)||(future.daily!=null&&future.daily.version>1)||(future.hub!=null&&future.hub.version>1)))throw new NotSupportedException("Yeni kayıt sürümü; daha güncel uygulama gerekli.");}catch(NotSupportedException){throw;}catch{}
                    try
                    {
                        data=ValidateJson(PlayerPrefs.GetString(key+".backup",""));
                        PlayerPrefs.SetString(key+".corrupt",raw);
                        PlayerPrefs.SetString(key,JsonUtility.ToJson(data));PlayerPrefs.Save();
                        if(key==StorageKey)RecoveryNotice="Bozuk ana kayıt yerine son doğrulanmış yedek yüklendi.";
                    }
                    catch(Exception recovery){throw new InvalidOperationException(L.V("error.load"),new AggregateException(primary,recovery));}
                }
            }
            return new ProgressLedger(data,copy=>
            {
                bool existed=PlayerPrefs.HasKey(key);string before=existed?PlayerPrefs.GetString(key):null;
                bool hadBackup=PlayerPrefs.HasKey(key+".backup");string backup=PlayerPrefs.GetString(key+".backup","");
                string next=JsonUtility.ToJson(copy);ValidateJson(next);
                try
                {
                    // Flush a validated previous state before replacing the primary document.
                    if(existed){ValidateJson(before);PlayerPrefs.SetString(key+".backup",before);PlayerPrefs.Save();}
                    PlayerPrefs.SetString(key,next);PlayerPrefs.Save();
                    // Identical committed copy: recovery keeps reward markers and balances together.
                    PlayerPrefs.SetString(key+".backup",next);PlayerPrefs.Save();
                }
                catch{if(existed)PlayerPrefs.SetString(key,before);else PlayerPrefs.DeleteKey(key);if(hadBackup)PlayerPrefs.SetString(key+".backup",backup);else PlayerPrefs.DeleteKey(key+".backup");throw;}
            },clock);
        }
        public static string RecoveryNotice {get;private set;}
        static ProgressData ValidateJson(string raw)
        {
            if(string.IsNullOrWhiteSpace(raw)||!raw.TrimStart().StartsWith("{")||!raw.TrimEnd().EndsWith("}")||!raw.Contains("\"version\""))throw new ArgumentException("Eksik kayıt belgesi");
            foreach(string field in new[]{"version","undo","hint","extra","shuffle","levels"})if(!raw.Contains("\""+field+"\""))throw new ArgumentException("Eksik temel kayıt alanı: "+field);
            var data=JsonUtility.FromJson<ProgressData>(raw);return new ProgressLedger(data,_=>{}).Snapshot;
        }

        public static void UseTestProfile(ProgressData data=null,Func<DateTimeOffset> clock=null){current=new ProgressLedger(data??new ProgressData(),_=>{},clock);IsTestProfile=true;LoadError=null;}
        public static void RestorePlayerProfile(){current=null;IsTestProfile=false;LoadError=null;RecoveryNotice=null;}
    }
}
