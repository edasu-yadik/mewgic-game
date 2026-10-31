using UnityEngine;
namespace Mewgic.UI
{
    [CreateAssetMenu(menuName="Mewgic/Paw tutorial")]
    public sealed class PawTutorialDefinition:ScriptableObject
    {
        public string tutorialId,conditionKey,completionKey,messageKey;
        [TextArea]public string message;
        public string targetName;
    }
}
