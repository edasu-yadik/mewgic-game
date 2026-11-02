using UnityEngine;
using UnityEngine.UI;
namespace Mewgic.Map
{
 public sealed class LibraryIslandPresentation : MonoBehaviour
 {
  public Image lights;
  public RectTransform[] books;
  public Image[] rings;
  public bool Animate = true;
  public float Clock { get; private set; }
  public float EntranceProgress { get; private set; }
  public float LightPhase => lightMaterial != null ? lightMaterial.GetFloat("_EnvironmentTime") : 0;
  Material lightMaterial;
  CanvasGroup fade;
  Vector2[] origins;
  float[] angles;
  float entered;
  void Awake()
  {
   fade = GetComponent<CanvasGroup>(); lightMaterial = new Material(lights.material); lights.material = lightMaterial;
   origins = new Vector2[books.Length]; angles = new float[books.Length];
   for (int i = 0; i < books.Length; i++) { origins[i] = books[i].anchoredPosition; angles[i] = books[i].localEulerAngles.z; }
  }
  public void BeginEntrance() { entered = Time.unscaledTime; Tick(Clock); }
  void Update() { if (Animate) Clock += Time.unscaledDeltaTime; Tick(Clock); }
  void Tick(float t)
  {
   EntranceProgress = Animate ? Mathf.Clamp01((Time.unscaledTime - entered) / .85f) : 1;
   float e = 1 - Mathf.Pow(1 - EntranceProgress, 3); fade.alpha = e;
   transform.localScale = Vector3.one * Mathf.Lerp(.975f, 1, e);
   lightMaterial.SetFloat("_EnvironmentTime", t); lights.materialForRendering.SetFloat("_EnvironmentTime", t);
   for (int i = 0; i < books.Length; i++)
   {
    books[i].anchoredPosition = origins[i] + new Vector2(Mathf.Sin(t * .20f + i * 1.3f) * 2, Mathf.Sin(t * .35f + i * 1.3f) * 7);
    books[i].localRotation = Quaternion.Euler(0, 0, angles[i] + Mathf.Sin(t * .25f + i) * 3);
   }
   for (int i = 0; i < rings.Length; i++) rings[i].color = new Color(1, 1, 1, .22f + .035f * Mathf.Sin(t * .65f + i * 1.7f));
  }
  void OnDestroy() { if (lightMaterial != null) Destroy(lightMaterial); }
 }
}
