using System;
using System.Collections;
using Audio;
using Characters.StateMachineBehaviours.Player;
using Core;
using SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Interactions;
using UnityEngine.Tilemaps;
using Utility;
using Mirror;

namespace Characters.MonoBehaviours
{
    /**
     * Main class for characters
     */
    [RequireComponent(typeof(CharacterController2D))]
    [RequireComponent(typeof(Animator))]
    public abstract class PlayerCharacter : NetworkBehaviour
    {
        private PlayerInput _playerInputs;
        protected Rope RopeInfo;

        private const float KMinHurtJumpAngle = 0.001f;
        private const float KMaxHurtJumpAngle = 89.999f;

        private const float
            GroundedStickingVelocityMultiplier =
                3f; // This is to help the character stick to vertically moving platforms.

        public SpriteRenderer[] spriteRenderer;
        public Damageable damageable;
        public Damager meleeDamager;
        public Transform cameraFollowTarget;

        public float maxSpeed = 10f;
        public float groundAcceleration = 100f;
        public float groundDeceleration = 100f;
        [Range(0f, 1f)] public float pushingSpeedProportion;

        [Range(0f, 1f)] public float airborneAccelProportion;
        [Range(0f, 1f)] public float airborneDecelProportion;
        public float gravity = 50f;
        public float jumpSpeed = 20f;
        public float jumpAbortSpeedReduction = 100f;
        public float bounceStrength = 1f;

        [Range(KMinHurtJumpAngle, KMaxHurtJumpAngle)]
        public float hurtJumpAngle = 45f;

        public float hurtJumpSpeed = 5f;
        public float flickeringDuration = 0.1f;

        public float meleeAttackDashSpeed = 5f;

        public RandomAudioPlayer footstepGrassAudioPlayer;
        public RandomAudioPlayer footstepMetalAudioPlayer;
        public RandomAudioPlayer landingAudioPlayer;
        public RandomAudioPlayer hurtAudioPlayer;
        public RandomAudioPlayer meleeAttackAudioPlayer;
        public RandomAudioPlayer rangedAttackAudioPlayer;

        public float cameraHorizontalFacingOffset;
        public float cameraHorizontalSpeedOffset;
        public float cameraVerticalInputOffset;
        public float maxHorizontalDeltaDampTime;
        public float maxVerticalDeltaDampTime;
        public float verticalCameraOffsetDelay;

        public bool spriteOriginallyFacesLeft;

        protected CharacterController2D CharacterController2D;
        [SerializeField] protected Animator animator;
        private CapsuleCollider2D _capsule;
        protected Transform Transform;

        private float _tanHurtJumpAngle;
        private WaitForSeconds _flickeringWait;
        private Coroutine _flickerCoroutine;

        private Checkpoint _lastCheckpoint;
        private Vector2 _startingPosition = Vector2.zero;
        private TileBase _currentSurface;
        public float currentAcceleration;

        [SyncVar] protected bool FacingRight = true;

        private float _camFollowHorizontalSpeed;
        private float _camFollowVerticalSpeed;
        private float _verticalCameraOffsetTimer;

        private bool _inPause;

        private Vector2 _move;

        private bool _receivingInput;


        private Vector2 _moveVector;

        private readonly int _hashHorizontalSpeedPara = Animator.StringToHash("HorizontalSpeed");
        private readonly int _hashVerticalSpeedPara = Animator.StringToHash("VerticalSpeed");
        private readonly int _hashGroundedPara = Animator.StringToHash("Grounded");
        private readonly int _hashMeleeAttack = Animator.StringToHash("MeleeAttack");
        protected readonly int HashSpecialMove = Animator.StringToHash("SpecialMove");
        public bool isJumping;
        public bool specialMove;
        public bool interact;

        public AssetsRefresh.WorldType worldType;

        public bool inZone;

        private bool _isPlayingAudio;

        [SerializeField] protected NetworkAnimator networkAnimator;


        public override void OnStartAuthority()
        {
            networkAnimator = GetComponent<NetworkAnimator>();
            if (worldType != AssetsRefresh.Instance.currentWorldType &&
                AssetsRefresh.Instance.hasSwitchedWorld == false)
            {
                AssetsRefresh.ChangeWorld(worldType);
            }
            else if (worldType == AssetsRefresh.Instance.currentWorldType &&
                     AssetsRefresh.Instance.hasSwitchedWorld)
            {
                AssetsRefresh.Instance.SwitchWorld();
            }
            
            BackgroundMusicPlayer._instance.UpdateMusicFromWorld();

            worldType = AssetsRefresh.Instance.currentWorldType;
        }

