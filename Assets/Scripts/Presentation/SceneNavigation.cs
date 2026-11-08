using System;using UnityEngine;using UnityEngine.SceneManagement;
namespace Mewgic
{
    public static class SceneNavigation
    {
        static int lastFrame=-1;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void Reset()=>lastFrame=-1;
        public static bool LoadOnce(string scene,Func<bool> select=null)
        {
            if(lastFrame==Time.frameCount||!Application.CanStreamedLevelBeLoaded(scene))return false;
            if(select!=null&&!select())return false;
            lastFrame=Time.frameCount;if(scene=="LevelMap"||SceneManager.GetActiveScene().name=="LevelMap")GameFeel.Play(GameCue.Map);SceneManager.LoadScene(scene);return true;
        }
    }
}
