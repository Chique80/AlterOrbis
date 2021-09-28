using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Events;
using Mirror;
using Mirror.Events;
using Mirror.MessageDictionary;
//using Characters.MonoBehaviours;

/* TODO
    -Avoir un NetworkSceneManager
    - Better event management -> probably using message
    - RoomSlots having null slots
*/	   

namespace Mirror.CharacterSelection
{
    [DisallowMultipleComponent]
    public class NetworkCharacterSelectionManager : NetworkRoomManager
    {
        [SerializeField] MyLogFilter.FilterLevel m_LogLevel = (MyLogFilter.FilterLevel)MyLogFilter.Info;

        /// <summary>
        ///     Script in charge of spawning player objects in the scene. The spawn system is
        ///     found in the scene after loading and before spawning the player objects.
        ///     It should only exist on the server.
        /// </summary>
        private PlayerSpawnSystem spawnSystem;

        public MyLogFilter.FilterLevel logLevel { get { return m_LogLevel; }  set { m_LogLevel = value; MyLogFilter.currentLogLevel = (int)value; } }
        
        //Callback => should be implemented through a more versatile system
        [HideInInspector] public UnityEvent OnRoomStartClientEvent;
        [HideInInspector] public UnityEvent OnRoomStopClientEvent;
        [HideInInspector] public UnityEvent OnRoomClientConnectEvent;
        [HideInInspector] public UnityEvent OnRoomClientDisconnectEvent;
        [HideInInspector] public UnityEvent OnRoomClientEnterEvent;
        [HideInInspector] public UnityEvent OnRoomClientExitEvent;
        [HideInInspector] public UnityEvent OnRoomClientAddPlayerFailedEvent;
        [HideInInspector] public UnityEvent OnClientSceneChangedEvent;
        [HideInInspector] public UnityEvent OnRoomStartHostEvent;
        [HideInInspector] public UnityEvent OnRoomStopHostEvent;
        [HideInInspector] public UnityEvent OnRoomStartServerEvent;
        [HideInInspector] public UnityEvent OnServerAddPlayerEvent;
        [HideInInspector] public NetworkRoomPlayerEvent OnRoomServerCreateRoomPlayerEvent;
        [HideInInspector] public GameObjectEvent OnRoomServerCreateGamePlayerEvent;
        [HideInInspector] public UnityEvent OnRoomServerConnectEvent;
        [HideInInspector] public UnityEvent OnRoomServerDisconnectEvent;
        [HideInInspector] public UnityEvent OnServerReadyEvent;
        [HideInInspector] public UnityEvent OnServerSceneChangedEvent;
        [HideInInspector] public UnityEvent OnRoomServerPlayersReadyEvent;
        [HideInInspector] public NetworkRoomPlayerEvent OnLobbyServerPlayerQuitEvent;
        [HideInInspector] public UnityEvent OnStartGameEvent;								

        // Start is called before the first frame update
        public override void Start()
        {
            base.Start();
        }

        // Update is called once per frame
        void Update()
        {
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            //do some cleanup => maybe?
        }

        protected override bool InitializeSingleton()
        {
            if (singleton != null && singleton == this) return true;

            //do this early
            var logLevel = (int)m_LogLevel;
            if (logLevel != MyLogFilter.SetInScripting)
            {
                MyLogFilter.currentLogLevel = logLevel;
            }

            return base.InitializeSingleton();
        }

        public override void OnGUI()
        {
            if (!showRoomGUI)
                return;

            if(isNetworkActive)
            {
                if(ClientScene.localPlayer != null)
                {
                    GUI.Box(new Rect(10, 15, 220, 25), ClientScene.localPlayer.name + ": " + ClientScene.localPlayer.netId);
                }
            }

            GUI.Box(new Rect(10, 45, 180, 25), "Is in room: " + InRoom);
        }

    #region Client
        public override void OnRoomStartClient()
        {
            if (MyLogFilter.logInfo) { Debug.Log("On Room Start Client"); }

            OnRoomStartClientEvent.Invoke();
        }
        public override void OnRoomStopClient()
        {
            if (MyLogFilter.logInfo) { Debug.Log("On Room Stop Client"); }

            OnRoomStopClientEvent.Invoke();  
        }

