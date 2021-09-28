using System.Collections;
using Mirror;
using Objects;
using UnityEngine;

namespace Characters.MonoBehaviours
{
    public class LeafCharacter : PlayerCharacter
    {
        public float leafCooldown = 15f;
        public Transform facingLeftLeafSpawnPoint;
        public Transform facingRightLeafSpawnPoint;
        public LeafPool leafPool;

        private Transform _currentLeafSpawnPoint;
        private float _nextThrowTime;
        private Coroutine _throwCoroutine;
        
         private new void Awake()
        {
            base.Awake();
            _currentLeafSpawnPoint =
                spriteOriginallyFacesLeft ?  facingLeftLeafSpawnPoint : facingRightLeafSpawnPoint;
        }

        private new void Start()
        {
            base.Start();
            _nextThrowTime = Time.time;
        }

        private new void FixedUpdate()
        {
            base.FixedUpdate();
            UpdateLeafSpawnPointPositions();
        }

        public override void UpdateFacing()
        {
            base.UpdateFacing();

            UpdateFacingWithoutInput();
        }
        
        private void UpdateFacingWithoutInput()
        {

            _currentLeafSpawnPoint = FacingRight ? facingRightLeafSpawnPoint : facingLeftLeafSpawnPoint;
        }


        private void UpdateLeafSpawnPointPositions()
        {
            Vector2 leftPosition = facingRightLeafSpawnPoint.localPosition;
            leftPosition.x *= -1f;
            facingLeftLeafSpawnPoint.localPosition = leftPosition;
        }
        
        private void SpawnLeafWithoutNotify()
        {
            //we check if there is a wall between the player and the bullet spawn position, if there is, we don't spawn a bullet
            //otherwise, the player can "shoot through wall" because the arm extend to the other side of the wall
            Vector2 testPosition = transform.position;
            var position = _currentLeafSpawnPoint.position;
            testPosition.y = position.y;
            var direction = (Vector2) position - testPosition;
            var distance = direction.magnitude;
            direction.Normalize();

            var results = new RaycastHit2D[12];
            if (Physics2D.Raycast(testPosition, direction, CharacterController2D.ContactFilter, results, distance) > 0)
                return;

            var leaf = leafPool.Pop(_currentLeafSpawnPoint.position);
            var facingLeft = _currentLeafSpawnPoint == facingLeftLeafSpawnPoint;
            leaf.SpriteRenderer.flipX = facingLeft ^ leaf.Leaf.spriteOriginallyFacesLeft;
            
        }
        
        
        [Command]
        private void SpawnLeafServer(bool facingRight)
        {
            FacingRight = facingRight;
            UpdateFacingWithoutInput();
            SpawnLeafWithoutNotify();
            SpawnLeafClient(FacingRight);
        }

        [ClientRpc(includeOwner = false)]
        private void SpawnLeafClient(bool facingRight)
        {
            if (!isClientOnly) return;
            FacingRight = facingRight;
            UpdateFacingWithoutInput();
            SpawnLeafWithoutNotify();
        }

        private void SpawnLeaf()
        {
            SpawnLeafWithoutNotify();
            
            if (isServer)
            {
                SpawnLeafClient(FacingRight);
            }
            else if (isClientOnly)
            {
                SpawnLeafServer(FacingRight);
            }

        }

        private IEnumerator ThrowLeaf()
        {
            while (specialMove)
            {
                if (Time.time >= _nextThrowTime)
                {
                    SpawnLeaf();
                    rangedAttackAudioPlayer.PlayRandomSound();
                    _nextThrowTime = Time.time + leafCooldown;
                }


                yield return null;
            }

           
        }

    
        public override void SpecialMove()
        {
            animator.SetTrigger(HashSpecialMove);
            if (networkAnimator)
            {
                networkAnimator.SetTrigger(HashSpecialMove); 
            }
            
            if (specialMove)
            {
                if (_throwCoroutine == null)
                    _throwCoroutine = StartCoroutine(ThrowLeaf());
            }

            StopCoroutine(ThrowLeaf());
            _throwCoroutine = null;
        
        }
    }
}
