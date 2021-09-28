using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Android;
using VivoxUnity;
using System.Linq;
using Mirror;
using Mirror.Matchmaking;
using Mirror.CharacterSelection;

[DisallowMultipleComponent]
public class ChannelManager : MonoBehaviour
{
    public static ChannelManager singleton;

    private VivoxVoiceManager vivoxVoiceManager;

    private bool PermissionsDenied;

    private bool isQuitting = false;

    [Header("Lobby Channel")]
    public string lobbyChannelName = "lobbyChannel";

    [Header("Team A Channel")]
    public string teamAChannelName = "TeamAChannel";

    [Header("Team B Channel")]
    public string teamBChannelName = "TeamBChannel";
    

    //Callbacks
    public delegate void ChannelParticipantStatusChangedHandler(string username, ChannelId channelId, IParticipant participant);
    public event ChannelParticipantStatusChangedHandler OnParticipantAddedEvent;
    public event ChannelParticipantStatusChangedHandler OnParticipantRemovedEvent;

    public delegate void LoginStatusChangedHandler();
    public event LoginStatusChangedHandler OnUserLoggedInEvent;
    public event LoginStatusChangedHandler OnUserLoggedOutEvent;

    void Awake()
    {
        InitializeSingleton();
    }

    // Start is called before the first frame update
    void Start()
    {
        vivoxVoiceManager = VivoxVoiceManager.Instance;
        if(vivoxVoiceManager == null)
        {
            if(MyLogFilter.logError) { Debug.LogError("Cannot find VivoxVoiceManager. Will disable this component."); }
            this.enabled = false;
            return;
        }

        vivoxVoiceManager.OnParticipantAddedEvent += OnParticipantAdded;
        vivoxVoiceManager.OnParticipantRemovedEvent += OnParticipantRemoved;
        vivoxVoiceManager.OnUserLoggedInEvent += OnUserLoggedIn;
        vivoxVoiceManager.OnUserLoggedOutEvent += OnUserLoggedOut;

        NetworkCharacterSelectionManager.singleton.OnRoomStartClientEvent.AddListener(delegate {
            Debug.Log("Login to Vivox");
            LoginToVivoxService();
        });
        NetworkCharacterSelectionManager.singleton.OnRoomStopClientEvent.AddListener(delegate {
            if(!isQuitting)
            {
                LogoutOfVivoxService();
            }
            else if(isQuitting)
            {
                if(MyLogFilter.logWarn) { Debug.LogWarning("Quitting application - will not log out of Vivox. This is normal."); }
            }
        });
    }

    // Update is called once per frame
    void Update()
    {
    }

    private void OnApplicationQuit()
    {
        Debug.Log("ChannelManager Application Quit");

        isQuitting = true;

        vivoxVoiceManager.OnUserLoggedInEvent -= OnUserLoggedIn;
        vivoxVoiceManager.OnUserLoggedOutEvent -= OnUserLoggedOut;
        vivoxVoiceManager.OnParticipantAddedEvent -= OnParticipantAdded;
        vivoxVoiceManager.OnParticipantRemovedEvent -= OnParticipantRemoved;
    }

    protected virtual void InitializeSingleton()
    {
        if(singleton != null && singleton == this) return;

        if(singleton != null)
        {
            Debug.Log("Multiple ChannelManagers detected in the scene. Only one ChannelManager can exist at a time. The duplicate ChannelManager will not be used."); 
            Destroy(gameObject);
            return;
        }

        Debug.Log("ChannelManager created singleton");
        singleton = this;
        if (Application.isPlaying) DontDestroyOnLoad(gameObject);
    }

    #region Login
    public void LoginToVivoxService(string displayName = null)
    {
        if (Permission.HasUserAuthorizedPermission(Permission.Microphone))
        {
            // The user authorized use of the microphone.
            vivoxVoiceManager.Login(displayName);
        }
        else
        {
            // Check if the users has already denied permissions
            if (PermissionsDenied)
            {
                PermissionsDenied = false;
                vivoxVoiceManager.Login(displayName);
            }
            else
            {
                PermissionsDenied = true;
                // We do not have permission to use the microphone.
                // Ask for permission or proceed without the functionality enabled.
                Permission.RequestUserPermission(Permission.Microphone);
            }
        }
    }

