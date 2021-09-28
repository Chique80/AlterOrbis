using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Mirror.CharacterSelection;
using VivoxUnity;

[DisallowMultipleComponent]
[RequireComponent(typeof(NetworkGamePlayer))]
public class GameCommunication : NetworkBehaviour
{
    private ChannelManager channelManager;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    #region Client
    public override void OnStartLocalPlayer()
    {
        channelManager = ChannelManager.singleton;
        if(channelManager == null)
        {
            if(MyLogFilter.logError) { Debug.LogError("Cannot find ChannelManager. Will disable this component."); }
            this.enabled = false;
            return;
        }
    }
    #endregion

    void OnTriggerEntere2D(Collider2D collider)         
    {
        if(isLocalPlayer)
        {
            if(collider.GetComponent<CommunicationArea>() != null)
            {
                OnEnterCommunicationArea();
            }
        }
    }

    void OnTriggerExit2D(Collider2D collider)
    {
        if(isLocalPlayer)
        {
            if(collider.GetComponent<CommunicationArea>() != null)
            {
                OnExitCommunicationArea();
            }
        }
    }

    public void OnEnterCommunicationArea()
    {
        if(isLocalPlayer)
        {
            Debug.Log("Has entered communication area");
            channelManager.SwitchToLobbyChannel();
        }
    }

    public void OnExitCommunicationArea()
    {
        if(isLocalPlayer)
        {
            Debug.Log("Has left communication area");
            channelManager.SwitchToTeamChannel();
        }
    }
}
