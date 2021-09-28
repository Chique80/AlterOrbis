using System;
using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

namespace Characters.MonoBehaviours
{
    public class RopeCharacter : PlayerCharacter
    {
        private struct Rope
        {
            public GameObject GameObj;
            public SkinnedMeshRenderer MeshRenderer;
            public Rigidbody2D RigidBody;
            public BoxCollider2D Collider;
            public HingeJoint2D Joint;
            public RopeProperties Properties;
            public List<GameObject> ballSprites;
            public List<GameObject> segmentSprites;
        }

        private const float SegmentGirth = 0.30f;
        private const float SegmentLength = 0.50f;
        private const float CharHeightOffset = 0.40f;
        private const double ActionDelay = 2.0;
        private float MinTargetDistance = 3.0f;
        private float CurrentRopeLength = SegmentLength;

        private Rope _rope;
        private Rope _arm;
        private GameObject _closestObject;
        private bool _isArmAttached;
        private bool _isRopeComplete;
        private bool updatePos = false;

        [SyncVar] private double _actionTimer;

        public Sprite[] ropeSprites;

        enum AddType { ARM, ROPE };

        private new void Start()
        {
            base.Start();
            AddArms(AddType.ROPE);
            AddArms(AddType.ARM);
            SearchForClosestObject();

            for(int i = 0; i < 30; i++)
                Physics2D.IgnoreLayerCollision(24, i);
        }

        private new void Update()
        {
            base.Update();

            // If rope incomplete
            if (_isRopeComplete) return;
            SearchForClosestObject();

            // Arms point to closest object
            PointToObject(_rope, !_isRopeComplete ? gameObject : _rope.Properties.object1);
            PointToObject(_arm, gameObject);

            var distance = !_isRopeComplete
                ? GetDistanceToObject(_rope.Properties.object1)
                : Vector2.Distance(_rope.Properties.object1.transform.position,
                    _rope.Properties.object2.transform.position);

            if(RopeInfo.isOnRope == true)
            {
                if(!_isArmAttached)
                    _rope.Joint.connectedBody = null;
                _arm.Joint.connectedBody = null;
                updatePos = true;
            }

            if(!RopeInfo.isOnRope && updatePos)
            {
                if(!_isArmAttached)
                    AttachRopeToBody(_rope);
                AttachRopeToBody(_arm);
                updatePos = false;
            }

            ExpandRope(distance);

            UpdateSprites(_rope, CurrentRopeLength);
            UpdateSprites(_arm, SegmentLength);
        }

        private void ExpandRope(float newLength)
        {
            if (!_isArmAttached) return;
            /* Setup rope segment mesh. */
            var mesh = new Mesh();

            // Vertices
            var vertices = new[]
            {
                new Vector3(0, 0, 0),
                new Vector3((float) newLength, 0, 0),
                new Vector3(0, SegmentGirth, 0),
                new Vector3((float) newLength, SegmentGirth, 0)
            };
            mesh.vertices = vertices;

            // Triangles
            var tris = new[]
            {
                // lower left triangle
                0, 2, 1,
                // upper right triangle
                2, 3, 1
            };
            mesh.triangles = tris;

            // Normals
            var normals = new[]
            {
                -Vector3.forward,
                -Vector3.forward,
                -Vector3.forward,
                -Vector3.forward
            };
            mesh.normals = normals;

            _rope.MeshRenderer.sharedMesh = mesh;
            
            _rope.GameObj.transform.localScale.Set(SegmentLength, SegmentGirth, 0);

            _rope.Collider.size = new Vector2((float) newLength, SegmentGirth);
            _rope.Collider.offset = new Vector2((float) newLength / 2, SegmentGirth / 2);
            CurrentRopeLength = newLength;
        }

