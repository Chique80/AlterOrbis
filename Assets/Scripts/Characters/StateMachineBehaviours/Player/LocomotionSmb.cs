using Characters.MonoBehaviours;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

namespace Characters.StateMachineBehaviours.Player
{
    public class LocomotionSmb : SceneLinkedSmb<PlayerCharacter>
    {
        public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (MonoBehaviour == null)
            {
                MonoBehaviour = animator.gameObject.GetComponent(typeof(PlayerCharacter)) as PlayerCharacter;
            }

            Debug.Assert(MonoBehaviour != null, nameof(MonoBehaviour) + " != null");
            MonoBehaviour.TeleportToColliderBottom();
        }

        public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (MonoBehaviour == null)
            {
                MonoBehaviour = animator.gameObject.GetComponent(typeof(PlayerCharacter)) as PlayerCharacter;
            }
            if (MonoBehaviour is null) return;
            if (!MonoBehaviour.isLocalPlayer && MonoBehaviour.isClient) return;
            MonoBehaviour.UpdateFacing();
            MonoBehaviour.GroundedHorizontalMovement();
            MonoBehaviour.GroundedVerticalMovement();
            MonoBehaviour.CheckForGrounded();
            //MonoBehaviour.CheckForPushing();
            if (MonoBehaviour.isJumping)
            {
                MonoBehaviour.currentAcceleration = MonoBehaviour.bounceStrength;
                MonoBehaviour.SetVerticalMovement(MonoBehaviour.jumpSpeed);
            }
            /*else if (MonoBehaviour.CheckForMeleeAttackInput())
                {
                    MonoBehaviour.MeleeAttack();
                }*/
            else if (MonoBehaviour.specialMove)
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