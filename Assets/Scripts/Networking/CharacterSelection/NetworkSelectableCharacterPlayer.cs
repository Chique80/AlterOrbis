using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Mirror.CharacterSelection
{
    //Add a hook for re-entering the lobby

    [DisallowMultipleComponent]
    public class NetworkSelectableCharacterPlayer : NetworkRoomPlayer
    {
        [HideInInspector] public UnityEvent OnClientEnterRoomEvent;
        [HideInInspector] public UnityEvent OnclientExitRoomEvent;
        [HideInInspector] public UnityEvent OnLocalPlayerLeavesRoomEvent;
        [HideInInspector] public UnityEvent OnSelectionChangeEvent;
        [HideInInspector] public UnityEvent OnStateChangeEvent;
        [HideInInspector] public UnityEvent OnPlayerQuitEvent;

        public void setReady()
        {
            this.CmdChangeReadyState(true);
        }
        public void setNotReady()
        {
            this.CmdChangeReadyState(false);
        }

        /// <summary>
        ///     Returns the character prefab selected by this player.
        ///     This is a default method and will return the playerPrefab of the network manager.
        ///     This should be override by extending classes.
        /// </summary>
        public virtual GameObject GetSelectedCharacter()
        {
            if(NetworkClient.active)
            {
                return NetworkManager.singleton.playerPrefab;
            }

            return null;
        }

        /// <summary>
        ///     Called to indicate that the player has changed it's selection.
        /// </summary>
        protected virtual void SelectionChanged()
        {
            OnSelectionChangeEvent.Invoke();
        }

        #region Client
        public override void OnClientEnterRoom()
        {
            if(MyLogFilter.logDebug) { Debug.Log("On Client Enter Room by " + netId); }

            OnClientEnterRoomEvent.Invoke();
        }
        public override void OnClientExitRoom()
        {
            if(MyLogFilter.logDebug) { Debug.Log("On Client Exit Room by " + netId); }

            OnclientExitRoomEvent.Invoke();
        }

        /// <summary>
        ///     This is a hook that is invoke on the local player when it leaves the room scene
        /// <summary>
        [ClientRpc] public virtual void RpcOnLocalPlayerLeavesRoom()
        {
            if(isLocalPlayer)
            {
                if(MyLogFilter.logInfo) { Debug.Log("On Local Client Leave Room"); }

                OnLocalPlayerLeavesRoomEvent.Invoke();
            }
        }

        public override void ReadyStateChanged(bool oldReadyState, bool newReadyState) 
        {
            OnStateChangeEvent.Invoke();
        }
        #endregion        
    }
}

