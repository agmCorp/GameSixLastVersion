using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;

public class Hud : MonoBehaviour
{
    #region Public Constants
    public const string TAG_HUD = "Hud";
    public const string PREFIX_CROSS = "x";
    public const string PREFIX_PLUS = "+";
    #endregion

    #region Private Constants
    private const string LOG_TAG = "Hud";
    private const string BOUNCE_ANIM_PARAM = "Bounce";
    private const string SHOW_OFF_ANIM_PARAM = "ShowOff";
    private const float ANIMATION_DELAY = 0.5f;
    private const int AMOUNT_POWER_UP = 300;
    private const float POWER_UP_POSX = -95.0f;
    private const float LEVEL_ANIMATION_TIME = 10.0f;
    private const float CANDY_ANIMATION_TIME = 3.0f;
    private const string TYPE_ANIM_PARAM = "Type";
    private const int MIN_TYPE_ANIM = 1;
    private const int MAX_TYPE_ANIM = 4;
    private const int MAX_ATTEMPTS = 7;
    #endregion

    #region Private Attributes
    [SerializeField] private RectTransform _canvasRect = null;

    [Space(15)]
    [SerializeField] private Animator _uiLevelAnimator = null;
    [SerializeField] private Animator _levelAnimator = null;
    [SerializeField] private TextMeshProUGUI _levelCounter = null;

    [Space(15)]
    [SerializeField] private Animator _uiCandyAnimator = null;
    [SerializeField] private Animator _candyAnimator = null;
    [SerializeField] private Animator _tastyAnimator = null;
    [SerializeField] private TextMeshProUGUI _candyCounter = null;

    [Space(15)]
    [SerializeField] private Animator _uiPowerUpAnimator = null;
    [SerializeField] private RectTransform _uiPowerUp = null;

    [Space(15)]
    [SerializeField] private GameObject _uiPowerUpOrange = null;
    [SerializeField] private Animator _chiAnimator = null;

    [Space(15)]
    [SerializeField] private GameObject _uiPowerUpRed = null;
    [SerializeField] private Animator _shieldAnimator = null;
    [SerializeField] private TextMeshProUGUI _redCounter = null;

    [Space(15)]
    [SerializeField] private GameObject _uiPowerUpGreen = null;
    [SerializeField] private Animator _clockAnimator = null;
    [SerializeField] private TextMeshProUGUI _greenCounter = null;

    [Space(15)]
    [SerializeField] private GameObject _uiPowerUpYellow = null;
    [SerializeField] private Animator _potionAnimator = null;
    [SerializeField] private TextMeshProUGUI _yellowCounter = null;

    [Space(15)]
    [SerializeField] private GameObject _uiPowerUpPink = null;
    [SerializeField] private Animator _bonBonAnimator = null;
    [SerializeField] private TextMeshProUGUI _pinkCounter = null;

    [Space(15)]
    [SerializeField] private GameObject _uiPowerUpBlue = null;
    [SerializeField] private Animator _bombAnimator = null;
    [SerializeField] private TextMeshProUGUI _blueCounter = null;

    [Space(15)]
    [SerializeField] private Button _btnPause = null;
    [SerializeField] private RectTransform _uiPauseScreenRect = null;
    [SerializeField] private CanvasGroup _pauseHolder = null;
    [SerializeField] private Toggle _toggleMusic = null;
    [SerializeField] private Toggle _toggleSFX = null;

    [Space(15)]
    [SerializeField] private RectTransform _uiFailScreenRect = null;
    [SerializeField] private Animator _playerAnimator = null;
    [SerializeField] private CanvasGroup _failHolder = null;

    [Space(15)]
    [SerializeField] private RectTransform _uiExitScreenRect = null;
    [SerializeField] private CanvasGroup _exitHolder = null;

    [Space(15)]
    [SerializeField] private Button _btnStore = null;
    [SerializeField] private RectTransform _uiStoreScreenRect = null;
    [SerializeField] private CanvasGroup _storeHolder = null;
    [SerializeField] private GameObject _cheatButtons = null;

    [Space(15)]
    [SerializeField] private GameObject _uiFingerTap = null;

