using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseHoverButton : MonoBehaviour
{
    public void PlaySound()
    {
        if(GetComponent<Button>().interactable)
            GameObject.Find("ButtonHover").GetComponent<AudioSource>().Play();
    }
}
