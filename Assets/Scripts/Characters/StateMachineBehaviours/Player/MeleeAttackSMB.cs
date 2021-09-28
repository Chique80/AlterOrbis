using Characters.MonoBehaviours;
using UnityEngine;

namespace Characters.StateMachineBehaviours.Player
{
    public class MeleeAttackSmb : SceneLinkedSmb<PlayerCharacter>
    {
        private int hashAirborneMeleeAttack = Animator.StringToHash("AirborneAttack");
        
        
        public override void OnSLStatePostEnter (Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            MonoBehaviour.EnableMeleeAttack();
            MonoBehaviour.SetHorizontalMovement(MonoBehaviour.meleeAttackDashSpeed * MonoBehaviour.GetFacing());
        }

    }
}
