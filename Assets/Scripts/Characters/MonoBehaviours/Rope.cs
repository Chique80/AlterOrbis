using UnityEngine;

namespace Characters.MonoBehaviours
{
    public class Rope : MonoBehaviour
    {
        public struct RopeSegment
        {
            public GameObject GameObj;
            public HingeJoint2D Joint;
            public BoxCollider2D Collider;
        }

        private const float RopeRadius = 5.0f;
        public RopeSegment CurrentRopeSegment; //ok
        private RopeProperties _ropeProperties;
        private Vector3 _closestPointToRope;
        private float _distanceToTravel;
        private float _distanceAchieved;
        public double ridingSpeed = 0.1;
        public bool isOnRope;
        public double ridingIterationTimer;
        public Sprite[] ballSprites;
        public Sprite[] segmentSprites;

        public double ridingIterationDelay = 1.0;
        //bool nullFlag;

        public float GetDistanceToTravel()
        {
            return _distanceToTravel;
        }

        public float GetDistanceAchieved()
        {
            return _distanceAchieved;
        }

        private void FindRope(Vector2 position)
        {
            var deltaCharObject = float.MaxValue;

            var ropes = GameObject.FindGameObjectsWithTag("Rope");

            foreach (var rope in ropes)
            {
                if (!rope.GetComponent<RopeProperties>().isAttached) continue;
                var tempDeltaChar =
                    Vector2.Distance(position, rope.transform.position);

                if (!(tempDeltaChar < deltaCharObject)) continue;
                deltaCharObject = tempDeltaChar;
                CurrentRopeSegment = new RopeSegment {GameObj = rope};
            }

            if (CurrentRopeSegment.GameObj == null) return;
            CurrentRopeSegment.Joint = CurrentRopeSegment.GameObj.GetComponent<HingeJoint2D>();
            CurrentRopeSegment.Collider = CurrentRopeSegment.GameObj.GetComponent<BoxCollider2D>();
            _ropeProperties = CurrentRopeSegment.GameObj.GetComponent<RopeProperties>();
        }

        public bool EmbarkOnRope(GameObject character, bool isCharacterFacingRight)
        {
            var isEmbarked = false;

            // Check if all items are found, if not, load them
            if (!FindEverything(character.transform.position))
            {
                Debug.Log("References missing");
                return false;
            }

            // Embark on rope, deactivate character collider and physics
            if (CurrentRopeSegment.GameObj != null && character != null)
            {
                var charPos = character.transform.position;
                var isInRopeRadius = IsCharacterInRopeRadius(character);

                if (!isInRopeRadius) return false;
                Vector3 closestRopePoint = CurrentRopeSegment.Collider.ClosestPoint(charPos);
                _closestPointToRope = closestRopePoint;
                character.GetComponent<CapsuleCollider2D>().enabled = false;
                character.GetComponent<Rigidbody2D>().simulated = false;
                character.transform.position = closestRopePoint;
                isEmbarked = true;
                isOnRope = true;
                _distanceToTravel = GetDistanceToTravelOnRope(isCharacterFacingRight);
            }
            else
            {
                Debug.Log("Null game object");
            }

            return isEmbarked;
        }

        private bool FindEverything(Vector2 position)
        {
            var isEverythingFound = false;

            FindRope(position);

            if (CurrentRopeSegment.GameObj == null)
            {
                Debug.Log("rope missing");
            }

            if (CurrentRopeSegment.GameObj != null && _ropeProperties.object1 != null &&
                _ropeProperties.object2 != null)
            {
                isEverythingFound = true;
            }

            return isEverythingFound;
        }

