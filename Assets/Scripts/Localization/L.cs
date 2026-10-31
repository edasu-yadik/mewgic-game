using System;using System.Collections;using System.Collections.Generic;using System.Globalization;using System.Linq;using Newtonsoft.Json;using UnityEngine;using UnityEngine.Localization;using UnityEngine.Localization.Settings;
namespace Mewgic.Localization
{
 // Runtime UI factories pass a key and arguments, never translated prose. Only the TMP binding decodes it.
 public static class L
 {
  public const string Table="Mewgic";public static readonly string[] Codes={"tr","en","de","fr","es","pt-BR","it","pl"};public static readonly string[] Names={"Türkçe","English","Deutsch","Français","Español","Português (Brasil)","Italiano","Polski"};
  public static bool Ready{get;internal set;}public static event Action Changed;public static readonly HashSet<string> Missing=new HashSet<string>();
  public static string Code=>Ready?LocalizationSettings.SelectedLocale.Identifier.Code:"en";
  public static CultureInfo Culture=>CultureInfo.GetCultureInfo(Code);
  public static string V(string key,params object[] args)=>"\u001e"+key+(args.Length==0?"":"\u001f"+JsonConvert.SerializeObject(args));
  public static string Match(string code){if(string.IsNullOrWhiteSpace(code))return null;code=code.Replace('_','-');var exact=Codes.FirstOrDefault(x=>string.Equals(x,code,StringComparison.OrdinalIgnoreCase));if(exact!=null)return exact;string root=code.Split('-')[0].ToLowerInvariant();return root=="pt"?"pt-BR":Codes.FirstOrDefault(x=>x==root);}
  public static string Choose(string manual,IEnumerable<string> device)=>Match(manual)??(device??Array.Empty<string>()).Select(Match).FirstOrDefault(x=>x!=null)??"en";
  public static string Resolve(string value)
  {
   if(string.IsNullOrEmpty(value)||value[0]!='\u001e')return value??"";if(!Ready)return "";int split=value.IndexOf('\u001f');string key=split<0?value.Substring(1):value.Substring(1,split-1);object[] args=split<0?Array.Empty<object>():JsonConvert.DeserializeObject<object[]>(value.Substring(split+1));
   for(int i=0;i<args.Length;i++)if(args[i] is string s&&s.StartsWith("\u001e"))args[i]=Resolve(s);
   var table=LocalizationSettings.StringDatabase.GetTable(Table);var entry=table?.GetEntry(key);if(entry==null||string.IsNullOrWhiteSpace(entry.Value)){Missing.Add(Code+":"+key);var english=LocalizationSettings.StringDatabase.GetTable(Table,LocalizationSettings.AvailableLocales.GetLocale("en"));if(english?.GetEntry(key)==null)return "";}
   return LocalizationSettings.StringDatabase.GetLocalizedString(Table,key,(IList<object>)args);
  }
  public static bool Select(string code,bool persist=true){code=Match(code);if(code==null||!Ready)return false;if(persist&&!GameplayProgress.Current.SetManualLocale(code))return false;LocalizationSettings.SelectedLocale=LocalizationSettings.AvailableLocales.GetLocale(code);Notify();return true;}
  internal static void Notify()=>Changed?.Invoke();
  public static string Island(int region){string[] keys={"village","crystal","moon","castle","woods","mirage","coral","frost","clockwork","starlight","library","aurora"};return region<keys.Length?V("map.island."+keys[region]):V("map.frontier",region+1);}
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]static void Reset(){Ready=false;Missing.Clear();Changed=null;}
  [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]static void Start(){var go=new GameObject("Mewgic Localization");UnityEngine.Object.DontDestroyOnLoad(go);go.AddComponent<LocalizationBoot>();}
 }
 public sealed class LocalizationBoot:MonoBehaviour
 {
  IEnumerator Start(){yield return LocalizationSettings.InitializationOperation;foreach(string code in L.Codes)yield return LocalizationSettings.StringDatabase.GetTableAsync(L.Table,LocalizationSettings.AvailableLocales.GetLocale(code));L.Ready=true;LocalizationSettings.SelectedLocaleChanged+=OnLocale;L.Notify();}
  void OnLocale(Locale locale)=>L.Notify();void OnDestroy(){LocalizationSettings.SelectedLocaleChanged-=OnLocale;}
 }
 [Serializable,UnityEngine.Scripting.Preserve]public sealed class SavedLocaleSelector:IStartupLocaleSelector{public Locale GetStartupLocale(ILocalesProvider locales){string code=L.Match(GameplayProgress.Current.Presentation.manualLocale);return code==null?null:locales.GetLocale(code);}}
 [Serializable,UnityEngine.Scripting.Preserve]public sealed class DeviceLocaleSelector:IStartupLocaleSelector
 {
  public Locale GetStartupLocale(ILocalesProvider locales)=>new SystemLocaleSelector().GetStartupLocale(new BestFitLocales(locales));
  public sealed class BestFitLocales:ILocalesProvider
  {
   readonly ILocalesProvider source;public BestFitLocales(ILocalesProvider provider){source=provider;}public List<Locale> Locales=>source.Locales;
   public Locale GetLocale(LocaleIdentifier id){var exact=source.GetLocale(id);if(exact!=null)return exact;string code=L.Match(id.Code);return code==null?null:source.GetLocale(code);}
   public void AddLocale(Locale locale)=>source.AddLocale(locale);public bool RemoveLocale(Locale locale)=>source.RemoveLocale(locale);
  }
 }
}