        public override void OnRoomClientConnect(NetworkConnection conn)
        {
            if (MyLogFilter.logInfo) { Debug.Log("On Room Client Connect"); }
            base.OnRoomClientConnect(conn);

            OnRoomClientConnectEvent.Invoke();

            if (MyLogFilter.logInfo) { Debug.Log("On Room Client Connect (end)"); }
        }
        public override void OnRoomClientDisconnect(NetworkConnection conn)
        {
            if (MyLogFilter.logInfo) { Debug.Log("On Room Client Disconnect"); }
            base.OnRoomClientDisconnect(conn);

            OnRoomClientDisconnectEvent.Invoke();

            if (MyLogFilter.logInfo) { Debug.Log("On Room Client Disconnect (end)"); }
        }

        //Call on all client whenever a client enters the room
        public override void OnRoomClientEnter()
        {
            if (MyLogFilter.logInfo) { Debug.Log("Room Client Enter"); }

            OnRoomClientEnterEvent.Invoke();
        }
        //Call on all client whenever a client leaves the room
        public override void OnRoomClientExit()
        {
            if (MyLogFilter.logInfo) { Debug.Log("Room Client Exit"); }

            OnRoomClientExitEvent.Invoke();
        }
       
        public override void OnRoomClientAddPlayerFailed()
        {
            if (MyLogFilter.logInfo) { Debug.Log("Add Player Failed"); }

            OnRoomClientAddPlayerFailedEvent.Invoke();
        }

        public override void OnClientSceneChanged(NetworkConnection conn)
        {
            if (MyLogFilter.logInfo){ Debug.Log("Client Scene Changed: " + SceneManager.GetActiveScene().name); }

            base.OnClientSceneChanged(conn);
            OnClientSceneChangedEvent.Invoke();    
        }
    #endregion

    #region Server
        public override void OnRoomStartHost()
        {
            if (MyLogFilter.logInfo) { Debug.Log("On Room Start Host"); }
            OnRoomStartHostEvent.Invoke();

            if (MyLogFilter.logInfo) { Debug.Log("On Room Start Host (end)"); }
        }
        public override void OnRoomStopHost()
        {
            if (MyLogFilter.logInfo) { Debug.Log("On Room Stop Host"); }

            OnRoomStopHostEvent.Invoke();

            if (MyLogFilter.logInfo) { Debug.Log("On Room Stop Host (end)"); }
        }
        public override void OnRoomStartServer()
        {
            if (MyLogFilter.logInfo) { Debug.Log("On Room Start Server"); }

            OnRoomStartServerEvent.Invoke();

            if (MyLogFilter.logInfo) { Debug.Log("On Room Start Server (end)"); }
        }
        
        public override void OnServerAddPlayer(NetworkConnection conn)
        {
            if (MyLogFilter.logInfo) { Debug.Log("On Server Add Player"); }
            base.OnServerAddPlayer(conn);

            OnServerAddPlayerEvent.Invoke();

            if (MyLogFilter.logInfo) { Debug.Log("On Server Add Player (end)"); }
        }
        public override void OnRoomServerAddPlayer(NetworkConnection conn)
        {
            if(MyLogFilter.logInfo) { Debug.Log("On Room Server Add Player"); }

            OnRoomServerCreateGamePlayer(conn, NetworkRoomCharacter.localPlayer.gameObject);

            if(MyLogFilter.logInfo) { Debug.Log("On Room Server Add Player (end)"); }
        }

