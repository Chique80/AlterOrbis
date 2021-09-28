using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Mirror.CharacterSelection
{
    public abstract class PlayerSpawnSystem : NetworkBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {
            
        }

        // Update is called once per frame
        void Update()
        {
            
        }

    #region Server
        [Server] public GameObject SpawnPlayerObject(NetworkSelectableCharacterPlayer roomPlayer)
        {
            GameObject gameObject = null;

            if(NetworkServer.active && roomPlayer != null)
            {
                Transform spawnPosition = GetSpawnPosition(roomPlayer);
                GameObject prefab = GetPlayerPrefab(roomPlayer);

                if(spawnPosition == null)
                {
                    if(MyLogFilter.logWarn) { Debug.LogWarning("Can't find position to spawn object for " + roomPlayer); }
                }
                else if(prefab == null)
                {
                    if(MyLogFilter.logWarn) { Debug.LogWarning("Can't find prefab to spawn object for " + roomPlayer); }
                }
                else if(prefab.GetComponent<NetworkIdentity>() == null)
                {
                    if(MyLogFilter.logError) { Debug.LogError("(" + roomPlayer + ") Prefab must have a NetworkIdentity to be spawned!"); }
                }
                else
                {
                    gameObject = (GameObject) Instantiate(prefab, spawnPosition.position, spawnPosition.rotation);
                }
            }

            return gameObject;
        }
    #endregion

    #region ABSTRACT
        protected abstract Transform GetSpawnPosition(NetworkSelectableCharacterPlayer roomPlayer);
        protected abstract GameObject GetPlayerPrefab(NetworkSelectableCharacterPlayer roomPlayer);
    #endregion
    }
}


