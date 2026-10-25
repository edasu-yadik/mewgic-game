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
                try{current=Open(StorageKey);LoadError=null;}
                catch(Exception e){LoadError=e.Message;current=new ProgressLedger(new ProgressData{undo=0,hint=0,extra=0,shuffle=0},_=>throw new InvalidOperationException("Kayıt okunamadı; özgün kayıt korundu."));}
                return current;
            }
        }
        public static ProgressLedger Open(string key,Func<DateTimeOffset> clock=null)
        {
            var data=PlayerPrefs.HasKey(key)?JsonUtility.FromJson<ProgressData>(PlayerPrefs.GetString(key)):new ProgressData();
            return new ProgressLedger(data,copy=>
            {
                bool existed=PlayerPrefs.HasKey(key);string before=existed?PlayerPrefs.GetString(key):null;
                try{PlayerPrefs.SetString(key,JsonUtility.ToJson(copy));PlayerPrefs.Save();}
                catch{if(existed)PlayerPrefs.SetString(key,before);else PlayerPrefs.DeleteKey(key);throw;}
            },clock);
        }
        public static void UseTestProfile(ProgressData data=null,Func<DateTimeOffset> clock=null){current=new ProgressLedger(data??new ProgressData(),_=>{},clock);IsTestProfile=true;LoadError=null;}
        public static void RestorePlayerProfile(){current=null;IsTestProfile=false;LoadError=null;}
    }
}