        public override void OnStartLocalPlayer()
        {
            if (!(FindObjectsOfType(typeof(PlayerCharacter)) is PlayerCharacter[] players)) return;
            foreach (var player in players)
            {
                if (isLocalPlayer && !player.isLocalPlayer && player.worldType != worldType)
                {
                    player.MakeEthereal();
                }
            }
        }

        public override void OnStartClient()
        {
            if (!(FindObjectsOfType(typeof(PlayerCharacter)) is PlayerCharacter[] players)) return;
            foreach (var player in players)
            {
                if (isLocalPlayer && !player.isLocalPlayer && player.worldType != worldType)
                {
                    player.MakeEthereal();
                }
            }
        }

        protected void Awake()
        {
            CharacterController2D = GetComponent<CharacterController2D>();
            animator = GetComponent<Animator>();
            _capsule = GetComponent<CapsuleCollider2D>();
            _playerInputs = GetComponent<PlayerInput>();
            Transform = transform;
            RopeInfo = GameObject.Find("RopeManager").GetComponent<Rope>();
        }

        // Start is called before the first frame update
        protected void Start()
        {
            hurtJumpAngle = Mathf.Clamp(hurtJumpAngle, KMinHurtJumpAngle, KMaxHurtJumpAngle);
            _tanHurtJumpAngle = Mathf.Tan(Mathf.Deg2Rad * hurtJumpAngle);
            _flickeringWait = new WaitForSeconds(flickeringDuration);

            meleeDamager.DisableDamage();

            if (!Mathf.Approximately(maxHorizontalDeltaDampTime, 0f))
            {
                var maxHorizontalDelta = maxSpeed * cameraHorizontalSpeedOffset + cameraHorizontalFacingOffset;
                _camFollowHorizontalSpeed = maxHorizontalDelta / maxHorizontalDeltaDampTime;
            }

            if (!Mathf.Approximately(maxVerticalDeltaDampTime, 0f))
            {
                var maxVerticalDelta = cameraVerticalInputOffset;
                _camFollowVerticalSpeed = maxVerticalDelta / maxVerticalDeltaDampTime;
            }

            SceneLinkedSmb<PlayerCharacter>.Initialise(animator, this);

            _startingPosition = transform.position;
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            // ReSharper disable once Unity.UnknownTag
            if (!other.gameObject.CompareTag("Bounce")) return;
            SetVerticalMovement(currentAcceleration);
            currentAcceleration *= airborneDecelProportion;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            var checkpoint = other.GetComponent<Checkpoint>();
            if (checkpoint == null) return;
            inZone = true;
            MakeReal();
            // ReSharper disable once Unity.UnknownTag
            /*Pushable pushable = other.GetComponent<Pushable>();
            if (pushable != null)
            {
                m_CurrentPushables.Add(pushable);
            }*/
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            var checkpoint = other.GetComponent<Checkpoint>();
            if (!ClientScene.localPlayer) return;
            if (!(ClientScene.localPlayer.gameObject.GetComponent(typeof(PlayerCharacter)) is PlayerCharacter
                localPlayer) || checkpoint == null || worldType == localPlayer.worldType || isLocalPlayer) return;
            inZone = false;
            MakeEthereal();
            /*Pushable pushable = other.GetComponent<Pushable>();
            if (pushable == null) return;
            if (m_CurrentPushables.Contains(pushable))
                m_CurrentPushables.Remove(pushable);*/
        }

        private void MakeEthereal()
        {
            foreach (var sprite in spriteRenderer)
            {
                sprite.color = new Color(0.16f, 0.16f, 0.16f, 0.33f);
            }
        }

        private void MakeReal()
        {
            foreach (var sprite in spriteRenderer)
            {
                sprite.color = new Color(1, 1, 1, 1);
            }
        }


