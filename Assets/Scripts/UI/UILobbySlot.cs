using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Mirror.CharacterSelection;

[System.Serializable]
public class UILobbySlot : MonoBehaviour
{
    [SerializeField] PlayerType m_Type;                         //Player type associated with this slot (given to the player)
    private NetworkRoomCharacter m_Player;                      //Player who selected this slot

    /// <summary>
    ///     Check to indicate this slot should auto-update to match the type of players
    /// </summary>
    public bool AutoTrackPlayers;

    [Header("Component")]
    public Button button;
    public Image border;


    // Awake is called before Start
    void Awake()
    {
        if(m_Type == PlayerType.None)
        {
            Debug.LogWarning("No type was assigned!");
        }

        if(button == null)
        {
            Debug.LogError("No button was assigned!");
        }
        else
        {
            button.onClick.AddListener(OnSelect);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Refresh();
    }

    //empty
    void OnEnable()
    {
        
    }
    //empty
    void OnDisable()
    {
        
    }

    public void Refresh()
    {
        if(border != null)
        {
            if(m_Player == null)
            {
                border.gameObject.SetActive(false);
            }
            else
            {
                border.gameObject.SetActive(true);
            }
        }
    }

    /// <summary>
    ///     Setup this slot to track changes of type of the player.
    ///     Only works if AutoTrackPlayers is enabled.
    /// </summary>
    /// <param name="player">The player to track</param>
    public void AddListenersToPlayer(NetworkRoomCharacter player)
    {
        if(AutoTrackPlayers)
        {
            if(MyLogFilter.logDebug) { Debug.Log("Add listeners to " + player.netId); }

            /* Player type changes */
            player.OnSelectionChangeEvent.AddListener(delegate {
                OnPlayerChangeType(player);
            });

            //If the created is of the type of this slot, select the slot
            if(this.Type != PlayerType.None && player.Type == this.Type)
            {
                SetPlayer(player);
            }
        }
    }

    /// <summary>
    ///     Remove the listeners place on the player by this UILobbySlot.
    /// </summary>
    /// <param name="player">The player</param>
    public void RemoveListenersToPlayer(NetworkRoomCharacter player)
    {
        if(AutoTrackPlayers)
        {
            if(MyLogFilter.logDebug) { Debug.Log("Remove listeners to " + player.netId); }

            /* Player type changes */
            player.OnSelectionChangeEvent.RemoveListener(delegate {
                OnPlayerChangeType(player);
            });

            SetPlayer(null);
        }
    }

    /// <summary>
    ///     Associate this slot with the given player.
    /// </summary>
    /// <param name="player">The new player.</param>
    public void SetPlayer(NetworkRoomCharacter player)
    {
        m_Player = player;

        if(border != null)
        {
            if(player == null)
            {
                border.gameObject.SetActive(false);
            }
            else
            {
                border.gameObject.SetActive(true);
            }
        }
        
    }

    private void OnSelect()
    {
        Debug.Log("Select " + Type);

        if(m_Type != PlayerType.None && !NetworkCharacterSelectionManager.singleton.RoomLocalPlayer.readyToBegin)
        {
            NetworkRoomCharacter player = (NetworkRoomCharacter) NetworkCharacterSelectionManager.singleton.RoomLocalPlayer;

            if(player == m_Player)              //unselect the slot
            {
                player.ChangePlayerType(PlayerType.None);
            }
            else if(m_Player == null)           //select the slot
            {
                player.ChangePlayerType(Type);
            }
        }
    }

    private void OnPlayerChangeType(NetworkRoomPlayer player)
    {   
        NetworkRoomCharacter character = player as NetworkRoomCharacter;

        //Unselect the slot
        if(m_Player == player && m_Type != character.Type)
        {
            SetPlayer(null);
        }
        //Select the slot
        else if(m_Type == character.Type)
        {
            SetPlayer(character);
        }
    }

    #region Property
        public PlayerType Type {
            get {
                return m_Type;
            }
            set {
                m_Type = value;
            }
        }
        
        public bool IsSelected {
            get {
                return m_Player != null;
            }
        }

        public NetworkRoomCharacter Player {
            get {
                return m_Player;
            }
        }
    #endregion
}
