using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Mirror.CharacterSelection;
using Mirror;

public class DescriptionPanel : ScriptablePanel
{
    public GameObject controlsDescription;

    [Header("Character Description")]
    public GameObject leafDescription;
    public GameObject snailDescription;
    public GameObject ropeDescription;
    public GameObject gunDescription;

    [Header("Loading Bar")]
    public Slider loadBar;
    private float updateTime;
    private float timer = 0f;

    [Header("Parameters")]
    public float GameDescriptionTime;
    public float CharacterDescriptionTime;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(loadBar != null)
        {
            timer += Time.deltaTime;
            if(timer >= updateTime)
            {
                loadBar.value += timer;
                updateTime = Random.Range(0.5f, 2);
                timer = 0;
            }
        }
    }

    protected override void Setup()
    {
        if(controlsDescription == null)
        {
            Debug.LogWarning(this + " is missing a Controls Description screen!");
        }

        if(leafDescription == null)
        {
            Debug.LogWarning(this + " is missing a Leaf Description screen!");
        }

        if(snailDescription == null)
        {
            Debug.LogWarning(this + " is missing a Snail Description screen!");
        }

        if(ropeDescription == null)
        {
            Debug.LogWarning(this + " is missing a Rope Description screen!");
        }

        if(gunDescription == null)
        {
            Debug.LogWarning(this + " is missing a Gun Description screen!");
        }

        if(loadBar == null)
        {
            Debug.LogWarning(this + " is missing a Loading Bar!");
        }
        else
        {
            loadBar.wholeNumbers = false;
            loadBar.maxValue = GameDescriptionTime + CharacterDescriptionTime;
            loadBar.minValue = 0;
        }
    }

    public override void Activate()
    {
        if(leafDescription != null)
        {
            leafDescription.SetActive(false);
        }

        if(snailDescription != null)
        {
            snailDescription.SetActive(false);
        }

        if(ropeDescription != null)
        {
            ropeDescription.SetActive(false);
        }

        if(gunDescription != null)
        {
            gunDescription.SetActive(false);
        }

        if(controlsDescription != null)
        {
            controlsDescription.SetActive(true);
        }

        if(loadBar != null)
        {
            loadBar.value = Random.Range(1, 3);
        }        

        StartCoroutine("ShowDescriptionScreens");
    }

    public override void Deactivate()
    {
        StopCoroutine("ShowDescriptionScreens");

        if(loadBar != null)
        {
            loadBar.value = 0;
        }
    }


    IEnumerator ShowDescriptionScreens()
    {
        //Show the general game description screen
        if(controlsDescription != null)
        {
            controlsDescription.SetActive(true);
        }

        yield return new WaitForSeconds(GameDescriptionTime);

        if(controlsDescription != null)
        {
            controlsDescription.SetActive(false);
        }

        //Show a description screen based on which character the player selected
        switch(NetworkRoomCharacter.localPlayer.Type)
        {
            case PlayerType.Leaf:
                if(leafDescription)
                {
                    leafDescription.SetActive(true);
                }
                break;

            case PlayerType.Snail:
                if(snailDescription)
                {
                    snailDescription.SetActive(true);
                }
                break;

            case PlayerType.Rope:
                if(ropeDescription)
                {
                    ropeDescription.SetActive(true);
                }
                break;

            case PlayerType.Gun:
                if(gunDescription)
                {
                    gunDescription.SetActive(true);
                }
                break;

            default:
                if(controlsDescription)
                {
                    controlsDescription.SetActive(true);
                }
                break;
        }

        yield return new WaitForSeconds(CharacterDescriptionTime);

        //Start the game
        if(NetworkRoomCharacter.localPlayer.isServer)
        {
            NetworkCharacterSelectionManager.singleton.StartGame();
        }

        yield return null;
    }
}
