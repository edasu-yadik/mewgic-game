using TMPro;using UnityEngine;
namespace Mewgic.Localization
{
 // One adapter for existing runtime-created TMP labels. Locale changes refresh even inactive labels on enable.
 public sealed class LocalizedTMP:TextMeshProUGUI
 {
  string request;
  public string Request=>request;
  public override string text {get=>base.text;set{if(request==value)return;request=value;Render();}}
  void Render(){base.text=L.Resolve(request);}
  protected override void OnEnable(){base.OnEnable();L.Changed+=Render;Render();}
  protected override void OnDisable(){L.Changed-=Render;base.OnDisable();}
 }
}
