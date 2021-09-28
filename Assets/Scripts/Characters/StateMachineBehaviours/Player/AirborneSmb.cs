using Characters.MonoBehaviours;
using UnityEngine;

namespace Characters.StateMachineBehaviours.Player
{
    public class AirborneSmb : SceneLinkedSmb<PlayerCharacter>
    {
        public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (MonoBehaviour == null)
            {
                MonoBehaviour = animator.gameObject.GetComponent(typeof(PlayerCharacter)) as PlayerCharacter;
            }
            if (MonoBehaviour is null) return;
            if (!MonoBehaviour.isLocalPlayer && MonoBehaviour.isClient) return;
            MonoBehaviour.UpdateFacing();
            MonoBehaviour.UpdateJump();
            MonoBehaviour.AirborneHorizontalMovement();
            MonoBehaviour.AirborneVerticalMovement();
            MonoBehaviour.CheckForGrounded();
            /*if (MonoBehaviour.CheckForMeleeAttackInput())
            {
                MonoBehaviour.MeleeAttack();
            }*/
            if (MonoBehaviour.specialMove)
            {
                MonoBehaviour.SpecialMove();
                MonoBehaviour.specialMove = false;
            }
            else if (MonoBehaviour.interact)
            {
                MonoBehaviour.EmbarkOnRope();
            }
        }
    }
}