using System;
using Mewgic.Core;
using UnityEngine;

namespace Mewgic
{
    static class UnityEconomyConfig
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Load()
        {
            var asset = Resources.Load<TextAsset>("Economy/Balance");
            try
            {
                EconomyConfig.Configure(asset == null ? new EconomyConfig() : JsonUtility.FromJson<EconomyConfig>(asset.text));
            }
            catch (Exception e)
            {
                EconomyConfig.Configure(new EconomyConfig());
                Debug.LogError("Ekonomi yapılandırması okunamadı: " + e.Message);
            }
        }
    }
}
