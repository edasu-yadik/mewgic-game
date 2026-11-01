using UnityEngine;
using UnityEngine.UI;
namespace Mewgic.Map
{
 public sealed class MirageIslandPresentation : MonoBehaviour
 {
  public Image water,lights,mist,sand;
  public RectTransform[] sparkles;
  public bool Animate=true;
  public float Clock {get;private set;}
  public float EntranceProgress {get;private set;}
  public float WaterPhase=>waterMaterial!=null?waterMaterial.GetFloat("_EnvironmentTime"):0;
  Material waterMaterial,lightMaterial,sandMaterial;CanvasGroup fade;Vector2 mistOrigin;float entered;
  void Awake()
  {
   fade=GetComponent<CanvasGroup>();mistOrigin=mist.rectTransform.anchoredPosition;
   waterMaterial=new Material(water.material);lightMaterial=new Material(lights.material);sandMaterial=new Material(sand.material);
   water.material=waterMaterial;lights.material=lightMaterial;sand.material=sandMaterial;
  }
  public void BeginEntrance(){entered=Time.unscaledTime;Tick(0);}
  void Update(){if(Animate)Clock+=Time.unscaledDeltaTime;Tick(Clock);}
  void Tick(float t)
  {
   EntranceProgress=Animate?Mathf.Clamp01((Time.unscaledTime-entered)/.85f):1;
   float e=1-Mathf.Pow(1-EntranceProgress,3);fade.alpha=e;transform.localScale=Vector3.one*Mathf.Lerp(.975f,1,e);
   Phase(water,waterMaterial,t);Phase(lights,lightMaterial,t);Phase(sand,sandMaterial,t);
   mist.rectTransform.anchoredPosition=mistOrigin+new Vector2(Mathf.Sin(t*.1f)*6,Mathf.Sin(t*.15f)*2);
   mist.color=new Color(1,.88f,.77f,.045f+Mathf.Sin(t*.21f)*.01f);
   for(int i=0;i<sparkles.Length;i++){
    float phase=t*.45f+i*2.13f;sparkles[i].anchoredPosition=new Vector2(Mathf.Sin(phase*.6f)*3,Mathf.Sin(phase)*2);
    sparkles[i].GetComponent<Graphic>().color=new Color(1,.89f,.55f,.12f+.28f*Mathf.Pow(.5f+.5f*Mathf.Sin(phase*1.3f),4));
   }
  }
  static void Phase(Image image,Material material,float t){material.SetFloat("_EnvironmentTime",t);image.materialForRendering.SetFloat("_EnvironmentTime",t);}
  void OnDestroy(){if(waterMaterial!=null)Destroy(waterMaterial);if(lightMaterial!=null)Destroy(lightMaterial);if(sandMaterial!=null)Destroy(sandMaterial);}
 }
}
