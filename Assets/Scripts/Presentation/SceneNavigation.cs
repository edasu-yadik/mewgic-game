using UnityEngine;using UnityEngine.SceneManagement;
namespace Mewgic
{
    public static class SceneNavigation
    {
        static int lastFrame=-1;
        public static bool LoadOnce(string scene)
        {
            if(lastFrame==Time.frameCount||!Application.CanStreamedLevelBeLoaded(scene))return false;
            lastFrame=Time.frameCount;SceneManager.LoadScene(scene);return true;
        }
    }
}
