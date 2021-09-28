using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DetectRoomNumber : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        if(gameObject.GetComponent<Text>().text.Length == 5)
        {
            GameObject.Find("FindGameButton").GetComponent<LabeledButton>().interactable = true;
        }
        else
        {
            GameObject.Find("FindGameButton").GetComponent<LabeledButton>().interactable = false;
        }

    }
}
