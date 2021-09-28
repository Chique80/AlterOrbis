using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Mirror.CharacterSelection;
using Mirror.Matchmaking;

public class TitleScreenPanel : ScriptablePanel
{
    public LabeledButton btnCreateGame;
    public LabeledButton btnFindGame;     
    public InputField inRoomName;         

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void Setup()
    {
        //Setup Create Game button
        if(btnCreateGame == null)
        {
            Debug.LogError(this + " is missing a Create Game button!");
        }
        else
        {
            btnCreateGame.onClick.AddListener(delegate {Debug.Log("Button: Create");});
            btnCreateGame.onClick.AddListener(CallCreateGame);        
        }

        //Setup Find Game button
        if(btnFindGame == null)
        {
            Debug.LogError(this + " is missing a Find Game button!");
        }
        else
        {
            btnFindGame.onClick.AddListener(delegate {Debug.Log("Button: Find");});
            btnFindGame.onClick.AddListener(CallFindGame);
        }

        //Setup Room Name Input
        if(inRoomName == null)
        {
            Debug.LogError(this + " is missing a Input Field for the Room Name!");
        }
        else if(NetworkMatchmakingManager.singleton != null)
        {
            inRoomName.characterLimit = NetworkMatchmakingManager.singleton.matchNameSize;
        }
    }

    public override void Activate()
    {
        btnCreateGame.interactable = true;
        btnFindGame.interactable = true;

        if(NetworkMatchmakingManager.singleton != null)
        {
            /* When the matchmaking ends */
            NetworkMatchmakingManager.singleton.OnSuccessfulCreateMatchEvent.AddListener(ReEnableButton);
            NetworkMatchmakingManager.singleton.OnFailedCreateMatchEvent.AddListener(ReEnableButton);
            NetworkMatchmakingManager.singleton.OnMatchNotFoundEvent.AddListener(ReEnableButton);
            NetworkMatchmakingManager.singleton.OnJoinMatchEvent.AddListener(ReEnableButton);
        }
    }

    public override void Deactivate()
    {
        if(NetworkMatchmakingManager.singleton != null)
        {
            /* When the matchmaking ends */
            NetworkMatchmakingManager.singleton.OnSuccessfulCreateMatchEvent.RemoveListener(ReEnableButton);
            NetworkMatchmakingManager.singleton.OnFailedCreateMatchEvent.RemoveListener(ReEnableButton);
            NetworkMatchmakingManager.singleton.OnMatchNotFoundEvent.RemoveListener(ReEnableButton);
            NetworkMatchmakingManager.singleton.OnJoinMatchEvent.RemoveListener(ReEnableButton);
        }
    }

    #region Button Events

    private void CallCreateGame()
    {
        btnCreateGame.interactable = false;
        btnFindGame.interactable = false;

        if(NetworkMatchmakingManager.singleton != null)
        {
            NetworkMatchmakingManager.singleton.CreateMatch();
        }
        else        //should not happen
        {
            //NetworkCharacterSelectionManager.singleton.StartHost();
        }
        
    }

    private void CallFindGame()
    {
        string matchName = inRoomName.text;

        btnCreateGame.interactable = false;
        btnFindGame.interactable = false;

        if(NetworkMatchmakingManager.singleton != null)
        {
            NetworkMatchmakingManager.singleton.SearchMatch(matchName);
        }
        else        //should not happen
        {
            //NetworkCharacterSelectionManager.singleton.StartClient();
        }
        
    }

    #endregion

    private void ReEnableButton()
    {
        btnCreateGame.interactable = true;
        btnFindGame.interactable = true;
    }   
}