        protected void Update()
        {
            if (FindObjectsOfType(typeof(PlayerCharacter)) is PlayerCharacter[] players)
            {
                foreach (var player in players)
                {
                    if (isLocalPlayer && !player.isLocalPlayer && player.worldType != worldType && !player.inZone)
                    {
                        player.MakeEthereal();
                    }
                }
            }


            if (_inPause)
            {
                if (ScreenFader.IsFading)
                    return;

                _playerInputs.currentActionMap.Disable();
                _playerInputs.actions["Pause"].Enable();
                _inPause = true;
                Time.timeScale = 0;
                //UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("UIMenus", UnityEngine.SceneManagement.LoadSceneMode.Additive);
            }
            else
            {
                Unpause();
            }

            if (!ClientScene.localPlayer || isLocalPlayer)
            {
                if (RopeInfo == null)
                {
                    RopeInfo = GameObject.Find("RopeManager").GetComponent<Rope>();
                }

                if (RopeInfo != null && RopeInfo.isOnRope)
                {
                    _playerInputs.currentActionMap.Disable();
                    RopeInfo.ridingIterationTimer = 0;
                    var position = transform.position;
                    var closestObjectPosition =
                        Vector2.Distance(
                            RopeInfo.CurrentRopeSegment.GameObj.GetComponent<RopeProperties>().object1.transform
                                .position, position) > Vector2.Distance(
                            RopeInfo.CurrentRopeSegment.GameObj.GetComponent<RopeProperties>().object2.transform
                                .position, position)
                            ? RopeInfo.CurrentRopeSegment.GameObj.GetComponent<RopeProperties>().object2.transform
                                .position
                            : RopeInfo.CurrentRopeSegment.GameObj.GetComponent<RopeProperties>().object1.transform
                                .position;
                    if (RopeInfo.isOnRope && RopeInfo.ridingIterationTimer < Time.time &&
                        RopeInfo.GetDistanceAchieved() <= RopeInfo.GetDistanceToTravel())
                    {
                        RopeInfo.RideRope(gameObject, FacingRight);
                        RopeInfo.ridingIterationTimer = Time.time + RopeInfo.ridingIterationDelay;
                    }
                    else
                    {
                        switch (FacingRight)
                        {
                            case true when position.x < closestObjectPosition.x && position.y < closestObjectPosition.y:
                                RopeInfo.RideRope(gameObject, FacingRight);
                                RopeInfo.ridingIterationTimer = Time.time + RopeInfo.ridingIterationDelay;
                                break;
                            case false
                                when position.x > closestObjectPosition.x && position.y > closestObjectPosition.y:
                                RopeInfo.RideRope(gameObject, FacingRight);
                                RopeInfo.ridingIterationTimer = Time.time + RopeInfo.ridingIterationDelay;
                                break;
                            default:
                                RopeInfo.DisembarkRope(gameObject);
                                GameObjectTeleporter.Teleport(gameObject, closestObjectPosition);
                                _playerInputs.currentActionMap.Enable();
                                break;
                        }
                    }
                }
            }


            CharacterController2D.Move(_moveVector * Time.deltaTime);


            if (CheckForGrounded() && (_moveVector.x > 0.1f || _moveVector.x < -0.1f) && _isPlayingAudio == false)
            {
                StartCoroutine(PlayFootstep());
            }

            animator.SetFloat(_hashHorizontalSpeedPara, _moveVector.x);
            animator.SetFloat(_hashVerticalSpeedPara, _moveVector.y);

            if(isLocalPlayer)
            {
                if (isServer)
                {
                    MoveClient(_moveVector);
                }
                else if (isClientOnly)
                {
                    MoveServer(_moveVector);
                }
            }
        }

        [Command]
        private void MoveServer(Vector2 movement)
        {
            animator.SetFloat(_hashHorizontalSpeedPara, movement.x);
            animator.SetFloat(_hashVerticalSpeedPara, movement.y);
            MoveClient(movement);
        }

        [ClientRpc(includeOwner = false)]
        private void MoveClient(Vector2 movement)
        {
            animator.SetFloat(_hashHorizontalSpeedPara, movement.x);
            animator.SetFloat(_hashVerticalSpeedPara, movement.y);
        }


        protected void FixedUpdate()
        {
            UpdateCameraFollowTargetPosition();
        }

        private void Unpause()
        {
            //if the timescale is already > 0, we 
            if (Time.timeScale > 0)
                return;

            StartCoroutine(UnpauseCoroutine());
        }


        private IEnumerator UnpauseCoroutine()
        {
            Time.timeScale = 1;
            // UnityEngine.SceneManagement.SceneManager.UnloadSceneAsync("UIMenus");
            _playerInputs.currentActionMap.Enable();
            //we have to wait for a fixed update so the pause button state change, otherwise we can get in case were the update
            //of this script happen BEFORE the input is updated, leading to setting the game in pause once again
            yield return new WaitForFixedUpdate();
            yield return new WaitForEndOfFrame();
            _inPause = false;
        }

