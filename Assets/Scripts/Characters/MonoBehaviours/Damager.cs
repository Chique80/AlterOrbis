using System;
using UnityEngine;
using UnityEngine.Events;

namespace Characters.MonoBehaviours
{
    public class Damager : MonoBehaviour
    {
         [Serializable]
        public class DamageableEvent : UnityEvent<Damager, Damageable>
        { }


        [Serializable]
        public class NonDamageableEvent : UnityEvent<Damager>
        { }

        //call that from inside the onDamageableHIt or OnNonDamageableHit to get what was hit.
        public Collider2D LastHit { get; private set; }

        public int damage = 1;
        public Vector2 offset = new Vector2(1.5f, 1f);
        public Vector2 size = new Vector2(2.5f, 1f);
        [Tooltip("If this is set, the offset x will be changed base on the sprite flipX setting. e.g. Allow to make the damager alway forward in the direction of sprite")]
        public bool offsetBasedOnSpriteFacing = true;
        [Tooltip("SpriteRenderer used to read the flipX value used by offset Based OnSprite Facing")]
        public SpriteRenderer spriteRenderer;
        [Tooltip("If disabled, damager ignore trigger when casting for damage")]
        public bool canHitTriggers;
        public bool disableDamageAfterHit;
        [Tooltip("If set, the player will be forced to respawn to latest checkpoint in addition to loosing life")]
        public bool forceRespawn;
        [Tooltip("If set, an invincible damageable hit will still get the onHit message (but won't loose any life)")]
        public bool ignoreInvincibility;
        public LayerMask hittableLayers;
        public DamageableEvent onDamageableHit;
        public NonDamageableEvent onNonDamageableHit;

        private bool _spriteOriginallyFlipped;
        private bool _canDamage = true;
        private ContactFilter2D _attackContactFilter;
        private readonly Collider2D[] _attackOverlapResults = new Collider2D[10];
        private Transform _damagerTransform;

        private void Awake()
        {
            _attackContactFilter.layerMask = hittableLayers;
            _attackContactFilter.useLayerMask = true;
            _attackContactFilter.useTriggers = canHitTriggers;

            if (offsetBasedOnSpriteFacing && spriteRenderer != null)
                _spriteOriginallyFlipped = spriteRenderer.flipX;

            _damagerTransform = transform;
        }

        public void EnableDamage()
        {
            _canDamage = true;
        }

        public void DisableDamage()
        {
            _canDamage = false;
        }
        
        private void FixedUpdate()
        {
            if (!_canDamage)
                return;

            Vector2 scale = _damagerTransform.lossyScale;

            var facingOffset = Vector2.Scale(offset, scale);
            if (offsetBasedOnSpriteFacing && spriteRenderer != null && spriteRenderer.flipX != _spriteOriginallyFlipped)
                facingOffset = new Vector2(-offset.x * scale.x, offset.y * scale.y);

            var scaledSize = Vector2.Scale(size, scale);

            var pointA = (Vector2)_damagerTransform.position + facingOffset - scaledSize * 0.5f;
            var pointB = pointA + scaledSize;

            var hitCount = Physics2D.OverlapArea(pointA, pointB, _attackContactFilter, _attackOverlapResults);

            for (var i = 0; i < hitCount; i++)
            {
                LastHit = _attackOverlapResults[i];
                var damageable = LastHit.GetComponent<Damageable>();

                if (damageable)
                {
                    onDamageableHit.Invoke(this, damageable);
                    damageable.TakeDamage(this, ignoreInvincibility);
                    if (disableDamageAfterHit)
                        DisableDamage();
                }
                else
                {
                    onNonDamageableHit.Invoke(this);
                }
            }
        }
    }
}
