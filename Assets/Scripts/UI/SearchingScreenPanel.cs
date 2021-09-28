using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror.CharacterSelection;

public class SearchingScreenPanel : ScriptablePanel
{
    //private void HasStopSearching = false;

    [Header("Text Strings")]
    public string strSearching;
    public string strMatchFound;
    public string strMatchNotFound;

    [Header("UI Components")]
    public LabeledButton btnCancelSearch;
    public Text txtSearchState;

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
        //Setup Cancel Search button
        if(btnCancelSearch == null)
        {
            Debug.LogError(this + " is missing a Cancel Search button!");
        }
        else
        {
            btnCancelSearch.onClick.AddListener(delegate {Debug.Log("Button: Cancel Search");});
            //btnCancelSearch.onClick.AddListener(NetworkMatchmakingManager.singleton.CancelSearch);        
        }

        //Setup Search State Text
        if(txtSearchState == null)
        {
            Debug.LogWarning(this + " is missing a label for the Search State!");
        }
    }

    public override void Activate()
    {
        UpdateSearchingText(strSearching);

        /* Match found */
        //Register listener of NetworkMatchmakingManager

        /* Match not found */
        //Register listener of NetworkMatchmakingManager
    }

    public override void Deactivate()
    {
        /* Match found */
        //Register listener of NetworkMatchmakingManager

        /* Match not found */
        //Register listener of NetworkMatchmakingManager
    }

    private void OnMatchFound()
    {
        UpdateSearchingText(strMatchFound);
    }

    private void OnMatchNotFound()
    {
        UpdateSearchingText(strMatchNotFound);

        if(btnCancelSearch != null)
        {
            btnCancelSearch.SetText("Return to Title");
        }
    }

    private void UpdateSearchingText(string newText)
    {
        if(txtSearchState != null)
        {
            txtSearchState.text = newText;
        }
    }
}
