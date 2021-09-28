using System.Collections;
using Mirror;
using UnityEngine;

namespace Objects
{
    public class FreezableObject : NetworkBehaviour
    {
        //[SyncVar]
        public bool started;
        
        public Material defaultMaterial;
        public Material freezeMaterial;
        
        public void StartMoving()
        {
            var rb = GetComponent<Rigidbody2D>();
            var bodyType = rb.bodyType;
            rb.bodyType = RigidbodyType2D.Kinematic;
            
            if (bodyType == RigidbodyType2D.Dynamic)
            {
                rb.bodyType = RigidbodyType2D.Dynamic;
            }
            
            rb.constraints = RigidbodyConstraints2D.None;

            
           
            started = true;
        }

        protected void StopMoving()
        {
            var rb = GetComponent<Rigidbody2D>();
            rb.constraints = RigidbodyConstraints2D.FreezeAll;
            started = false;
        }

        public void Freeze(float time)
        {
            StartCoroutine(FreezeObject(time));
            
            if (isServer)
            {
                FreezeOnClients(time);
            }
            else if (isClientOnly)
            {
                FreezeOnServer(time);
            }
        }

        
        [Command]
        private void FreezeOnServer(float time)
        {
            StartCoroutine(FreezeObject(time));
            FreezeOnClients(time);
        }

        [ClientRpc(includeOwner = false)]
        private void FreezeOnClients(float time)
        {
            StartCoroutine(FreezeObject(time));  
        }
        
        

        private IEnumerator FreezeObject(float time)
        {
            var sprites = gameObject.GetComponentsInChildren<SpriteRenderer>();

            foreach (var sprite in sprites)
            {
                sprite.material = freezeMaterial;
            }

            StopMoving();
            yield return new WaitForSeconds(time);
            StartMoving();
            foreach (var sprite in sprites)
            {
                sprite.material = defaultMaterial;
            }
        }
    }
}