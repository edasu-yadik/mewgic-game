using UnityEngine;
using UnityEngine.UI;
namespace Mewgic.Characters
{
    public sealed class WizardCatTestPanel : MonoBehaviour
    {
        public WizardCatPresentation cat;
        public Button magicButton,blinkButton;
        public Text status;
        public Image background;
        public void Magic(){cat.TriggerMagic();}
        public void Blink(){cat.TriggerBlink();}
        public void ToggleBackground(){bool light=background.color.r<.5f;background.color=light?new Color(.85f,.88f,.94f):new Color(.055f,.065f,.12f);foreach(var text in GetComponentsInChildren<Text>())if(text.name=="Header"||text.name=="Subtitle"||text.name=="AnimationStatus")text.color=light?new Color(.18f,.1f,.28f):new Color(1,.85f,.55f);}
        void Update()
        {
            magicButton.interactable=!cat.MagicBusy;
            blinkButton.interactable=cat.CanBlink;
            status.text=cat.CanBlink?"Idle • Blink • Wand Magic   |   Kıvılcım: "+cat.ActiveSparks+"/"+cat.PoolSize:"Idle • Wand Magic   |   Blink: göz kapağı sprite'ı gerekli";
        }
    }
}
