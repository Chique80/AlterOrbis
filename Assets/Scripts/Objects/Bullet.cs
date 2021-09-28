using Characters.MonoBehaviours;
using UnityEngine;
using Utility;

namespace Objects
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Damager))]
    public class Bullet : MonoBehaviour
    {
        public bool destroyWhenOutOfView = true;
        public bool spriteOriginallyFacesLeft;

        [Tooltip("If -1 never auto destroy, otherwise bullet is return to pool when that time is reached")]
        public float timeBeforeAutodestruct = -1.0f;

        public BulletObject BulletPoolObject;
        [HideInInspector] public Camera mainCamera;

        private SpriteRenderer _spriteRenderer;
        private static readonly int VFXHash = VFXController.StringToHash("BulletImpact");

        private const float KOffScreenError = 0.01f;

        private float _timer;

        public float freezeTime = 5.0f;

        private void OnEnable()
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
            _timer = 0.0f;
        }

        public void ReturnToPool()
        {
            BulletPoolObject.ReturnToPool();
        }

        private void FixedUpdate()
        {
            if (destroyWhenOutOfView)
            {
                var screenPoint = mainCamera.WorldToViewportPoint(transform.position);
                var onScreen = screenPoint.z > 0 && screenPoint.x > -KOffScreenError &&
                               screenPoint.x < 1 + KOffScreenError && screenPoint.y > -KOffScreenError &&
                               screenPoint.y < 1 + KOffScreenError;
                if (!onScreen)
                    BulletPoolObject.ReturnToPool();
            }

            if (!(timeBeforeAutodestruct > 0)) return;
            _timer += Time.deltaTime;
            if (_timer > timeBeforeAutodestruct)
            {
                BulletPoolObject.ReturnToPool();
            }
        }

        public void OnHitDamageable(Damager origin, Damageable damageable)
        {
            FindSurface(origin.LastHit);
        }

        public void OnHitNonDamageable(Damager origin)
        {
            FindSurface(origin.LastHit);
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

            var freezableObject = surfaceCollider2D.gameObject.GetComponent(typeof(FreezableObject)) as FreezableObject;
            if (freezableObject)
            {
                freezableObject.Freeze(freezeTime);
            }

            VFXController.Instance.Trigger(VFXHash, position, 0, _spriteRenderer.flipX, null, surfaceHit);
            var bulletImpact = gameObject.GetComponent<AudioSource>();
            AudioSource.PlayClipAtPoint(bulletImpact.clip, position, 1.0f);

        }

        
    }
}