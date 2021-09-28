using Mirror;
using UnityEngine;

namespace SceneManagement
{
    public class ChangeSceneSmb : StateMachineBehaviour
    {
        [Scene] public string sceneName;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (NetworkManager.singleton != null && NetworkServer.active)
            {
                Debug.Log("----- Change Scene -----");
                NetworkManager.singleton.ServerChangeScene(sceneName);
            }
        }
    }
}