using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Mirror.CharacterSelection;

/*
    THIS IS FOR TESTING ONLY
*/

public class BasePlayerScript : NetworkBehaviour
{
    public float speed;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(isLocalPlayer)
        {
            if(Input.GetKey(KeyCode.W))
            {
                transform.position += Vector3.forward*speed;
            }
            if(Input.GetKey(KeyCode.S))
            {
                transform.position -= Vector3.forward*speed;
            }
            if(Input.GetKey(KeyCode.A))
            {
                transform.position += Vector3.left*speed;
            }
            if(Input.GetKey(KeyCode.D))
            {
                transform.position += Vector3.right*speed;
            }
        }
    }
}
