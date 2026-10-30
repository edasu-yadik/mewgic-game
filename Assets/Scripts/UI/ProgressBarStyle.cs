using UnityEngine;
namespace Mewgic.UI
{
    [CreateAssetMenu(menuName="Mewgic/Progress bar style")]
    public sealed class ProgressBarStyle:ScriptableObject
    {
        public Color gold=new Color32(255,200,78,255),goldShadow=new Color32(129,65,18,255);
        public Color empty=new Color32(22,5,51,255),violet=new Color32(125,22,220,255),light=new Color32(243,125,255,255);
        public float changeSeconds=.28f;
    }
}
