using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace Mewgic.Map
{
    // Her havuz slotu yalnız görünür adanın yerel sanat handle'ına sahip olur.
    public sealed class IslandArtSlot : MonoBehaviour
    {
        AsyncOperationHandle<GameObject> operation;
        bool ownsHandle;
        int generation;
        string boundKey;
        GameObject instance;
        public bool Loaded => instance != null;
        public string BoundKey => boundKey;
        public string LoadError { get; private set; }
        public WoodsIslandPresentation Presentation => instance != null ? instance.GetComponent<WoodsIslandPresentation>() : null;
        public static int LiveHandles { get; private set; }
        public static int LiveInstances { get; private set; }

        public void Bind(string key, bool animate)
        {
            if (boundKey == key && (ownsHandle || key == null))
            {
                if (Presentation != null) Presentation.Animate = animate;
                return;
            }
            Release();
            if (key != "woods") return;
            boundKey = key;
            LoadError = null;
            int token = ++generation;
            operation = Addressables.LoadAssetAsync<GameObject>("Map/woods");
            ownsHandle = true;
            LiveHandles++;
            operation.Completed += completed =>
            {
                // Ayrılan slota geç gelen sonuç hiçbir objeye bağlanmaz; handle Release tarafından bırakıldı.
                if (this == null || token != generation || !ownsHandle) return;
                if (completed.Status != AsyncOperationStatus.Succeeded)
                {
                    LoadError = "Whispering Woods sanatı yüklenemedi.";
                    Release();
                    Debug.LogWarning(LoadError);
                    return;
                }
                instance = Instantiate(completed.Result, transform, false);
                instance.name = "WhisperingWoods_LayeredArt";
                LiveInstances++;
                Presentation.Animate = animate;
                Presentation.BeginEntrance();
                // Sanat UI düğümleri ve isim plakasının arkasında kalır.
                instance.transform.SetAsFirstSibling();
            };
        }
        public void Release()
        {
            generation++;
            if (instance != null)
            {
                instance.SetActive(false);
                Destroy(instance);
                instance = null;
                LiveInstances--;
            }
            if (ownsHandle)
            {
                if (operation.IsValid()) Addressables.Release(operation);
                ownsHandle = false;
                LiveHandles--;
            }
            boundKey = null;
        }
        void OnDestroy() => Release();
    }
}
