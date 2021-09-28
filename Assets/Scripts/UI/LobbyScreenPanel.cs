using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Mirror.CharacterSelection;
using Mirror.Matchmaking;
using Mirror.MessageDictionary;

//Check when player disconnect because it unready other players

public class LobbyScreenPanel : ScriptablePanel
{
    public Button btnReady;
    public Button btnQuit;
    public Button btnUnselect;
    public Button btnStart;
    public GameObject imgNotReadyState;
    public GameObject imgReadyState;
    public Text txtMatchName;
    public List<UILobbySlot> lobbySlots;        //fixme maybe unecessary

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if(ClientScene.localPlayer.isServer)   //is server
        {
            if(NetworkCharacterSelectionManager.singleton.allPlayersReady)
            {
                btnStart.interactable = true;
            }
            else
            {
                btnStart.interactable = false;
            }
        }
    }

    protected override void Setup()
    {
        //Button to indicate the player is ready to start
        if(btnReady == null)
        {
            Debug.LogError(this + " is missing a Ready button!");
        }
        else
        {
            btnReady.onClick.AddListener(TogglePlayerState);
        }

        //Button to quit the lobby
        if(btnQuit == null)
        {
            Debug.LogError(this + " is missing a Quit button!");
        }
        else
        {
            btnQuit.onClick.AddListener(CallQuitGame);
        }

        //Button to unselect the player
        if(btnUnselect == null)
        {
            Debug.Log(this + " is missing an Unselect button!");
        }
        else
        {
            btnUnselect.onClick.AddListener(CallUnselect);
        }

        //Button to start the game
        if(btnStart == null)
        {
            Debug.LogError(this + " is missing an Start button!");
        }
        else
        {
            btnStart.onClick.AddListener(CallStartGame);
        }
    }

    public override void Activate()
    {
        if(NetworkClient.active)
        {
            if(NetworkCharacterSelectionManager.singleton.RoomLocalPlayer == null)
            {
                Debug.LogError("Local Player doesn't exist!");
                return;
            }
            
            //Add listeners to local player
            AddListenersToLocalPlayer();

            //Launch some update
            OnPlayerStateChange((NetworkRoomCharacter) NetworkCharacterSelectionManager.singleton.RoomLocalPlayer);

            //Matchmaking listeners => maybe?
            if(NetworkMatchmakingManager.singleton != null)
            {
                if(txtMatchName != null)
                {
                    txtMatchName.text = NetworkMatchmakingManager.singleton.matchName;
                }
            }
        }
        else
        {
            if(MyLogFilter.logWarn) { Debug.LogWarning("Can't register handlers and listeners because client isn't active"); }
        }

        btnReady.interactable = false;
        btnUnselect.interactable = false;

        if(ClientScene.localPlayer.isServer)   //is server
        {
            btnStart.interactable = false;
        }
        else                                    //is client
        {
            btnStart.gameObject.SetActive(false);
        }
    }

    public override void Deactivate()
    {
        if(NetworkClient.active && NetworkCharacterSelectionManager.singleton.RoomLocalPlayer != null)
        {
            RemoveListenersToLocalPlayer();
        }
    }

#region Button Event
    private void CallQuitGame()
    {
        if(NetworkClient.active)
        {
            NetworkRoomCharacter.Quit();
        }
    }

    private void CallUnselect()
    {
        ((NetworkRoomCharacter) NetworkCharacterSelectionManager.singleton.RoomLocalPlayer).ChangePlayerType(PlayerType.None);
    }

    //There is a better way to manage this
    private void CallStartGame()
    {
        ShowLoadingScreenMessage msg = new ShowLoadingScreenMessage();
        NetworkServer.SendToAll<ShowLoadingScreenMessage>(msg);

        ConnectToTeamChannelMessage msg2 = new ConnectToTeamChannelMessage();
        NetworkServer.SendToAll<ConnectToTeamChannelMessage>(msg2);
    }

    private void TogglePlayerState()
    {
        NetworkSelectableCharacterPlayer player = NetworkCharacterSelectionManager.singleton.RoomLocalPlayer;

        if(player.readyToBegin) 
        {
            player.setNotReady();
        } 
        else
        {
            player.setReady();
        }
    }
#endregion

    private void AddListenersToLocalPlayer()
    {
        NetworkRoomCharacter player = (NetworkRoomCharacter) NetworkCharacterSelectionManager.singleton.RoomLocalPlayer;

        if(MyLogFilter.logDebug) { Debug.Log("Add listeners to local player " + player.netId); }

        /* A client enters the room */
        player.OnClientEnterRoomEvent.AddListener(OnPlayerEnterRoom);

        /* Local player type changes */
        player.OnSelectionChangeEvent.AddListener(delegate {
            OnPlayerTypeChange(player);
        });

        /* Local player state changes */
        player.OnStateChangeEvent.AddListener(delegate {
            OnPlayerStateChange(player);
        });
    }

    private void RemoveListenersToLocalPlayer()
    {
        NetworkRoomCharacter player = (NetworkRoomCharacter) NetworkCharacterSelectionManager.singleton.RoomLocalPlayer;

        if(MyLogFilter.logDebug) { Debug.Log("Remove listeners to local player " + player.netId); }

        /* A client enters the room */
        player.OnClientEnterRoomEvent.RemoveListener(OnPlayerEnterRoom);

        /* Local player type changes */
        player.OnSelectionChangeEvent.RemoveListener(delegate {
            OnPlayerTypeChange(player);
        });

        /* Local player state changes */
        player.OnStateChangeEvent.RemoveListener(delegate {
            OnPlayerStateChange(player);
        });
    }

    private void OnPlayerEnterRoom()
    {
        if(MyLogFilter.logDebug) { Debug.Log("A player has enter the room - update listeners"); }

        //Reset listeners of lobby slots
        foreach(NetworkRoomPlayer player in NetworkCharacterSelectionManager.singleton.roomSlots)
        {
            if(player != null)
            {
                foreach(UILobbySlot slot in lobbySlots)
                {
                    slot.AddListenersToPlayer((NetworkRoomCharacter) player);
                }
            }
        }
    }

    private void OnPlayerTypeChange(NetworkRoomCharacter player)
    {
        if(player.isLocalPlayer)
        {
            if(player.Type == PlayerType.None)
            {
                btnReady.interactable = false;
                btnUnselect.interactable = false;
            }
            else
            {
                btnReady.interactable = true;
                btnUnselect.interactable = true;
            }
        }
        
    }

    private void OnPlayerStateChange(NetworkRoomCharacter player)
    {
        if(NetworkCharacterSelectionManager.singleton.InRoom &&
            player.isLocalPlayer)
        {
            if(player.readyToBegin)
            {
                btnUnselect.interactable = false;

                if(imgNotReadyState != null && imgReadyState != null)
                {
                    imgNotReadyState.SetActive(false);
                    imgReadyState.SetActive(true);
                }
            }
            else
            {
                btnUnselect.interactable = true;

                if(imgNotReadyState != null && imgReadyState != null)
                {
                    imgNotReadyState.SetActive(true);
                    imgReadyState.SetActive(false);
                }
            }    
        }
    }
}
