using System;
using System.Collections.Generic;
using Mewgic.Core;
using UnityEngine;

namespace Mewgic.Characters
{
    public enum CostumePose { Front, Sitting, StepA, StepB, Curled, Wake, Stretch, Standing, Victory }

    public static class CostumeArt
    {
        [Serializable] sealed class Frame { public int x, y, width, height; }
        [Serializable] sealed class Manifest { public Frame[] frames; }
        static readonly Dictionary<ShopItem, Sprite[]> loaded = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()
        {
            foreach (var set in loaded.Values)
                if (set != null)
                    foreach (var sprite in set)
                        if (sprite != null) UnityEngine.Object.Destroy(sprite);
            loaded.Clear();
        }

        public static bool IsReferenceCostume(ShopItem item) => item is ShopItem.PrincessCat or ShopItem.ExplorerCat
            or ShopItem.CrystalCat or ShopItem.NightCat or ShopItem.FairyCat or ShopItem.RoyalCat;

        public static Sprite Get(ShopItem item, CostumePose pose)
        {
            if (!IsReferenceCostume(item)) return null;
            if (!loaded.TryGetValue(item, out var frames))
                loaded[item] = frames = Load(item);
            return frames?[(int)pose];
        }

        static Sprite[] Load(ShopItem item)
        {
            string path = "CatCostumes/" + item;
            var data = Resources.Load<TextAsset>(path + "Frames");
            if (data == null) return null;
            var texture = Resources.Load<Texture2D>(path);
            var manifest = JsonUtility.FromJson<Manifest>(data.text);
            if (texture == null || manifest?.frames?.Length != 9) return null;
            var sprites = new Sprite[9];
            for (int i = 0; i < sprites.Length; i++)
            {
                var frame = manifest.frames[i];
                if (frame == null || frame.width <= 0 || frame.height <= 0 || frame.x < 0 || frame.y < 0 ||
                    frame.x + frame.width > texture.width || frame.y + frame.height > texture.height) continue;
                var rect = new Rect(frame.x, texture.height - frame.y - frame.height, frame.width, frame.height);
                var sprite = Sprite.Create(texture, rect, new Vector2(.5f, 0), 100, 0, SpriteMeshType.FullRect);
                sprite.name = item + "_" + (CostumePose)i;
                sprites[i] = sprite;
            }
            return sprites;
        }

        public static Rect UV(Sprite sprite)
        {
            var rect = sprite.rect;
            return new Rect(rect.x / sprite.texture.width, rect.y / sprite.texture.height,
                rect.width / sprite.texture.width, rect.height / sprite.texture.height);
        }
    }
}
