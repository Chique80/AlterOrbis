using System.Collections;
using Mirror;
using Objects;
using UnityEngine;

namespace Characters.MonoBehaviours
{
    public class StasisCharacter : PlayerCharacter
    {
        public float shotsPerSecond = 1f;
        public float bulletSpeed = 5f;
        public Transform facingLeftBulletSpawnPoint;
        public Transform facingRightBulletSpawnPoint;
        public BulletPool bulletPool;
        public bool rightBulletSpawnPointAnimated = true;

        private Transform _currentBulletSpawnPoint;
        private float _shotSpawnGap;
        private float _nextShotTime;
        private Coroutine _shootingCoroutine;

        private new void Start()
        {
            base.Start();
            _shotSpawnGap = 1f / shotsPerSecond;
            _nextShotTime = Time.time;

        }

        private new void Awake()
        {
            base.Awake();
            _currentBulletSpawnPoint =
                spriteOriginallyFacesLeft ? facingLeftBulletSpawnPoint : facingRightBulletSpawnPoint;
        }

        private new void FixedUpdate()
        {
            base.FixedUpdate();
            UpdateBulletSpawnPointPositions();
        }

        public override void UpdateFacing()
        {
            base.UpdateFacing();

            UpdateFacingWithoutInput();

        }
        
        private void UpdateFacingWithoutInput()
        {

            _currentBulletSpawnPoint = FacingRight ? facingRightBulletSpawnPoint : facingLeftBulletSpawnPoint;

        }

        private void UpdateBulletSpawnPointPositions()
        {
            Vector2 leftPosition = facingRightBulletSpawnPoint.localPosition;
            leftPosition.x *= -1f;
            facingLeftBulletSpawnPoint.localPosition = leftPosition;
        }
        
        private void SpawnBulletWithoutNotify()
        {
            //we check if there is a wall between the player and the bullet spawn position, if there is, we don't spawn a bullet
            //otherwise, the player can "shoot through wall" because the arm extend to the other side of the wall
            Vector2 testPosition = transform.position;
            var position = _currentBulletSpawnPoint.position;
            testPosition.y = position.y;
            var direction = (Vector2) position - testPosition;
            var distance = direction.magnitude;
            direction.Normalize();

            var results = new RaycastHit2D[12];
            if (Physics2D.Raycast(testPosition, direction, CharacterController2D.ContactFilter, results, distance) > 0)
                return;

            var bullet = bulletPool.Pop(_currentBulletSpawnPoint.position);
            var facingLeft = _currentBulletSpawnPoint == facingLeftBulletSpawnPoint;
            bullet.Rigidbody2D.velocity = new Vector2(facingLeft ? -bulletSpeed : bulletSpeed, 0f);
            bullet.SpriteRenderer.flipX = facingLeft ^ bullet.Bullet.spriteOriginallyFacesLeft;

            rangedAttackAudioPlayer.PlayRandomSound();
            
        }
        
        
        [Command]
        private void SpawnBulletServer(bool facingRight)
        {
            FacingRight = facingRight;
            UpdateFacingWithoutInput();
            SpawnBulletWithoutNotify();
            SpawnBulletClient(FacingRight);
        }

        [ClientRpc(includeOwner = false)]
        private void SpawnBulletClient(bool facingRight)
        {
            if (!isClientOnly) return;
            FacingRight = facingRight;
            UpdateFacingWithoutInput();
            SpawnBulletWithoutNotify();
        }
        
        
        private void SpawnBullet()
        {
            SpawnBulletWithoutNotify();
            
            if (isServer)
            {
                SpawnBulletClient(FacingRight);
            }
            else if (isClientOnly) 
            {
                SpawnBulletServer(FacingRight);
            }
        }

       

        private IEnumerator Shoot()
        {
            while (specialMove)
            {
                if (Time.time >= _nextShotTime)
                {
                    SpawnBullet();
                    _nextShotTime = Time.time + _shotSpawnGap;
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
                if (_shootingCoroutine == null)
                    _shootingCoroutine = StartCoroutine(Shoot());
            }
            
            
            StopCoroutine(_shootingCoroutine);
            _shootingCoroutine = null;

        }
    }
}