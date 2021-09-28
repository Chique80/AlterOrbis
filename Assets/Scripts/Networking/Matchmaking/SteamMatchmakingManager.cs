using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Steamworks;
using Mirror.CharacterSelection;

namespace Mirror.Matchmaking
{
    public class SteamMatchmakingManager : NetworkMatchmakingManager
    {
        private const string HostAdressKey = "HostAdressKey";
        private const string MatchNameKey = "MatchNameKey";

        // Steam callback
        protected Callback<LobbyCreated_t> c_LobbyCreated;
        protected Callback<LobbyMatchList_t> c_MatchListRequested;
        protected Callback<GameLobbyJoinRequested_t> c_JoinLobbyRequested;
        protected Callback<LobbyEnter_t> c_LobbyEntered;

        //CCallResult<SteamMatchmakingManager, LobbyMatchList_t> m_CallResultLobbyMatchList;
        

        // Start is called before the first frame update
        void Start()
        {
            c_LobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
            c_MatchListRequested = Callback<LobbyMatchList_t>.Create(OnMatchListRequested);

            c_JoinLobbyRequested = Callback<GameLobbyJoinRequested_t>.Create(OnJoinLobbyRequested);
            c_LobbyEntered = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
        }

        // Update is called once per frame
        void Update()
        {
            
        }

        #region Override

        public override void CreateMatch()
        {
            if(MyLogFilter.logInfo) { Debug.Log("Create Match"); }
            if(!SteamManager.Initialized)
            {
                Debug.LogWarning("Steam manager is not initialized");
                //call event or return false
                return;
            }

            if(randomMatchName)
            {
                matchName = GenerateRandomMatchName(matchNameSize);          //Generated a random name for the match, if necessary
            }
            else
            {
                if(matchName.Length != matchNameSize)
                {
                    Debug.LogError("Invalid Match Name");
                    //call event or return false
                    return;
                }
            }
            OnCreateMatch(matchName);

            //Create the steam lobby
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, NetworkManager.singleton.maxConnections);
        }

        public override void SearchMatch(string matchName)
        {
            if(MyLogFilter.logInfo) { Debug.Log("Search for match " + matchName); }
            if(!SteamManager.Initialized)
            {
                Debug.LogWarning("Steam manager is not initialized");
                //call event or return false
                return;
            }

            if(matchName.Length != matchNameSize)
            {
                Debug.LogError("Invalid Match Name");
                //call event or return false
                return;
            }
            this.matchName = matchName;

            OnSearchMatch(matchName);

            //Search for possible lobby
            SteamMatchmaking.AddRequestLobbyListStringFilter(MatchNameKey, matchName, ELobbyComparison.k_ELobbyComparisonEqual);
            SteamMatchmaking.RequestLobbyList();
        }

        #endregion

        #region Matchmaking Callback
        #endregion

        #region Steam Callback
        private void OnLobbyCreated(LobbyCreated_t callback)
        {
            if(callback.m_eResult != EResult.k_EResultOK)
            {
                if(MyLogFilter.logWarn) { Debug.LogWarning("Failed to create steam lobby"); }
                OnFailedCreateMatch();
                return;
            }

            //Save save data in the lobby
            CSteamID steamID = new CSteamID(callback.m_ulSteamIDLobby);
            SteamMatchmaking.SetLobbyData(steamID, HostAdressKey, SteamUser.GetSteamID().ToString());       //address to connect to
            SteamMatchmaking.SetLobbyData(steamID, MatchNameKey, matchName);                                //match name

            OnSuccessfulCreateMatch(matchName);

            //Start the NetworkServer as the host
            NetworkManager.singleton.StartHost();
        }

        private void OnMatchListRequested(LobbyMatchList_t callback)
        {
            if(MyLogFilter.logInfo) { Debug.Log("Searching Match List"); }

            if(callback.m_nLobbiesMatching > 1)
            {
                Debug.LogWarning("Multiple lobby with this name. Will only take the first one.");
            }
            else if(callback.m_nLobbiesMatching == 0)
            {
                Debug.LogWarning("No lobby available");
                OnMatchNotFound(matchName);
                return;
            }

            //Try to join the lobby
            CSteamID steamID = SteamMatchmaking.GetLobbyByIndex(0);
            SteamMatchmaking.JoinLobby(steamID);

            if(MyLogFilter.logInfo) { Debug.Log("Trying to join lobby " + steamID); }
        }
        private void OnJoinLobbyRequested(GameLobbyJoinRequested_t callback)
        {
            if(MyLogFilter.logInfo) { Debug.Log("Request to join lobby"); }

            OnSearchMatch(null);

            //Try to join the lobby
            SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);

            if(MyLogFilter.logInfo) { Debug.Log("Trying to join lobby " + callback.m_steamIDLobby); }
        }

        private void OnLobbyEntered(LobbyEnter_t callback)
        {
            if(MyLogFilter.logInfo) { Debug.Log("Enter lobby"); }
            if(callback.m_EChatRoomEnterResponse != (int) EChatRoomEnterResponse.k_EChatRoomEnterResponseSuccess)
            {
                Debug.LogWarning("Can't enter lobby");
                OnMatchNotFound(null);                          //could use a better callback
                return;
            }

            //Get data from the lobby
            CSteamID steamID = new CSteamID(callback.m_ulSteamIDLobby);
            matchName = SteamMatchmaking.GetLobbyData(steamID, MatchNameKey);                                       //match name
            NetworkManager.singleton.networkAddress = SteamMatchmaking.GetLobbyData(steamID, HostAdressKey);        //address to connect to

            OnMatchFound(this.matchName);

            //If we are already the server?
            if(NetworkServer.active) {
                Debug.Log("Network Server is active");
                return;
            }

            //Start the NetworkClient
            NetworkManager.singleton.StartClient();

            OnJoinMatch(matchName);

            if(MyLogFilter.logInfo) { Debug.Log("Enter lobby (end)"); }
        }
        #endregion

        #region Property

        #endregion
    }
}

