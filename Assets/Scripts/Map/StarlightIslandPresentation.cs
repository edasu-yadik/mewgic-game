using UnityEngine;
using UnityEngine.UI;
namespace Mewgic.Map
{
 public sealed class StarlightIslandPresentation : MonoBehaviour
 {
  public Image water, treeGlow;
  public RectTransform[] petals, stars;
  public bool Animate = true;
  public float Clock { get; private set; }
  public float EntranceProgress { get; private set; }
  public float WaterPhase => waterMaterial != null ? waterMaterial.GetFloat("_EnvironmentTime") : 0;
  Material waterMaterial, lightMaterial;
  Graphic[] petalGraphics, starGraphics;
  CanvasGroup fade;
  float entered;
  void Awake()
  {
   fade = GetComponent<CanvasGroup>();
   waterMaterial = new Material(water.material); lightMaterial = new Material(treeGlow.material);
   water.material = waterMaterial; treeGlow.material = lightMaterial;
   petalGraphics = new Graphic[petals.Length]; starGraphics = new Graphic[stars.Length];
   for (int i = 0; i < petals.Length; i++) petalGraphics[i] = petals[i].GetComponent<Graphic>();
   for (int i = 0; i < stars.Length; i++) starGraphics[i] = stars[i].GetComponent<Graphic>();
  }
  public void BeginEntrance() { entered = Time.unscaledTime; Tick(Clock); }
  void Update() { if (Animate) Clock += Time.unscaledDeltaTime; Tick(Clock); }
  void Tick(float t)
  {
   EntranceProgress = Animate ? Mathf.Clamp01((Time.unscaledTime - entered) / .85f) : 1;
   float e = 1 - Mathf.Pow(1 - EntranceProgress, 3); fade.alpha = e;
   transform.localScale = Vector3.one * Mathf.Lerp(.975f, 1, e);
   waterMaterial.SetFloat("_EnvironmentTime", t); lightMaterial.SetFloat("_EnvironmentTime", t);
   water.materialForRendering.SetFloat("_EnvironmentTime", t); treeGlow.materialForRendering.SetFloat("_EnvironmentTime", t);
   for (int i = 0; i < petals.Length; i++)
   {
    float life = Mathf.Repeat(t * .045f + i * .167f, 1);
    petals[i].anchoredPosition = new Vector2(Mathf.Sin(t * .35f + i) * 12, -life * 95);
    petals[i].localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * .4f + i) * 25);
    petals[i].localScale = new Vector3(.8f + .2f * Mathf.Sin(t * .6f + i), 1, 1);
    petalGraphics[i].color = new Color(1, .83f, .96f, .35f * Mathf.Sin(life * Mathf.PI));
   }
   for (int i = 0; i < stars.Length; i++)
    starGraphics[i].color = new Color(1, .86f, .57f, .08f + .12f * (.5f + .5f * Mathf.Sin(t * .8f + i * 1.7f)));
  }
  void OnDestroy() { if (waterMaterial != null) Destroy(waterMaterial); if (lightMaterial != null) Destroy(lightMaterial); }
 }
}
