using UnityEngine;

namespace Utility
{
    public class VisibleBubbleUp : MonoBehaviour
    {
        public System.Action<VisibleBubbleUp> ObjectBecameVisible;

        private void OnBecameVisible()
        {
            ObjectBecameVisible(this);
        }
    }
}
