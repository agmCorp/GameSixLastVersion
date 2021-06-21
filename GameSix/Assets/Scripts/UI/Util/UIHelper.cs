using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIHelper : MonoBehaviour
{
    #region Private Constants
    private const float SELECTABLE_REACTIVATE_DELAY = 1.0f;
    #endregion

    #region Private Constants
    private const string LOG_TAG = "UIHelper";
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();

    private AudioAssetManager _audioAssetManager;
    private float _clickLength;
    #endregion

    #region MonoBehaviour
    private void Start()
    {
        // Audio and Clips
        _audioAssetManager = AudioAssetManager.GetInstance();

        // Button sound duration
        _clickLength = _audioAssetManager.GetClip(AudioAssetManager.CLICK_SFXUI).length;
    }
    #endregion

    #region Utils
    private IEnumerator EnableSelectableAfterDelay(Selectable selectable, float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        selectable.interactable = true;
    }

    private IEnumerator ExecuteActionAfterDelay(Action action, float seconds)
    {
        yield return new WaitForSecondsRealtime(seconds);
        action.Invoke();
    }
    #endregion

    #region API
    public void AvoidDoubleClick(Selectable selectable)
    {
        selectable.interactable = false;
        StartCoroutine(EnableSelectableAfterDelay(selectable, SELECTABLE_REACTIVATE_DELAY));
    }

    public void ExecuteActionAfterClickAudio(AudioSource sFXUI, Action action)
    {
        PlayClickAudio(sFXUI);
        StartCoroutine(ExecuteActionAfterDelay(action, _clickLength));
    }

    public void PlayClickAudio(AudioSource sFXUI)
    {
        sFXUI.PlayOneShot(_audioAssetManager.GetClip(AudioAssetManager.CLICK_SFXUI));
    }

    public void PlayPanelIn(AudioSource sFXUI)
    {
        sFXUI.PlayOneShot(_audioAssetManager.GetClip(AudioAssetManager.PANEL_IN_SFXUI));
    }

    public void PlayPanelOut(AudioSource sFXUI)
    {
        sFXUI.PlayOneShot(_audioAssetManager.GetClip(AudioAssetManager.PANEL_OUT_SFXUI));
    }

    public Button GetButtonClicked()
    {
        return EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
    }
    #endregion
}
