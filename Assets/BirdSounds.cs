using Audio;
using SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdSounds : MonoBehaviour
{
    double audioTimer = 0.0;
    double delayBetweenSounds = 0.0;

    // Update is called once per frame
    void Update()
    {
        if(delayBetweenSounds == 0.0)
            delayBetweenSounds = Random.Range(20, 40);

        if(AssetsRefresh.Instance.currentWorldType == AssetsRefresh.WorldType.Organic && audioTimer < Time.time)
        {
            //play
            Debug.Log("Play");
            gameObject.GetComponent<RandomAudioPlayer>().PlayRandomSound();
            audioTimer = Time.time + delayBetweenSounds;
            delayBetweenSounds = 0.0;
        }
    }
}
