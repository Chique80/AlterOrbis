using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SceneManagement
{
    /// <summary>
    /// This class is used to move gameobjects from one position to another in the scene.
    /// </summary>
    public class GameObjectTeleporter : MonoBehaviour
    {
        private static GameObjectTeleporter Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                _instance = FindObjectOfType<GameObjectTeleporter>();

                if (_instance != null)
                    return _instance;

                var gameObjectTeleporter = new GameObject("GameObjectTeleporter");
                _instance = gameObjectTeleporter.AddComponent<GameObjectTeleporter>();

                return _instance;
            }
        }

        public static bool Transitioning => Instance._mTransitioning;

        private static GameObjectTeleporter _instance;
        
        private bool _mTransitioning;

        private void Awake ()
        {
            if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            
        }

        public static void Teleport (GameObject transitioningGameObject, Transform destination)
        {
            Instance.StartCoroutine (Instance.Transition (transitioningGameObject, false, false, destination.position, false));
        }

        public static void Teleport (GameObject transitioningGameObject, Vector3 destinationPosition)
        {
            Instance.StartCoroutine (Instance.Transition (transitioningGameObject, false, false, destinationPosition, false));
        }

        private IEnumerator Transition (GameObject transitioningGameObject, bool releaseControl, bool resetInputValues, Vector3 destinationPosition, bool fade)
        {
            _mTransitioning = true;

            if (releaseControl)
            {
                //m_PlayerInput.ReleaseControl (resetInputValues);
            }

            if(fade)
                yield return StartCoroutine (ScreenFader.FadeSceneOut ());

            transitioningGameObject.transform.position = destinationPosition;
        
            if(fade)
                yield return StartCoroutine (ScreenFader.FadeSceneIn ());

            if (releaseControl)
            {
                //m_PlayerInput.GainControl ();
            }

            transitioningGameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
            transitioningGameObject.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Dynamic;

            _mTransitioning = false;
        }

        private static SceneTransitionDestination GetDestination(SceneTransitionDestination.DestinationTag destinationTag)
        {
            var entrances = FindObjectsOfType<SceneTransitionDestination>();
            foreach (var entrance in entrances)
            {
                if (entrance.destinationTag == destinationTag)
                    return entrance;
            }
            Debug.LogWarning("No entrance was found with the " + destinationTag + " tag.");
            return null;
        }
    }
}