        public override GameObject OnRoomServerCreateRoomPlayer(NetworkConnection conn)
        {
            if (MyLogFilter.logInfo) { Debug.Log("Create Room Player for connection " + conn.connectionId); }

            GameObject playerPrefab = null;

            //Spawn the game object to represent the player in the room
            if(roomPlayerPrefab != null)
            {
                //Verification
                if(roomPlayerPrefab.GetComponent<NetworkSelectableCharacterPlayer>() == null)
                {
                    if(MyLogFilter.logError) { Debug.LogError("Prefab " + roomPlayerPrefab + " must have a NetworkSelectableCharacterPlayer script!"); }
                }

                playerPrefab = Instantiate(roomPlayerPrefab.gameObject);

                //Event to notify the server
                OnRoomServerCreateRoomPlayerEvent.Invoke(playerPrefab.GetComponent<NetworkSelectableCharacterPlayer>());

                //Message to notify clients
                PlayerCreatedMessage msg = new PlayerCreatedMessage();
                msg.playerId = playerPrefab.GetComponent<NetworkSelectableCharacterPlayer>().netId;
                NetworkServer.SendToAll<PlayerCreatedMessage>(msg);
            }

            if (MyLogFilter.logInfo) { Debug.Log("Create Room Player (end)"); }
            return playerPrefab;
        }
        public override GameObject OnRoomServerCreateGamePlayer(NetworkConnection conn, GameObject roomPlayer)
        {
            if(MyLogFilter.logInfo) { Debug.Log("Create Game Player for connection " + conn.connectionId); }

            GameObject playerObject = null;

            //Spawn the game object to represent the player in game
            if(roomPlayer.GetComponent<NetworkSelectableCharacterPlayer>() != null)
            {
                if (MyLogFilter.logDebug) { Debug.Log("Create Game Player for player: " + roomPlayer.GetComponent<NetworkSelectableCharacterPlayer>().netId); }
                
                //Spawn the game object
                if(spawnSystem != null)
                {
                    playerObject = spawnSystem.SpawnPlayerObject(roomPlayer.GetComponent<NetworkSelectableCharacterPlayer>());
                }
                else
                {
					if(MyLogFilter.logWarn) { Debug.LogWarning("Can't find spawn system."); }
                    playerObject = DefaultSpawnPlayer(playerObject);
                }

                OnRoomServerCreateGamePlayerEvent.Invoke(playerObject);
            }
            else
            {
                if (MyLogFilter.logError) { Debug.LogError("(" + conn + ") Room Player must have a NetworkSelectableCharacter script in order to be spawned"); }
            }

            return playerObject;
        }

        public override void OnRoomServerConnect(NetworkConnection conn)
        {
            if(MyLogFilter.logInfo) { Debug.Log("On Room Server Connect"); }

            OnRoomServerConnectEvent.Invoke();

            if(MyLogFilter.logInfo) { Debug.Log("On Room Server Connect (end)"); }
        }
        public override void OnRoomServerDisconnect(NetworkConnection conn)
        {
            if(MyLogFilter.logInfo) { Debug.Log("On Room Server Disconnect"); }

            OnRoomServerDisconnectEvent.Invoke();

            if(MyLogFilter.logInfo) { Debug.Log("On Room Server Disconnect (end)"); }
        }

        public override void OnServerReady(NetworkConnection conn)
        {
            if(MyLogFilter.logInfo) { Debug.Log("On Server Ready"); }
            base.OnServerReady(conn);

            OnServerReadyEvent.Invoke();
            
            if(MyLogFilter.logInfo) { Debug.Log("On Server Ready (end)"); }
        }


        public override void ServerChangeScene(string newSceneName)
        {
            if(MyLogFilter.logWarn) { Debug.Log("Server Change Scene to " + newSceneName); }

            //TODO more info on what scene are changing

            //Temporary fix -> idealy, would need to fix all the spawning things
            if(!InRoom && newSceneName != RoomScene)
            {
                foreach(NetworkRoomPlayer roomPlayer in roomSlots)
                {
                    if(roomPlayer != null)
                    {
                        NetworkConnection conn = roomPlayer.connectionToClient;
                        Debug.Log("("+roomPlayer.netId+") " + conn);
                        
                        NetworkServer.ReplacePlayerForConnection(conn, roomPlayer.gameObject, true);        //maybe null for client object
                    }
                }
            }
            

            base.ServerChangeScene(newSceneName);   //we might want to redefine that
            

            if(MyLogFilter.logWarn) { Debug.Log("Finish changing scene"); }
        }
        public override void OnRoomServerSceneChanged(string sceneName)
        {
            if(MyLogFilter.logInfo) { Debug.Log("On Room Server Scene Changed to " + sceneName); }
            
            //TODO more info on what scene are changing

            if(!InRoom)
            {
                //Search for the spawn system
                spawnSystem = GameObject.FindObjectOfType<PlayerSpawnSystem>();
                if(MyLogFilter.logInfo)
                {
                    Debug.Log("Spawn system is " + spawnSystem);
                }
            }

            OnServerSceneChangedEvent.Invoke();
            base.OnRoomServerSceneChanged(sceneName);
        }
        public override bool OnRoomServerSceneLoadedForPlayer(NetworkConnection conn, GameObject roomPlayer, GameObject gamePlayer)
        {
            if (MyLogFilter.logInfo) { Debug.Log("Scene loaded for " + roomPlayer.GetComponent<NetworkSelectableCharacterPlayer>().netId); }

            if(roomPlayer.GetComponent<NetworkSelectableCharacterPlayer>() == null)
            {
                if(MyLogFilter.logInfo) { Debug.LogError("Room Player Prefab does not have a NetworkSelectableCharacterPlayer script!"); }
            }
            else if(gamePlayer.GetComponent<NetworkIdentity>() == null)
            {
                if(MyLogFilter.logError) { Debug.LogError("Player Prefab does not have a NetworkIdentity script!"); }
            }
            else
            {
                NetworkSelectableCharacterPlayer networkRoomPlayer = roomPlayer.GetComponent<NetworkSelectableCharacterPlayer>();
                networkRoomPlayer.RpcOnLocalPlayerLeavesRoom();             //shouldn't be there, but it works (for now)

                NetworkServer.ReplacePlayerForConnection(conn, gamePlayer, true);           //assign newly spawned object to the client 
            }

            return false;           //return false to stop the chain
        }

