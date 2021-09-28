using Characters.MonoBehaviours;
using UnityEngine;

namespace Characters.StateMachineBehaviours.Player
{
    public class SpecialMoveSmb : SceneLinkedSmb<PlayerCharacter>
    {
        public override void OnSLStateNoTransitionUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (MonoBehaviour == null)
            {
                MonoBehaviour = animator.gameObject.GetComponent(typeof(PlayerCharacter)) as PlayerCharacter;
            }
            if (MonoBehaviour is null) return;
            if (MonoBehaviour.specialMove)
            {
                MonoBehaviour.SpecialMove();
            }
        }
    }
}