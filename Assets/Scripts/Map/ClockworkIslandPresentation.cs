using UnityEngine;
using UnityEngine.UI;
namespace Mewgic.Map
{
 public sealed class ClockworkIslandPresentation : MonoBehaviour
 {
  public Image energy, towerEnergy, workshopEnergy;
  public RectTransform[] gears;
  public bool Animate = true;
  public float RotationDegreesPerSecond = 2.5f;
  public float Clock { get; private set; }
  public float EntranceProgress { get; private set; }
  public float EnergyPhase => energyMaterial != null ? energyMaterial.GetFloat("_EnvironmentTime") : 0;
  Material energyMaterial;
  CanvasGroup fade;
  float entered;
  float[] initialAngles;
  void Awake()
  {
   fade = GetComponent<CanvasGroup>();
   energyMaterial = new Material(energy.material);
   energy.material = energyMaterial; towerEnergy.material = energyMaterial; workshopEnergy.material = energyMaterial;
   initialAngles = new float[gears.Length];
   for (int i = 0; i < gears.Length; i++) initialAngles[i] = gears[i].localEulerAngles.z;
  }
  public void BeginEntrance() { entered = Time.unscaledTime; Tick(Clock); }
  void Update() { if (Animate) Clock += Time.unscaledDeltaTime; Tick(Clock); }
  void Tick(float t)
  {
   EntranceProgress = Animate ? Mathf.Clamp01((Time.unscaledTime - entered) / .85f) : 1;
   float e = 1 - Mathf.Pow(1 - EntranceProgress, 3); fade.alpha = e;
   transform.localScale = Vector3.one * Mathf.Lerp(.975f, 1, e);
   energyMaterial.SetFloat("_EnvironmentTime", t);
   energy.materialForRendering.SetFloat("_EnvironmentTime", t);
   towerEnergy.materialForRendering.SetFloat("_EnvironmentTime", t);
   workshopEnergy.materialForRendering.SetFloat("_EnvironmentTime", t);
   // Dekoratif görsel dönüş; fizik, kuvvet veya mekanik oyun kuralı yok.
   for (int i = 0; i < gears.Length; i++)
    gears[i].localRotation = Quaternion.Euler(0, 0, initialAngles[i] + t * RotationDegreesPerSecond * (i % 2 == 0 ? 1 : -1));
  }
  void OnDestroy() { if (energyMaterial != null) Destroy(energyMaterial); }
 }
}
