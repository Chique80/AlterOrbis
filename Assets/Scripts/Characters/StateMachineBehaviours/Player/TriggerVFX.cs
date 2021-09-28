using Characters.MonoBehaviours;
using UnityEngine;

namespace Characters.StateMachineBehaviours.Player
{
    public class TriggerVFX : StateMachineBehaviour
    {
        public string vfxName;
        public Vector3 offset = Vector3.zero;
        public bool attachToParent;
        public float startDelay;
        public bool onEnter = true;
        public bool onExit;
        private int _vfxId;

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (onEnter)
            {
                Trigger(animator.transform);
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
        {
            if (onExit)
            {
                Trigger(animator.transform);
            }
        }

        private void Trigger(Transform transform)
        {
            var flip = false;
            var spriteRender = transform.GetComponent<SpriteRenderer>();
            if (spriteRender)
                flip = spriteRender.flipX;
            VFXController.Instance.Trigger(vfxName, offset, startDelay, flip, attachToParent ? transform : null);
        }
    }
}