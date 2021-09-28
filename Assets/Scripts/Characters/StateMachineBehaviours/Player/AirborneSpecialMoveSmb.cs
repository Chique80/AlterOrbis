using Characters.MonoBehaviours;
using UnityEngine;

namespace Characters.StateMachineBehaviours.Player
{
    public class AirborneSpecialMoveSmb : SceneLinkedSmb<PlayerCharacter>
    {
        public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (MonoBehaviour == null)
            {
                MonoBehaviour = animator.gameObject.GetComponent(typeof(PlayerCharacter)) as PlayerCharacter;
            }
            if (MonoBehaviour is null) return;
            
            MonoBehaviour.UpdateJump();
            MonoBehaviour.AirborneHorizontalMovement();
            MonoBehaviour.AirborneVerticalMovement();
            MonoBehaviour.CheckForGrounded();
        }
    }
}
