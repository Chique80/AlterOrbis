using UnityEngine;

namespace Utility
{
    public class SetMaterialRenderQueue : MonoBehaviour
    {

        public Material material;
        public int queueOverrideValue;
    
        // Start is called before the first frame update
        private void Start()
        {
            material.renderQueue = queueOverrideValue;

        }

  
    }
}
