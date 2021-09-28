using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;
using Mirror.Events;
using Mirror.CharacterSelection;
using VivoxUnity;

namespace Mirror
{
    public class NetworkRoomCharacter : NetworkSelectableCharacterPlayer
    {
        public static NetworkRoomCharacter localPlayer;

        [SyncVar] private PlayerType type;

        // Update is called once per frame
        void Update()
        {
        }

        public override void OnDisable()
        {
            localPlayer = null;

            base.OnDisable();
        }

        public void ChangePlayerType(PlayerType newType)
        {
            CmdSetPlayerType(newType);
        }

		public override GameObject GetSelectedCharacter()
        {
            GameObject prefab = GameManager.singleton.GetPrefabOfType(Type);

            if(prefab == null)
            {
                if(MyLogFilter.logError) { Debug.LogError("There is no prefab for type " + Type); }
            }

            return prefab;
        }

        #region Client
        public override void OnStartLocalPlayer()
        {
            if(MyLogFilter.logInfo) { Debug.Log("On Start Local Player from " + netId); }

            if(localPlayer != null && localPlayer == this) return;
            if(localPlayer != null && localPlayer != this)
            {
                if(MyLogFilter.logWarn) { Debug.LogWarning("There is already a local player (NetworkRoomCharacter): " + localPlayer + ". Destroying this object."); }
                Destroy(gameObject);
                return;
            }
            Debug.Log("NetworkRoomCharacter created localPlayer");
            localPlayer = this;
        }

        [ClientRpc] private void RpcSetPlayerType(PlayerType newType)
        {
            this.type = newType;

            SelectionChanged();
        }
        
        #endregion
        
        #region Server

        [Command] private void CmdSetPlayerType(PlayerType newType)
        {
            RpcSetPlayerType(newType);
        }
        
        #endregion

        #region Property

        public PlayerType Type {
            get {
                return type;
            }
        }
        
        #endregion

        /// <summary>
        ///     Disconnect the player from the game.
        ///     Shutdown the server if it is the host.
        /// </summary>
        public static bool Quit()
        {
            if(localPlayer == null) return false;

            if(MyLogFilter.logInfo) { Debug.Log("Player " + localPlayer.netId + " Quit"); }

            localPlayer.OnPlayerQuitEvent.Invoke();

            if(localPlayer.isServer)
            {
                NetworkManager.singleton.StopHost();
                return true;
            }
            else    //isClient
            {
                NetworkManager.singleton.StopClient();        //fixme
                return true;
            }
        }
    }
}