        private void UpdateCameraFollowTargetPosition()
        {
            var newLocalPosY = 0f;

            var desiredLocalPosX = (spriteOriginallyFacesLeft ^ spriteRenderer[0].flipX ? -1f : 1f) *
                                   cameraHorizontalFacingOffset;
            desiredLocalPosX += _moveVector.x * cameraHorizontalSpeedOffset;

            var newLocalPosX = Mathf.Approximately(_camFollowHorizontalSpeed, 0f)
                ? desiredLocalPosX
                : Mathf.Lerp(cameraFollowTarget.localPosition.x, desiredLocalPosX,
                    _camFollowHorizontalSpeed * Time.deltaTime);

            var moveVertically = false;
            if (!Mathf.Approximately(_move.y, 0f))
            {
                _verticalCameraOffsetTimer += Time.deltaTime;

                if (_verticalCameraOffsetTimer >= verticalCameraOffsetDelay)
                    moveVertically = true;
            }
            else
            {
                moveVertically = true;
                _verticalCameraOffsetTimer = 0f;
            }

            if (moveVertically)
            {
                var desiredLocalPosY = _move.y * cameraVerticalInputOffset;
                if (Mathf.Approximately(_camFollowVerticalSpeed, 0f))
                    newLocalPosY = desiredLocalPosY;
                else
                    newLocalPosY = Mathf.MoveTowards(cameraFollowTarget.localPosition.y, desiredLocalPosY,
                        _camFollowVerticalSpeed * Time.deltaTime);
            }

            cameraFollowTarget.localPosition = new Vector2(newLocalPosX, newLocalPosY);
        }

        private IEnumerator Flicker()
        {
            var timer = 0f;

            while (timer < damageable.invulnerabilityDuration)
            {
                foreach (var sprites in spriteRenderer)
                {
                    sprites.enabled = !sprites.enabled;
                }

                yield return _flickeringWait;
                timer += flickeringDuration;
            }

            foreach (var sprites in spriteRenderer)
            {
                sprites.enabled = true;
            }
        }

        public void SetMoveVector(Vector2 newMoveVector)
        {
            _moveVector = newMoveVector;
        }

        public void TeleportToColliderBottom()
        {
            var colliderBottom = CharacterController2D.Rigidbody2D.position + _capsule.offset +
                                 Vector2.down * _capsule.size.y * 0.5f;
            CharacterController2D.Teleport(colliderBottom);
        }

        public virtual void UpdateFacing()
        {
            var faceRight = _move.x > 0f;

            if (FacingRight == faceRight || _move.x == 0f) return;
            FacingRight = faceRight;
            Transform.Find("body").Rotate(0, 180, 0);
        }

        private void UpdateFacing(bool faceLeft)
        {
            if (faceLeft)
            {
                Transform.Find("body").Rotate(0, 180, 0);
            }
        }

        public float GetFacing()
        {
            return spriteRenderer[0].flipX != spriteOriginallyFacesLeft ? -1f : 1f;
        }

        public void SetHorizontalMovement(float newHorizontalMovement)
        {
            _moveVector.x = newHorizontalMovement;
        }

        public void SetVerticalMovement(float newVerticalMovement)
        {
            _moveVector.y = newVerticalMovement;
        }

        public void IncrementMovement(Vector2 additionalMovement)
        {
            _moveVector += additionalMovement;
        }

        public void IncrementHorizontalMovement(float additionalHorizontalMovement)
        {
            _moveVector.x += additionalHorizontalMovement;
        }

        public void IncrementVerticalMovement(float additionalVerticalMovement)
        {
            _moveVector.y += additionalVerticalMovement;
        }

        public void GroundedVerticalMovement()
        {
            _moveVector.y -= gravity * Time.deltaTime;

            if (_moveVector.y < -gravity * Time.deltaTime * GroundedStickingVelocityMultiplier)
            {
                _moveVector.y = -gravity * Time.deltaTime * GroundedStickingVelocityMultiplier;
            }
        }

        public void GroundedHorizontalMovement(float speedScale = 1f)
        {
            var desiredSpeed = _move.x * maxSpeed * speedScale;
            var acceleration = _receivingInput
                ? groundAcceleration
                : groundDeceleration;
            _moveVector.x = Mathf.MoveTowards(_moveVector.x, desiredSpeed, acceleration * Time.deltaTime);
        }

