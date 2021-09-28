using Audio;
using Mirror;
using SceneManagement;
using UnityEngine;

namespace Objects
{
    public class AnomalyZone : NetworkBehaviour
    {
        private AssetsRefresh _assetsRefresh;
        private SpriteRenderer _sprite;


        private void Awake()
        {
            _sprite = GetComponent<SpriteRenderer>();
            _sprite.color = new Color(0.08f, 0.08f, 0.08f, 1f);
            _assetsRefresh = GameObject.Find("AssetsRefresh").GetComponent<AssetsRefresh>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (ClientScene.localPlayer && (!other.gameObject.GetComponent<NetworkIdentity>() ||
                                            !other.gameObject.GetComponent<NetworkIdentity>().isLocalPlayer)) return;
            var audioEnter = GetComponent<AudioSource>();
            audioEnter.clip = gameObject.GetComponent<RandomAudioPlayer>().clips[0];
            audioEnter.Play();

            BackgroundMusicPlayer._instance.Mute();
            gameObject.transform.Find("Audio Source").GetComponent<AudioSource>().Play();
            _sprite.color = new Color(1f, 1f, 1f, 1f);
            _assetsRefresh.SwitchWorld();
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (ClientScene.localPlayer && (!other.gameObject.GetComponent<NetworkIdentity>() ||
                                            !other.gameObject.GetComponent<NetworkIdentity>().isLocalPlayer)) return;
            var audioExit = GetComponent<AudioSource>();
            audioExit.clip = gameObject.GetComponent<RandomAudioPlayer>().clips[1];
            audioExit.Play();

            BackgroundMusicPlayer._instance.Unmute();
            gameObject.transform.Find("Audio Source").GetComponent<AudioSource>().Stop();
            _assetsRefresh.SwitchWorld();
        }
    }
}