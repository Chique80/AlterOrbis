using UnityEngine;

namespace Objects
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Leaf : FreezableObject
    {
        public bool spriteOriginallyFacesLeft;

        [Tooltip("If -1 never auto destroy, otherwise shell is return to pool when that time is reached")]
        public float timeBeforeAutodestruct = 15f;

        public LeafObject LeafPoolObject;
        
        private float _timer;
        
        private void OnEnable()
        {
            var transform1 = transform;
            var transformRotation = transform1.rotation;
            transformRotation.z = 0;
            transform1.rotation = transformRotation;
            _timer = 0.0f;
        }
        
        private void FixedUpdate()
        {
            if (!(timeBeforeAutodestruct > 0)) return;
            _timer += Time.deltaTime;
            if (_timer > timeBeforeAutodestruct)
            {
                LeafPoolObject?.ReturnToPool();

            }
        }


    }
}