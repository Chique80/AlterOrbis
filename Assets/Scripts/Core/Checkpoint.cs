using Characters.MonoBehaviours;
using UnityEngine;

namespace Core
{
    [RequireComponent(typeof(CircleCollider2D))]
    public class Checkpoint : MonoBehaviour
    {
        public bool respawnFacingLeft;
        
        private void OnTriggerEnter2D(Collider2D collision)
        {
            var c = collision.GetComponent(typeof(PlayerCharacter)) as PlayerCharacter;
            if(c != null)
            {
                c.SetCheckpoint(this);
            }
        }
    }
}