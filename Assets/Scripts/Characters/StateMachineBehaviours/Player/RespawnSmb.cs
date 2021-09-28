using Characters.MonoBehaviours;
using UnityEngine;

namespace Characters.StateMachineBehaviours.Player
{
    public class RespawnSmb : SceneLinkedSmb<PlayerCharacter>
    {
        public override void OnSLStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (MonoBehaviour == null)
            {
                MonoBehaviour = animator.gameObject.GetComponent(typeof(PlayerCharacter)) as PlayerCharacter;
            }
            if (MonoBehaviour is null) return;
            base.OnSLStateEnter(animator, stateInfo, layerIndex);

            MonoBehaviour.SetMoveVector(Vector2.zero);
        }
    }
}