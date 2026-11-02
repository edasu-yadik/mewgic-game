using UnityEngine;
using UnityEngine.UI;
namespace Mewgic.Map
{
 public sealed class CoralIslandPresentation : MonoBehaviour
 {
  public Image water,palaceWater,lights,rays;
  public RectTransform[] bubbles;
  public bool Animate=true;
  public float Clock {get;private set;}
  public float EntranceProgress {get;private set;}
  public float WaterPhase=>waterMaterial!=null?waterMaterial.GetFloat("_EnvironmentTime"):0;
  Material waterMaterial,lightMaterial;CanvasGroup fade;Vector2 rayOrigin;float entered;
  void Awake(){fade=GetComponent<CanvasGroup>();rayOrigin=rays.rectTransform.anchoredPosition;waterMaterial=new Material(water.material);lightMaterial=new Material(lights.material);water.material=waterMaterial;palaceWater.material=waterMaterial;lights.material=lightMaterial;}
  public void BeginEntrance(){entered=Time.unscaledTime;Tick(0);}
  void Update(){if(Animate)Clock+=Time.unscaledDeltaTime;Tick(Clock);}
  void Tick(float t)
  {
   EntranceProgress=Animate?Mathf.Clamp01((Time.unscaledTime-entered)/.85f):1;
   float e=1-Mathf.Pow(1-EntranceProgress,3);fade.alpha=e;transform.localScale=Vector3.one*Mathf.Lerp(.975f,1,e);
   waterMaterial.SetFloat("_EnvironmentTime",t);lightMaterial.SetFloat("_EnvironmentTime",t);
   water.materialForRendering.SetFloat("_EnvironmentTime",t);palaceWater.materialForRendering.SetFloat("_EnvironmentTime",t);lights.materialForRendering.SetFloat("_EnvironmentTime",t);
   rays.rectTransform.anchoredPosition=rayOrigin+new Vector2(Mathf.Sin(t*.09f)*4,Mathf.Sin(t*.12f)*2);
   rays.color=new Color(.72f,.95f,1,.035f+.012f*Mathf.Sin(t*.2f));
   for(int i=0;i<bubbles.Length;i++){
    float life=Mathf.Repeat(t*.10f+i*.137f,1);bubbles[i].anchoredPosition=new Vector2(Mathf.Sin(t*.4f+i)*3,life*46);
    bubbles[i].GetComponent<Graphic>().color=new Color(.75f,.96f,1,.32f*Mathf.Sin(life*Mathf.PI));
   }
  }
  void OnDestroy(){if(waterMaterial!=null)Destroy(waterMaterial);if(lightMaterial!=null)Destroy(lightMaterial);}
 }
}
