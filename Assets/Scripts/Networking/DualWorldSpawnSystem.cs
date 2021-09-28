using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror.CharacterSelection;

namespace Mirror
{
    public class DualWorldSpawnSystem : PlayerSpawnSystem
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
        public override void OnStartServer()
        {
            if(MyLogFilter.logInfo) { Debug.Log("On Start Server from Spawn System"); }
            //do some shit

            base.OnStartServer();
        }

    #endregion

    #region Override
        protected override Transform GetSpawnPosition(NetworkSelectableCharacterPlayer roomPlayer)
        {
            //Get a default start position from the network manager
            Transform spawnPosition = NetworkManager.singleton.GetStartPosition();

            //Use zero instead
            if(spawnPosition == null)
            {
                spawnPosition = this.transform;
            }
            
            return spawnPosition;
        }

        protected override GameObject GetPlayerPrefab(NetworkSelectableCharacterPlayer roomPlayer)
        {
            Debug.Log("Spawn player as " + roomPlayer.GetSelectedCharacter());
            return roomPlayer.GetSelectedCharacter();
        }
    #endregion
    }
}

