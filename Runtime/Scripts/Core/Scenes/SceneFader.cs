using System.Collections;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Events;

namespace DaftAppleGames.Scenes
{
    public enum FadeState {Idle, FadingIn, FadingOut, FadeComplete }

    public class SceneFader : MonoBehaviour
    {
        [Header("Control")]
        public bool fadeInOnStart = false;

        [Header("Canvas Fade")]
        public float delayBeforeFadeIn = 2.0f;
        public float delayBeforeFadeOut = 2.0f;
        public float fadeOutDuration = 5.0f;
        public float fadeInDuration = 5.0f;
        public GameObject fadePanel;
        public CanvasGroup fadeCanvasGroup;

        [Header("Audio Fade")]
        public bool fadeAudio = true;
        public AudioMixer audioMixer;

        [Header("Events")]
        public UnityEvent fadeStartEvent;
        public UnityEvent fadeEndEvent;

        internal bool IsReady => _fadeState is FadeState.Idle or FadeState.FadeComplete;
        internal FadeState FadeState => _fadeState;

        private FadeState _fadeState = FadeState.Idle;
        private bool _isFading;

        public void Awake()
        {
            EnableFaderPanel();

            // Get the Canvas group for fading
            fadeCanvasGroup = GetComponentInChildren<CanvasGroup>(true);

            _fadeState = FadeState.Idle;
        }

        /// <summary>
        /// Begin fade, if enabled
        /// </summary>
        private void Start()
        {
            if (fadeAudio)
            {
                audioMixer.SetFloat("MasterVolume", -80.0f);
            }

            if (fadeInOnStart)
            {
                FadeIn();
            }
        }

        /// <summary>
        /// Hide the scene behind a black canvas
        /// </summary>
        private void EnableFaderPanel()
        {
            fadePanel.SetActive(true);
            fadeCanvasGroup.alpha = 1.0f;
        }

        /// <summary>
        /// Show the scene, remove the black canvas
        /// </summary>
        private void DisableFaderPanel()
        {
            fadePanel.SetActive(false);
            fadeCanvasGroup.alpha = 1.0f;
        }

        /// <summary>
        /// Can be called by an event to mute audio, prior to fading.
        /// For example, prior to loading new scenes.
        /// </summary>
        public void MuteAudio()
        {
            audioMixer.SetFloat("MasterVolume", -80.0f);
        }

        public void FadeIn()
        {
            if (!IsReady)
            {
                return;
            }
            _fadeState = FadeState.FadingIn;
            StartCoroutine(FadeASync());
        }
        
        public void FadeOut()
        {
            if (!IsReady)
            {
                return;
            }
            _fadeState = FadeState.FadingOut;
            StartCoroutine(FadeASync());
        }

        internal IEnumerator FadeASync()
        {
            // Call any event listeners
            fadeStartEvent.Invoke();

            // Determine start and end values
            float startAudioVolume;
            float endAudioVolume;
            float startScreenAlpha;
            float endScreenAlpha;

            if (_fadeState == FadeState.FadingIn)
            {
                startAudioVolume = -80.0f;
                endAudioVolume = 0.0f;
                startScreenAlpha = 1.0f;
                endScreenAlpha = 0.0f;
            }
            else
            {
                startAudioVolume = 0f;
                endAudioVolume = -80.0f;
                startScreenAlpha = 0f;
                endScreenAlpha = 1.0f;
            }

            fadeCanvasGroup.gameObject.SetActive(true);
            fadeCanvasGroup.alpha = startScreenAlpha;

            // Wait for the delay period
            float delayBeforeFade = _fadeState == FadeState.FadingIn ? delayBeforeFadeIn : delayBeforeFadeOut;
            float fadeDuration = _fadeState == FadeState.FadingIn ? fadeInDuration : fadeOutDuration;
            yield return new WaitForSecondsRealtime(delayBeforeFade);
            float currTime = 0.0f;
            while (currTime < fadeDuration)
            {
                if (fadeAudio)
                {
                    // Calculate and set Audio mixer volume
                    float newVolume = Mathf.Lerp(startAudioVolume, endAudioVolume, currTime / fadeDuration);
                    audioMixer.SetFloat("MasterVolume", newVolume);
                }

                // Calculate and set Panel alpha
                float alphaValue = Mathf.Lerp(startScreenAlpha, endScreenAlpha, currTime / fadeDuration);
                fadeCanvasGroup.alpha = alphaValue;

                currTime += Time.unscaledDeltaTime;
                yield return null;
            }

            fadeCanvasGroup.alpha = endScreenAlpha;

            // Set final values
            if (fadeAudio)
            {
                audioMixer.SetFloat("MasterVolume", endAudioVolume);
            }

            // Call event listeners
            fadeEndEvent.Invoke();
            _fadeState = FadeState.FadeComplete;
        }
    }
}