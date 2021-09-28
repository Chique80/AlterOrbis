using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror;
using Mirror.MessageDictionary;
using Mirror.CharacterSelection;

public class LobbyUIManager : UIManager
{
    [Header("UI Scriptable Panel")]
    public ScriptablePanel pnlTitleScreen;
    public ScriptablePanel pnlSearchingScreen;
    public ScriptablePanel pnlLobby;
    public ScriptablePanel pnlDescription;

    // Start is called before the first frame update
    void Start()
    {
        SetupEvents();
        showTitleScreen();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnApplicationStart()
    {
        /* Title Screen */
        if(pnlTitleScreen == null)
        {
            Debug.LogError(this + " is missing a Title Screen!");
        }
        else
        {
            pnlTitleScreen.gameObject.SetActive(false);
        }

        /* Search Screen */
        if(pnlSearchingScreen == null)
        {
            Debug.LogError(this + " is missing a Search Screen!");
        }
        else
        {
            pnlSearchingScreen.gameObject.SetActive(false);
        }

        /* Lobby Screen */
        if(pnlLobby == null)
        {
            Debug.LogError(this + " is missing a Lobby Screen!");
        }
        else
        {
            pnlLobby.gameObject.SetActive(false);
        }

        /* Description Screen */
        if(pnlDescription == null)
        {
            Debug.LogError(this + " is missing a Description Screen!");
        }
        else
        {
            pnlDescription.gameObject.SetActive(false);
        }
    }

    public void showTitleScreen()
    {
        if(MyLogFilter.logInfo) { Debug.Log("Show Title Screen"); }

        if(pnlTitleScreen == null) return;

        if(pnlLobby != null)
        {
            if(pnlLobby.gameObject.activeInHierarchy)
            {
                pnlLobby.Deactivate();
                pnlLobby.gameObject.SetActive(false);
            }
        }

        if(pnlSearchingScreen != null)
        {
            if(pnlSearchingScreen.gameObject.activeInHierarchy)
            {
                pnlSearchingScreen.Deactivate();
                pnlSearchingScreen.gameObject.SetActive(false);
            }
        }
        
        if(pnlDescription != null)
        {
            if(!pnlDescription.gameObject.activeInHierarchy)
            {
                pnlDescription.Deactivate();
                pnlDescription.gameObject.SetActive(false);
            }
        }

        if(!pnlTitleScreen.gameObject.activeInHierarchy)
        {
            pnlTitleScreen.gameObject.SetActive(true);
            pnlTitleScreen.Activate();
        }
    }

    public void showLobbyScreen()
    {
        if(MyLogFilter.logInfo) { Debug.Log("Show Lobby"); }

        if(pnlLobby == null) return;

        if(pnlTitleScreen != null)
        {
            if(pnlTitleScreen.gameObject.activeInHierarchy)
            {
                pnlTitleScreen.Deactivate();
                pnlTitleScreen.gameObject.SetActive(false);
            }
        }

        if(pnlSearchingScreen != null)
        {
            if(pnlSearchingScreen.gameObject.activeInHierarchy)
            {
                pnlSearchingScreen.Deactivate();
                pnlSearchingScreen.gameObject.SetActive(false);
            }
        }

        if(pnlDescription != null)
        {
            if(!pnlDescription.gameObject.activeInHierarchy)
            {
                pnlDescription.Deactivate();
                pnlDescription.gameObject.SetActive(false);
            }
        }

        if(!pnlLobby.gameObject.activeInHierarchy)
        {
            pnlLobby.gameObject.SetActive(true);
            pnlLobby.Activate();
        }

        
    }

    public void showSearchScreen()
    {
        if(MyLogFilter.logInfo) { Debug.Log("Show Searching Screen"); }

        if(pnlSearchingScreen == null) return;

        if(pnlLobby != null)
        {
            if(pnlLobby.gameObject.activeInHierarchy)
            {
                pnlLobby.Deactivate();
                pnlLobby.gameObject.SetActive(false);
            }
        }    

        if(pnlTitleScreen != null)
        {
            if(pnlTitleScreen.gameObject.activeInHierarchy)
            {
                pnlTitleScreen.Deactivate();
                pnlTitleScreen.gameObject.SetActive(false);
            }
        }

        if(pnlDescription != null)
        {
            if(!pnlDescription.gameObject.activeInHierarchy)
            {
                pnlDescription.Deactivate();
                pnlDescription.gameObject.SetActive(false);
            }
        }

        if(!pnlSearchingScreen.gameObject.activeInHierarchy)
        {
            pnlSearchingScreen.gameObject.SetActive(true);
            pnlSearchingScreen.Activate();
        }
    }

    public void showDescriptionScreen(ShowLoadingScreenMessage msg)
    {
        if(MyLogFilter.logInfo) { Debug.Log("Show Description Screen"); }

        if(pnlDescription == null) return;

        if(pnlLobby != null)
        {
            if(pnlLobby.gameObject.activeInHierarchy)
            {
                pnlLobby.Deactivate();
                pnlLobby.gameObject.SetActive(false);
            }
        }

        if(pnlSearchingScreen != null)
        {
            if(pnlSearchingScreen.gameObject.activeInHierarchy)
            {
                pnlSearchingScreen.Deactivate();
                pnlSearchingScreen.gameObject.SetActive(false);
            }
        }

        if(pnlTitleScreen != null)
        {
            if(pnlTitleScreen.gameObject.activeInHierarchy)
            {
                pnlTitleScreen.Deactivate();
                pnlTitleScreen.gameObject.SetActive(false);
            }
        }

        if(!pnlDescription.gameObject.activeInHierarchy)
        {
            pnlDescription.gameObject.SetActive(true);
            pnlDescription.Activate();
        }
    }

    private void SetupEvents()
    {
        if(NetworkCharacterSelectionManager.singleton != null)
        {
            /* Enter the lobby */
            NetworkCharacterSelectionManager.singleton.OnRoomClientEnterEvent.AddListener(showLobbyScreen);

            /* Exit as client */
            NetworkCharacterSelectionManager.singleton.OnRoomStopClientEvent.AddListener(showTitleScreen);

            /* Show the loading screen */
            NetworkCharacterSelectionManager.singleton.OnRoomClientConnectEvent.AddListener(delegate {
                NetworkClient.RegisterHandler<ShowLoadingScreenMessage>(showDescriptionScreen);     //there are better ways to manage this
            });
            NetworkCharacterSelectionManager.singleton.OnRoomStopClientEvent.AddListener(delegate {
                NetworkClient.UnregisterHandler<ShowLoadingScreenMessage>();                        //there are better ways to manage this
            });
        }
    }
}
