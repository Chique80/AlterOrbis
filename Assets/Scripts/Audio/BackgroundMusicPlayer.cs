using SceneManagement;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Audio
{
    public class BackgroundMusicPlayer : MonoBehaviour
    {
        private static BackgroundMusicPlayer Instance
        {
            get
            {
                if (_instance != null)
                    return _instance;

                _instance = FindObjectOfType<BackgroundMusicPlayer> ();

                return _instance;
            }
        }

        public static BackgroundMusicPlayer _instance;

        [Header("Music Settings")]
        public AudioClip currentMusicAudioClip;
        public AudioClip organicMusicAudioClip;
        public AudioClip mecanicMusicAudioClip;
        public AudioMixerGroup musicOutput;
        public bool musicPlayOnAwake = true;
        [Range(0f, 1f)]
        public float musicVolume = 1f;
        [Range(0f, 2f)]
        public float pitch = 1f;
        [Header("Ambient Settings")]
        public AudioClip ambientAudioClip;
        public AudioMixerGroup ambientOutput;
        public bool ambientPlayOnAwake = true;
        [Range(0f, 1f)]
        public float ambientVolume = 1f;

        private AudioSource _musicAudioSource;
        private AudioSource _ambientAudioSource;

        private bool _transferMusicTime;
        private bool _transferAmbientTime;
        private BackgroundMusicPlayer _oldInstanceToDestroy;

        //every clip pushed on that stack through "PushClip" function will play until completed, then pop
        //once that stack is empty, it revert to the musicAudioClip
        private readonly Stack<AudioClip> _musicStack = new Stack<AudioClip>();

        private void Awake ()
        {
            // If there's already a player...
            if (Instance != null && Instance != this)
            {
                //...if it use the same music clip, we set the audio source to be at the same position, so music don't restart
                if(Instance.currentMusicAudioClip == currentMusicAudioClip)
                {
                    _transferMusicTime = true;
                }

                //...if it use the same ambient clip, we set the audio source to be at the same position, so ambient don't restart
                if (Instance.ambientAudioClip == ambientAudioClip)
                {
                    _transferAmbientTime = true;
                }

                // ... destroy the pre-existing player.
                _oldInstanceToDestroy = Instance;
            }
        
            _instance = this;

            _musicAudioSource = gameObject.AddComponent<AudioSource> ();
            _musicAudioSource.clip = currentMusicAudioClip;
            _musicAudioSource.outputAudioMixerGroup = musicOutput;
            _musicAudioSource.loop = true;
            _musicAudioSource.volume = musicVolume;

            if (musicPlayOnAwake)
            {
                _musicAudioSource.time = 0f;
                _musicAudioSource.Play();
            }

            _ambientAudioSource = gameObject.AddComponent<AudioSource>();
            _ambientAudioSource.clip = ambientAudioClip;
            _ambientAudioSource.outputAudioMixerGroup = ambientOutput;
            _ambientAudioSource.loop = true;
            _ambientAudioSource.volume = ambientVolume;

            if (!ambientPlayOnAwake) return;
            _ambientAudioSource.time = 0f;
            _ambientAudioSource.Play();
        }

        private void Start()
        {
            //if delete & transfer time only in Start so we avoid the small gap that doing everything at the same time in Awake would create 
            if (_oldInstanceToDestroy == null) return;
            if (_transferAmbientTime) _ambientAudioSource.timeSamples = _oldInstanceToDestroy._ambientAudioSource.timeSamples;
            if (_transferMusicTime) _musicAudioSource.timeSamples = _oldInstanceToDestroy._musicAudioSource.timeSamples;
            _oldInstanceToDestroy.Stop();
            Destroy(_oldInstanceToDestroy.gameObject);
            UpdateMusicFromWorld();
        }

        private void Update()
        {
            if (_musicStack.Count <= 0) return;
            //isPlaying will be false once the current clip end up playing
            if (_musicAudioSource.isPlaying) return;
            _musicStack.Pop();
            if(_musicStack.Count > 0)
            {
                _musicAudioSource.clip = _musicStack.Peek();
                _musicAudioSource.Play();
            }
            else
            {//Back to looping music clip
                _musicAudioSource.clip = currentMusicAudioClip;
                _musicAudioSource.loop = true;
                _musicAudioSource.Play();
            }
        }

        public void UpdateMusicFromWorld()
        {
            if(currentMusicAudioClip != organicMusicAudioClip && AssetsRefresh.Instance.currentWorldType == AssetsRefresh.WorldType.Organic)
            {
                ChangeMusic(organicMusicAudioClip);
                PushClip(organicMusicAudioClip);
            }
            else if (currentMusicAudioClip != mecanicMusicAudioClip && AssetsRefresh.Instance.currentWorldType == AssetsRefresh.WorldType.Mechanic)
            {
                ChangeMusic(mecanicMusicAudioClip);
                PushClip(mecanicMusicAudioClip);
            }
        }

        public void PushClip(AudioClip clip)
        {
            _musicStack.Push(clip);
            _musicAudioSource.Stop();
            _musicAudioSource.loop = false;
            _musicAudioSource.time = 0;
            _musicAudioSource.clip = clip;
            _musicAudioSource.Play();
        }

        public void ChangeMusic(AudioClip clip)
        {
            currentMusicAudioClip = clip;
            _musicAudioSource.clip = clip;
        }

        public void ChangeAmbient(AudioClip clip)
        {
            ambientAudioClip = clip;
            _ambientAudioSource.clip = clip;
        }


        public void Play ()
        {
            PlayJustAmbient ();
            PlayJustMusic ();
        }

        private void PlayJustMusic ()
        {
            _musicAudioSource.time = 0f;
            _musicAudioSource.Play();
        }

        private void PlayJustAmbient ()
        {
            _ambientAudioSource.Play();
        }

        public void Stop ()
        {
            StopJustAmbient ();
            StopJustMusic ();
        }

        private void StopJustMusic ()
        {
            _musicAudioSource.Stop ();
        }

        private void StopJustAmbient ()
        {
            _ambientAudioSource.Stop ();
        }

        public void Mute ()
        {
            MuteJustAmbient ();
            MuteJustMusic ();
        }

        private void MuteJustMusic ()
        {
            _musicAudioSource.volume = 0f;
        }

        private void MuteJustAmbient ()
        {
            _ambientAudioSource.volume = 0f;
        }

        public void Unmute ()
        {
            UnmuteJustAmbient ();
            UnmuteJustMusic ();
        }

        private void UnmuteJustMusic ()
        {
            _musicAudioSource.volume = musicVolume;
        }

        private void UnmuteJustAmbient ()
        {
            _ambientAudioSource.volume = ambientVolume;
        }

        public void Mute (float fadeTime)
        {
            MuteJustAmbient(fadeTime);
            MuteJustMusic(fadeTime);
        }

        private void MuteJustMusic (float fadeTime)
        {
            StartCoroutine(VolumeFade(_musicAudioSource, 0f, fadeTime));
        }

        private void MuteJustAmbient (float fadeTime)
        {
            StartCoroutine(VolumeFade(_ambientAudioSource, 0f, fadeTime));
        }

        public void Unmute (float fadeTime)
        {
            UnmuteJustAmbient(fadeTime);
            UnmuteJustMusic(fadeTime);
        }

        private void UnmuteJustMusic (float fadeTime)
        {
            StartCoroutine(VolumeFade(_musicAudioSource, musicVolume, fadeTime));
        }

        private void UnmuteJustAmbient (float fadeTime)
        {
            StartCoroutine(VolumeFade(_ambientAudioSource, ambientVolume, fadeTime));
        }

        private static IEnumerator VolumeFade (AudioSource source, float finalVolume, float fadeTime)
        {
            var volumeDifference = Mathf.Abs(source.volume - finalVolume);
            var inverseFadeTime = 1f / fadeTime;

            while (!Mathf.Approximately(source.volume, finalVolume))
            {
                var delta = Time.deltaTime * volumeDifference * inverseFadeTime;
                source.volume = Mathf.MoveTowards(source.volume, finalVolume, delta);
                yield return null;
            }
            source.volume = finalVolume;
        }
    }
}