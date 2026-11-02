using UnityEngine;using UnityEngine.UI;
namespace Mewgic.Map
{
 public sealed class AuroraIslandPresentation : MonoBehaviour
 {
  public Image water, lights, aurora;
  public RectTransform[] islets, stars;
  public bool Animate=true;
  public float Clock {get;private set;}
  public float EntranceProgress {get;private set;}
  public float WaterPhase=>waterMaterial!=null?waterMaterial.GetFloat("_EnvironmentTime"):0;
  Material waterMaterial,lightMaterial;CanvasGroup fade;Vector2 auroraOrigin;Vector2[] isletOrigins;Graphic[] starGraphics;float entered;
  void Awake(){fade=GetComponent<CanvasGroup>();auroraOrigin=aurora.rectTransform.anchoredPosition;waterMaterial=new Material(water.material);lightMaterial=new Material(lights.material);water.material=waterMaterial;lights.material=lightMaterial;isletOrigins=new Vector2[islets.Length];for(int i=0;i<islets.Length;i++)isletOrigins[i]=islets[i].anchoredPosition;starGraphics=new Graphic[stars.Length];for(int i=0;i<stars.Length;i++)starGraphics[i]=stars[i].GetComponent<Graphic>();}
  public void BeginEntrance(){entered=Time.unscaledTime;Tick(Clock);}
  void Update(){if(Animate)Clock+=Time.unscaledDeltaTime;Tick(Clock);}
  void Tick(float t)
  {
   EntranceProgress=Animate?Mathf.Clamp01((Time.unscaledTime-entered)/.85f):1;float e=1-Mathf.Pow(1-EntranceProgress,3);fade.alpha=e;transform.localScale=Vector3.one*Mathf.Lerp(.975f,1,e);
   waterMaterial.SetFloat("_EnvironmentTime",t);lightMaterial.SetFloat("_EnvironmentTime",t);water.materialForRendering.SetFloat("_EnvironmentTime",t);lights.materialForRendering.SetFloat("_EnvironmentTime",t);
   aurora.rectTransform.anchoredPosition=auroraOrigin+new Vector2(Mathf.Sin(t*.08f)*5,Mathf.Sin(t*.11f)*2);aurora.color=new Color(.9f,.85f,1,.20f+.025f*Mathf.Sin(t*.2f));
   for(int i=0;i<islets.Length;i++)islets[i].anchoredPosition=isletOrigins[i]+new Vector2(Mathf.Sin(t*.16f+i)*1.5f,Mathf.Sin(t*.22f+i)*2.5f);
   for(int i=0;i<stars.Length;i++){stars[i].anchoredPosition=new Vector2(Mathf.Sin(t*.19f+i)*2,Mathf.Sin(t*.23f+i)*4);starGraphics[i].color=new Color(1,.87f,.66f,.08f+.10f*(.5f+.5f*Mathf.Sin(t*.7f+i*1.7f)));}
  }
  void OnDestroy(){if(waterMaterial!=null)Destroy(waterMaterial);if(lightMaterial!=null)Destroy(lightMaterial);}
 }
}
