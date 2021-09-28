using System.Linq;
using Mirror;
using Telepathy;
using UnityEngine;

namespace SceneManagement
{
    [RequireComponent(typeof(Collider2D))]
    public class TransitionPoint : MonoBehaviour
    {
        public AudioSource soundEffect;
        public Animator animator;

        public enum TransitionType
        {
            DifferentZone,
            DifferentNonGameplayScene,
            SameScene,
        }


        public enum TransitionWhen
        {
            ExternalCall,
            InteractPressed,
            OnTriggerEnter,
        }

        public int numberOfPlayers = 4;


        [Tooltip("Whether the transition will be within this scene, to a different zone or a non-gameplay scene.")]
        public TransitionType transitionType;

        [SceneName] public string newSceneName;

        [Tooltip("The tag of the SceneTransitionDestination script in the scene being transitioned to.")]
        public SceneTransitionDestination.DestinationTag transitionDestinationTag;

        [Tooltip("The destination in this scene that the transitioning game object will be teleported.")]
        public TransitionPoint destinationTransform;

        [Tooltip("What should trigger the transition to start.")]
        public TransitionWhen transitionWhen;

        [Tooltip(
            "The player will lose control when the transition happens but should the axis and button values reset to the default when control is lost.")]
        public bool resetInputValuesOnTransition = true;

        private readonly GameObject[] _players = new GameObject[4];
        private int _playerCounter;

        private bool _transitioningGameObjectsPresent;

        private void Start()
        {
            if (transitionWhen == TransitionWhen.ExternalCall)
                _transitioningGameObjectsPresent = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                _players[_playerCounter] = other.gameObject;
                _playerCounter++;
            }


            if (_playerCounter == numberOfPlayers)
            {
                _transitioningGameObjectsPresent = true;
            }

            if (transitionWhen == TransitionWhen.OnTriggerEnter && _transitioningGameObjectsPresent)
                TransitionInternal();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                _players[_playerCounter] = null;
                _playerCounter--;
            }

            if (_playerCounter != numberOfPlayers)
            {
                _transitioningGameObjectsPresent = false;
            }
        }

        private void Update()
        {
            if (!_transitioningGameObjectsPresent)
                return;

            if (transitionWhen != TransitionWhen.InteractPressed) return;
        }

        private void TransitionInternal()
        {
            if (transitionType == TransitionType.SameScene)
            {
                foreach (var player in _players)
                {
                    GameObjectTeleporter.Teleport(player, destinationTransform.transform);
                }
            }
            else
            {
                soundEffect.Play();
                animator.Play("Fade_out");
                //SceneController.TransitionToScene(this, _players);
            }
        }
    }
}