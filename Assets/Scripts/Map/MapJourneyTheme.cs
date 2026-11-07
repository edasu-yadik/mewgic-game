using UnityEngine;

namespace Mewgic.Map
{
    [CreateAssetMenu(menuName = "Mewgic/Map Journey Theme")]
    public sealed class MapJourneyTheme : ScriptableObject
    {
        public Texture2D background;
        public Sprite foreground;
        public Texture2D referenceTerrace;
        public Texture2D pavingTexture;
        public bool flowingWater;
        public Color paving = new Color32(178, 138, 160, 255);
        public Color stoneWall = new Color32(55, 40, 84, 255);
        public Color gold = new Color32(236, 183, 116, 255);
    }
}
