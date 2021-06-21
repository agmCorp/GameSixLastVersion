using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Audio;

public class AudioHelper
{
    #region Public Constants
    public const float AUDIO_FADE_DELAY = 1.5f;
    public const float SLIDER_VOLUME_MIN_VALUE = 0.0001f;
    public const float SLIDER_VOLUME_MAX_VALUE = 1.0f;
    #endregion

    #region Private Constants
    private const string LOG_TAG = "AudioFade";
    private const float AUDIO_MIXER_MAX_DB = 20.0f;
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();

    // Singleton: unique instance
    private static AudioHelper _instance;
    #endregion

    #region Utils
    // Singleton: prevent instantiation from other classes
    private AudioHelper()
    {
    }
    #endregion

    #region API
    // Singleton: retrieve instance
    public static AudioHelper GetInstance()
    {
        if (_instance == null)
        {
            _instance = new AudioHelper();
        }
        return _instance;
    }

    // Volume (sliderFromVolume and sliderToVolume) range [SLIDER_VOLUME_MIN_VALUE, SLIDER_VOLUME_MAX_VALUE]
    public IEnumerator StartAudioFade(AudioMixer audioMixer, string exposedParam, float duration, float sliderFromVolume, float sliderToVolume, Action callback = null)
    {
        // Check boundaries
        float sliderFromVol = Mathf.Clamp(sliderFromVolume, SLIDER_VOLUME_MIN_VALUE, SLIDER_VOLUME_MAX_VALUE);
        float sliderToVol = Mathf.Clamp(sliderToVolume, SLIDER_VOLUME_MIN_VALUE, SLIDER_VOLUME_MAX_VALUE);
        
        float currentTime = 0;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float newSliderVol = Mathf.Lerp(sliderFromVol, sliderToVol, currentTime / duration);
            audioMixer.SetFloat(exposedParam, Mathf.Log10(newSliderVol) * AUDIO_MIXER_MAX_DB);
            yield return null;
        }
        callback?.Invoke();
    }

    // SliderTargetVolume range [SLIDER_VOLUME_MIN_VALUE, SLIDER_VOLUME_MAX_VALUE]
    public IEnumerator StartAudioFade(AudioMixer audioMixer, string exposedParam, float duration, float sliderTargetVolume, Action callback = null)
    {
        // Check boundaries
        float sliderTargetValue = Mathf.Clamp(sliderTargetVolume, SLIDER_VOLUME_MIN_VALUE, SLIDER_VOLUME_MAX_VALUE);

        float sliderCurrentVol;
        audioMixer.GetFloat(exposedParam, out sliderCurrentVol);
        sliderCurrentVol = Mathf.Pow(10, sliderCurrentVol / AUDIO_MIXER_MAX_DB);

        float currentTime = 0;
        while (currentTime < duration)
        {
            currentTime += Time.deltaTime;
            float newSliderVol = Mathf.Lerp(sliderCurrentVol, sliderTargetValue, currentTime / duration);
            audioMixer.SetFloat(exposedParam, Mathf.Log10(newSliderVol) * AUDIO_MIXER_MAX_DB);
            yield return null;
        }
        callback?.Invoke();
    }

    // SliderVolume range [SLIDER_VOLUME_MIN_VALUE, SLIDER_VOLUME_MAX_VALUE]
    public void SetAudioVolume(AudioMixer audioMixer, string exposedParam, float sliderVolume)
    {
        // Check boundaries
        float sliderTargetValue = Mathf.Clamp(sliderVolume, SLIDER_VOLUME_MIN_VALUE, SLIDER_VOLUME_MAX_VALUE);

        audioMixer.SetFloat(exposedParam, Mathf.Log10(sliderTargetValue) * AUDIO_MIXER_MAX_DB);
    }
    #endregion
}