        private void UpdateSprites(Rope rope, float newLength)
        {
            if (gameObject == null || rope.GameObj == null) return;

            float maxDistanceBetweenBalls = 1.0f;
            int nbBalls = (int) Math.Round(newLength / maxDistanceBetweenBalls) + 1;
            const float segmentLength = 0.75f;
            float ropePosX;
            float ropePosY;
            bool isRopeBlocked = IsRopeBlocked();
            Color color;
            float ballZ = 0.0f;
            bool isRobotArm = false;

            if(rope.GameObj.name.Equals("Robot_arm"))
            {
                ballZ = gameObject.transform.position.z - 0.1f;
                isRobotArm = true;
            }
            else
            {
                ballZ = gameObject.transform.position.z + 0.1f;
            }

            if (rope.Properties.isAttached)
            {
                ropePosX = rope.Properties.object2.transform.position.x;
                ropePosY = rope.Properties.object2.transform.position.y;
            }
            else
            {
                ropePosX = gameObject.transform.position.x;
                ropePosY = gameObject.transform.position.y + CharHeightOffset;
            }

            if(_isArmAttached && !rope.Properties.isAttached && !isRobotArm)
            {
                color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
            }
            else
            {
                color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
            }

            if (isRopeBlocked)
            {
                color.g = 0.0f;
                color.b = 0.0f;
            }

            // Minimum de 2 pivots
            if(nbBalls < 2)
            {
                nbBalls = 2;
            }

            /*-------------------------- Rope Pivots ---------------------------*/
            int nbMissingBalls = nbBalls - rope.ballSprites.Count;

            // Add or remove ball sprites as needed
            if (nbMissingBalls > 0)
            { 
                for(int i = 0; i < nbMissingBalls; i++)
                {
                    rope.ballSprites.Add(new GameObject());
                    rope.ballSprites[rope.ballSprites.Count - 1].name = "Rope_pivot " + (rope.ballSprites.Count - 1);
                    rope.ballSprites[rope.ballSprites.Count - 1].AddComponent<SpriteRenderer>();
                    rope.ballSprites[rope.ballSprites.Count - 1].layer = 24;
                }        
            }
            else if (nbMissingBalls < 0)
            {
                for(int i = 0; i > nbMissingBalls; i--)
                {
                    Destroy(rope.ballSprites[rope.ballSprites.Count - 1]);
                    rope.ballSprites.RemoveAt(rope.ballSprites.Count - 1);
                }
            }

            for(int i = 0; i < rope.ballSprites.Count; i++)
            {
                rope.ballSprites[i].GetComponent<SpriteRenderer>().sprite = ropeSprites[0];
            }

            if(rope.ballSprites.Count >= 1)
                rope.ballSprites[rope.ballSprites.Count - 1].GetComponent<SpriteRenderer>().sprite = ropeSprites[2];

            float ropeAngle;

            ropeAngle = (float)(rope.GameObj.transform.rotation.eulerAngles.z * Math.PI / 180.0f);

            float distanceBetweenBalls = newLength / (nbBalls - 1);

            // Set the balls' positions
            for (int i = 0; i < nbBalls; i++)
            {
                float ballX = ropePosX + (float) Math.Cos(ropeAngle) * distanceBetweenBalls * i;
                float ballY = ropePosY + (float) Math.Sin(ropeAngle) * distanceBetweenBalls * i;
                rope.ballSprites[i].transform.position = new Vector3(ballX, ballY, ballZ);
                rope.ballSprites[i].GetComponent<SpriteRenderer>().color = color;
            }

            ropeAngle = ropeAngle * 180.0f / (float)Math.PI;

            if (nbMissingBalls != 0)
            {
                if(rope.ballSprites.Count >= 2)
                    rope.ballSprites[rope.ballSprites.Count - 2].GetComponent<SpriteRenderer>().sprite = ropeSprites[0];
                if(rope.ballSprites.Count >= 1)
                    rope.ballSprites[rope.ballSprites.Count - 1].GetComponent<SpriteRenderer>().sprite = ropeSprites[2];
            }

            if(rope.ballSprites.Count >= 1)
                rope.ballSprites[rope.ballSprites.Count - 1].transform.rotation = Quaternion.Euler(0, 0, ropeAngle);

            /*-------------------------- Rope Segments ---------------------------*/
            int nbSegments = nbBalls - 1;
            int nbMissingSegments = nbBalls - rope.segmentSprites.Count;

            // Add or remove ball sprites as needed
            if (nbMissingSegments > 0)
            {
                for (int i = 0; i < nbMissingSegments; i++)
                {
                    rope.segmentSprites.Add(new GameObject());
                    rope.segmentSprites[rope.segmentSprites.Count - 1].name = "Rope_segment " + (rope.segmentSprites.Count - 1);
                    rope.segmentSprites[rope.segmentSprites.Count - 1].AddComponent<SpriteRenderer>();
                    rope.segmentSprites[rope.segmentSprites.Count - 1].GetComponent<SpriteRenderer>().sprite = ropeSprites[1];
                    rope.segmentSprites[rope.segmentSprites.Count - 1].layer = 24;
                }
            }
            else if (nbMissingSegments < 0)
            {
                for (int i = 0; i > nbMissingSegments - 1; i--)
                {
                    Destroy(rope.segmentSprites[rope.segmentSprites.Count - 1]);
                    rope.segmentSprites.RemoveAt(rope.segmentSprites.Count - 1);
                }
            }

            float segmentScale = distanceBetweenBalls / segmentLength;

            // Set the segments' positions
            for (int i = 0; i < nbSegments; i++)
            {
                rope.segmentSprites[i].transform.position = rope.ballSprites[i].transform.position;
                rope.segmentSprites[i].transform.rotation = Quaternion.Euler(0, 0, ropeAngle);
                rope.segmentSprites[i].transform.localScale = new Vector3(segmentScale, 1, 1);
                rope.segmentSprites[i].GetComponent<SpriteRenderer>().color = color;
            }
        }

