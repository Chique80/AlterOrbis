using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Audio
{
    [RequireComponent(typeof(AudioSource))]
    public class RandomAudioPlayer : MonoBehaviour
    {
        [System.Serializable]
        public struct TileOverride
        {
            public TileBase tile;
            public AudioClip[] clips;
        }

        public AudioClip[] clips;

        public TileOverride[] overrides;

        public bool randomizePitch;
        public float pitchRange = 0.2f;

        private AudioSource _source;
        private Dictionary<TileBase, AudioClip[]> _lookupOverride;

        private void Awake()
        {
            _source = GetComponent<AudioSource>();
            _lookupOverride = new Dictionary<TileBase, AudioClip[]>();

            for(var i = 0; i < overrides.Length; ++i)
            {
                if (overrides[i].tile == null)
                    continue;

                _lookupOverride[overrides[i].tile] = overrides[i].clips;
            }
        }

        public void PlayRandomSound(TileBase surface = null)
        {
            var source = clips;

            if (surface != null && _lookupOverride.TryGetValue(surface, out var temp))
                source = temp;

            var choice = Random.Range(0, source.Length);

            if(randomizePitch)
                _source.pitch = Random.Range(1.0f - pitchRange, 1.0f + pitchRange);

            _source.PlayOneShot(source[choice]);
        }

        public void Stop()
        {
            _source.Stop();
        }
    }
}