        public bool CheckForGrounded()
        {
            var wasGrounded = animator.GetBool(_hashGroundedPara);
            var grounded = CharacterController2D.IsGrounded;

            if (grounded)
            {
                FindCurrentSurface();

                if (!wasGrounded && _moveVector.y < -1.0f)
                {
                    //only play the landing sound if falling "fast" enough (avoid small bump playing the landing sound)
                    //landingAudioPlayer.PlayRandomSound(_currentSurface);
                }
            }
            else
            {
                _currentSurface = null;
            }


            animator.SetBool(_hashGroundedPara, grounded);
            if (networkAnimator)
            {
                //NetworkAnimator.SetTrigger(_hashGroundedPara);
            }

            return grounded;
        }

        private void FindCurrentSurface()
        {
            var groundCollider = CharacterController2D.GroundColliders[0];

            if (groundCollider == null)
                groundCollider = CharacterController2D.GroundColliders[1];

            if (groundCollider == null)
                return;

            var b = PhysicsHelper.FindTileForOverride(groundCollider, transform.position, Vector2.down);
            if (b != null)
            {
                _currentSurface = b;
            }
        }

        public void UpdateJump()
        {
            if (!isJumping && _moveVector.y > 0.0f)
            {
                _moveVector.y -= jumpAbortSpeedReduction * Time.deltaTime;
            }
        }

        public void AirborneHorizontalMovement()
        {
            var desiredSpeed = _move.x * maxSpeed;

            float acceleration;

            if (_receivingInput)
                acceleration = groundAcceleration * airborneAccelProportion;
            else
                acceleration = groundDeceleration * airborneDecelProportion;

            _moveVector.x = Mathf.MoveTowards(_moveVector.x, desiredSpeed, acceleration * Time.deltaTime);
        }

        public void AirborneVerticalMovement()
        {
            if (Mathf.Approximately(_moveVector.y, 0f) || CharacterController2D.IsCeilinged && _moveVector.y > 0f)
            {
                _moveVector.y = 0f;
            }

            _moveVector.y -= gravity * Time.deltaTime;
        }