        // Breaks the rope and reattaches it to the body
        private void AttachRopeToBody(Rope rope)
        {
            // Reset attributes
            rope.Properties.object1 = null;
            rope.Properties.object2 = null;
            _isArmAttached = true;
            _isRopeComplete = false;

            // Modify rope length
            ExpandRope(SegmentLength);

            _isArmAttached = false;

            // Teleport rope to player
            Vector3 position = gameObject.transform.position;
            position.y += CharHeightOffset;
            position.z = 0;
            rope.GameObj.transform.position = position;
            rope.GameObj.transform.rotation = Quaternion.Euler(0, 0, -90);

            
            if (!ClientScene.localPlayer || isLocalPlayer)
            {
                // Modify hinge
                rope.Joint.connectedBody = gameObject.GetComponent<Rigidbody2D>();
                rope.Joint.connectedAnchor = new Vector2(0, CharHeightOffset);
            }
            else
            {
                rope.Joint.connectedBody = gameObject.transform.Find("RopeAnchor").GetComponent<Rigidbody2D>();
                rope.Joint.connectedAnchor = new Vector2(0, CharHeightOffset);
            }
            

            // Update sprites
            UpdateSprites(_rope, SegmentLength);
        }

        private bool IsRopeBlocked()
        {
            return _rope.Collider.IsTouchingLayers(Physics2D.GetLayerCollisionMask(31));
        }

        private void AttachRope()
        {
            if (!_isArmAttached && _closestObject != null && GetDistanceToObject(_closestObject) < MinTargetDistance)
            {
                _rope.Properties.object1 = _closestObject;
                _rope.Properties.object1.GetComponent<HingeProperties>().objectNo = 1;
                _isArmAttached = true;
                rangedAttackAudioPlayer.PlayRandomSound();
            }
            else if (_closestObject != null && _isArmAttached && GetDistanceToObject(_closestObject) < MinTargetDistance)
            {
                _rope.Joint.connectedBody = _closestObject.GetComponent<Rigidbody2D>();
                if (_rope.Joint.connectedBody == null)
                    Debug.Log("ConnectedBody null.");
                _rope.Joint.connectedAnchor = new Vector2(0, 0);
                _rope.Joint.anchor = new Vector2(0, SegmentGirth / 2);
                _isRopeComplete = true;
                Debug.Log("Rope attached!");
                _rope.Properties.object2 = _closestObject;
                _rope.Properties.object2.GetComponent<HingeProperties>().objectNo = 2;

                // Adjust rope length
                float length = Vector2.Distance(_rope.Properties.object1.transform.position,
                    _rope.Properties.object2.transform.position);
                ExpandRope(length);

                // Adjust the angle of the rope
                var angle = (float) GetAngleBetweenObjects(_rope.Properties.object2, _rope.Properties.object1);
                _rope.GameObj.transform.rotation = Quaternion.Euler(0, 0, angle);
                _rope.Properties.isAttached = true;

                UpdateSprites(_rope, length);

                AddArms(AddType.ROPE);
                _isArmAttached = false;
                rangedAttackAudioPlayer.PlayRandomSound();
            }
        }

