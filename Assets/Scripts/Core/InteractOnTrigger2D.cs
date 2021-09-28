using UnityEngine;
using UnityEngine.Events;

namespace Core
{
    [RequireComponent(typeof(Collider2D))]
    public sealed class InteractOnTrigger2D : MonoBehaviour
    {
        public LayerMask layers;
        public UnityEvent onEnter;
        public UnityEvent onExit;

        private Collider2D _collider;

        private void Reset()
        {
            // ReSharper disable once Unity.UnknownLayer
            layers = LayerMask.NameToLayer("Everything");
            _collider = GetComponent<Collider2D>();
            _collider.isTrigger = true;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if(!enabled)
                return;
            
            if (layers.Contains(other.gameObject))
            {
                ExecuteOnEnter(other);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if(!enabled)
                return;
        
            if (layers.Contains(other.gameObject))
            {
                ExecuteOnExit(other);
            }
        }

        // ReSharper disable once UnusedParameter.Local
        private void ExecuteOnEnter(Collider2D other)
        {
            onEnter.Invoke();
        }

        // ReSharper disable once UnusedParameter.Local
        private void ExecuteOnExit(Collider2D other)
        {
            onExit.Invoke();
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawIcon(transform.position, "InteractionTrigger", false);
        }
    }
}