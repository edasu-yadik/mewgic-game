using System.Collections.Generic;
using Mewgic.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Mewgic
{
    public static class ShopCosmetics
    {
        static readonly int Costume = Shader.PropertyToID("_Costume");
        static readonly Dictionary<ShopItem, Material> costumes = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            foreach (var material in costumes.Values)
                if (material != null) Object.Destroy(material);
            costumes.Clear();
        }

        public static Material CostumeMaterial(ShopItem item)
        {
            if (item == ShopItem.ClassicCat) return null;
            if (costumes.TryGetValue(item, out var existing) && existing != null) return existing;
            var shader = Resources.Load<Shader>("UI/ShopCostume");
            if (shader == null) return null;
            var material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            material.SetColor(Costume, item switch
            {
                ShopItem.CrystalCat => new Color(.08f, .78f, 1),
                ShopItem.LeagueCat => new Color(1,.25f,.57f),
                ShopItem.GoldCat => new Color(1, .63f, .12f),
                _ => new Color(.27f, .32f, .88f)
            });
            costumes[item] = material;
            return material;
        }

        public static Color BottleTint(ShopItem item) => item switch
        {
            ShopItem.CrystalBottle => new Color(.53f, .95f, 1),
            ShopItem.GoldBottle => new Color(1, .82f, .48f),
            _ => Color.white
        };

        public static Color EffectTint(ShopItem item) => item switch
        {
            ShopItem.RoseEffect => new Color(1, .32f, .68f),
            ShopItem.CrystalEffect => new Color(.30f, .88f, 1),
            _ => new Color(1, .76f, .22f)
        };
    }
}
