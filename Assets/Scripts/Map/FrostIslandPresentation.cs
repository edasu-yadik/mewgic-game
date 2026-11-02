using UnityEngine;
using UnityEngine.UI;
namespace Mewgic.Map
{
 public sealed class FrostIslandPresentation : MonoBehaviour
 {
  public Image ice, palaceIce, aurora;
  public RectTransform[] snow;
  public bool Animate = true;
  public float Clock { get; private set; }
  public float EntranceProgress { get; private set; }
  public float IcePhase => iceMaterial != null ? iceMaterial.GetFloat("_EnvironmentTime") : 0;
  Material iceMaterial;
  Graphic[] flakes;
  CanvasGroup fade;
  Vector2 auroraOrigin;
  float entered;
  void Awake()
  {
   fade = GetComponent<CanvasGroup>(); auroraOrigin = aurora.rectTransform.anchoredPosition;
   iceMaterial = new Material(ice.material); ice.material = iceMaterial; palaceIce.material = iceMaterial;
   flakes = new Graphic[snow.Length];
   for (int i = 0; i < snow.Length; i++) flakes[i] = snow[i].GetComponent<Graphic>();
  }
  public void BeginEntrance() { entered = Time.unscaledTime; Tick(0); }
  void Update() { if (Animate) Clock += Time.unscaledDeltaTime; Tick(Clock); }
  void Tick(float t)
  {
   EntranceProgress = Animate ? Mathf.Clamp01((Time.unscaledTime - entered) / .85f) : 1;
   float e = 1 - Mathf.Pow(1 - EntranceProgress, 3); fade.alpha = e;
   transform.localScale = Vector3.one * Mathf.Lerp(.975f, 1, e);
   iceMaterial.SetFloat("_EnvironmentTime", t);
   ice.materialForRendering.SetFloat("_EnvironmentTime", t); palaceIce.materialForRendering.SetFloat("_EnvironmentTime", t);
   aurora.rectTransform.anchoredPosition = auroraOrigin + new Vector2(Mathf.Sin(t * .07f) * 5, Mathf.Sin(t * .11f) * 2);
   aurora.color = new Color(.8f, .88f, 1, .24f + .025f * Mathf.Sin(t * .21f));
   for (int i = 0; i < snow.Length; i++)
   {
    float life = Mathf.Repeat(t * .055f + i * .117f, 1);
    snow[i].anchoredPosition = new Vector2(Mathf.Sin(t * .3f + i) * 7, -life * 100);
    flakes[i].color = new Color(.88f, .96f, 1, .28f * Mathf.Sin(life * Mathf.PI));
   }
  }
  void OnDestroy() { if (iceMaterial != null) Destroy(iceMaterial); }
 }
}