        public void RideRope(GameObject character, bool isCharacterFacingRight)
        {
            // Ride

            Debug.Log("Travel rope");
            double angle;

            if (_distanceToTravel == 0.0)
            {
                _distanceToTravel = GetDistanceToTravelOnRope(isCharacterFacingRight);
            }

            // Get the correct angle based on the character's facing direction
            if (isCharacterFacingRight)
            {
                if (_ropeProperties.object2.transform.position.x > _ropeProperties.object1.transform.position.x)
                {
                    angle = GetAngleBetweenObjects(_ropeProperties.object1, _ropeProperties.object2) *
                        System.Math.PI / 180;
                }
                else
                {
                    angle = GetAngleBetweenObjects(_ropeProperties.object2, _ropeProperties.object1) *
                        System.Math.PI / 180;
                }
            }
            else
            {
                if (_ropeProperties.object2.transform.position.x > _ropeProperties.object1.transform.position.x)
                {
                    angle = GetAngleBetweenObjects(_ropeProperties.object2, _ropeProperties.object1) *
                        System.Math.PI / 180;
                }
                else
                {
                    angle = GetAngleBetweenObjects(_ropeProperties.object1, _ropeProperties.object2) *
                        System.Math.PI / 180;
                }
            }

            // Determine movement ratio
            var deltaX = (float) (System.Math.Cos(angle) * ridingSpeed);
            var deltaY = (float) (System.Math.Sin(angle) * ridingSpeed);

            var newPosition = character.transform.position;
            newPosition.x += deltaX;
            newPosition.y += deltaY;
            character.transform.position = newPosition;

            _distanceAchieved += System.Math.Abs(deltaX) + System.Math.Abs(deltaY);

            // Disembark
            /*else if (_distanceToTravel <= _distanceAchieved)
            {
                DisembarkRope(character);
            }*/
        }

        public void DisembarkRope(GameObject character)
        {
            character.GetComponent<CapsuleCollider2D>().enabled = true;
            character.GetComponent<Rigidbody2D>().simulated = true;
            _distanceToTravel = 0;
            _distanceAchieved = 0;
            isOnRope = false;
        }

        private bool IsCharacterInRopeRadius(GameObject character)
        {
            var isInRadius = false;

            var boxCollider2D = CurrentRopeSegment.Collider;

            if (boxCollider2D == null || character == null) return false;
            Vector3 closestRopePoint = boxCollider2D.ClosestPoint(character.transform.position);

            if (Vector2.Distance(closestRopePoint, character.transform.position) < RopeRadius)
            {
                isInRadius = true;
            }

            return isInRadius;
        }

        /**
         * @brief Calculates the angle in radians between point A (object) and point B (robot body).
         *        Possible values [-180.0, 180,0]. Used to point the robot arm towards an object.
         */
        private static double GetAngleBetweenObjects(GameObject obj1, GameObject obj2)
        {
            var angleRadian = 0.0;

            if (obj1 == null || obj2 == null) return angleRadian * 180.0 / System.Math.PI;
            var position = obj1.transform.position;
            var robotPos = new Vector2(position.x, position.y);
            var position1 = obj2.transform.position;
            var objectPos = new Vector2(position1.x, position1.y);

            angleRadian = System.Math.Atan2(objectPos.y - robotPos.y, objectPos.x - robotPos.x);

            return angleRadian * 180.0 / System.Math.PI;
        }

        private float GetDistanceToTravelOnRope(bool isCharacterFacingRight)
        {
            float distance;

            var position = _ropeProperties.object1.transform.position;
            var x = position.x;
            var y = position.y;
            var firstPos = new Vector3(x, y, 0);

            var angle = GetAngleBetweenObjects(_ropeProperties.object2, _ropeProperties.object1) * System.Math.PI / 180;
            var ropeLength = CurrentRopeSegment.Collider.size.x;
            var newX = (float) (x + System.Math.Cos(angle) * ropeLength);
            var newY = (float) (y + System.Math.Sin(angle) * ropeLength);
            var secondPos = _ropeProperties.object2.transform.position;

            // Find which side of the mesh to get the distance from
            if (isCharacterFacingRight)
            {
                distance = _ropeProperties.object1.transform.position.x < _ropeProperties.object2.transform.position.x
                    ? Vector2.Distance(secondPos, _closestPointToRope)
                    : Vector2.Distance(_closestPointToRope, firstPos);
            }
            else
            {
                distance = _ropeProperties.object1.transform.position.x < _ropeProperties.object2.transform.position.x
                    ? Vector2.Distance(_closestPointToRope, firstPos)
                    : Vector2.Distance(secondPos, _closestPointToRope);
            }

            return distance;
        }
    }
}