    public void LogoutOfVivoxService()
    {
        if(vivoxVoiceManager.LoginSession.State == LoginState.LoggedIn || vivoxVoiceManager.LoginSession.State == LoginState.LoggingIn)
        {
            vivoxVoiceManager.DisconnectAllChannels();
            vivoxVoiceManager.Logout();
        }
        else
        {
            if(MyLogFilter.logInfo) { Debug.LogWarning("User is not logged in and therefore will not be logout. This is normal."); }
        }
        
    }

    #endregion

    #region Channel

    private void JoinChannel(string channelName, ChannelType type, VivoxVoiceManager.ChatCapability chatCapability, bool switchTransmission = true)
    {
        //Make the real name of the channel
        String realChannelName = channelName;
        if(NetworkMatchmakingManager.singleton != null)
        {
            realChannelName += NetworkMatchmakingManager.singleton.matchName;
        }

        var channel = GetChannelSession(channelName);
        if (vivoxVoiceManager.ActiveChannels.Count == 0 || channel == null)     //Connect to channel
        {
            vivoxVoiceManager.JoinChannel(realChannelName, type, chatCapability, switchTransmission);
        }
        else                                                                   //Connect to audio if already in channel
        {
            if (channel.AudioState == ConnectionState.Disconnected)
            {
                channel.BeginSetAudioConnected(true, true, ar =>
                {
                    if(MyLogFilter.logWarn) { vivoxVoiceManager.VivoxLog("Now transmitting into " + channelName); }
                });
            }
        }
    }

    private void LeaveChannel(string channelName, AsyncCallback callback = null)
    {
        IChannelSession channelSession = GetChannelSession(channelName);

        if(channelSession != null)
        {
            if(vivoxVoiceManager.LoginState == LoginState.LoggedIn)
            {
                channelSession.Disconnect(callback);
            }
            else
            {
                if(MyLogFilter.logWarn) { vivoxVoiceManager.VivoxLogError("Trying to leave a channel when logged out!"); }
            }
        }
        else
        {
            if(MyLogFilter.logError) { vivoxVoiceManager.VivoxLogError("Not connected to " + channelName); }
        }
    }

    private void SwitchToChannel(string channelName)
    {
        IChannelSession channelSession = GetChannelSession(channelName);

        if(channelSession != null)
        {
            vivoxVoiceManager.LoginSession.SetTransmissionMode(TransmissionMode.Single, channelSession.Channel);
        }
        else
        {
            if(MyLogFilter.logError) { vivoxVoiceManager.VivoxLogError("Not connected to " + channelName); }
        }
    }

    public void JoinLobbyChannel(bool switchTransmission = true)
    {
        if(NetworkRoomCharacter.localPlayer == null)
        {
            if(MyLogFilter.logWarn) { Debug.LogWarning("Cannot find local player. Will not join lobby channel"); }
            return;
        }

        JoinChannel(lobbyChannelName, ChannelType.NonPositional, VivoxVoiceManager.ChatCapability.AudioOnly, switchTransmission);
    }

    public void LeaveLobbyChannel(AsyncCallback callback = null)
    {
        if(NetworkRoomCharacter.localPlayer == null)
        {
            if(MyLogFilter.logWarn) { Debug.LogWarning("Cannot find local player. Will not leave lobby channel"); }
            return;
        }

        LeaveChannel(lobbyChannelName, callback);
    }

    public void JoinTeamChannel(bool switchTransmission = true)
    {
        if(NetworkRoomCharacter.localPlayer == null)
        {
            if(MyLogFilter.logWarn) { Debug.LogWarning("Cannot find local player. Will not join team channel"); }
            return;
        }

        if(NetworkRoomCharacter.localPlayer.Type == PlayerType.Leaf || NetworkRoomCharacter.localPlayer.Type == PlayerType.Snail)
        {
            JoinChannel(teamAChannelName, ChannelType.NonPositional, VivoxVoiceManager.ChatCapability.AudioOnly, switchTransmission);
        }
        else if(NetworkRoomCharacter.localPlayer.Type == PlayerType.Rope || NetworkRoomCharacter.localPlayer.Type == PlayerType.Gun)
        {
            JoinChannel(teamBChannelName, ChannelType.NonPositional, VivoxVoiceManager.ChatCapability.AudioOnly, switchTransmission);
        }
    }