    private readonly AudioHelper _audioHelper = AudioHelper.GetInstance();
    private readonly Logging _log = Logging.GetInstance();
    private readonly Interpolation _interpolation = Interpolation.GetInstance();
    private AudioAssetManager _audioAssetManager;
    private AudioMixer _masterMixer;
    private AudioSource _music;
    private SceneLoader _sceneLoader;
    private AudioSource _sFXUI;
    private bool _turnOnMusic;
    private bool _turnOnSFX;
    private float _sliderMusicVolume;
    private float _sliderSFXVolume;
    private float _canvasWidth;
    private float _canvasHeight;
    private Vector2 _tmp;
    private UIHelper _uiHelper;
    private GameManager _gameManager;
    private ChallengeManager _challengeManager;
    private Animator _uiStoreAnimator;
    private bool _uiStoreBounce;
    private float _levelAnimationTime;
    private float _candyAnimationTime;
    private bool _uiLevelBounce;
    private bool _levelBounce;
    private bool _uiCandyBounce;
    private bool _uiCandyShowOff;
    private bool _uiPowerUpBounce;
    private bool _showPowerUp;
    private bool _hidePowerUp;
    private bool _showPauseScreen;
    private bool _hidePauseScreen;
    private bool _pauseScreenActive;
    private bool _pauseAnimFinish;
    private bool _audioFadeFinish;
    private bool _showFailScreen;
    private bool _hideFailScreen;
    private bool _failScreenActive;
    private bool _playerBounce;
    private bool _showExitScreen;
    private bool _hideExitScreen;
    private bool _exitScreenActive;
    private bool _showStoreScreen;
    private bool _hideStoreScreen;
    private bool _storeScreenActive;
    private bool _forceGameOver;
    private bool _grandFinale;
    private float _percentageCompleted;
    private float _timeSinceStarted;
    private bool _interstitialAdWasDisplayed;
    private bool _applyPower;
    private string _powerKey;
    private int _powerParam;
    private int _toggleShopButtons;
    private int _score;     // Owns the score
    #endregion

    #region Properties
    public int Score
    {
        get { return _score; }
    }
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        _music = _canvasRect.gameObject.transform.parent.gameObject.GetComponent<AudioSource>();
        _uiHelper = GetComponent<UIHelper>();
        _gameManager = GameObject.FindGameObjectWithTag(GameManager.TAG_GAME_MANAGER).GetComponent<GameManager>();
        _challengeManager = GameObject.FindGameObjectWithTag(ChallengeManager.TAG_CHALLENGE_MANAGER).GetComponent<ChallengeManager>();
        _uiStoreAnimator = _btnStore.gameObject.transform.parent.gameObject.GetComponent<Animator>();
        _levelAnimationTime = LEVEL_ANIMATION_TIME;
        _candyAnimationTime = CANDY_ANIMATION_TIME;
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

        // Retrives score
        _score = PlayerPrefs.GetInt(GlobalConstants.PREF_SCORE, 0);
        _candyCounter.text = _score.ToString();

        // Pause screen (position and dimension)
        _canvasWidth = _canvasRect.rect.width;
        _canvasHeight = _canvasRect.rect.height;
        _uiPauseScreenRect.anchoredPosition = new Vector2(_uiPauseScreenRect.anchoredPosition.x, _canvasHeight);
        _uiPauseScreenRect.sizeDelta = new Vector2(_canvasWidth, _canvasHeight);

        // Music and SFX
        _turnOnMusic = PlayerPrefs.GetInt(GlobalConstants.PREF_MUSIC_ON) != 0;
        _turnOnSFX = PlayerPrefs.GetInt(GlobalConstants.PREF_SFX_ON) != 0;

        _toggleMusic.SetIsOnWithoutNotify(_turnOnMusic);
        _toggleSFX.SetIsOnWithoutNotify(_turnOnSFX);

        _sliderMusicVolume = PlayerPrefs.GetFloat(GlobalConstants.PREF_SLIDER_MUSIC_VOLUME);
        _sliderMusicVolume = (_sliderMusicVolume > AudioHelper.SLIDER_VOLUME_MIN_VALUE) ? _sliderMusicVolume : AudioHelper.SLIDER_VOLUME_MAX_VALUE;
        PlayerPrefs.SetFloat(GlobalConstants.PREF_SLIDER_MUSIC_VOLUME, _sliderMusicVolume);

        _sliderSFXVolume = PlayerPrefs.GetFloat(GlobalConstants.PREF_SLIDER_SFX_VOLUME);
        _sliderSFXVolume = (_sliderSFXVolume > AudioHelper.SLIDER_VOLUME_MIN_VALUE) ? _sliderSFXVolume : AudioHelper.SLIDER_VOLUME_MAX_VALUE;
        PlayerPrefs.SetFloat(GlobalConstants.PREF_SLIDER_SFX_VOLUME, _sliderSFXVolume);

        // Fail screen (position and dimension)
        _uiFailScreenRect.anchoredPosition = new Vector2(_uiPauseScreenRect.anchoredPosition.x, _canvasHeight);
        _uiFailScreenRect.sizeDelta = new Vector2(_canvasWidth, _canvasHeight);

        // Exit screen (position and dimension)
        _uiExitScreenRect.anchoredPosition = new Vector2(_uiExitScreenRect.anchoredPosition.x, _canvasHeight);
        _uiExitScreenRect.sizeDelta = new Vector2(_canvasWidth, _canvasHeight);

