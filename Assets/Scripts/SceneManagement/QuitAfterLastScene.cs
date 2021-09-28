using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class QuitAfterLastScene : StateMachineBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        Debug.Log("Quit Game");
        if(NetworkClient.active || NetworkServer.active)
        {
            if(!NetworkRoomCharacter.Quit())
            {
                if(!NetworkGamePlayer.Quit())
                {
                    Debug.LogWarning("Well, there's not much I can do for you. I about you quit the game?");
                }
            }
        }
    }
}
