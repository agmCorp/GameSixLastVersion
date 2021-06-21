using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using System;

public class SceneLoader : MonoBehaviour
{
    #region Public Enum
    public enum Transition
    {
        NONE,
        CROSSFADE,
        CIRCLE_WIPE,
        FLUBBIE_WIPE
    }
    #endregion

    #region Private Constants
    private const string LOG_TAG = "SceneLoader";
    private const string START_TRANSITION_ANIM_PARAM = "Start";
    private const string END_TRANSITION_ANIM_PARAM = "End";
    private const float UNITY_MAX_PROGRESS = 0.9f;
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();
    private readonly AudioHelper _audioHelper = AudioHelper.GetInstance();

    // Singleton MonoBehaviour
    private static SceneLoader _instance;

    [SerializeField] private Animator _crossfadeAnimator = null;
    [SerializeField] private Animator _circleWipeAnimator = null;
    [SerializeField] private Animator _flubbieWipeAnimator = null;
    [SerializeField] private TextMeshProUGUI _crossfadeProgress = null;
    [SerializeField] private TextMeshProUGUI _circleWipeProgress = null;
    [SerializeField] private TextMeshProUGUI _flubbieWipeProgress = null;
    [SerializeField] private GameObject _display = null;

    private AudioAssetManager _audioAssetManager;
    private AudioMixer _masterMixer;
    private AudioSource _sFXUI;
    private bool _startTransition;
    private bool _endTransition;
    private bool _newSceneVisible;
    private Transition _currentTransition;
    private int _sceneIndex;
    private Action _callbackBeforeLoad;
    #endregion

    #region Properties
    public bool IsNewSceneVisible
    {
        get { return _newSceneVisible; }
    }
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        // Singleton MonoBehaviour
        if (_instance == null)
        {
            DontDestroyOnLoad(gameObject);
            _instance = this;
            AwakeSingletonMonoBehaviour();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void AwakeSingletonMonoBehaviour()
    {
        _currentTransition = Transition.NONE;
        _sFXUI = GetComponent<AudioSource>();
        _newSceneVisible = true;
    }

    private void Start()
    {
        // Audio and Clips
        _audioAssetManager = AudioAssetManager.GetInstance();
        _masterMixer = _audioAssetManager.MasterMixer;
    }

    private void LateUpdate()
    {
        switch (_currentTransition)
        {
            case Transition.CROSSFADE:
                _crossfadeAnimator.SetBool(START_TRANSITION_ANIM_PARAM, _startTransition);
                _crossfadeAnimator.SetBool(END_TRANSITION_ANIM_PARAM, _endTransition);
                break;
            case Transition.CIRCLE_WIPE:
                _circleWipeAnimator.SetBool(START_TRANSITION_ANIM_PARAM, _startTransition);
                _circleWipeAnimator.SetBool(END_TRANSITION_ANIM_PARAM, _endTransition);
                break;
            case Transition.FLUBBIE_WIPE:
                _flubbieWipeAnimator.SetBool(START_TRANSITION_ANIM_PARAM, _startTransition);
                _flubbieWipeAnimator.SetBool(END_TRANSITION_ANIM_PARAM, _endTransition);
                break;
            default:
                break;
        }
    }
    #endregion

    #region Utils
    private IEnumerator LoadSceneAsync()
    {
        float progress;
        string progressText;

        // Wait till next frame
        yield return null;

        // Begin to load the Scene
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(_sceneIndex);

        // When the load is still in progress, output the progress
        while (!asyncOperation.isDone)
        {
            progress = Mathf.Clamp01(asyncOperation.progress / UNITY_MAX_PROGRESS);
            progressText = "Loading progress" +
                           System.Environment.NewLine +
                           Mathf.RoundToInt(progress * 100.0f) + "%";

            switch (_currentTransition)
            {
                case Transition.CROSSFADE:
                    _crossfadeProgress.text = progressText;
                    break;
                case Transition.CIRCLE_WIPE:
                    _circleWipeProgress.text = progressText;
                    break;
                case Transition.FLUBBIE_WIPE:
                    _flubbieWipeProgress.text = progressText;
                    break;
                default:
                    break;
            }

            yield return null;
        }
        StartCoroutine(_audioHelper.StartAudioFade(_masterMixer,
                                                   AudioAssetManager.MASTER_MIXER_CROSSFADE_EXP_PARAM,
                                                   AudioHelper.AUDIO_FADE_DELAY, 1));
    }
    #endregion

    #region API
    // Singleton MonoBehaviour: retrieve instance
    public static SceneLoader GetInstance()
    {
        return _instance;
    }

    public void TransitionStart(Transition transition, int sceneIndex, Action callbackBeforeLoad = null)
    {
        StartCoroutine(_audioHelper.StartAudioFade(_masterMixer,
                                                   AudioAssetManager.MASTER_MIXER_CROSSFADE_EXP_PARAM,
                                                   AudioHelper.AUDIO_FADE_DELAY, 0));
        _startTransition = true;
        _endTransition = false;
        _currentTransition = transition;
        _sceneIndex = sceneIndex;
        _callbackBeforeLoad = callbackBeforeLoad;
    }

    public void TransitionEnd(Transition transition)
    {
        if (_currentTransition == Transition.NONE)
        {
            _currentTransition = transition;
        }
        TransitionEnd();
    }

    public void TransitionEnd()
    {
        _startTransition = false;
        _endTransition = true;
    }

    public void LoadScene()
    {
        _newSceneVisible = false;
        _callbackBeforeLoad?.Invoke();
        StartCoroutine(LoadSceneAsync());
    }

    public void FinalizeTransition()
    {
        _crossfadeProgress.text = "";
        _circleWipeProgress.text = "";
        _flubbieWipeProgress.text = "";
        _newSceneVisible = true;
    }

    public void PlayOpenCandyAudio()
    {
        _sFXUI.PlayOneShot(_audioAssetManager.GetClip(AudioAssetManager.OPEN_CANDY_SFXUI));
    }

    public void PlayCloseCandyAudio()
    {
        _sFXUI.PlayOneShot(_audioAssetManager.GetClip(AudioAssetManager.CLOSE_CANDY_SFXUI));
    }

    public void PlayGateAudio()
    {
        _sFXUI.PlayOneShot(_audioAssetManager.GetClip(AudioAssetManager.GATE_SFXUI));
    }

    public void ToggleDisplay()
    {
        _display.SetActive(!_display.activeSelf);
    }
    #endregion
}
