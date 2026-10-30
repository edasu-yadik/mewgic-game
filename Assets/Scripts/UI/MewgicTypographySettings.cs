using TMPro;
using UnityEngine;
namespace Mewgic.UI
{
    [CreateAssetMenu(menuName="Mewgic/Typography")]
    public sealed class MewgicTypographySettings:ScriptableObject
    {
        [System.Serializable]public sealed class TextStyle
        {
            public float scale=1,minimumSize=14;
            public FontStyles weight=FontStyles.Normal;
        }
        public TMP_FontAsset heading,body;
        public TextStyle headingStyle=new TextStyle{scale=1.12f,minimumSize=18};
        public TextStyle descriptionStyle=new TextStyle{minimumSize=15};
        public TextStyle numericStyle=new TextStyle{minimumSize=14};
        public TextStyle buttonStyle=new TextStyle{minimumSize=14};
        static MewgicTypographySettings cached;
        public static MewgicTypographySettings Current=>cached!=null?cached:cached=Resources.Load<MewgicTypographySettings>("UI/Typography");
        public static TMP_FontAsset FontFor(Transform t)
        {
            var theme=Current;if(theme==null)throw new System.InvalidOperationException("Mewgic TMP teması eksik");
            string n=t.name.ToLowerInvariant(),p=t.parent!=null?t.parent.name.ToLowerInvariant():"";
            bool title=(n.Contains("title")&&n!="tasktitle")||n=="success"||n=="visibleregion"||p.StartsWith("regionname");
            return title?theme.heading:theme.body;
        }
        public static void Prepare(TMP_Text text)
        {
            text.richText=false;text.raycastTarget=false;text.textWrappingMode=TextWrappingModes.Normal;
            text.overflowMode=TextOverflowModes.Truncate;text.extraPadding=true;
            text.font=FontFor(text.transform);
            string n=text.name.ToLowerInvariant();var role=text.font==Current.heading?Current.headingStyle:
                n.Contains("count")||n.Contains("value")||n.Contains("amount")||n=="threshold"?Current.numericStyle:
                n=="label"?Current.buttonStyle:Current.descriptionStyle;
            text.fontSize*=role.scale;text.fontSizeMax*=role.scale;
            text.fontSizeMin=Mathf.Min(text.fontSizeMax,Mathf.Max(text.fontSizeMin,role.minimumSize));text.fontStyle|=role.weight;
            text.lineSpacing=0;
        }
    }
}
