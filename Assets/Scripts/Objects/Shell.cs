using Characters.MonoBehaviours;
using UnityEngine;
using Utility;

namespace Objects
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Shell : MonoBehaviour
    {
        public bool spriteOriginallyFacesLeft;

        [Tooltip("If -1 never auto destroy, otherwise shell is return to pool when that time is reached")]
        public float timeBeforeAutodestruct = 15f;

        public ShellObject ShellPoolObject;

        private const float BufferBeforeBounce = 1f;
        
        private float _timer;
        
        
        private SpriteRenderer _spriteRenderer;
        
        private static readonly int VFXHash = VFXController.StringToHash("BigDustPuff");
        
        private void OnEnable()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _timer = 0.0f;
        }
        
        private void FixedUpdate()
        {
            if (!(timeBeforeAutodestruct > 0)) return;
            _timer += Time.deltaTime;
            if (_timer > BufferBeforeBounce)
            {
                gameObject.tag = "Bounce";
            }

            if (!(_timer > timeBeforeAutodestruct) || ShellPoolObject == null) return;
            gameObject.tag = "Untagged";
            ShellPoolObject?.ReturnToPool();
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            FindSurface(other.collider);
            if (other.gameObject.CompareTag("Player")) return;
                gameObject.GetComponent<AudioSource>().Play();
        }

        private void FindSurface(Collider2D surfaceCollider2D)
        {
            var forward = spriteOriginallyFacesLeft ? Vector3.left : Vector3.right;
            if (_spriteRenderer.flipX)
            {
                forward.x = -forward.x;
            }

            var position = transform.position;
            var surfaceHit = PhysicsHelper.FindTileForOverride(surfaceCollider2D, position, forward);

            VFXController.Instance.Trigger(VFXHash, position, 0, _spriteRenderer.flipX, null, surfaceHit);
        }


    }
}