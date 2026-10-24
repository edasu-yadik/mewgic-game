using UnityEngine;
using UnityEngine.UI;
namespace Mewgic {
    public sealed class HomePlayButtonPresenter : MonoBehaviour
    {
        public Sprite normal,pressed,glow;
        public void Bind(Button button){var p=button.gameObject.AddComponent<PremiumPlayButton>();p.normal=normal;p.pressed=pressed;p.glow=glow;p.Bind(button);}
    }
}

