using System.Collections;
using System.Linq;
using Mirror;
using Objects;
using UnityEngine;

namespace Characters.MonoBehaviours
{
    public class SnailCharacter : PlayerCharacter
    {
        public float shellCooldown = 15f;
        public Transform facingLeftShellSpawnPoint;
        public Transform facingRightShellSpawnPoint;
        
        public ShellPool shellPool;

        private Transform _currentShellSpawnPoint;

        private float _nextThrowTime;
        private Coroutine _throwCoroutine;


        private new void Awake()
        {
            base.Awake();
            _currentShellSpawnPoint =
                spriteOriginallyFacesLeft ? facingRightShellSpawnPoint : facingLeftShellSpawnPoint;
        }

        private new void Start()
        {
            base.Start();
            _nextThrowTime = Time.time;
        }

        private new void FixedUpdate()
        {
            base.FixedUpdate();
            UpdateShellSpawnPointPositions();
            if (shellPool.pool.Count > 0 && !shellPool.pool.First().Shell.isActiveAndEnabled &&
                !transform.Find("body").Find("UpperBody").Find("Shell").gameObject.activeSelf)
            {
                transform.Find("body").Find("UpperBody").Find("Shell").gameObject.SetActive(true);
            }
        }

        public override void UpdateFacing()
        {
            base.UpdateFacing();

            UpdateFacingWithoutInput();
        }
        
        private void UpdateFacingWithoutInput()
        {
            base.UpdateFacing();
            _currentShellSpawnPoint = FacingRight ? facingLeftShellSpawnPoint : facingRightShellSpawnPoint;
        }

        private void UpdateShellSpawnPointPositions()
        {
            Vector2 leftPosition = facingRightShellSpawnPoint.localPosition;
            leftPosition.x *= -1f;
            facingLeftShellSpawnPoint.localPosition = leftPosition;
        }


        private void SpawnShellWithoutNotify()
        {
            //we check if there is a wall between the player and the bullet spawn position, if there is, we don't spawn a bullet
            //otherwise, the player can "shoot through wall" because the arm extend to the other side of the wall
            Vector2 testPosition = transform.position;
            var position = _currentShellSpawnPoint.position;
            testPosition.y = position.y;
            var direction = (Vector2) position - testPosition;
            var distance = direction.magnitude;
            direction.Normalize();

            var results = new RaycastHit2D[12];
            if (Physics2D.Raycast(testPosition, direction, CharacterController2D.ContactFilter, results, distance) > 0)
                return;

            var shell = shellPool.Pop(_currentShellSpawnPoint.position);
            var facingRight = _currentShellSpawnPoint == facingRightShellSpawnPoint;
            shell.SpriteRenderer.flipY = facingRight ^ shell.Shell.spriteOriginallyFacesLeft;
            Transform.Find("body").Find("UpperBody").Find("Shell").gameObject.SetActive(false);
        }
        
        [Command]
        private void SpawnShellServer(bool facingRight )
        {
            FacingRight = facingRight;
            UpdateFacingWithoutInput();
            SpawnShellWithoutNotify();
            SpawnShellClient(FacingRight);
        }

        [ClientRpc(includeOwner = false)]
        private void SpawnShellClient(bool facingRight)
        {
            if (!isClientOnly) return;
            FacingRight = facingRight;
            UpdateFacingWithoutInput();
            SpawnShellWithoutNotify();
        }


        private void SpawnShell()
        {
            SpawnShellWithoutNotify();

            if (isServer)
            {
                SpawnShellClient(FacingRight);
            }
            else if (isClientOnly)
            {
                SpawnShellServer(FacingRight);
            }
        }

        private IEnumerator ThrowShell()
        {
            while (specialMove)
            {
                if (Time.time >= _nextThrowTime)
                {
                    SpawnShell();
                    _nextThrowTime = Time.time + shellCooldown;
                }


                yield return null;
            }

            //_transform.Find("body").Find("UpperBody").Find("Shell").gameObject.SetActive(true);
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
                    _throwCoroutine = StartCoroutine(ThrowShell());
            }

            StopCoroutine(ThrowShell());
            _throwCoroutine = null;
        }
    }
}