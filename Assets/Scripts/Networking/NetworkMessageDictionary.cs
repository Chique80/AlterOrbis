using Mirror;

namespace Mirror.MessageDictionary 
{
    public struct PlayerCreatedMessage : NetworkMessage
    {
        public uint playerId;
    }

    public struct PlayerQuitMessage : NetworkMessage
    {
        public uint playerId;
        public bool isServer;
    }

    public struct ShowLoadingScreenMessage : NetworkMessage { }

    public struct ConnectToTeamChannelMessage : NetworkMessage { }
}

