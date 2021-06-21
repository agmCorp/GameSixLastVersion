using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    #region Private Constants
    private const string LOG_TAG = "MainMenu";
    private const string BUTTON_CANCEL = "Cancel";
    private const float ANIMATION_DELAY = 0.5f;
    private const string ANIM_PARAM = "Animate";
    private const string BLINK_PARAM = "Blink";
    #endregion

    #region Private Attributes
    [SerializeField] private GameObject _crown = null;
    [SerializeField] private Button _btnGooglePlayServices = null;
    [SerializeField] private Button _btnSettings = null;
    [SerializeField] private Button _btnLevels = null;
    [SerializeField] private Button _btnCredits = null;
    [SerializeField] private Button _btnBackSettings = null;
    [SerializeField] private Button _btnBackLevels = null;
    [SerializeField] private Button _btnBackCredits = null;
    [SerializeField] private Toggle _toggleLowDetail = null;
    [SerializeField] private Toggle _toggleMediumDetail = null;
    [SerializeField] private Toggle _toggleHighDetail = null;
    [SerializeField] private Slider _sliderMusic = null;
    [SerializeField] private Slider _sliderSFX = null;

    [Space(15)]
    [SerializeField] private RectTransform _canvasRect = null;
    [SerializeField] private RectTransform _uiSettingsScreenRect = null;
    [SerializeField] private RectTransform _uiLevelsScreenRect = null;
    [SerializeField] private RectTransform _uiCreditsScreenRect = null;
    [SerializeField] private RectTransform _uiDimScreenRect = null;

    private readonly AudioHelper _audioHelper = AudioHelper.GetInstance();
    private readonly Logging _log = Logging.GetInstance();
    private readonly GPGS _gpgs = GPGS.GetInstance();
    private readonly Interpolation _interpolation = Interpolation.GetInstance();
    private AudioAssetManager _audioAssetManager;
    private AudioMixer _masterMixer;
    private SceneLoader _sceneLoader;
    private AudioSource _sFXUI;
    private int _highLevel;
    private Vector2 _tmp;
    private UIHelper _uIHelper;
    private Animator _mainMenuAnimator;
    private LevelSelector _levelSelector;
    private float _canvasHeight;
    private bool _showLevelsScreen;
    private bool _hideLevelsScreen;
    private bool _showCreditsScreen;
    private bool _hideCreditsScreen;
    private bool _showSettingsScreen;
    private bool _hideSettingsScreen;
    private float _percentageCompleted;
    private float _timeSinceStarted;
    private PageSwiper _pageSwiper;
    private BackToMainScreen _settingsBackToMainMenu;
    private BackToMainScreen _levelsBackToMainMenu;
    private BackToMainScreen _creditsBackToMainMenu;
    private bool _backButtonOnMainScreen;
    private int _toggleDisplay;
    private int _toggleDebugLog;
    private bool _redFriendBlink;
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        _uIHelper = GetComponent<UIHelper>();
        _mainMenuAnimator = GetComponent<Animator>();
        _levelSelector = GetComponent<LevelSelector>();
        _backButtonOnMainScreen = true;
        _settingsBackToMainMenu = _uiSettingsScreenRect.gameObject.GetComponent<BackToMainScreen>();
        _levelsBackToMainMenu = _uiLevelsScreenRect.gameObject.GetComponent<BackToMainScreen>();
        _creditsBackToMainMenu = _uiCreditsScreenRect.gameObject.GetComponent<BackToMainScreen>();

        // Heavy and slow operation
        GameMap.GetInstance().LoadMap();
    }

    private void Start()
    {
        // Audio and Clips
        _audioAssetManager = AudioAssetManager.GetInstance();
        _masterMixer = _audioAssetManager.MasterMixer;

        // Transitions
        _sceneLoader = SceneLoader.GetInstance();
        _sceneLoader.TransitionEnd();

        // Allows this Audio Source to continue to play even when the game is paused
        _sFXUI = gameObject.AddComponent<AudioSource>();
        _sFXUI.ignoreListenerPause = true;
        _sFXUI.priority = 0; // High
        _sFXUI.outputAudioMixerGroup = _audioAssetManager.SFXUIGroup;

        // Player preferences
        InitPlayerPrefs();

        // Disables multitouch to avoid pressing two buttons at the same time
        Input.multiTouchEnabled = false;

        // Settings screen (position and dimension)
        _canvasHeight = _canvasRect.rect.height;
        _uiSettingsScreenRect.anchoredPosition = new Vector2(0, -_canvasHeight);
        _uiSettingsScreenRect.sizeDelta = new Vector2(0, _canvasHeight);

        // Settings screen controls
        InitSettingsScreenControls();

        // This component is only active if the settings screen is *fully* visible
        _settingsBackToMainMenu.enabled = false;

        // Levels screen (position and dimension)
        _uiLevelsScreenRect.anchoredPosition = new Vector2(0, -_canvasHeight);
        _uiLevelsScreenRect.sizeDelta = new Vector2(0, _canvasHeight);

        // Component _pageSwiper is available after Build(...)
        int levelCount = GameMap.GetInstance().GetLevelCount();
        _levelSelector.Build(levelCount);
        _pageSwiper = _uiLevelsScreenRect.gameObject.GetComponentInChildren<PageSwiper>();

        // This component is only active if the levels screen is *fully* visible
        _levelsBackToMainMenu.enabled = false;

        // Wink-Wink
        if (_highLevel == levelCount)
        {
            _crown.SetActive(true);
        }

        // Credtis screen (position and dimension)
        _uiCreditsScreenRect.anchoredPosition = new Vector2(0, -_canvasHeight);
        _uiCreditsScreenRect.sizeDelta = new Vector2(0, _canvasHeight);

        // This component is only active if the credits screen is *fully* visible
        _creditsBackToMainMenu.enabled = false;

        // Dim screen (position and dimension)
        _uiDimScreenRect.anchoredPosition = new Vector2(0, -_canvasHeight);
        _uiDimScreenRect.sizeDelta = new Vector2(0, _canvasHeight);

        // Audio
        _audioHelper.SetAudioVolume(_masterMixer, AudioAssetManager.MASTER_MIXER_MUSIC_EXP_PARAM, _sliderMusic.value);
        _audioHelper.SetAudioVolume(_masterMixer, AudioAssetManager.MASTER_MIXER_SFX_EXP_PARAM, _sliderSFX.value);
        _audioHelper.SetAudioVolume(_masterMixer, AudioAssetManager.MASTER_MIXER_SFXUI_EXP_PARAM, _sliderSFX.value);

        // Google Play Game Services
        _gpgs.AuthenticateOnStart(CallBackAuthenticateOnStart);
    }

    private void Update()
    {
        if (Input.GetButtonDown(BUTTON_CANCEL))
        {
            if (_backButtonOnMainScreen)
            {
                _uIHelper.ExecuteActionAfterClickAudio(_sFXUI, QuitAction);
            }
        }

        UpdateSettingsScreen();
        UpdateLevelsScreen();
        UpdateCreditsScreen();
    }

    private void LateUpdate()
    {
        _mainMenuAnimator.SetBool(ANIM_PARAM, _sceneLoader.IsNewSceneVisible);
        _mainMenuAnimator.SetBool(BLINK_PARAM, _redFriendBlink);
    }
    #endregion

    #region Utils
    private void InitPlayerPrefs()
    {
        // Initializes Highlevel preference
        _highLevel = PlayerPrefs.GetInt(GlobalConstants.PREF_HIGH_LEVEL, 1);
        PlayerPrefs.SetInt(GlobalConstants.PREF_HIGH_LEVEL, _highLevel);

        // Initializes preferences used by Settings screen
        int checkPref = PlayerPrefs.GetInt(GlobalConstants.PREF_LOW_DETAIL_ON, -1);
        if (checkPref == -1)
        {
            // Preferences don't exist
            PlayerPrefs.SetInt(GlobalConstants.PREF_LOW_DETAIL_ON, 0);
            PlayerPrefs.SetInt(GlobalConstants.PREF_MEDIUM_DETAIL_ON, 1);
            PlayerPrefs.SetInt(GlobalConstants.PREF_HIGH_DETAIL_ON, 0);
            PlayerPrefs.SetInt(GlobalConstants.PREF_MUSIC_ON, 1);
            PlayerPrefs.SetInt(GlobalConstants.PREF_SFX_ON, 1);
            PlayerPrefs.SetFloat(GlobalConstants.PREF_SLIDER_MUSIC_VOLUME, AudioHelper.SLIDER_VOLUME_MAX_VALUE);
            PlayerPrefs.SetFloat(GlobalConstants.PREF_SLIDER_SFX_VOLUME, AudioHelper.SLIDER_VOLUME_MAX_VALUE);
        }

        // Music off (ensures consistency)
        if (PlayerPrefs.GetInt(GlobalConstants.PREF_MUSIC_ON) == 0)
        {
            PlayerPrefs.SetFloat(GlobalConstants.PREF_SLIDER_MUSIC_VOLUME, AudioHelper.SLIDER_VOLUME_MIN_VALUE);
        }

        // SFX off (ensures consistency)
        if (PlayerPrefs.GetInt(GlobalConstants.PREF_SFX_ON) == 0)
        {
            PlayerPrefs.SetFloat(GlobalConstants.PREF_SLIDER_SFX_VOLUME, AudioHelper.SLIDER_VOLUME_MIN_VALUE);
        }
    }

    private void InitSettingsScreenControls()
    {
        _toggleLowDetail.SetIsOnWithoutNotify(PlayerPrefs.GetInt(GlobalConstants.PREF_LOW_DETAIL_ON) != 0);
        _toggleMediumDetail.SetIsOnWithoutNotify(PlayerPrefs.GetInt(GlobalConstants.PREF_MEDIUM_DETAIL_ON) != 0);
        _toggleHighDetail.SetIsOnWithoutNotify(PlayerPrefs.GetInt(GlobalConstants.PREF_HIGH_DETAIL_ON) != 0);
        _sliderMusic.SetValueWithoutNotify(PlayerPrefs.GetFloat(GlobalConstants.PREF_SLIDER_MUSIC_VOLUME));
        _sliderSFX.SetValueWithoutNotify(PlayerPrefs.GetFloat(GlobalConstants.PREF_SLIDER_SFX_VOLUME));
    }

    private void UpdateSettingsScreen()
    {
        ShowSettingsScreen();
        HideSettingsScreen();

        // Settings back button is only active if the settings screen is *fully* visible
        _btnBackSettings.interactable = _settingsBackToMainMenu.enabled;
    }

    private void ShowSettingsScreen()
    {
        if (_showSettingsScreen)
        {
            _timeSinceStarted += Time.unscaledDeltaTime;
            _percentageCompleted = _timeSinceStarted / ANIMATION_DELAY;
            _tmp.x = _uiSettingsScreenRect.anchoredPosition.x;
            _tmp.y = _interpolation.BounceOut(-_canvasHeight, 0, _percentageCompleted);
            _uiSettingsScreenRect.anchoredPosition = _tmp;
            if (_percentageCompleted >= 1)
            {
                _settingsBackToMainMenu.enabled = true;
                _showSettingsScreen = false;
            }
        }
    }

    private void HideSettingsScreen()
    {
        if (_hideSettingsScreen)
        {
            _timeSinceStarted += Time.unscaledDeltaTime;
            _percentageCompleted = _timeSinceStarted / ANIMATION_DELAY;
            _tmp.x = _uiSettingsScreenRect.anchoredPosition.x;
            _tmp.y = _interpolation.BackIn(0, -_canvasHeight, _percentageCompleted);
            _uiSettingsScreenRect.anchoredPosition = _tmp;
            if (_percentageCompleted >= 1)
            {
                _backButtonOnMainScreen = true;
                _hideSettingsScreen = false;
                _btnSettings.interactable = true;
            }
        }
    }

    private void UpdateLevelsScreen()
    {
        ShowLevelsScreen();
        HideLevelsScreen();

        // Levels back button is only active if the levels screen is *fully* visible and the panel with the levels isn't moving
        _btnBackLevels.interactable = _levelsBackToMainMenu.enabled && !_pageSwiper.IsMoving;
    }

    private void ShowLevelsScreen()
    {
        if (_showLevelsScreen)
        {
            _timeSinceStarted += Time.unscaledDeltaTime;
            _percentageCompleted = _timeSinceStarted / ANIMATION_DELAY;
            _tmp.x = _uiLevelsScreenRect.anchoredPosition.x;
            _tmp.y = _interpolation.BounceOut(-_canvasHeight, 0, _percentageCompleted);
            _uiLevelsScreenRect.anchoredPosition = _tmp;

            if (_percentageCompleted >= 1)
            {
                _levelsBackToMainMenu.enabled = true;
                _showLevelsScreen = false;
                _pageSwiper.PanelLocation = _pageSwiper.gameObject.transform.position;
                _pageSwiper.EnableDrag = true;
            }
        }
    }

    private void HideLevelsScreen()
    {
        if (_hideLevelsScreen)
        {
            _timeSinceStarted += Time.unscaledDeltaTime;
            _percentageCompleted = _timeSinceStarted / ANIMATION_DELAY;
            _tmp.x = _uiLevelsScreenRect.anchoredPosition.x;
            _tmp.y = _interpolation.BackIn(0, -_canvasHeight, _percentageCompleted);
            _uiLevelsScreenRect.anchoredPosition = _tmp;
            if (_percentageCompleted >= 1)
            {
                _backButtonOnMainScreen = true;
                _hideLevelsScreen = false;
                _btnLevels.interactable = true;
            }
        }
    }

    private void UpdateCreditsScreen()
    {
        ShowCreditsScreen();
        HideCreditsScreen();

        // Credits back button is only active if the credits screen is *fully* visible
        _btnBackCredits.interactable = _creditsBackToMainMenu.enabled;
    }

    private void ShowCreditsScreen()
    {
        if (_showCreditsScreen)
        {
            _timeSinceStarted += Time.unscaledDeltaTime;
            _percentageCompleted = _timeSinceStarted / ANIMATION_DELAY;
            _tmp.x = _uiCreditsScreenRect.anchoredPosition.x;
            _tmp.y = _interpolation.BounceOut(-_canvasHeight, 0, _percentageCompleted);
            _uiCreditsScreenRect.anchoredPosition = _tmp;
            if (_percentageCompleted >= 1)
            {
                _creditsBackToMainMenu.enabled = true;
                _showCreditsScreen = false;
            }
        }
    }

    private void HideCreditsScreen()
    {
        if (_hideCreditsScreen)
        {
            _timeSinceStarted += Time.unscaledDeltaTime;
            _percentageCompleted = _timeSinceStarted / ANIMATION_DELAY;
            _tmp.x = _uiCreditsScreenRect.anchoredPosition.x;
            _tmp.y = _interpolation.BackIn(0, -_canvasHeight, _percentageCompleted);
            _uiCreditsScreenRect.anchoredPosition = _tmp;
            if (_percentageCompleted >= 1)
            {
                _backButtonOnMainScreen = true;
                _hideCreditsScreen = false;
                _btnCredits.interactable = true;
            }
        }
    }

    private void ShowDimScreen()
    {
        _tmp.x = _uiDimScreenRect.anchoredPosition.x;
        _tmp.y = 0;
        _uiDimScreenRect.anchoredPosition = _tmp;
    }

    private void HideDimScreen()
    {
        _tmp.x = _uiDimScreenRect.anchoredPosition.x;
        _tmp.y = -_canvasHeight;
        _uiDimScreenRect.anchoredPosition = _tmp;
    }

    private void CallBackAuthenticateOnStart(bool success)
    {
        _log.DebugLog(LOG_TAG, "CallBackAuthenticateOnStart success: " + success, gameObject);
        if (success)
        {
            // Synchronizes score
            _gpgs.PostHighLevel(_highLevel);
        }
    }

    private void CallBackAuthenticateByButton(bool success)
    {
        _log.DebugLog(LOG_TAG, "CallBackAuthenticateByButton success: " + success, gameObject);
        if (success)
        {
            // Synchronizes score
            _gpgs.PostHighLevel(_highLevel);

            // Leaderboard
            _gpgs.ShowLeaderboard();
        }

        HideDimScreen();
        _btnGooglePlayServices.interactable = true;
    }

    private void GooglePlayServicesAction()
    {
        if (!_gpgs.IsAuthenticated())
        {
            // The user mustn't interact with the application (other buttons) during this operation
            ShowDimScreen();
            _gpgs.AuthenticateByButton(CallBackAuthenticateByButton);
        }
        else
        {
            CallBackAuthenticateByButton(true);
        }
    }

    private void SettingsAction()
    {
        _backButtonOnMainScreen = false;
        _showSettingsScreen = true;
        _hideSettingsScreen = false;
        _timeSinceStarted = 0;
        _uIHelper.PlayPanelIn(_sFXUI);
    }

    private void BackSettingsAction()
    {
        _showSettingsScreen = false;
        _hideSettingsScreen = true;
        _timeSinceStarted = 0;
        _uIHelper.PlayPanelOut(_sFXUI);
    }

    private void LevelsAction()
    {
        // Start from high level page
        _pageSwiper.PanelLocation = _pageSwiper.gameObject.transform.position;
        int page = Mathf.CeilToInt((float)_highLevel / _levelSelector.AmountPerPage);
        _pageSwiper.GoToPage(page);

        _pageSwiper.EnableDrag = false;
        _backButtonOnMainScreen = false;
        _showLevelsScreen = true;
        _hideLevelsScreen = false;
        _timeSinceStarted = 0;
        _uIHelper.PlayPanelIn(_sFXUI);
    }

    private void BackLevelsAction()
    {
        _pageSwiper.EnableDrag = false;
        _showLevelsScreen = false;
        _hideLevelsScreen = true;
        _timeSinceStarted = 0;
        _uIHelper.PlayPanelOut(_sFXUI);
    }

    private void RateAction()
    {
        Application.OpenURL(GlobalConstants.URL_FLUBBIE);
    }

    private void CreditsAction()
    {
        _backButtonOnMainScreen = false;
        _showCreditsScreen = true;
        _hideCreditsScreen = false;
        _timeSinceStarted = 0;
        _uIHelper.PlayPanelIn(_sFXUI);
    }

    private void BackCreditsAction()
    {
        _showCreditsScreen = false;
        _hideCreditsScreen = true;
        _timeSinceStarted = 0;
        _uIHelper.PlayPanelOut(_sFXUI);
    }

    private void DownloadAction()
    {
        Application.OpenURL(GlobalConstants.URL_GAMES);
    }

    private void QuitAction()
    {
        _gpgs.SignOut();
        AdMob.GetInstance().Dispose();
        Application.Quit();
    }
    #endregion

    #region API
    public void Play()
    {
        Play(_highLevel);
    }

    public void Play(int fromLevel)
    {
        _uIHelper.GetButtonClicked().interactable = false;
        _uIHelper.PlayClickAudio(_sFXUI);
        PlayerPrefs.SetInt(GlobalConstants.PREF_FROM_LEVEL, fromLevel);
        _redFriendBlink = true;
        _sceneLoader.TransitionStart(GlobalConstants.CURRENT_TRANSITION, SceneManager.GetActiveScene().buildIndex + 1);
    }

    public void GooglePlayServices()
    {
#if UNITY_ANDROID
        _btnGooglePlayServices.interactable = false;
        _uIHelper.ExecuteActionAfterClickAudio(_sFXUI, GooglePlayServicesAction);
#endif
    }

    public void Settings()
    {
        _btnSettings.interactable = false;
        _uIHelper.ExecuteActionAfterClickAudio(_sFXUI, SettingsAction);
    }

    public void BackSettings()
    {
        _settingsBackToMainMenu.enabled = false;
        _uIHelper.ExecuteActionAfterClickAudio(_sFXUI, BackSettingsAction);
    }

    public void Levels()
    {
        _btnLevels.interactable = false;
        _uIHelper.ExecuteActionAfterClickAudio(_sFXUI, LevelsAction);
    }

    public void BackLevels()
    {
        _levelsBackToMainMenu.enabled = false;
        _uIHelper.ExecuteActionAfterClickAudio(_sFXUI, BackLevelsAction);
    }

    public void PlaySlidePanelSound()
    {
        _uIHelper.PlayPanelIn(_sFXUI);
    }

    public void Rate()
    {
        _uIHelper.ExecuteActionAfterClickAudio(_sFXUI, RateAction);
    }

    public void Credits()
    {
        _btnCredits.interactable = false;
        _uIHelper.ExecuteActionAfterClickAudio(_sFXUI, CreditsAction);
    }

    public void BackCredits()
    {
        _creditsBackToMainMenu.enabled = false;
        _uIHelper.ExecuteActionAfterClickAudio(_sFXUI, BackCreditsAction);
    }

    public void Download()
    {
        _uIHelper.ExecuteActionAfterClickAudio(_sFXUI, DownloadAction);
    }

    public void SetMusicVolume(float sliderMusicValue)
    {
        _audioHelper.SetAudioVolume(_masterMixer, AudioAssetManager.MASTER_MIXER_MUSIC_EXP_PARAM, sliderMusicValue);

        PlayerPrefs.SetInt(GlobalConstants.PREF_MUSIC_ON, (sliderMusicValue != AudioHelper.SLIDER_VOLUME_MIN_VALUE) ? 1 : 0);
        PlayerPrefs.SetFloat(GlobalConstants.PREF_SLIDER_MUSIC_VOLUME, sliderMusicValue);
    }

    public void SetSFXVolume(float sliderSFXValue)
    {
        _audioHelper.SetAudioVolume(_masterMixer, AudioAssetManager.MASTER_MIXER_SFX_EXP_PARAM, sliderSFXValue);
        _audioHelper.SetAudioVolume(_masterMixer, AudioAssetManager.MASTER_MIXER_SFXUI_EXP_PARAM, sliderSFXValue);

        PlayerPrefs.SetInt(GlobalConstants.PREF_SFX_ON, (sliderSFXValue != AudioHelper.SLIDER_VOLUME_MIN_VALUE) ? 1 : 0);
        PlayerPrefs.SetFloat(GlobalConstants.PREF_SLIDER_SFX_VOLUME, sliderSFXValue);
    }

    public void SFXEndDrag()
    {
        _sFXUI.PlayOneShot(_audioAssetManager.GetRandomSFXClip());
    }

    public void ToggleLowDetail(bool checkedValue)
    {
        _uIHelper.PlayClickAudio(_sFXUI);
        PlayerPrefs.SetInt(GlobalConstants.PREF_LOW_DETAIL_ON, checkedValue ? 1 : 0);
    }

    public void ToggleMediumDetail(bool checkedValue)
    {
        _uIHelper.PlayClickAudio(_sFXUI);
        PlayerPrefs.SetInt(GlobalConstants.PREF_MEDIUM_DETAIL_ON, checkedValue ? 1 : 0);
    }

    public void ToggleHighDetail(bool checkedValue)
    {
        _uIHelper.PlayClickAudio(_sFXUI);
        PlayerPrefs.SetInt(GlobalConstants.PREF_HIGH_DETAIL_ON, checkedValue ? 1 : 0);
    }

    public void ToggleDisplay()
    {
        _toggleDisplay++;
        if (_toggleDisplay >= GlobalConstants.CLICKS_CHEAT_TOGGLE)
        {
            _toggleDisplay = 0;
            _sceneLoader.ToggleDisplay();
            _log.DebugLog(LOG_TAG, "Toggle display", gameObject);
        }
    }

    public void ToggleDebugLog()
    {
        _toggleDebugLog++;
        if (_toggleDebugLog >= GlobalConstants.CLICKS_CHEAT_TOGGLE)
        {
            _toggleDebugLog = 0;
            _log.EnableDebugLog = !_log.EnableDebugLog;
            _log.DebugLog(LOG_TAG, "Toggle debug log", gameObject);
        }
    }
    #endregion
}