        public void OnMove(InputAction.CallbackContext context)
        {
            _move = context.ReadValue<Vector2>();
            switch (context.phase)
            {
                case InputActionPhase.Performed:
                    _receivingInput = context.interaction is SlowTapInteraction;
                    break;

                case InputActionPhase.Started:
                    if (context.interaction is SlowTapInteraction)
                        _receivingInput = true;
                    break;

                case InputActionPhase.Canceled:
                    _receivingInput = true;
                    break;
                case InputActionPhase.Disabled:
                    break;
                case InputActionPhase.Waiting:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OnJump(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Performed:

                    isJumping = true;


                    break;
                case InputActionPhase.Started:
                    if (context.interaction is SlowTapInteraction)
                    {
                        isJumping = true;
                    }

                    break;
                case InputActionPhase.Canceled:
                    isJumping = false;
                    break;
                case InputActionPhase.Disabled:
                    break;
                case InputActionPhase.Waiting:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OnSpecialMove(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    if (context.interaction is SlowTapInteraction)
                        specialMove = true;
                    break;
                case InputActionPhase.Canceled:
                    specialMove = false;
                    break;
                case InputActionPhase.Disabled:
                    specialMove = false;
                    break;
                case InputActionPhase.Waiting:
                    break;
                case InputActionPhase.Performed:
                    specialMove = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void OnInteract(InputAction.CallbackContext context)
        {
            switch (context.phase)
            {
                case InputActionPhase.Started:
                    if (context.interaction is SlowTapInteraction)
                        interact = true;
                    break;
                case InputActionPhase.Canceled:
                    interact = false;
                    break;
                case InputActionPhase.Disabled:
                    interact = false;
                    break;
                case InputActionPhase.Waiting:
                    break;
                case InputActionPhase.Performed:
                    interact = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public void EmbarkOnRope()
        {
            if (!RopeInfo.isOnRope)
                RopeInfo.EmbarkOnRope(gameObject, FacingRight);
        }

        public Vector2 GetHurtDirection()
        {
            var damageDirection = damageable.GetDamageDirection();

            if (damageDirection.y < 0f)
                return new Vector2(Mathf.Sign(damageDirection.x), 0f);

            var y = Mathf.Abs(damageDirection.x) * _tanHurtJumpAngle;

            return new Vector2(damageDirection.x, y).normalized;
        }

        public void OnHurt(Damager damager, Damageable myDamageable)
        {
            //if the player don't have control, we shouldn't be able to be hurt as this wouldn't be fair
            if (!_playerInputs.currentActionMap.enabled)
                return;

            //UpdateFacing(damageable.GetDamageDirection().x > 0f);
            myDamageable.EnableInvulnerability();

            //m_Animator.SetTrigger(m_HashHurtPara);

            //we only force respawn if health > 0, otherwise both forceRespawn & Death trigger are set in the animator, messing with each other.
            // if (myDamageable.CurrentHealth > 0 && damager.forceRespawn)
            //Animator.SetTrigger(_hashForcedRespawnPara);

            animator.SetBool(_hashGroundedPara, false);

            //hurtAudioPlayer.PlayRandomSound();

            //if the health is < 0, mean die callback will take care of respawn
            if (damager.forceRespawn && myDamageable.CurrentHealth > 0)
            {
                if(isLocalPlayer)
                {
                    StartCoroutine(DieRespawnCoroutine(true, true));
                }
            }
        }

        public void OnDie()
        {
            if(isLocalPlayer)
            {
                StartCoroutine(DieRespawnCoroutine(true, true));
            }
        }

        private IEnumerator DieRespawnCoroutine(bool resetHealth, bool useCheckPoint)
        {
            _playerInputs.currentActionMap.Disable();
            yield return new WaitForSeconds(1.0f); //wait one second before respawing
            yield return StartCoroutine(
                ScreenFader.FadeSceneOut(useCheckPoint ? ScreenFader.FadeType.Black : ScreenFader.FadeType.GameOver));
            if (!useCheckPoint)
                yield return new WaitForSeconds(2f);
            Respawn(resetHealth, useCheckPoint);
            yield return new WaitForEndOfFrame();
            yield return StartCoroutine(ScreenFader.FadeSceneIn());
            _playerInputs.currentActionMap.Enable();
        }

        public void StartFlickering()
        {
            _flickerCoroutine = StartCoroutine(Flicker());
        }

        private void StopFlickering()
        {
            StopCoroutine(_flickerCoroutine);

            foreach (var sprite in spriteRenderer)
            {
                sprite.enabled = true;
            }
        }


        public void EnableMeleeAttack()
        {
            meleeDamager.EnableDamage();
            meleeDamager.disableDamageAfterHit = true;
        }

        public void DisableMeleeAttack()
        {
            meleeDamager.DisableDamage();
        }

        public void MeleeAttack()
        {
            animator.SetTrigger(_hashMeleeAttack);
        }

        public abstract void SpecialMove();

        private IEnumerator PlayFootstep()
        {
            _isPlayingAudio = true;
            if (AssetsRefresh.Instance.currentWorldType == AssetsRefresh.WorldType.Organic)
            {
                footstepGrassAudioPlayer.PlayRandomSound(_currentSurface);
            }
            else
            {
                footstepMetalAudioPlayer.PlayRandomSound(_currentSurface);
            }

            var footstepPosition = transform.position;
            footstepPosition.z -= 1;
            VFXController.Instance.Trigger("DustPuff", footstepPosition, 0, false, null, _currentSurface);
            yield return new WaitForSeconds(0.5f);
            _isPlayingAudio = false;
        }

        private void Respawn(bool resetHealth, bool useCheckpoint)
        {
            if (resetHealth)
                damageable.SetHealth(damageable.startingHealth);

            //we reset the hurt trigger, as we don't want the player to go back to hurt animation once respawned
            //Animator.ResetTrigger(_hashHurtPara);
            if (_flickerCoroutine != null)
            {
                //we stop flickering for the same reason
                StopFlickering();
            }

            //Animator.SetTrigger(_hashRespawnPara);

            if (useCheckpoint && _lastCheckpoint != null)
            {
                UpdateFacing(_lastCheckpoint.respawnFacingLeft);
                GameObjectTeleporter.Teleport(gameObject, _lastCheckpoint.transform.position);
            }
            else
            {
                //UpdateFacing(_startingFacingLeft);
                GameObjectTeleporter.Teleport(gameObject, _startingPosition);
            }
        }


        public void SetCheckpoint(Checkpoint checkpoint)
        {
            _lastCheckpoint = checkpoint;
        }
    }
}