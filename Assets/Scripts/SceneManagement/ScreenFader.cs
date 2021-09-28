using System.Collections;
using UnityEngine;

namespace SceneManagement
{
    public class ScreenFader : MonoBehaviour
    {
        public enum FadeType
        {
            Black, Loading, GameOver,
        }

        private static ScreenFader Instance
        {
            get
            {
                if (_sInstance != null)
                    return _sInstance;

                _sInstance = FindObjectOfType<ScreenFader> ();

                if (_sInstance != null)
                    return _sInstance;

                Create ();

                return _sInstance;
            }
        }

        public static bool IsFading => Instance._mIsFading;

        private static ScreenFader _sInstance;

        private static void Create ()
        {
            var controllerPrefab = Resources.Load<ScreenFader> ("ScreenFader");
            _sInstance = Instantiate (controllerPrefab);
        }


        public CanvasGroup faderCanvasGroup;
        public CanvasGroup loadingCanvasGroup;
        public CanvasGroup gameOverCanvasGroup;
        public float fadeDuration = 1f;

        private bool _mIsFading;
        

        private void Awake ()
        {
            if (Instance != this)
            {
                Destroy (gameObject);
                return;
            }
        }

        private IEnumerator Fade(float finalAlpha, CanvasGroup canvasGroup)
        {
            _mIsFading = true;
            canvasGroup.blocksRaycasts = true;
            var fadeSpeed = Mathf.Abs(canvasGroup.alpha - finalAlpha) / fadeDuration;
            while (!Mathf.Approximately(canvasGroup.alpha, finalAlpha))
            {
                canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, finalAlpha,
                    fadeSpeed * Time.deltaTime);
                yield return null;
            }
            canvasGroup.alpha = finalAlpha;
            _mIsFading = false;
            canvasGroup.blocksRaycasts = false;
        }

        public static void SetAlpha (float alpha)
        {
            Instance.faderCanvasGroup.alpha = alpha;
        }

        public static IEnumerator FadeSceneIn ()
        {
            CanvasGroup canvasGroup;
            if (Instance.faderCanvasGroup.alpha > 0.1f)
                canvasGroup = Instance.faderCanvasGroup;
            else if (Instance.gameOverCanvasGroup.alpha > 0.1f)
                canvasGroup = Instance.gameOverCanvasGroup;
            else
                canvasGroup = Instance.loadingCanvasGroup;
            
            yield return Instance.StartCoroutine(Instance.Fade(0f, canvasGroup));

            canvasGroup.gameObject.SetActive (false);
        }

        public static IEnumerator FadeSceneOut (FadeType fadeType = FadeType.Black)
        {
            CanvasGroup canvasGroup;
            switch (fadeType)
            {
                case FadeType.Black:
                    canvasGroup = Instance.faderCanvasGroup;
                    break;
                case FadeType.GameOver:
                    canvasGroup = Instance.gameOverCanvasGroup;
                    break;
                case FadeType.Loading:
                    canvasGroup = Instance.loadingCanvasGroup;
                    break;
                default:
                    canvasGroup = Instance.loadingCanvasGroup;
                    break;
            }
            
            canvasGroup.gameObject.SetActive (true);
            
            yield return Instance.StartCoroutine(Instance.Fade(1f, canvasGroup));
        }
    }
}