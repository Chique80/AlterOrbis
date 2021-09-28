using System.Collections;
using Characters.MonoBehaviours;
using Mirror;
using Mirror.CharacterSelection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneManagement
{
    /// <summary>
    /// This class is used to transition between scenes. This includes triggering all the things that need to happen on transition such as data persistence.
    /// </summary>
    public class SceneController : NetworkBehaviour
    {
        private static SceneController Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                _instance = FindObjectOfType<SceneController>();

                if (_instance != null)
                    return _instance;

                Create();

                return _instance;
            }
        }

        public static bool Transitioning => Instance._mTransitioning;

        private static SceneController _instance;

        private static SceneController Create()
        {
            var sceneControllerGameObject = new GameObject("SceneController");
            _instance = sceneControllerGameObject.AddComponent<SceneController>();

            return _instance;
        }

        public SceneTransitionDestination initialSceneTransitionDestination;

        private Scene _mCurrentZoneScene;
        private bool _mTransitioning;

        private void Awake()
        {
            if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);


            if (initialSceneTransitionDestination != null)
            {
                SetEnteringGameObjectLocation(initialSceneTransitionDestination);
                ScreenFader.SetAlpha(1f);
                StartCoroutine(ScreenFader.FadeSceneIn());
                initialSceneTransitionDestination.onReachDestination.Invoke();
            }
            else
            {
                _mCurrentZoneScene = SceneManager.GetActiveScene();
            }
        }

        public static void TransitionToScene(TransitionPoint transitionPoint, GameObject[] players)
        {
            Instance.StartCoroutine(Instance.Transition(players, transitionPoint.newSceneName,
                transitionPoint.resetInputValuesOnTransition, transitionPoint.transitionDestinationTag,
                transitionPoint.transitionType));
        }

        private IEnumerator Transition(GameObject[] players, string newSceneName, bool resetInputValues,
            SceneTransitionDestination.DestinationTag destinationTag,
            TransitionPoint.TransitionType transitionType = TransitionPoint.TransitionType.DifferentZone)
        {
            _mTransitioning = true;
            PersistentDataManager.SaveAllData();

            // _mPlayerInput.ReleaseControl(resetInputValues);
            yield return StartCoroutine(ScreenFader.FadeSceneOut(ScreenFader.FadeType.Loading));
            //PersistentDataManager.ClearPersisters();
            // _mPlayerInput = FindObjectOfType<PlayerInput>();
            //_mPlayerInput.ReleaseControl(resetInputValues);
            if (isServer)
            {
                AssetsRefresh.Instance.SwitchWorld();
                AssetsRefresh.Instance.hasSwitchedWorld = true;
                NetworkManager.singleton.ServerChangeScene(newSceneName);
            }


            
            PersistentDataManager.LoadAllData();
            yield return StartCoroutine(ScreenFader.FadeSceneIn());
            //_mPlayerInput.GainControl();

            _mTransitioning = false;
        }
        
        private static void SetEnteringGameObjectLocation(Component entrance, GameObject gameObject = null)
        {
            if (entrance == null)
            {
                Debug.LogWarning("Entering Transform's location has not been set.");
                return;
            }

            var entranceLocation = entrance.transform;
            if (gameObject == null) return;
            var enteringTransform = gameObject.transform;
            enteringTransform.position = entranceLocation.position;
            enteringTransform.rotation = entranceLocation.rotation;
        }
        
    }
}