        public override void OnRoomServerPlayersReady()
        {
            if (MyLogFilter.logInfo) { Debug.Log("On Room Players Ready"); }

            OnRoomServerPlayersReadyEvent.Invoke();

            if (MyLogFilter.logInfo) { Debug.Log("On Room Players Ready (end)"); }
        }

        public void StartGame()
        {
            OnStartGameEvent.Invoke();

            ServerChangeScene(GameplayScene);
        }
    #endregion

    #region Utility
        private GameObject DefaultSpawnPlayer(GameObject prefab)
        {
            if(prefab == null) return null;

            GameObject obj = null;

            if(NetworkServer.active)
            {
                Transform startPosition = NetworkManager.singleton.GetStartPosition();      //Get a start position

                if(startPosition != null)       //If a start position is found in the scene
                {
                    obj = (GameObject) Instantiate(prefab.gameObject, startPosition.position, startPosition.rotation);
                }
                else                            //Default spawning (without start position)
                {
                    if(MyLogFilter.logError) { Debug.LogWarning("No start position - spawning at (0,0,0)"); }

                    obj = (GameObject) Instantiate(prefab.gameObject, Vector3.zero, Quaternion.identity);
                }
            }

            return obj;
        }
    #endregion

    #region Property
        public new static NetworkCharacterSelectionManager singleton {
            get {
                if(NetworkManager.singleton != null) {
                    return (NetworkCharacterSelectionManager) NetworkManager.singleton;
                }
                return null;   
            }
        }

        /// <summary>
        ///     The room object of this client. Only available when in room.
        ///     Equivalent to ClientScene.localplayer.
        /// </summary>
        public NetworkSelectableCharacterPlayer RoomLocalPlayer {
            get {
                if(InRoom && ClientScene.localPlayer != null)
                {
                    return ClientScene.localPlayer.GetComponent<NetworkSelectableCharacterPlayer>();
                }

                return null;
            }
        }

        /// <summary>
        ///     The player object of this client. Only available when in game.
        ///     Equivalent to ClientScene.localplayer.
        /// </summary>
        public GameObject GameLocalPlayer {
            get {
                if(!InRoom && isNetworkActive && ClientScene.localPlayer != null)
                {
                    return ClientScene.localPlayer.gameObject;
                }

                return null;
            }
        }

        /// <summary>
        ///     True if the player is in the room and connected.
        /// <summry>
        public bool InRoom {
            get {
                return (NetworkClient.isConnected && IsSceneActive(RoomScene));
            }
        } 
    #endregion

    #region Editor
        public override void OnValidate()
        {
            base.OnValidate();

            if(roomPlayerPrefab != null)
            {
                if(roomPlayerPrefab.GetComponent<NetworkSelectableCharacterPlayer>() == null)
                {
                    Debug.LogError("NetworkManager - Room Player Prefab must have a NetworkSelectableCharacterPlayer script!");
                    roomPlayerPrefab = null;
                }
            }
        }
    #endregion

    }
}


