using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror;
using Mirror.MessageDictionary;
using Mirror.CharacterSelection;

//Don't know what to do with this
namespace Mirror
{
    //[RequireComponent(typeof(PlayerCharacter))]
    public class NetworkGamePlayer : NetworkBehaviour
    {
        public static NetworkGamePlayer localPlayer;

        [HideInInspector] public UnityEvent OnPlayerQuitEvent;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            
        }

        void OnDestroy()
        {
            localPlayer = null;
        }

        #region Client

        public override void OnStartLocalPlayer()
        {
            if(MyLogFilter.logInfo) { Debug.Log("On Start Local Player from " + netId); }

            if(localPlayer != null && localPlayer == this) return;
            if(localPlayer != null && localPlayer != this)
            {
                if(MyLogFilter.logWarn) { Debug.LogWarning("There is already a local player (NetworkGamePlayer): " + localPlayer + ". Destroying this object."); }
                Destroy(gameObject);
                return;
            }
            Debug.Log("NetworkGamePlayer created localPlayer");
            localPlayer = this;
            return;
        }

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
        #endregion
        
    }
}

