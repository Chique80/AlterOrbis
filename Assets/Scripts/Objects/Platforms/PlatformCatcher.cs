using System;
using System.Collections.Generic;
using System.Linq;
using Characters.MonoBehaviours;
using UnityEngine;

namespace Objects.Platforms
{
    public class PlatformCatcher : MonoBehaviour
    {
        [Serializable]
        public class CaughtObject
        {
            public Rigidbody2D rigidbody;
            public Collider2D collider;
            public CharacterController2D character;
            public bool inContact;
            public bool checkedThisFrame;

            public void Move (Vector2 movement)
            {
                if(!inContact)
                    return;

                if (character != null)
                    character.Move(movement);
                else
                    rigidbody.MovePosition(rigidbody.position + movement);
            }
        }


        public Rigidbody2D platformRigidbody;
        public ContactFilter2D contactFilter;

        private readonly List<CaughtObject> _caughtObjects = new List<CaughtObject> (128);
        private readonly ContactPoint2D[] _contactPoints = new ContactPoint2D[20];
        private Collider2D _collider;
        private PlatformCatcher _parentCatcher;

        private Action<Vector2> _moveDelegate;

        public int CaughtObjectCount
        {
            get
            {
                return _caughtObjects.Count(caughtObject => caughtObject.inContact);
            }
        }

        public float CaughtObjectsMass
        {
            get
            {
                return _caughtObjects.Where(caughtObject => caughtObject.inContact).Sum(caughtObject => caughtObject.rigidbody.mass);
            }
        }

        private void Awake ()
        {
            if (platformRigidbody == null)
                platformRigidbody = GetComponent<Rigidbody2D>();

            if (_collider == null)
                _collider = GetComponent<Collider2D>();


            _parentCatcher = null;
            var currentParent = transform.parent;
            while(currentParent != null)
            {
                var catcher = currentParent.GetComponent<PlatformCatcher>();
                if (catcher != null)
                    _parentCatcher = catcher;
                currentParent = currentParent.parent;
            }

            //if we have a parent platform catcher, we make it's move "bubble down" to that catcher,
            //so any object caught by that platform catcher will also
            //be moved by the parent catcher (e.g. a platform catcher on a pressure plate on top of a moving platform)
            if (_parentCatcher != null)
                _parentCatcher._moveDelegate += MoveCaughtObjects;
        }

        private void FixedUpdate ()
        {
            for (int i = 0, count = _caughtObjects.Count; i < count; i++)
            {
                var caughtObject = _caughtObjects[i];
                caughtObject.inContact = false;
                caughtObject.checkedThisFrame = false;
            }
        
            CheckRigidbodyContacts (platformRigidbody);

            bool checkAgain;
            do
            {
                for (int i = 0, count = _caughtObjects.Count; i < count; i++)
                {
                    var caughtObject = _caughtObjects[i];

                    switch (caughtObject.inContact)
                    {
                        case true:
                        {
                            if (!caughtObject.checkedThisFrame)
                            {
                                CheckRigidbodyContacts(caughtObject.rigidbody);
                                caughtObject.checkedThisFrame = true;
                            }

                            break;
                        }
                        //Some cases will remove all contacts (collider resize etc.) leading to loosing contact with the platform
                        //so we check the distance of the object to the top of the platform.
                        case false:
                        {
                            var caughtObjectCollider = _caughtObjects[i].collider;

                            if (caughtObjectCollider == null) break;

                            //check if we are aligned with the moving platform, otherwise the yDiff test under would be true even if far from the platform as long as we are on the same y level...
                            var caughtObjectBounds = caughtObjectCollider.bounds;
                            var colliderBounds = _collider.bounds;
                            var verticalAlignment = (caughtObjectBounds.max.x > colliderBounds.min.x) && (caughtObjectBounds.min.x < colliderBounds.max.x);
                            if (verticalAlignment)
                            {
                                var yDiff = _caughtObjects[i].collider.bounds.min.y - _collider.bounds.max.y;

                                if (yDiff > 0 && yDiff < 0.05f)
                                {
                                    caughtObject.inContact = true;
                                    caughtObject.checkedThisFrame = true;
                                }
                            }

                            break;
                        }
                    }
                }

                checkAgain = false;

                for (int i = 0, count = _caughtObjects.Count; i < count; i++)
                {
                    var caughtObject = _caughtObjects[i];
                    if (!caughtObject.inContact || caughtObject.checkedThisFrame) continue;
                    checkAgain = true;
                    break;
                }
            }
            while (checkAgain);
        }

        private void CheckRigidbodyContacts (Rigidbody2D rb)
        {
            var contactCount = rb.GetContacts(contactFilter, _contactPoints);

            for (var j = 0; j < contactCount; j++)
            {
                var contactPoint2D = _contactPoints[j];
                var contactRigidbody = contactPoint2D.rigidbody == rb ? contactPoint2D.otherRigidbody : contactPoint2D.rigidbody;
                var listIndex = -1;

                for (var k = 0; k < _caughtObjects.Count; k++)
                {
                    if (contactRigidbody != _caughtObjects[k].rigidbody) continue;
                    listIndex = k;
                    break;
                }

                if (listIndex == -1)
                {
                    if (contactRigidbody == null) continue;
                    if (contactRigidbody.bodyType == RigidbodyType2D.Static ||
                        contactRigidbody == platformRigidbody) continue;
                    var dot = Vector2.Dot(contactPoint2D.normal, Vector2.down);
                    if (!(dot > 0.8f)) continue;
                    var newCaughtObject = new CaughtObject
                    {
                        rigidbody = contactRigidbody,
                        character = contactRigidbody.GetComponent<CharacterController2D>(),
                        collider = contactRigidbody.GetComponent<Collider2D>(),
                        inContact = true,
                        checkedThisFrame = false
                    };

                    _caughtObjects.Add(newCaughtObject);
                }
                else
                {
                    _caughtObjects[listIndex].inContact = true;
                }
            }
        }

        public void MoveCaughtObjects (Vector2 velocity)
        {
            _moveDelegate?.Invoke(velocity);

            for (int i = 0, count = _caughtObjects.Count; i < count; i++)
            {
                var caughtObject = _caughtObjects[i];
                if (_parentCatcher != null && _parentCatcher._caughtObjects.Find(a => a.rigidbody == caughtObject.rigidbody) != null)
                {
                    continue;
                }

                _caughtObjects[i].Move(velocity);
            }
        }

        public bool HasCaughtObject (GameObject caughtGameObject)
        {
            return _caughtObjects.Any(caughtObject => caughtObject.collider.gameObject == caughtGameObject && caughtObject.inContact);
        }
    }
}