    public void LeaveTeamChannel(AsyncCallback callback = null)
    {
        if(NetworkRoomCharacter.localPlayer == null)
        {
            if(MyLogFilter.logWarn) { Debug.LogWarning("Cannot find local player. Will not leave team channel"); }
            return;
        }

        if(NetworkRoomCharacter.localPlayer.Type == PlayerType.Leaf 
            || NetworkRoomCharacter.localPlayer.Type == PlayerType.Snail)
        {
            LeaveChannel(teamAChannelName, callback);
        }
        else if(NetworkRoomCharacter.localPlayer.Type == PlayerType.Rope 
            || NetworkRoomCharacter.localPlayer.Type == PlayerType.Gun)
        {
            LeaveChannel(teamBChannelName, callback);
        }
    }

    public void SwitchToTeamChannel()
    {
        if(NetworkRoomCharacter.localPlayer == null)
        {
            if(MyLogFilter.logWarn) { Debug.LogWarning("Cannot find local player. Will not switch to team channel"); }
            return;
        }

        //Find the channel session of the appropriate team
        if(NetworkRoomCharacter.localPlayer.Type == PlayerType.Leaf 
            || NetworkRoomCharacter.localPlayer.Type == PlayerType.Snail)
        {
            SwitchToChannel(teamAChannelName);
        }
        else if(NetworkRoomCharacter.localPlayer.Type == PlayerType.Rope 
            || NetworkRoomCharacter.localPlayer.Type == PlayerType.Gun)
        {
            SwitchToChannel(teamBChannelName);
        }
    }

    public void SwitchToLobbyChannel()
    {
        if(NetworkRoomCharacter.localPlayer == null)
        {
            if(MyLogFilter.logWarn) { Debug.LogWarning("Cannot find local player. Will not switch to lobby channel"); }
            return;
        }

        SwitchToChannel(lobbyChannelName);
    }

    #endregion

    #region Callback

    private void OnUserLoggedIn()
    {
        if(MyLogFilter.logWarn) { vivoxVoiceManager.VivoxLog("On User Login"); }

        OnUserLoggedInEvent?.Invoke();
    }
    private void OnUserLoggedOut()
    {
        if(MyLogFilter.logWarn) { vivoxVoiceManager.VivoxLog("On User Logout"); }

        OnUserLoggedOutEvent?.Invoke();
    }

    private void OnParticipantAdded(string username, ChannelId channel, IParticipant participant)
    {
        if(MyLogFilter.logWarn) { vivoxVoiceManager.VivoxLog(username + " joined channel " + channel.Name); }

        OnParticipantAddedEvent?.Invoke(username, channel, participant);
    }
    private void OnParticipantRemoved(string username, ChannelId channel, IParticipant participant)
    {
        if(MyLogFilter.logWarn) { vivoxVoiceManager.VivoxLog(username + " left channel " + channel.Name); }

        OnParticipantRemovedEvent?.Invoke(username, channel, participant);
    }

    #endregion

    #region Utility

    public IChannelSession GetChannelSession(string channelName)
    {
        //Make the real name of the channel
        String realChannelName = channelName;
        if(NetworkMatchmakingManager.singleton != null)
        {
            realChannelName += NetworkMatchmakingManager.singleton.matchName;
        }

        int i = 0;
        IChannelSession channelSession = null;

        while(i < vivoxVoiceManager.ActiveChannels.Count && channelSession == null)
        {
            if(vivoxVoiceManager.ActiveChannels.ElementAt(i).Channel.Name == realChannelName)
            {
                channelSession = vivoxVoiceManager.ActiveChannels.ElementAt(i);
            }

            i++;
        }

        return channelSession;
    }

    #endregion
}
