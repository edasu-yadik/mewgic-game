using Mewgic.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Mewgic.Map
{
    internal enum JourneyCatPose { Sitting, StepA, StepB, Curled, Blink, Stretch, Standing }

    public sealed class JourneyCatImage : RawImage
    {
        // Three padded UV crops share a single atlas and the same paw baseline.
        static readonly Rect[] Poses =
        {
            new Rect(32f / 1536, 244f / 1024, 424f / 1536, 548f / 1024),
            new Rect(513f / 1536, 244f / 1024, 484f / 1536, 548f / 1024),
            new Rect(1025f / 1536, 244f / 1024, 484f / 1536, 548f / 1024)
        };
        static readonly Rect[] RestPoses =
        {
            AtlasCrop(100, 130, 468, 407), AtlasCrop(683, 130, 468, 407),
            AtlasCrop(46, 691, 544, 511), AtlasCrop(650, 635, 532, 567)
        };

        static Rect AtlasCrop(float x, float y, float width, float height)
        {
            const float size = 1254;
            return new Rect(x / size, 1 - (y + height) / size, width / size, height / size);
        }

        internal static Rect SittingUV => Poses[0];
        Material poseMaterial;
        bool usingCostume;
        public ShopItem WornCostume { get; private set; }
        public int PoseIndex { get; private set; }
        internal JourneyCatPose Pose => (JourneyCatPose)PoseIndex;

        internal float ShowPose(Texture2D fallback, Texture2D walkAtlas, Texture2D restAtlas, JourneyCatPose pose)
        {
            WornCostume = GameplayProgress.Current.Equipped(ShopCategory.Cats);
            var dressed = Characters.CostumeArt.Get(WornCostume, (Characters.CostumePose)((int)pose + 1));
            if (dressed != null)
            {
                if (!usingCostume) { poseMaterial = material; material = null; usingCostume = true; }
                PoseIndex = (int)pose;
                texture = dressed.texture;
                uvRect = Characters.CostumeArt.UV(dressed);
                return dressed.rect.width / dressed.rect.height;
            }
            if (usingCostume) { material = poseMaterial; usingCostume = false; }
            bool rest = pose >= JourneyCatPose.Curled && restAtlas != null;
            PoseIndex = rest || walkAtlas != null ? (int)pose : 0;
            Texture desired = rest ? restAtlas : walkAtlas != null ? walkAtlas : fallback;
            if (texture != desired) texture = desired;
            var crop = rest ? RestPoses[PoseIndex - (int)JourneyCatPose.Curled] :
                walkAtlas != null ? Poses[Mathf.Min(PoseIndex, 2)] : new Rect(0, 0, 1, 1);
            if (uvRect != crop) uvRect = crop;
            return texture.width * crop.width / (texture.height * crop.height);
        }
    }
}
