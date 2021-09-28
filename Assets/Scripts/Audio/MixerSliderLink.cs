using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

namespace Audio
{
    [RequireComponent(typeof(Slider))]
    public class MixerSliderLink : MonoBehaviour
    {
        public AudioMixer mixer;
        public string mixerParameter;

        public float maxAttenuation;
        public float minAttenuation = -80.0f;

        private Slider _slider;


        private void Awake ()
        {
            _slider = GetComponent<Slider>();

            mixer.GetFloat(mixerParameter, out var value);

            _slider.value = (value - minAttenuation) / (maxAttenuation - minAttenuation);

            _slider.onValueChanged.AddListener(SliderValueChange);
        }


        private void SliderValueChange(float value)
        {
            mixer.SetFloat(mixerParameter, minAttenuation + value * (maxAttenuation - minAttenuation));
        }
    }
}