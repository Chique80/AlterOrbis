using System;
using Mirror;
using UnityEngine;
using Utility;

namespace Objects.Platforms
{
    [SelectionBase]
    [RequireComponent(typeof(Rigidbody2D))]
    public class MovingPlatform : FreezableObject
    {
        public enum MovingPlatformType
        {
            BackForth,
            Loop,
            Once
        }

        public PlatformCatcher platformCatcher;
        public float speed = 1.0f;
        public MovingPlatformType platformType;

        public bool startMovingOnlyWhenVisible;
        public bool isMovingAtStart = true;

        [HideInInspector] public Vector3[] localNodes = new Vector3[1];

        public float[] waitTimes = new float[1];

        public Vector3[] WorldNode { get; private set; }

        private int _current;
        private int _next;
        private int _dir = 1;

        private float _waitTime = -1.0f;

        private Rigidbody2D _rigidbody2D;


        private bool _veryFirstStart;

        public Vector2 Velocity { get; private set; }

        private void Reset()
        {
            //we always have at least a node which is the local position
            localNodes[0] = Vector3.zero;
            waitTimes[0] = 0;

            _rigidbody2D = GetComponent<Rigidbody2D>();
            _rigidbody2D.isKinematic = true;

            if (platformCatcher == null)
                platformCatcher = GetComponent<PlatformCatcher>();
        }

        private void Start()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _rigidbody2D.isKinematic = true;

            if (platformCatcher == null)
                platformCatcher = GetComponent<PlatformCatcher>();

            //Allow to make platform only move when they became visible
            var renderers = GetComponentsInChildren<Renderer>();
            foreach (var childrenRenderer in renderers)
            {
                var b = childrenRenderer.gameObject.AddComponent<VisibleBubbleUp>();
                b.ObjectBecameVisible = BecameVisible;
            }

            //we make point in the path being defined in local space so game designer can move the platform & path together
            //but as the platform will move during gameplay, that would also move the node. So we convert the local nodes
            // (only used at edit time) to world position (only use at runtime)
            WorldNode = new Vector3[localNodes.Length];
            for (var i = 0; i < WorldNode.Length; ++i)
                WorldNode[i] = transform.TransformPoint(localNodes[i]);

            Init();
        }

        private void Init()
        {
            _current = 0;
            _dir = 1;
            _next = localNodes.Length > 1 ? 1 : 0;

            _waitTime = waitTimes[0];

            _veryFirstStart = false;
            if (isMovingAtStart)
            {
                started = !startMovingOnlyWhenVisible;
                _veryFirstStart = true;
            }
            else
                started = false;
        }

        private void FixedUpdate()
        {
            var sprites = gameObject.GetComponentsInChildren<ChangingSprite>();

            foreach (var sprite in sprites)
            {
                sprite.Refresh();
            }

            if (!started)
                return;

            //no need to update we have a single node in the path
            if (_current == _next)
                return;

            if (_waitTime > 0)
            {
                _waitTime -= Time.deltaTime;
                return;
            }

            var distanceToGo = speed * Time.deltaTime;

            while (distanceToGo > 0)
            {
                Vector2 direction = WorldNode[_next] - transform.position;

                var dist = distanceToGo;
                if (direction.sqrMagnitude < dist * dist)
                {
                    //we have to go farther than our current goal point, so we set the distance to the remaining distance
                    //then we change the current & next indexes
                    dist = direction.magnitude;

                    _current = _next;

                    _waitTime = waitTimes[_current];

                    if (_dir > 0)
                    {
                        _next += 1;
                        if (_next >= WorldNode.Length)
                        {
                            //we reach the end

                            switch (platformType)
                            {
                                case MovingPlatformType.BackForth:
                                    _next = WorldNode.Length - 2;
                                    _dir = -1;
                                    break;
                                case MovingPlatformType.Loop:
                                    _next = 0;
                                    break;
                                case MovingPlatformType.Once:
                                    _next -= 1;
                                    StopMoving();
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                    }
                    else
                    {
                        _next -= 1;
                        if (_next < 0)
                        {
                            //reached the beginning again

                            switch (platformType)
                            {
                                case MovingPlatformType.BackForth:
                                    _next = 1;
                                    _dir = 1;
                                    break;
                                case MovingPlatformType.Loop:
                                    _next = WorldNode.Length - 1;
                                    break;
                                case MovingPlatformType.Once:
                                    _next += 1;
                                    StopMoving();
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }
                    }
                }

                Velocity = direction.normalized * dist;


                //transform.position +=  new Vector3(Velocity.x, Velocity.y);
                _rigidbody2D.MovePosition(_rigidbody2D.position + Velocity);
                platformCatcher.MoveCaughtObjects(Velocity);
                


                /* else if (isClientOnly)
                {
                    MoveCaughtObjectsServer(Velocity);
                }*/
                //We remove the distance we moved. That way if we didn't had enough distance to the next goal, we will do a new loop to finish
                //the remaining distance we have to cover this frame toward the new goal
                distanceToGo -= dist;

                // we have some wait time set, that mean we reach a point where we have to wait. So no need to continue to move the platform, early exit.
                if (_waitTime > 0.001f)
                    break;
            }
        }

        [Command]
        private void MoveCaughtObjectsServer(Vector2 velocity)
        {
            platformCatcher.MoveCaughtObjects(velocity);
            MoveCaughtObjectsClient(velocity);
        }

        [ClientRpc(includeOwner = false)]
        private void MoveCaughtObjectsClient(Vector2 velocity)
        {
            platformCatcher.MoveCaughtObjects(velocity);
        }


        public void ResetPlatform()
        {
            transform.position = WorldNode[0];
            Init();
        }

        private void BecameVisible(VisibleBubbleUp obj)
        {
            if (!_veryFirstStart) return;
            started = true;
            _veryFirstStart = false;
        }
    }
}