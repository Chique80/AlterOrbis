using UnityEngine;
using Utility;

namespace Characters.MonoBehaviours
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(Collider2D))]
    public class CharacterController2D : MonoBehaviour
    {
        public LayerMask groundedLayerMask;
        public float groundedRaycastDistance = 0.1f;

        private CapsuleCollider2D _capsule;
        private Vector2 _previousPosition;
        private Vector2 _currentPosition;
        private Vector2 _nextMovement;
        private ContactFilter2D _contactFilter;
        private readonly RaycastHit2D[] _hitBuffer = new RaycastHit2D[5];
        private readonly RaycastHit2D[] _foundHits = new RaycastHit2D[3];
        private readonly Vector2[] _raycastPositions = new Vector2[3];

        public bool IsGrounded { get; private set; }
        public bool IsCeilinged { get; private set; }
        private Vector2 Velocity { get; set; }

        public Rigidbody2D Rigidbody2D { get; private set; }

        public Collider2D[] GroundColliders { get; } = new Collider2D[3];

        public ContactFilter2D ContactFilter => _contactFilter;

        private void Awake()
        {
            Rigidbody2D = GetComponent<Rigidbody2D>();
            _capsule = GetComponent<CapsuleCollider2D>();

            var position = Rigidbody2D.position;
            _currentPosition = position;
            _previousPosition = position;

            _contactFilter.layerMask = groundedLayerMask;
            _contactFilter.useLayerMask = true;
            _contactFilter.useTriggers = false;

            Physics2D.queriesStartInColliders = false;
        }

        private void FixedUpdate()
        {
            _previousPosition = Rigidbody2D.position;
            _currentPosition = _previousPosition + _nextMovement;
            Velocity = (_currentPosition - _previousPosition) / Time.deltaTime;

            Rigidbody2D.MovePosition(_currentPosition);
            _nextMovement = Vector2.zero;

            CheckCapsuleEndCollisions();
            CheckCapsuleEndCollisions(false);
        }

        /// <summary>
        /// This moves a rigidbody and so should only be called from FixedUpdate or other Physics messages.
        /// </summary>
        /// <param name="movement">The amount moved in global coordinates relative to the rigidbody2D's position.</param>
        public void Move(Vector2 movement)
        {
            _nextMovement += movement;
        }

        /// <summary>
        /// This moves the character without any implied velocity.
        /// </summary>
        /// <param name="position">The new position of the character in global space.</param>
        public void Teleport(Vector2 position)
        {
            var delta = position - _currentPosition;
            _previousPosition += delta;
            _currentPosition = position;
            Rigidbody2D.MovePosition(position);
        }

        private void CheckCapsuleEndCollisions(bool bottom = true)
        {
            Vector2 raycastDirection;
            Vector2 raycastStart;
            float raycastDistance;

            if (_capsule == null)
            {
                raycastStart = Rigidbody2D.position + Vector2.up;
                raycastDistance = 1f + groundedRaycastDistance;

                if (bottom)
                {
                    raycastDirection = Vector2.down;

                    _raycastPositions[0] = raycastStart + Vector2.left * 0.4f;
                    _raycastPositions[1] = raycastStart;
                    _raycastPositions[2] = raycastStart + Vector2.right * 0.4f;
                }
                else
                {
                    raycastDirection = Vector2.up;

                    _raycastPositions[0] = raycastStart + Vector2.left * 0.4f;
                    _raycastPositions[1] = raycastStart;
                    _raycastPositions[2] = raycastStart + Vector2.right * 0.4f;
                }
            }
            else
            {
                var size = _capsule.size;
                raycastStart = Rigidbody2D.position + _capsule.offset;
                raycastDistance = size.x * 0.5f + groundedRaycastDistance * 2f;

                if (bottom)
                {
                    raycastDirection = Vector2.down;

                    var raycastStartBottomCentre = raycastStart + Vector2.down * (size.y * 0.5f - size.x * 0.5f);

                    _raycastPositions[0] = raycastStartBottomCentre + Vector2.left * (size.x * 0.5f);
                    _raycastPositions[1] = raycastStartBottomCentre;
                    _raycastPositions[2] = raycastStartBottomCentre + Vector2.right * (size.x * 0.5f);
                }
                else
                {
                    raycastDirection = Vector2.up;
                    var raycastStartTopCentre = raycastStart + Vector2.up * (size.y * 0.5f - size.x * 0.5f);

                    _raycastPositions[0] = raycastStartTopCentre + Vector2.left * (size.x * 0.5f);
                    _raycastPositions[1] = raycastStartTopCentre;
                    _raycastPositions[2] = raycastStartTopCentre + Vector2.right * (size.x * 0.5f);
                }
            }

            for (var i = 0; i < _raycastPositions.Length; i++)
            {
                var count = Physics2D.Raycast(_raycastPositions[i], raycastDirection, _contactFilter, _hitBuffer,
                    raycastDistance);

                if (bottom)
                {
                    _foundHits[i] = count > 0 ? _hitBuffer[0] : new RaycastHit2D();
                    GroundColliders[i] = _foundHits[i].collider;
                }
                else
                {
                    IsCeilinged = false;

                    foreach (var hit in _hitBuffer)
                    {
                        if (hit.collider == null) continue;
                        if (!PhysicsHelper.ColliderHasPlatformEffector(hit.collider))
                        {
                            IsCeilinged = true;
                        }
                    }
                }
            }

            if (bottom)
            {
                var groundNormal = Vector2.zero;
                var hitCount = 0;

                foreach (var hit in _foundHits)
                {
                    if (hit.collider == null) continue;
                    groundNormal += hit.normal;
                    hitCount++;
                }

                if (hitCount > 0)
                {
                    groundNormal.Normalize();
                }

                var relativeVelocity = Velocity;
                foreach (var groundCollider in GroundColliders)
                {
                    if (groundCollider == null)
                        continue;

                    if (!PhysicsHelper.TryGetMovingPlatform(groundCollider, out var movingPlatform)) continue;
                    if (movingPlatform.started)
                    {
                        relativeVelocity -= (movingPlatform.Velocity / Time.deltaTime) + new Vector2(0, 2); 
                    }
                    break;
                }

                if (Mathf.Approximately(groundNormal.x, 0f) && Mathf.Approximately(groundNormal.y, 0f))
                {
                    IsGrounded = false;
                }
                else
                {
                    IsGrounded = relativeVelocity.y <= 0f;

                    if (_capsule != null)
                    {
                        if (GroundColliders[1] != null)
                        {
                            var capsuleBottomHeight =
                                Rigidbody2D.position.y + _capsule.offset.y - _capsule.size.y * 0.5f;
                            var middleHitHeight = _foundHits[1].point.y;
                            IsGrounded &= middleHitHeight < capsuleBottomHeight + groundedRaycastDistance;
                        }
                    }
                }
            }

            for (var i = 0; i < _hitBuffer.Length; i++)
            {
                _hitBuffer[i] = new RaycastHit2D();
            }
        }
    }
}