        /**
         * @brief Changes the direction of the robot's arm to point towards the nearest hingeable object (then locks it).
         */
        private void PointToObject(Rope rope, GameObject obj)
        {
            // Unfreeze the arm's rotation.
            rope.RigidBody.constraints = RigidbodyConstraints2D.None;

            if (rope.GameObj != null)
            {
                float angle;

                if (rope.GameObj.name.Equals("Robot_arm"))
                {
                    if (GetDistanceToObject(_closestObject) > MinTargetDistance) return;

                    angle = (float)GetAngleBetweenObjects(obj, _closestObject);

                    rope.GameObj.transform.rotation = Quaternion.Euler(0, 0, angle);
                    rope.RigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;
                }
                else
                {
                    if (!_isArmAttached && !_isRopeComplete && !(GetDistanceToObject(_closestObject) < MinTargetDistance)) return;

                    if (!_isArmAttached)
                        angle = (float)GetAngleBetweenObjects(obj, _closestObject);
                    else if (!_isRopeComplete)
                        angle = (float)GetAngleBetweenObjects(obj, rope.Properties.object1);
                    else
                        angle = (float)GetAngleBetweenObjects(rope.Properties.object2, obj);

                    rope.GameObj.transform.rotation = Quaternion.Euler(0, 0, angle);
                    rope.RigidBody.constraints = RigidbodyConstraints2D.FreezeRotation;
                }
            }
        }

        /**
         * @brief Calculates the angle in radians between point A (object) and point B (robot body).
         *        Possible values [-180.0, 180,0]. Used to point the robot arm towards an object.
         */
        private static double GetAngleBetweenObjects(GameObject obj1, GameObject obj2)
        {
            var angleRadian = 0.0;

            if (obj1 == null || obj2 == null) return angleRadian * 180.0 / Math.PI;
            if(obj1.name == "RopeBot")
            {

            }
            var position = obj1.transform.position;
            var robotPos = new Vector2(position.x, position.y);
            if (obj1.name == "RopeBot")
            {
                robotPos.y += CharHeightOffset;
            }
            var position1 = obj2.transform.position;
            var objectPos = new Vector2(position1.x, position1.y);

            angleRadian = Math.Atan2(objectPos.y - robotPos.y, objectPos.x - robotPos.x);

            return angleRadian * 180.0 / Math.PI;
        }

        /**
         * @brief Calculates the distance between the robot's body coordinates center and an object's center coordinates.
         * 
         * @return The distance between 
         */
        private float GetDistanceToObject(GameObject obj)
        {
            var distance = float.MaxValue;
            Vector2 gameCharPos = transform.position;
            gameCharPos.y += CharHeightOffset;

            if (obj != null)
            {
                distance = Vector2.Distance(gameCharPos, obj.transform.position);
            }

            return distance;
        }

        //

        
        /**
         * @brief 
         */
        private void SearchForClosestObject()
        {
            var deltaCharObject = float.MaxValue;

            var hingeableObjects = GameObject.FindGameObjectsWithTag("Hingeable");

            foreach (var hingeableObject in hingeableObjects)
            {
                var tempDeltaChar =
                    Vector2.Distance(transform.position, hingeableObject.transform.position);

                if (hingeableObject == _rope.Properties.object1 || hingeableObject == _rope.Properties.object2 || !(tempDeltaChar < deltaCharObject)) continue;
                deltaCharObject = tempDeltaChar;
                _closestObject = hingeableObject;
            }

            if (hingeableObjects.Length == 0)
            {
                _closestObject = null;
            }
        }



