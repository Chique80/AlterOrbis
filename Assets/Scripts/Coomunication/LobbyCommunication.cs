using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Mirror.MessageDictionary;
using VivoxUnity;

[DisallowMultipleComponent]
[RequireComponent(typeof(NetworkRoomCharacter))]
public class LobbyCommunication : NetworkBehaviour
{
    private ChannelManager channelManager;
    private NetworkRoomCharacter roomCharacter;

    private bool isQuitting = false;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnApplicationQuit()
    {
        isQuitting = true;   
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

        /* Join team channel */
        NetworkClient.RegisterHandler<ConnectToTeamChannelMessage>(JoinTeamChannel);    //there are better ways to manage

        //Join lobby channel (or join when logged in)
        if(VivoxVoiceManager.Instance.LoginSession.State == LoginState.LoggedIn)
        {
            channelManager.JoinLobbyChannel();
        }
        else
        {
            channelManager.OnUserLoggedInEvent += OnUserLogin;
        }
        
    }
    public override void OnStopClient()
    {
        /* Join team channel */
        NetworkClient.UnregisterHandler<ConnectToTeamChannelMessage>();    //there are better ways to manage

        if(isLocalPlayer && !isQuitting)
        {
            channelManager.OnUserLoggedInEvent -= OnUserLogin;
            channelManager.LogoutOfVivoxService();
        }
        else if(isQuitting)
        {
            if(MyLogFilter.logWarn) { Debug.LogWarning("Quitting application - will not log out of Vivox. This is normal."); }
        }
    }
    #endregion

    #region Callback
    //there are better ways to manage this
    private void JoinTeamChannel(ConnectToTeamChannelMessage msg)
    {
        channelManager.JoinTeamChannel();
    }

    private void OnUserLogin()
    {
        channelManager.JoinLobbyChannel();
    }
    #endregion
}