        // Store screen (position and dimension)
        _uiStoreScreenRect.anchoredPosition = new Vector2(_uiStoreScreenRect.anchoredPosition.x, _canvasHeight);
        _uiStoreScreenRect.sizeDelta = new Vector2(_canvasWidth, _canvasHeight);
        _cheatButtons.SetActive(GlobalConstants.ENABLE_STORE_CANDY_BUTTONS);
    }

    private void Update()
    {
        UpdatePowerUp();
        UpdatePauseScreen();
        UpdateFailScreen();
        UpdateExitScreen();
        UpdateStoreScreen();
    }

    private void LateUpdate()
    {
        _uiStoreAnimator.SetBool(BOUNCE_ANIM_PARAM, _uiStoreBounce);

        _uiPowerUpAnimator.SetBool(BOUNCE_ANIM_PARAM, _uiPowerUpBounce);
        _uiPowerUpBounce = false;

        _uiLevelAnimator.SetBool(BOUNCE_ANIM_PARAM, _uiLevelBounce);
        _uiLevelBounce = false;

        _uiCandyAnimator.SetBool(BOUNCE_ANIM_PARAM, _uiCandyBounce);
        _uiCandyBounce = false;

        _uiCandyAnimator.SetBool(SHOW_OFF_ANIM_PARAM, _uiCandyShowOff);
        _uiCandyShowOff = false;

        _levelAnimationTime += Time.unscaledDeltaTime;
        if (_levelAnimationTime >= LEVEL_ANIMATION_TIME)
        {
            _levelBounce = true;
            _levelAnimationTime = 0;
        }
        else
        {
            _levelBounce = false;
        }
        _levelAnimator.SetBool(BOUNCE_ANIM_PARAM, _levelBounce);

        _candyAnimationTime += Time.unscaledDeltaTime;
        if (_candyAnimationTime >= CANDY_ANIMATION_TIME)
        {
            _candyAnimator.SetInteger(TYPE_ANIM_PARAM, UnityEngine.Random.Range(MIN_TYPE_ANIM, MAX_TYPE_ANIM + 1));
            _candyAnimationTime = 0;
        }

        _playerAnimator.SetBool(BOUNCE_ANIM_PARAM, _playerBounce);
    }
    #endregion

    #region Uitls
    private void UpdatePowerUp()
    {
        ShowPowerUp();
        HidePowerUp();
    }

    private void UpdatePauseScreen()
    {
        ShowPauseScreen();
        HidePauseScreen();
    }

    private void UpdateFailScreen()
    {
        ShowFailScreen();
        HideFailScreen();
    }

    private void UpdateExitScreen()
    {
        ShowExitScreen();
        HideExitScreen();
    }

    private void UpdateStoreScreen()
    {
        ShowStoreScreen();
        HideStoreScreen();
    }

    private string FormatCounter(string prefix, int counter)
    {
        return counter > 0 ? prefix + counter.ToString() : "";
    }

    private void ShowPowerUp()
    {
        if (_showPowerUp)
        {
            _timeSinceStarted += Time.unscaledDeltaTime;
            _percentageCompleted = _timeSinceStarted / ANIMATION_DELAY;
            _tmp.x = _interpolation.BounceOut(POWER_UP_POSX + AMOUNT_POWER_UP, POWER_UP_POSX, _percentageCompleted);
            _tmp.y = _uiPowerUp.anchoredPosition.y;
            _uiPowerUp.anchoredPosition = _tmp;
            _showPowerUp = !(_percentageCompleted >= 1);
        }
    }

    private void HidePowerUp()
    {
        if (_hidePowerUp)
        {
            _timeSinceStarted += Time.unscaledDeltaTime;
            _percentageCompleted = _timeSinceStarted / ANIMATION_DELAY;
            _tmp.x = _interpolation.BackIn(POWER_UP_POSX, POWER_UP_POSX + AMOUNT_POWER_UP, _percentageCompleted);
            _tmp.y = _uiPowerUp.anchoredPosition.y;
            _uiPowerUp.anchoredPosition = _tmp;
            if (_percentageCompleted >= 1)
            {
                _hidePowerUp = false;
                HideUIPowerUps();
            }
        }
    }

    private void HideUIPowerUps()
    {
        _uiPowerUpOrange.SetActive(false);
        _uiPowerUpRed.SetActive(false);
        _uiPowerUpGreen.SetActive(false);
        _uiPowerUpYellow.SetActive(false);
        _uiPowerUpPink.SetActive(false);
        _uiPowerUpBlue.SetActive(false);
    }

    private void ShowPauseScreen()
    {
        if (_showPauseScreen)
        {
            _timeSinceStarted += Time.unscaledDeltaTime;
            _percentageCompleted = _timeSinceStarted / ANIMATION_DELAY;
            _tmp.x = _uiPauseScreenRect.anchoredPosition.x;
            _tmp.y = _interpolation.BounceOut(_canvasHeight, 0, _percentageCompleted);
            _uiPauseScreenRect.anchoredPosition = _tmp;
            if (_percentageCompleted >= 1)
            {
                _showPauseScreen = false;
                _pauseHolder.interactable = true;
                _pauseHolder.blocksRaycasts = true;
            }
        }
    }

    private void HidePauseScreen()
    {
        if (_hidePauseScreen)
        {
            _timeSinceStarted += Time.unscaledDeltaTime;
            _percentageCompleted = _timeSinceStarted / ANIMATION_DELAY;
            _tmp.x = _uiPauseScreenRect.anchoredPosition.x;
            _tmp.y = _interpolation.BackIn(0, _canvasHeight, _percentageCompleted);
            _uiPauseScreenRect.anchoredPosition = _tmp;
            if (_percentageCompleted >= 1)
            {
                EnableStoreButton();
                _hidePauseScreen = false;
                _pauseAnimFinish = true;
                if (_audioFadeFinish)
                {
                    _pauseScreenActive = false;
                    _btnPause.interactable = true;
                }
            }
        }
    }

    private void ShowFailScreen()
    {
        if (_showFailScreen)
        {
            _timeSinceStarted += Time.unscaledDeltaTime;
            _percentageCompleted = _timeSinceStarted / ANIMATION_DELAY;
            _tmp.x = _uiFailScreenRect.anchoredPosition.x;
            _tmp.y = _interpolation.BounceOut(_canvasHeight, 0, _percentageCompleted);
            _uiFailScreenRect.anchoredPosition = _tmp;
            if (_percentageCompleted >= 1)
            {
                _showFailScreen = false;
                _failHolder.interactable = true;
                _failHolder.blocksRaycasts = true;

            }
        }
    }

    private void HideFailScreen()
    {
        if (_hideFailScreen)
        {
            float unscaledDeltaTime = Time.unscaledDeltaTime;
            if (_interstitialAdWasDisplayed)
            {
                unscaledDeltaTime = 0.0f;
                _interstitialAdWasDisplayed = false;
            }
            _timeSinceStarted += unscaledDeltaTime;
            _percentageCompleted = _timeSinceStarted / ANIMATION_DELAY;
            _tmp.x = _uiFailScreenRect.anchoredPosition.x;
            _tmp.y = _interpolation.BackIn(0, _canvasHeight, _percentageCompleted);
            _uiFailScreenRect.anchoredPosition = _tmp;
            if (_percentageCompleted >= 1)
            {
                _failScreenActive = false;
                _hideFailScreen = false;
                _btnPause.interactable = true;
            }
        }
    }

    private void ShowExitScreen()
    {
        if (_showExitScreen)
        {
            _timeSinceStarted += Time.unscaledDeltaTime;
            _percentageCompleted = _timeSinceStarted / ANIMATION_DELAY;
            _tmp.x = _uiExitScreenRect.anchoredPosition.x;
            _tmp.y = _interpolation.BounceOut(_canvasHeight, 0, _percentageCompleted);
            _uiExitScreenRect.anchoredPosition = _tmp;
            if (_percentageCompleted >= 1)
            {
                _showExitScreen = false;
                _exitHolder.interactable = true;
                _exitHolder.blocksRaycasts = true;
            }
        }
    }

    private void HideExitScreen()
    {
        if (_hideExitScreen)
        {
            _timeSinceStarted += Time.unscaledDeltaTime;
            _percentageCompleted = _timeSinceStarted / ANIMATION_DELAY;
            _tmp.x = _uiExitScreenRect.anchoredPosition.x;
            _tmp.y = _interpolation.BackIn(0, _canvasHeight, _percentageCompleted);
            _uiExitScreenRect.anchoredPosition = _tmp;
            if (_percentageCompleted >= 1)
            {
                _exitScreenActive = false;
                _hideExitScreen = false;
                if (_grandFinale)
                {
                    HomeAction();
                }
                else
                {
                    if (_forceGameOver)
                    {
                        _gameManager.GameOver();
                        _forceGameOver = false;
                    }
                    else
                    {
                        EnableStoreButton();
                        _btnPause.interactable = true;
                        _gameManager.IsProcessInput = true;
                    }
                }
            }
        }
    }

    private void ShowStoreScreen()
    {
        if (_showStoreScreen)
        {
            _timeSinceStarted += Time.unscaledDeltaTime;
            _percentageCompleted = _timeSinceStarted / ANIMATION_DELAY;
            _tmp.x = _uiStoreScreenRect.anchoredPosition.x;
            _tmp.y = _interpolation.BounceOut(_canvasHeight, 0, _percentageCompleted);
            _uiStoreScreenRect.anchoredPosition = _tmp;
            if (_percentageCompleted >= 1)
            {
                _showStoreScreen = false;
                _storeHolder.interactable = true;
                _storeHolder.blocksRaycasts = true;
            }
        }
    }

    private void HideStoreScreen()
    {
        if (_hideStoreScreen)
        {
            _timeSinceStarted += Time.unscaledDeltaTime;
            _percentageCompleted = _timeSinceStarted / ANIMATION_DELAY;
            _tmp.x = _uiStoreScreenRect.anchoredPosition.x;
            _tmp.y = _interpolation.BackIn(0, _canvasHeight, _percentageCompleted);
            _uiStoreScreenRect.anchoredPosition = _tmp;
            if (_percentageCompleted >= 1)
            {
                if (_applyPower)
                {
                    _applyPower = false;
                    ApplyPower(_powerKey, _powerParam);
                }
                _storeScreenActive = false;
                _hideStoreScreen = false;
                _btnPause.interactable = true;
                _btnStore.interactable = true;
                _gameManager.IsProcessInput = true;
            }
        }
    }

    private void ApplyPower(string powerKey, int powerParam)
    {
        _gameManager.BuyPower(powerKey, powerParam);
        ShowPowerUp(powerKey, powerParam);
    }

    private void PauseAnimations(bool pause)
    {
        AnimatorUpdateMode animatorUpdateMode = pause ? AnimatorUpdateMode.Normal : AnimatorUpdateMode.UnscaledTime;

        _uiLevelAnimator.updateMode = animatorUpdateMode;
        _levelAnimator.updateMode = animatorUpdateMode;

        _uiCandyAnimator.updateMode = animatorUpdateMode;
        _candyAnimator.updateMode = animatorUpdateMode;
        _tastyAnimator.updateMode = animatorUpdateMode;

        _uiPowerUpAnimator.updateMode = animatorUpdateMode;
        _chiAnimator.updateMode = animatorUpdateMode;
        _shieldAnimator.updateMode = animatorUpdateMode;
        _clockAnimator.updateMode = animatorUpdateMode;
        _potionAnimator.updateMode = animatorUpdateMode;
        _bonBonAnimator.updateMode = animatorUpdateMode;
        _bombAnimator.updateMode = animatorUpdateMode;
    }

    private void Reload()
    {
        _showFailScreen = false;
        _hideFailScreen = true;
        _playerBounce = true;
        _timeSinceStarted = 0;

        _sFXUI.PlayOneShot(_audioAssetManager.GetClip(AudioAssetManager.RELOAD_SFXUI));
        _uiHelper.PlayPanelOut(_sFXUI);

        _gameManager.ReloadGame();
    }

    private void PauseAction()
    {
        if (!_pauseScreenActive)
        {
            _gameManager.Pause(true);
            _gameManager.PauseGameAnimations(true);
            PauseAnimations(true);

            _pauseScreenActive = true;
            _pauseAnimFinish = false;
            _audioFadeFinish = false;
            _showPauseScreen = true;
            _hidePauseScreen = false;
            _timeSinceStarted = 0;

            _uiHelper.PlayPanelIn(_sFXUI);
        }
    }

    private void ResumeGameAction()
    {
        _gameManager.Pause(false);
        _gameManager.PauseGameAnimations(false);
        PauseAnimations(false);

        _showPauseScreen = false;
        _hidePauseScreen = true;
        _timeSinceStarted = 0;

        ApplyAudioSettings();

        _uiHelper.PlayPanelOut(_sFXUI);
    }

    private void ApplyAudioSettings()
    {
        if (_turnOnMusic)
        {
            _log.DebugLog(LOG_TAG, "Turn on Music", gameObject);
            PlayerPrefs.SetInt(GlobalConstants.PREF_MUSIC_ON, 1);

            // Fades music in when resume the game
            StartCoroutine(_audioHelper.StartAudioFade(_masterMixer, AudioAssetManager.MASTER_MIXER_MUSIC_EXP_PARAM,
                                                       AudioHelper.AUDIO_FADE_DELAY, AudioHelper.SLIDER_VOLUME_MIN_VALUE,
                                                       _sliderMusicVolume));
        }
        else
        {
            _log.DebugLog(LOG_TAG, "Turn off Music", gameObject);
            PlayerPrefs.SetInt(GlobalConstants.PREF_MUSIC_ON, 0);
            _audioHelper.SetAudioVolume(_masterMixer, AudioAssetManager.MASTER_MIXER_MUSIC_EXP_PARAM, AudioHelper.SLIDER_VOLUME_MIN_VALUE);
        }

        if (_turnOnSFX)
        {
            _log.DebugLog(LOG_TAG, "Turn on SFX", gameObject);
            PlayerPrefs.SetInt(GlobalConstants.PREF_SFX_ON, 1);

            // Fades SFX in when resume the game
            StartCoroutine(_audioHelper.StartAudioFade(_masterMixer, AudioAssetManager.MASTER_MIXER_SFX_EXP_PARAM,
                                                       AudioHelper.AUDIO_FADE_DELAY, AudioHelper.SLIDER_VOLUME_MIN_VALUE,
                                                       _sliderSFXVolume));
        }
        else
        {
            _log.DebugLog(LOG_TAG, "Turn off SFX", gameObject);
            PlayerPrefs.SetInt(GlobalConstants.PREF_SFX_ON, 0);
            _audioHelper.SetAudioVolume(_masterMixer, AudioAssetManager.MASTER_MIXER_SFX_EXP_PARAM, AudioHelper.SLIDER_VOLUME_MIN_VALUE);
        }

        if (_turnOnMusic || _turnOnSFX)
        {
            Invoke(nameof(FinalizeAudioSettings), AudioHelper.AUDIO_FADE_DELAY);
        }
        else
        {
            FinalizeAudioSettings();
        }
    }

    private void FinalizeAudioSettings()
    {
        _audioFadeFinish = true;
        if (_pauseAnimFinish)
        {
            _pauseScreenActive = false;
            _btnPause.interactable = true;
        }
    }

    private void HomeAction()
    {
        Action callbackBeforeLoad = _gameManager.DisposeGrandFinale;
        _sceneLoader.TransitionStart(GlobalConstants.CURRENT_TRANSITION,
                             SceneManager.GetActiveScene().buildIndex - 1,
                             _grandFinale ? callbackBeforeLoad : null);
    }

    private void ReloadGameAction()
    {
        int attempts = PlayerPrefs.GetInt(GlobalConstants.PREF_ATTEMPTS, 0);
        attempts++;
        if (attempts > MAX_ATTEMPTS)
        {
            _interstitialAdWasDisplayed = true;
            attempts = 0;
            _log.DebugLog(LOG_TAG, "AdMob Show Interstitial Ad", gameObject);

            if (Application.platform == RuntimePlatform.Android)
            {
                AdMob.GetInstance().ShowInterstitialAd(Reload);
            }
            else
            {
                Reload();
            }
        }
        else
        {
            Reload();
        }
        PlayerPrefs.SetInt(GlobalConstants.PREF_ATTEMPTS, attempts);
    }

    private void ExitAction()
    {
        //  As the action is deferred in time, I must verify the initial condition again and the gameover condition
        if (CanShowExitScreen() && !_gameManager.IsGameOver())
        {
            _exitScreenActive = true;
            _showExitScreen = true;
            _hideExitScreen = false;
            _timeSinceStarted = 0;

            _uiHelper.PlayPanelIn(_sFXUI);
        }
    }

    private void CancelExitAction()
    {
        _showExitScreen = false;
        _hideExitScreen = true;
        _timeSinceStarted = 0;

        _uiHelper.PlayPanelOut(_sFXUI);
    }

    private void AcceptExitAction()
    {
        if (_gameManager.IsGrandFinale)
        {
            _grandFinale = true;
        }
        else
        {
            _forceGameOver = true;
        }

        _showExitScreen = false;
        _hideExitScreen = true;
        _timeSinceStarted = 0;

        _uiHelper.PlayPanelOut(_sFXUI);
    }

    private void StoreAction()
    {
        if (!_storeScreenActive)
        {
            _storeScreenActive = true;
            _showStoreScreen = true;
            _hideStoreScreen = false;
            _timeSinceStarted = 0;

            _uiHelper.PlayPanelIn(_sFXUI);
        }
    }

    private void CancelStoreAction()
    {
        _showStoreScreen = false;
        _hideStoreScreen = true;
        _timeSinceStarted = 0;

        _uiHelper.PlayPanelOut(_sFXUI);
    }

    private void BuyPower()
    {
        Button button = _uiHelper.GetButtonClicked();
        UIStore uiStoreScreen = button.gameObject.GetComponent<UIStore>();
        _applyPower = true;
        _powerParam = uiStoreScreen.Amount;
        AddScoreBouncing(-uiStoreScreen.Price);
        CancelStoreAction();
    }

    private void BuyRedPowerAction()
    {
        _powerKey = ObjectPooler.POWER_UP_RED_KEY;
        BuyPower();
    }

    private void BuyOrangePowerAction()
    {
        _powerKey = ObjectPooler.POWER_UP_ORANGE_KEY;
        BuyPower();
    }

    private void BuyGreenPowerAction()
    {
        _powerKey = ObjectPooler.POWER_UP_GREEN_KEY;
        BuyPower();
    }

    private void BuyYellowPowerAction()
    {
        _powerKey = ObjectPooler.POWER_UP_YELLOW_KEY;
        BuyPower();
    }

    private void RateAction()
    {
        Application.OpenURL(GlobalConstants.URL_FLUBBIE);
    }

    private bool CanShowExitScreen()
    {
        return !_pauseScreenActive && !_failScreenActive && !_storeScreenActive && !_exitScreenActive;
    }

    private void AddScore(int value)
    {
        _score += value;
        _candyCounter.text = _score.ToString();
        PlayerPrefs.SetInt(GlobalConstants.PREF_SCORE, _score);
    }
    #endregion

    #region API
    public void ShowPowerUp(string powerKey, int counter)
    {
        HideUIPowerUps();
        _uiPowerUpBounce = false;
        _showPowerUp = true;
        _hidePowerUp = false;
        _timeSinceStarted = 0;

        switch (powerKey)
        {
            case ObjectPooler.POWER_UP_ORANGE_KEY:
                _uiPowerUpOrange.SetActive(true);
                break;

            case ObjectPooler.POWER_UP_RED_KEY:
                _uiPowerUpRed.SetActive(true);
                _redCounter.text = FormatCounter(PREFIX_CROSS, counter);
                break;

            case ObjectPooler.POWER_UP_GREEN_KEY:
                _uiPowerUpGreen.SetActive(true);
                _greenCounter.text = FormatCounter(PREFIX_CROSS, counter);
                break;

            case ObjectPooler.POWER_UP_YELLOW_KEY:
                _uiPowerUpYellow.SetActive(true);
                _yellowCounter.text = FormatCounter(PREFIX_CROSS, counter);
                break;

            case ObjectPooler.POWER_UP_PINK_KEY:
                _uiPowerUpPink.SetActive(true);
                _pinkCounter.text = FormatCounter(PREFIX_PLUS, counter);
                break;

            case ObjectPooler.POWER_UP_BLUE_KEY:
                _uiPowerUpBlue.SetActive(true);
                _blueCounter.text = FormatCounter("", counter);
                break;

            default:
                _log.DebugLogError(LOG_TAG, "Unknow power up", gameObject);
                break;
        }
    }

    public void UpdatePowerUp(string powerKey, int counter)
    {
        _uiPowerUpBounce = true;

        switch (powerKey)
        {
            case ObjectPooler.POWER_UP_ORANGE_KEY:
                break;

            case ObjectPooler.POWER_UP_RED_KEY:
                _redCounter.text = FormatCounter(PREFIX_CROSS, counter);
                if (counter <= 0)
                {
                    _uiPowerUpBounce = false;
                }
                break;

            case ObjectPooler.POWER_UP_GREEN_KEY:
                _greenCounter.text = FormatCounter(PREFIX_CROSS, counter);
                break;

            case ObjectPooler.POWER_UP_YELLOW_KEY:
                _yellowCounter.text = FormatCounter(PREFIX_CROSS, counter);
                break;

            case ObjectPooler.POWER_UP_PINK_KEY:
                _pinkCounter.text = FormatCounter(PREFIX_PLUS, counter);
                break;

            case ObjectPooler.POWER_UP_BLUE_KEY:
                _blueCounter.text = FormatCounter("", counter);
                break;

            default:
                _log.DebugLogError(LOG_TAG, "Unknow power up", gameObject);
                break;
        }
    }

    public void PowersDown()
    {
        _uiPowerUpBounce = false;
        _showPowerUp = false;
        _hidePowerUp = true;
        _timeSinceStarted = 0;
    }

    public void AddScoreBouncing(int value)
    {
        AddScore(value);
        _uiCandyBounce = true;
    }

    public void AddScoreShowingOff(int value)
    {
        AddScore(value);
        _uiCandyShowOff = true;
    }

    public void ShowLevel(int level, int maxLevel)
    {
        _levelCounter.text = level.ToString() + " / " + maxLevel.ToString();
        _uiLevelBounce = true;
    }

    public void HintStore(bool hint)
    {
        if (hint)
        {
            if (_score >= UIStore.MIN_PRICE)
            {
                _uiStoreBounce = true;
            }
        } else
        {
            _uiStoreBounce = false;
        }
    }

    public void Pause()
    {
        _btnStore.interactable = false;
        _btnPause.interactable = false;

        _pauseHolder.interactable = false;
        _pauseHolder.blocksRaycasts = false;

        _uiHelper.ExecuteActionAfterClickAudio(_sFXUI, PauseAction);
    }

    public void ResumeGame()
    {
        _pauseHolder.interactable = false;
        _pauseHolder.blocksRaycasts = false;

        _uiHelper.ExecuteActionAfterClickAudio(_sFXUI, ResumeGameAction);
    }

    public void Home()
    {
        _failHolder.interactable = false;
        _failHolder.blocksRaycasts = false;

        _uiHelper.ExecuteActionAfterClickAudio(_sFXUI, HomeAction);
    }

    public void DisablePauseButton()
    {
        _btnPause.interactable = false;
    }

    public void DisableStoreButton()
    {
        _btnStore.interactable = false;
    }

    public void EnableStoreButton()
    {
        if (!_gameManager.IsGrandFinale)
        {
            _btnStore.interactable = true;
        }
    }

    public void PlayHitSound()
    {
        // Plays the clip only if SFX audio is enabled
        if (_toggleSFX.isOn)
        {
            // This clip must continue to play even when the game is paused
            _sFXUI.PlayOneShot(_audioAssetManager.GetClip(AudioAssetManager.HIT_SFXUI));
        }
    }

    public void PlayGameOverMusic()
    {
        _music.Stop();
        _music.clip = _audioAssetManager.GetClip(AudioAssetManager.GAME_OVER_MUSIC);
        _music.Play();
    }

    public void PlayGameMusic()
    {
        _music.Stop();
        _music.clip = _audioAssetManager.GetClip(AudioAssetManager.GAME_MUSIC);
        _music.Play();

        StartCoroutine(_audioHelper.StartAudioFade(_masterMixer,
                                           AudioAssetManager.MASTER_MIXER_CROSSFADE_EXP_PARAM,
                                           AudioHelper.AUDIO_FADE_DELAY,
                                           AudioHelper.SLIDER_VOLUME_MIN_VALUE, 1));
    }

    public void PlayGrandFinaleMusic()
    {
        _music.Stop();
        _music.clip = _audioAssetManager.GetClip(AudioAssetManager.GRAND_FINALE_MUSIC);
        _music.Play();

        StartCoroutine(_audioHelper.StartAudioFade(_masterMixer,
                                           AudioAssetManager.MASTER_MIXER_CROSSFADE_EXP_PARAM,
                                           AudioHelper.AUDIO_FADE_DELAY,
                                           AudioHelper.SLIDER_VOLUME_MIN_VALUE, 1));
    }

    public void ReloadGame()
    {
        _failHolder.interactable = false;
        _failHolder.blocksRaycasts = false;

        _uiHelper.ExecuteActionAfterClickAudio(_sFXUI, ReloadGameAction);
    }

    public void Fail()
    {
        _failScreenActive = true;

        _failHolder.interactable = false;
        _failHolder.blocksRaycasts = false;

        _showFailScreen = true;
        _hideFailScreen = false;

        _playerBounce = false;
        _timeSinceStarted = 0;

        _uiHelper.PlayPanelIn(_sFXUI);
    }

    public void Exit()
    {
        if (CanShowExitScreen())
        {
            _btnStore.interactable = false;
            _btnPause.interactable = false;

            _exitHolder.interactable = false;
            _exitHolder.blocksRaycasts = false;

            _gameManager.IsProcessInput = false;

            _uiHelper.ExecuteActionAfterClickAudio(_sFXUI, ExitAction);
        }
    }

    public void CancelExit()
    {
        _exitHolder.interactable = false;
        _exitHolder.blocksRaycasts = false;

        _uiHelper.ExecuteActionAfterClickAudio(_sFXUI, CancelExitAction);
    }

    public void AcceptExit()
    {
        _exitHolder.interactable = false;
        _exitHolder.blocksRaycasts = false;

        _uiHelper.ExecuteActionAfterClickAudio(_sFXUI, AcceptExitAction);
    }

    public void Store()
    {
        _storeHolder.interactable = false;
        _storeHolder.blocksRaycasts = false;

        _btnPause.interactable = false;
        _btnStore.interactable = false;

        _gameManager.IsProcessInput = false;

        _uiHelper.ExecuteActionAfterClickAudio(_sFXUI, StoreAction);
    }

    public void CancelStore()
    {
        _storeHolder.interactable = false;
        _storeHolder.blocksRaycasts = false;

        _uiHelper.ExecuteActionAfterClickAudio(_sFXUI, CancelStoreAction);
    }

    public void BuyRedPower()
    {
        _storeHolder.interactable = false;
        _storeHolder.blocksRaycasts = false;

        _uiHelper.ExecuteActionAfterClickAudio(_sFXUI, BuyRedPowerAction);
    }

    public void BuyOrangePower()
    {
        _storeHolder.interactable = false;
        _storeHolder.blocksRaycasts = false;

        _uiHelper.ExecuteActionAfterClickAudio(_sFXUI, BuyOrangePowerAction);
    }

    public void BuyGreenPower()
    {
        _storeHolder.interactable = false;
        _storeHolder.blocksRaycasts = false;

        _uiHelper.ExecuteActionAfterClickAudio(_sFXUI, BuyGreenPowerAction);
    }

    public void BuyYellowPower()
    {
        _storeHolder.interactable = false;
        _storeHolder.blocksRaycasts = false;

        _uiHelper.ExecuteActionAfterClickAudio(_sFXUI, BuyYellowPowerAction);
    }

    public void Rate()
    {
        _uiHelper.ExecuteActionAfterClickAudio(_sFXUI, RateAction);
    }

    public void ToggleMusic(bool checkedValue)
    {
        _uiHelper.PlayClickAudio(_sFXUI);
        _turnOnMusic = checkedValue;
    }

    public void ToggleSFX(bool checkedValue)
    {
        _uiHelper.PlayClickAudio(_sFXUI);
        if (checkedValue)
        {
            _turnOnSFX = true;

            // Adjusts SFXUI in real time
            _audioHelper.SetAudioVolume(_masterMixer, AudioAssetManager.MASTER_MIXER_SFXUI_EXP_PARAM, _sliderSFXVolume);
        }
        else
        {
            _turnOnSFX = false;

            // Adjusts SFXUI in real time
            _audioHelper.SetAudioVolume(_masterMixer, AudioAssetManager.MASTER_MIXER_SFXUI_EXP_PARAM, AudioHelper.SLIDER_VOLUME_MIN_VALUE);
        }
    }

    public void ShowHelp()
    {
        _uiFingerTap.SetActive(true);
    }

    public void TapAudio()
    {
        _sFXUI.PlayOneShot(_audioAssetManager.GetClip(AudioAssetManager.TAP_SFXUI));
    }

    public void ToggleShopButtons()
    {
        _toggleShopButtons++;
        if (_toggleShopButtons >= GlobalConstants.CLICKS_CHEAT_TOGGLE)
        {
            _toggleShopButtons = 0;
            _cheatButtons.SetActive(!_cheatButtons.activeSelf);
            _log.DebugLog(LOG_TAG, "Toggle shop buttons", gameObject);
        }
    }

    public void CheatAddScore(int value)
    {
        _uiHelper.PlayClickAudio(_sFXUI);
        AddScoreBouncing(value);
    }
    #endregion
}