        private void AddArms(AddType type)
        {
            var tempRope = new Rope {GameObj = new GameObject()};
            tempRope.MeshRenderer = tempRope.GameObj.AddComponent<SkinnedMeshRenderer>();
            tempRope.GameObj.transform.localScale.Set(SegmentLength, SegmentGirth, 0);

            tempRope.GameObj.name = "Rope";

            tempRope.MeshRenderer = tempRope.GameObj.GetComponent<SkinnedMeshRenderer>();

            /* Setup rope segment mesh. */
            var mesh = new Mesh();

            // Vertices
            var vertices = new[]
            {
                new Vector3(0, 0, 0),
                new Vector3(SegmentLength, 0, 0),
                new Vector3(0, SegmentGirth, 0),
                new Vector3(SegmentLength, SegmentGirth, 0)
            };
            mesh.vertices = vertices;

            // Triangles
            var tris = new[]
            {
                // lower left triangle
                0, 2, 1,
                // upper right triangle
                2, 3, 1
            };
            mesh.triangles = tris;

            // Normals
            var normals = new[]
            {
                new Vector3(0,0,-1),
                new Vector3(0,0,-1),
                new Vector3(0,0,-1),
                new Vector3(0,0,-1)
            };
            mesh.normals = normals;

            var uvs = new Vector2[vertices.Length];

            uvs[0] = new Vector2(0, 0);
            uvs[1] = new Vector2(SegmentLength, 0);
            uvs[2] = new Vector2(0, SegmentGirth);
            uvs[3] = new Vector2(SegmentLength, SegmentGirth);

            mesh.uv = uvs;

            tempRope.MeshRenderer.sharedMesh = mesh;
           

            // Setup 2D collider
            tempRope.Collider = tempRope.GameObj.AddComponent<BoxCollider2D>();
            var colliderSize = new Vector2(SegmentLength, SegmentGirth);
            
            tempRope.Collider.size = colliderSize;
            tempRope.Collider.isTrigger = true;
            
            tempRope.RigidBody = tempRope.GameObj.AddComponent<Rigidbody2D>();
            tempRope.RigidBody.centerOfMass = new Vector2(SegmentLength, SegmentGirth / 2);
            tempRope.RigidBody.inertia = 0;
            tempRope.RigidBody.drag = 2;
            //tempRope.MeshRenderer.updateWhenOffscreen = true;

            tempRope.GameObj.tag = "Rope";
            tempRope.Properties = tempRope.GameObj.AddComponent<RopeProperties>();

            tempRope.MeshRenderer.enabled = false;

            // Setup hinge joint
            tempRope.Joint = tempRope.GameObj.AddComponent<HingeJoint2D>();
            tempRope.Joint.anchor = new Vector2(0, SegmentGirth / 2);
            tempRope.Joint.enableCollision = false;
            tempRope.Joint.autoConfigureConnectedAnchor = false;
            tempRope.GameObj.layer = 24;

            tempRope.ballSprites = new List<GameObject>();
            tempRope.segmentSprites = new List<GameObject>();

            if(type == AddType.ROPE)
            {
                _rope = tempRope;
                AttachRopeToBody(_rope);
            }
            else if (type == AddType.ARM)
            {
                _arm = tempRope;
                _arm.GameObj.name = "Robot_arm";
                AttachRopeToBody(_arm);
            }

        }

        
        [ClientRpc(includeOwner = false)]
        private void AttachRopeClients()
        {
            AttachRope();
            _actionTimer = Time.time + ActionDelay;
        }

        [Command]
        private void AttachRopeServer()
        {
            AttachRope();
            AttachRopeClients();
            _actionTimer = Time.time + ActionDelay;
        }

        [ClientRpc(includeOwner = false)]
        private void AttachRopeToBodyClient()
        {
            AttachRopeToBody(_rope);
            _actionTimer = Time.time + ActionDelay;
        }
        
        [Command]
        private void AttachRopeToBodyServer()
        {
            AttachRopeToBody(_rope);
            AttachRopeToBodyClient();
            _actionTimer = Time.time + ActionDelay;
        }

        public override void SpecialMove()
        {
            if (!_isArmAttached)
            {
                AttachRope();
                _actionTimer = Time.time + ActionDelay;
                
                if (isServer)
                {
                    AttachRopeClients();
                }
                else if (isClientOnly)
                {
                    AttachRopeServer();
                }
            }
            else if ((GetDistanceToObject(_closestObject) > MinTargetDistance || IsRopeBlocked()) && !_isRopeComplete && _actionTimer < Time.time)
            {
                AttachRopeToBody(_rope);
                _actionTimer = Time.time + ActionDelay;
                
                if (isServer)
                {
                    AttachRopeToBodyClient();
                }
                else if (isClientOnly)
                {
                    AttachRopeToBodyServer();
                }
                
            }
            else if (!_isRopeComplete && _actionTimer < Time.time)
            {
                AttachRope();
                _actionTimer = Time.time + ActionDelay;
                
                if (isServer)
                {
                    AttachRopeClients();
                }
                else if (isClientOnly)
                {
                    AttachRopeServer();
                }
            }

            
        }
    }
}