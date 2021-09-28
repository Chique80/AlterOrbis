using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Mirror.CharacterSelection;

namespace Mirror.Events
{
    [System.Serializable] public class NetworkRoomPlayerEvent : UnityEvent<NetworkRoomPlayer> {}
    [System.Serializable] public class NetworkGamePlayerEvent : UnityEvent<NetworkGamePlayer> {}
    [System.Serializable] public class GameObjectEvent : UnityEvent<GameObject> {}
}
