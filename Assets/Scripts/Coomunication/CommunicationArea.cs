using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CommunicationArea : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.GetComponent<GameCommunication>() != null)
        {
            other.GetComponent<GameCommunication>().OnEnterCommunicationArea();
        }
    }

    void OnTriggerExit2D(Collider2D other)
    {
        if(other.GetComponent<GameCommunication>() != null)
        {
            other.GetComponent<GameCommunication>().OnExitCommunicationArea();
        }
    }
}
