using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    #region State machine
    private enum State
    {
        IDLE,
        AIM_FORWARD,
        AIM_BACKWARD,
        JUMP_FORWARD,
        JUMP_BACKWARD,
        DEAD,
        FALL,
        DISPOSE
    }

    private enum PowerState
    {
        NORMAL,
        ORANGE,
        RED,
        GREEN,
        YELLOW,
        PINK,
        BLUE
    }
    #endregion

    #region Public Constants
    public const string TAG_PLAYER = "Player";
    #endregion

    #region Private Constants
    private const string LOG_TAG = "PlayerController";
    private const string CHARACTER_NAME = "Character";
    private const string CROWN_NAME = "Crown";
    private const string HURT_BOX_NAME = "HurtBox";
    private const string HIT_BOX_NAME = "HitBox";
    private const string CAM_FOLLOW_NAME = "CamFollow";
    private const float FUTURE_CAM_FOLLOW_RAIDUS = 0.3f;
    private const float CAM_FOLOW_SPEED = 6.0f;
    private const float VERTICAL_GAME_OVER_PANNING = 20.0f;
    private const float CAM_FOLOW_FALL_SPEED = 20.0f;
    private const float PLAYER_SPEED = 9.0f;
    private const float PLAYER_FALL_SPEED = 19.0f;
    private const float PLAYER_OUT_OF_SIGHT_DISTANCE = 7.0f;
    private const float SLEEP_ANIMATION_TIME = 30.0f;
    private const float CHANGE_ANIMATION_TIME = 3.0f;
    private const string COLLISION_ANIM_PARAM = "Collision";
    private const string SLEEP_ANIM_PARAM = "Sleep";
    private const string TYPE_ANIM_PARAM = "Type";
    private const int MIN_TYPE_ANIM = 1;
    private const int MAX_TYPE_ANIM = 5;
    private const float SHRINK_SCALE = 0.4f;
    private const string BUTTON_JUMP = "Jump";
    private const string BUTTON_FIRE = "Fire1";
    private const string BUTTON_CANCEL = "Cancel";
    private const float FINAL_POSITION_Y = 0.0f;
    private const float FIREWORKS_VOLUME_SCALE = 0.1f;
    private const float PINK_POWER_TIME = 4.0f;
    private const float LITTLE_MORE_THAN_A_SECOND = 1.3f;
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();

    [SerializeField] private GameObject _crashEffect;
    [SerializeField] private GameObject _powerOrangeEffect;
    [SerializeField] private GameObject _powerRedEffect;
    [SerializeField] private GameObject _powerGreenEffect;
    [SerializeField] private GameObject _powerYellowEffect;
    [SerializeField] private GameObject _powerPinkEffect;
    [SerializeField] private GameObject _powerBlueEffect;

    private AudioAssetManager _audioAssetManager;
    private GameObject _camFollow; // The object that the camera wants to move with
    private GameObject _character;
    private Animator _characterAnimator;
    private GameObject _crown;
    private Animator _crownAnimator;
    private Rigidbody2D _rigidbody;
    private AudioSource _sFX;
    private BoxCollider2D _hurtBox;
    private BoxCollider2D _hitBox;
    private TrailRenderer _trailRenderer;
    private float _initalTrailRendererWidth;
    private State _currentState;
    private PowerState _currentPowerState;
    private ChallengeManager _challengeManager;
    private GameManager _gameManager;
    private Circle _futureCamFollowLocalPos;
    private Hud _hud;
    private float _changeAnimationTime;
    private float _idleTime;
    private bool _sleepAnimation;
    private bool _faintAnimation;
    private bool _disposed;
    [SerializeField] private Sprite _spriteDebugCamFollow = null;
    [SerializeField] private GameObject _prefabDebugFutureCamFollow = null;
    private GameObject _debugFutureCamFollow;
    private IEnumerator _focusCoroutine;
    private IEnumerator _sleepAudioCoroutine;
    private IEnumerator _powerRedAudioCoroutine; 
    private IEnumerator _powerGreenAudioCoroutine;
    private IEnumerator _powerYellowAudioCoroutine;
    private IEnumerator _powerPinkFinalizeCoroutine;
    private IEnumerator _powerBlueFinalizeCoroutine;
    private List<AudioClip> _powerOrangeClips;
    private List<AudioClip> _crunchClips;
    private List<AudioClip> _fireworkClips;

    // Powers
    private int _targetsInARow;
    private int _shields;
    private Vector3 _currentTarget;
    private bool _applySlowDown;
    private int _slowDownCount;
    private int _potions;
    private bool _applyPotion;
    private int _tastyValue;
    private bool _applyTasty;
    private bool _applyBomb;
    private int _bombCount;
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        _challengeManager = GameObject.FindGameObjectWithTag(ChallengeManager.TAG_CHALLENGE_MANAGER).GetComponent<ChallengeManager>();
        _gameManager = GameObject.FindGameObjectWithTag(GameManager.TAG_GAME_MANAGER).GetComponent<GameManager>();
        _hud = GameObject.FindGameObjectWithTag(Hud.TAG_HUD).GetComponent<Hud>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _sFX = GetComponent<AudioSource>();
        _character = transform.Find(CHARACTER_NAME).gameObject;
        _characterAnimator = _character.GetComponent<Animator>();
        _crown = _character.transform.Find(CROWN_NAME).gameObject;
        _crownAnimator = _crown.GetComponent<Animator>();
        _hurtBox = _character.transform.Find(HURT_BOX_NAME).gameObject.GetComponent<BoxCollider2D>();
        _hitBox = _character.transform.Find(HIT_BOX_NAME).gameObject.GetComponent<BoxCollider2D>();
        _trailRenderer = _character.gameObject.GetComponent<TrailRenderer>();
        _camFollow = transform.Find(CAM_FOLLOW_NAME).gameObject;
        _currentState = State.IDLE;
        _currentPowerState = PowerState.NORMAL;
        IdleAnimation();
    }

    private void Start()
    {
        // Audio and Clips
        _audioAssetManager = AudioAssetManager.GetInstance();
        _powerOrangeClips = new List<AudioClip>
        {
            _audioAssetManager.GetClip(AudioAssetManager.ZIP1_SFX),
            _audioAssetManager.GetClip(AudioAssetManager.ZIP2_SFX),
            _audioAssetManager.GetClip(AudioAssetManager.ZIP3_SFX),
            _audioAssetManager.GetClip(AudioAssetManager.ZIP4_SFX)
        };
        _crunchClips = new List<AudioClip>
        {
            _audioAssetManager.GetClip(AudioAssetManager.CRUNCH1_SFX),
            _audioAssetManager.GetClip(AudioAssetManager.CRUNCH2_SFX),
            _audioAssetManager.GetClip(AudioAssetManager.CRUNCH3_SFX)
        };
        _fireworkClips = new List<AudioClip>
        {
            _audioAssetManager.GetClip(AudioAssetManager.FIREWORK_01_SFX),
            _audioAssetManager.GetClip(AudioAssetManager.FIREWORK_02_SFX),
            _audioAssetManager.GetClip(AudioAssetManager.FIREWORK_03_SFX),
            _audioAssetManager.GetClip(AudioAssetManager.FIREWORK_04_SFX),
            _audioAssetManager.GetClip(AudioAssetManager.FIREWORK_05_SFX),
            _audioAssetManager.GetClip(AudioAssetManager.FIREWORK_06_SFX),
            _audioAssetManager.GetClip(AudioAssetManager.FIREWORK_07_SFX),
            _audioAssetManager.GetClip(AudioAssetManager.FIREWORK_08_SFX),
            _audioAssetManager.GetClip(AudioAssetManager.FIREWORK_09_SFX),
            _audioAssetManager.GetClip(AudioAssetManager.FIREWORK_10_SFX),
            _audioAssetManager.GetClip(AudioAssetManager.FIREWORK_11_SFX),
            _audioAssetManager.GetClip(AudioAssetManager.FIREWORK_12_SFX),
            _audioAssetManager.GetClip(AudioAssetManager.FIREWORK_13_SFX),
            _audioAssetManager.GetClip(AudioAssetManager.FIREWORK_14_SFX),
            _audioAssetManager.GetClip(AudioAssetManager.FIREWORK_15_SFX)
        };

        _initalTrailRendererWidth = _trailRenderer.startWidth;
        _futureCamFollowLocalPos = new Circle(_camFollow.transform.localPosition, FUTURE_CAM_FOLLOW_RAIDUS);
        CinemachineVirtualCamera cinemachineVirtualCamera = GameObject.FindGameObjectWithTag(GlobalConstants.TAG_VIRTUAL_CAMERA_ONE).
                                                            GetComponent<CinemachineVirtualCamera>();
        cinemachineVirtualCamera.Follow = _camFollow.transform;
        _gameManager.IsProcessInput = true;
        AimForward();

        /* 
         * Sets debug sprites.
         * _camFollow: It's a cam icon followed by Cinemachine's camera.
         * _debugFutureCamFollow: It's a white circle which points out to the future position of _camFollow (i.e. _futureCamFollowLocalPos). 
         *                        To achieve a smooth camera movement, _camFollow tries to reach _futureCamFollowLocalPos at
         *                        CAM_FOLOW_SPEED speed.
         */
        if (GlobalConstants.ENABLE_CAM_FOLLOW_DEBUG)
        {
            _camFollow.GetComponent<SpriteRenderer>().sprite = _spriteDebugCamFollow;
            _debugFutureCamFollow = Instantiate(_prefabDebugFutureCamFollow) as GameObject;
            _debugFutureCamFollow.transform.parent = transform;
            _debugFutureCamFollow.transform.localPosition = new Vector2(_futureCamFollowLocalPos.Center.x, _futureCamFollowLocalPos.Center.y);
            _debugFutureCamFollow.transform.rotation = Quaternion.identity;
        }
    }

    private void Update()
    {
        if (!_gameManager.IsGamePaused)
        {
            // Calls functions that don't involve physics
            switch (_currentState)
            {
                case State.IDLE:
                    HandleInput();
                    StateIdle();
                    StatePower();
                    break;
                case State.AIM_FORWARD:
                    break;
                case State.AIM_BACKWARD:
                    break;
                case State.JUMP_FORWARD:
                    break;
                case State.JUMP_BACKWARD:
                    break;
                case State.DEAD:
                    break;
                case State.FALL:
                    StateFall();
                    break;
                case State.DISPOSE:
                    break;
                default:
                    break;
            }
        }
    }

    private void FixedUpdate()
    {
        // Calls functions involving physics
        switch (_currentState)
        {
            case State.IDLE:
                break;
            case State.AIM_FORWARD:
                StateAimForward();
                break;
            case State.AIM_BACKWARD:
                StateAimBackward();
                break;
            case State.JUMP_FORWARD:
                break;
            case State.JUMP_BACKWARD:
                break;
            case State.DEAD:
                break;
            case State.FALL:
                break;
            case State.DISPOSE:
                StateDispose();
                break;
            default:
                break;
        }
    }

    private void LateUpdate()
    {
        _characterAnimator.SetBool(SLEEP_ANIM_PARAM, _sleepAnimation);
        _characterAnimator.SetBool(COLLISION_ANIM_PARAM, _faintAnimation);
        _changeAnimationTime += Time.deltaTime;
        if (_changeAnimationTime >= CHANGE_ANIMATION_TIME)
        {
            _characterAnimator.SetInteger(TYPE_ANIM_PARAM, Random.Range(MIN_TYPE_ANIM, MAX_TYPE_ANIM + 1));
            _changeAnimationTime = 0;
        }
    }
    #endregion

    #region Utils
    private void HandleInput()
    {
        if (Input.GetButtonDown(BUTTON_CANCEL)) // Exit Game (PC & Android)
        {
            _hud.Exit();
        }
        else
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                HandleInputAndroid();
            }
            else
            {
                HandleInputPC();
            }
        }
    }

    private void HandleInputAndroid()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                if (!EventSystem.current.IsPointerOverGameObject(touch.fingerId)) // Event isn't on UI GameObject
                {
                    AimForward();
                }
            }
        }
    }

    private void HandleInputPC()
    {
        if (Input.GetButtonDown(BUTTON_JUMP) || Input.GetButtonDown(BUTTON_FIRE))
        {
            if (!EventSystem.current.IsPointerOverGameObject()) // Event isn't on UI GameObject
            {
                AimForward();
            }
        }
    }

    private void AimForward()
    {
        _log.DebugLog(LOG_TAG, "AimForward _currentState: " + _currentState + 
                               ", _currentPowerState: " + _currentPowerState +
                               ", _gameManager.IsProcessInput " + _gameManager.IsProcessInput,
                               gameObject);

        if (_gameManager.IsProcessInput)
        {
            WakeUp();
            _hud.DisableStoreButton();
            _currentState = State.AIM_FORWARD;
        }
    }

    private void StateIdle()
    {
        SleepTime();
    }

    private void WakeUp()
    {
        _idleTime = 0;
        if (_sleepAnimation)
        {
            _sleepAnimation = false;
            _challengeManager.IdleAnimation();
            if (_sleepAudioCoroutine != null)
            {
                _hud.HintStore(false);
                StopCoroutine(_sleepAudioCoroutine);
                _sleepAudioCoroutine = null;
            }
        }
    }

    private void SleepTime()
    {
        if (!_sleepAnimation && !_gameManager.IsGrandFinale)
        {
            _idleTime += Time.deltaTime;
            if (_idleTime >= SLEEP_ANIMATION_TIME)
            {
                _sleepAnimation = true;
                _challengeManager.SleepAnimation();
                _sleepAudioCoroutine = SleepAudio();
                StartCoroutine(_sleepAudioCoroutine);
                _hud.HintStore(true);
            }
        }
    }

    private IEnumerator SleepAudio()
    {
        AudioClip clip = _audioAssetManager.GetClip(AudioAssetManager.CRICKETS_SFX);
        while (_currentState == State.IDLE)
        {
            _sFX.PlayOneShot(clip);
            yield return new WaitForSeconds(clip.length);
        }
    }

    // Runs when Flubbie _currentState is IDLE
    private void StatePower()
    {
        switch (_currentPowerState)
        {
            case PowerState.NORMAL:
                break;
            case PowerState.ORANGE:
                PowerStateOrange();
                break;
            case PowerState.RED:
                PowerStateRed();
                break;
            case PowerState.GREEN:
                PowerStateGreen();
                break;
            case PowerState.YELLOW:
                PowerStateYellow();
                break;
            case PowerState.PINK:
                PowerStatePink();
                break;
            case PowerState.BLUE:
                PowerStateBlue();
                break;
            default:
                break;
        }
    }

    private void PowerStateOrange()
    {
        _log.DebugLog(LOG_TAG, "Targets in a row count " + _targetsInARow, gameObject);

        if (_targetsInARow <= 0)
        {
            PowersDown();
        }
        else
        {
            _sFX.PlayOneShot(_powerOrangeClips[Random.Range(0, _powerOrangeClips.Count)]);
            AimForward();
            _targetsInARow--;
            _hud.UpdatePowerUp(ObjectPooler.POWER_UP_ORANGE_KEY, _targetsInARow);
        }
    }

    private void PowerStateRed()
    {
        if (_shields <= 0)
        {
            PowersDown();
        }
    }

    private void PowerStateGreen()
    {
        // Just once
        if (_applySlowDown)
        {
            _challengeManager.SlowDown();
            _applySlowDown = false;
        }
    }

    private void PowerStateYellow()
    {
        // Just once
        if (_applyPotion)
        {
            _character.transform.localScale = Vector3.one * SHRINK_SCALE;
            _trailRenderer.startWidth *= SHRINK_SCALE;
            _applyPotion = false;
        }
    }

    private void PowerStatePink()
    {
        // Just once
        if (_applyTasty)
        {
            _hud.AddScoreShowingOff(_tastyValue);
            _applyTasty = false;
        }
    }

    private void PowerStateBlue()
    {
        // Just once
        if (_applyBomb)
        {
            _applyBomb = false;
        }
    }

    private void PowersDown()
    {
        _log.DebugLog(LOG_TAG, "PowersDown", gameObject);

        _currentPowerState = PowerState.NORMAL;

        // Effects
        _powerOrangeEffect.SetActive(false);
        _powerRedEffect.SetActive(false);
        _powerGreenEffect.SetActive(false);
        _powerYellowEffect.SetActive(false);
        // Shrink effect is maintained during death
        if (!IsDead()) { 
            _character.transform.localScale = Vector3.one;
            _trailRenderer.startWidth = _initalTrailRendererWidth;
        }
        _powerPinkEffect.SetActive(false);
        _powerBlueEffect.SetActive(false);

        // Hud
        _hud.PowersDown();

        // Coroutines
        if (_powerRedAudioCoroutine != null)
        {
            StopCoroutine(_powerRedAudioCoroutine);
            _powerRedAudioCoroutine = null;
        }
        if (_powerGreenAudioCoroutine != null)
        {
            StopCoroutine(_powerGreenAudioCoroutine);
            _powerGreenAudioCoroutine = null;
        }
        if (_powerYellowAudioCoroutine != null)
        {
            StopCoroutine(_powerYellowAudioCoroutine);
            _powerYellowAudioCoroutine = null;
        }
        if (_powerPinkFinalizeCoroutine != null)
        {
            StopCoroutine(_powerPinkFinalizeCoroutine);
            _powerPinkFinalizeCoroutine = null;
        }
        if (_powerBlueFinalizeCoroutine != null)
        {
            StopCoroutine(_powerBlueFinalizeCoroutine);
            _powerBlueFinalizeCoroutine = null;
        }
    }

    private IEnumerator PowerRedAudio()
    {
        AudioClip clip = _audioAssetManager.GetClip(AudioAssetManager.ELECTRICITY_SFX);
        while (_currentPowerState == PowerState.RED)
        {
            _sFX.PlayOneShot(clip);
            yield return new WaitForSeconds(clip.length);
        }
    }

    private IEnumerator PowerGreenAudio()
    {
        AudioClip clip = _audioAssetManager.GetClip(AudioAssetManager.CLOCK_SFX);
        while (_currentPowerState == PowerState.GREEN)
        {
            _sFX.PlayOneShot(clip);
            yield return new WaitForSeconds(clip.length);
        }
    }

    private IEnumerator PowerYellowAudio()
    {
        AudioClip clip = _audioAssetManager.GetClip(AudioAssetManager.MAGIC_SFX);
        while (_currentPowerState == PowerState.YELLOW)
        {
            _sFX.PlayOneShot(clip);
            yield return new WaitForSeconds(clip.length);
        }
    }

    private IEnumerator PowerPinkFinalize()
    {
        yield return new WaitForSeconds(PINK_POWER_TIME / 2);
        _hud.UpdatePowerUp(ObjectPooler.POWER_UP_PINK_KEY, _tastyValue);

        yield return new WaitForSeconds(PINK_POWER_TIME / 2);
        PowersDown();

        _log.DebugLog(LOG_TAG, "Pink power ends because time is up", gameObject);
    }

    private IEnumerator PowerBlueFinalize()
    {
        yield return new WaitForSeconds(LITTLE_MORE_THAN_A_SECOND);

        _bombCount--;
        _hud.UpdatePowerUp(ObjectPooler.POWER_UP_BLUE_KEY, _bombCount);

        if (_bombCount > 0)
        {
            _sFX.PlayOneShot(_audioAssetManager.GetClip(AudioAssetManager.BEEP_SFX));
            if (_bombCount == 1)
            {
                _sFX.PlayOneShot(_audioAssetManager.GetClip(AudioAssetManager.OH_OH_SFX));
            }

            _powerBlueFinalizeCoroutine = PowerBlueFinalize();
            StartCoroutine(_powerBlueFinalizeCoroutine);
        }
        else
        {
            // Time is up
            if (_currentState == State.IDLE)
            {
                _gameManager.GameOver();
                _log.DebugLog(LOG_TAG, "Blue power ends because time is up", gameObject);
            }
        }
    }

    private void StateAimForward()
    {
        _log.DebugLog(LOG_TAG, "StateAimForward", gameObject);

        GameObject target = _challengeManager.PeekNextTarget();
        if (target != null)
        {
            // Stops previous camera movement
            if (_focusCoroutine != null)
            {
                StopCoroutine(_focusCoroutine);
                _focusCoroutine = null;
            }

            LookAt(target.transform.position);
            _rigidbody.velocity = (target.transform.position - transform.position).normalized * PLAYER_SPEED;
            _currentState = State.JUMP_FORWARD;
        }
        else
        {
            // There are no more Targets
            _currentState = State.IDLE;
        }
    }

    private void StateAimBackward()
    {
        _log.DebugLog(LOG_TAG, "StateAimBackward", gameObject);

        // Stops previous camera movement
        if (_focusCoroutine != null)
        {
            StopCoroutine(_focusCoroutine);
            _focusCoroutine = null;
        }

        LookAt(_currentTarget);
        _rigidbody.velocity = (_currentTarget - transform.position).normalized * PLAYER_SPEED;
        _currentState = State.JUMP_BACKWARD;
    }

    private IEnumerator Focus()
    {
        do
        {
            // Moves _camFollow to _futureCamFollowLocalPos (using local position) at CAM_FOLOW_SPEED speed
            // Cinemachine follows _camFollow
            Vector2 _velocityDirection = (_futureCamFollowLocalPos.Center - (Vector2)_camFollow.transform.localPosition).normalized;
            _camFollow.transform.localPosition = (Vector2)_camFollow.transform.localPosition +
                                                         _velocityDirection *
                                                         CAM_FOLOW_SPEED * Time.deltaTime;

            yield return null;
        } while (!_futureCamFollowLocalPos.Contains(_camFollow.transform.localPosition));

        // Centers _camFollow in _futureCamFollowLocalPos (just for neatness)
        _camFollow.transform.localPosition = _futureCamFollowLocalPos.Center;
    }

    private void LookAt(Vector3 target)
    {
        // Preserves _camFollow position because we don't want it to rotate along with player
        _camFollow.transform.parent = null;

        // Preserves _debugFutureCamFollow position because we don't want it to rotate along with player
        if (GlobalConstants.ENABLE_CAM_FOLLOW_DEBUG)
        {
            _debugFutureCamFollow.transform.parent = null;
        }

        // WA: Prevents error "Look rotation viewing vector is zero"
        Vector3 relativePos = transform.position - target;
        if (relativePos != Vector3.zero)
        {
            // Rotates Player
            Quaternion rotation = Quaternion.LookRotation(relativePos, transform.TransformDirection(Vector3.forward));
            transform.rotation = new Quaternion(0, 0, rotation.z, rotation.w);
        }

        // Restore
        _camFollow.transform.parent = transform;
        if (GlobalConstants.ENABLE_CAM_FOLLOW_DEBUG)
        {
            _debugFutureCamFollow.transform.parent = transform;
        }
    }

    private void StateFall()
    {
        // Moves _camFollow down at CAM_FOLOW_FALL_SPEED speed
        _camFollow.transform.position = _camFollow.transform.position +
                                        Vector3.down * CAM_FOLOW_FALL_SPEED * Time.deltaTime;

        // Dispose the game after reaching _futureCamFollowLocalPos.Center.y
        if (_futureCamFollowLocalPos.Center.y >= _camFollow.transform.position.y)
        {
            // Centers _camFollow in _futureCamFollowLocalPos (just for neatness)
            _camFollow.transform.position = _futureCamFollowLocalPos.Center;

            _currentState = State.DISPOSE;
        }
    }

    private void StateDispose()
    {
        if (IsPlayerOutOfSight() && !_disposed)
        {
            _rigidbody.velocity = Vector3.zero;
            _gameManager.DisposeGame(_camFollow.transform.position);
            _disposed = true;
            _hud.Fail();
        }
    }

    private bool IsPlayerOutOfSight()
    {
        return Vector2.Distance(_camFollow.transform.position, transform.position) > PLAYER_OUT_OF_SIGHT_DISTANCE;

    }

    private void SetActiveColliders(bool value)
    {
        _hurtBox.enabled = value;
        _hitBox.enabled = value;
    }

    private void IdleAnimation()
    {
        _sleepAnimation = false;
        _faintAnimation = false;
        _changeAnimationTime = CHANGE_ANIMATION_TIME;
    }

    private void FaintAnimation()
    {
        _sleepAnimation = false;
        _faintAnimation = true;
    }

    private void StopAndCenterPlayer(Vector3 finalPosition)
    {
        // Stops Player and moves him (teleports) to finalPosition.
        // Don't use _rigidbody.MovePosition, it's unpredictable.
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.transform.position = finalPosition;

        // Preserves _camFollow position because we don't want it to rotate along with player
        _camFollow.transform.parent = null;

        // Preserves _debugFutureCamFollow position because we don't want it to rotate along with player
        if (GlobalConstants.ENABLE_CAM_FOLLOW_DEBUG)
        {
            _debugFutureCamFollow.transform.parent = null;
        }

        // Rotates Player
        transform.rotation = Quaternion.identity;

        // Restore
        _camFollow.transform.parent = transform;
        if (GlobalConstants.ENABLE_CAM_FOLLOW_DEBUG)
        {
            _debugFutureCamFollow.transform.parent = transform;
        }
    }

    private void CheckGreenPower()
    {
        if (IsCurrentPowerGreen() && !_applySlowDown)
        {
            _slowDownCount--;
            _hud.UpdatePowerUp(ObjectPooler.POWER_UP_GREEN_KEY, _slowDownCount);

            if (_slowDownCount <= 0)
            {
                PowersDown();
            }
            else
            {
                _applySlowDown = true;
            }

            _log.DebugLog(LOG_TAG, "Slow down count " + _slowDownCount, gameObject);
        }
    }

    private void CheckYellowPower()
    {
        if (IsCurrentPowerYellow() && !_applyPotion)
        {
            _potions--;
            _hud.UpdatePowerUp(ObjectPooler.POWER_UP_YELLOW_KEY, _potions);

            if (_potions <= 0)
            {
                PowersDown();
            }

            _log.DebugLog(LOG_TAG, "Potions count " + _potions, gameObject);
        }
    }

    private void CheckPinkPower()
    {
        if (IsCurrentPowerPink() && !_applyTasty)
        {
            _hud.UpdatePowerUp(ObjectPooler.POWER_UP_PINK_KEY, 0);
            PowersDown();
            _log.DebugLog(LOG_TAG, "Pink power ends for having reached another target", gameObject);
        }
    }

    private void CheckBluePower()
    {
        if (IsCurrentPowerBlue() && !_applyBomb)
        {
            _hud.UpdatePowerUp(ObjectPooler.POWER_UP_BLUE_KEY, 0);
            PowersDown();
            _log.DebugLog(LOG_TAG, "Blue power ends for having reached another target", gameObject);
        }
    }
    #endregion

    #region API
    public void LogState()
    {
        _log.DebugLog(LOG_TAG, "LogState _currentState: " + _currentState + 
                               ", _currentPowerState: " + _currentPowerState +
                               ", _gameManager.IsProcessInput " + _gameManager.IsProcessInput,
                               gameObject);
    }

    public void Awake(bool awake)
    {
        if (awake)
        {
            WakeUp();
        }
        else
        {
            _idleTime = SLEEP_ANIMATION_TIME;
            SleepTime();
        }
    }

    public void Success(TargetController targetReached)
    {
        _log.DebugLog(LOG_TAG, "Success", gameObject);

        // Updates currentTarget
        _currentTarget = targetReached.gameObject.transform.position;

        // Stops Player and moves him at the center of the target
        StopAndCenterPlayer(_currentTarget);

        GameObject nextTarget = _challengeManager.PeekNextTarget();
        if (nextTarget != null)
        {
            // The new camFollow local position is the middle point between _currentTarget and nextTarget
            _futureCamFollowLocalPos.Center = (nextTarget.transform.position - _currentTarget) / 2;
            _hud.EnableStoreButton();

            // Check powers
            CheckGreenPower();
            CheckYellowPower();
            CheckPinkPower();
            CheckBluePower();
        }
        else
        {
            // Last Challenge of the game
            PowersDown();
            _futureCamFollowLocalPos.Center = new Vector2(0, -FINAL_POSITION_Y);
            _crown.SetActive(true);
            _gameManager.GrandFinale(targetReached);
        }

        // Moves the camera
        _focusCoroutine = Focus();
        StartCoroutine(_focusCoroutine);

        _currentState = State.IDLE;

        // Sets new debug position
        if (GlobalConstants.ENABLE_CAM_FOLLOW_DEBUG)
        {
            _debugFutureCamFollow.transform.localPosition = new Vector2(_futureCamFollowLocalPos.Center.x, _futureCamFollowLocalPos.Center.y);
        }
    }

    public void Fall()
    {
        // Stops previous camera movement
        if (_focusCoroutine != null)
        {
            StopCoroutine(_focusCoroutine);
            _focusCoroutine = null;
        }

        // From now, _camFollow is not attached to the Player
        _camFollow.transform.parent = null;

        // Sets future camera position below current position
        _futureCamFollowLocalPos.Center = _camFollow.transform.position - new Vector3(0, VERTICAL_GAME_OVER_PANNING, 0);

        // Player freefall
        _rigidbody.velocity = Vector2.down * PLAYER_FALL_SPEED;
        SetActiveColliders(false);

        _currentState = State.FALL;

        // Sets new debug position
        if (GlobalConstants.ENABLE_CAM_FOLLOW_DEBUG)
        {
            _debugFutureCamFollow.transform.parent = null;
            _debugFutureCamFollow.transform.position = new Vector2(_futureCamFollowLocalPos.Center.x, _futureCamFollowLocalPos.Center.y);
        }

        // Audio SFX
        _sFX.PlayOneShot(_audioAssetManager.GetClip(AudioAssetManager.FALL_SFX));
    }

    public void RIP()
    {
        _currentState = State.DEAD;

        // If user press the exit button when Flubbie is sleeping 
        WakeUp();

        // Disables previous powers (if any)
        PowersDown();

        // Disables collision effect and activates animation
        _crashEffect.SetActive(true);
        FaintAnimation();
    }

    public void Reload()
    {
        // Shrink effect is restored on realod
        _character.transform.localScale = Vector3.one;
        _trailRenderer.startWidth = _initalTrailRendererWidth;

        // Aligns Player with _camFollow
        transform.position = new Vector3(_camFollow.transform.position.x, transform.position.y, transform.position.z);
        transform.rotation = Quaternion.identity;

        // Initializes Player
        SetActiveColliders(true);
        IdleAnimation();
        _crashEffect.SetActive(false);
        _disposed = false;

        // Sets Player as _camFollow's parent
        _camFollow.transform.parent = transform;

        // Go to next Target
        _gameManager.IsProcessInput = true;
        AimForward();

        // Sets Player as _debugFutureCamFollow's parent
        if (GlobalConstants.ENABLE_CAM_FOLLOW_DEBUG)
        {
            _debugFutureCamFollow.transform.parent = transform;
        }
    }

    public void ApplyPowerUpOrange(int targetsInARow)
    {
        // Disables previous powers (if any)
        PowersDown();

        // Apply Power Up
        _currentPowerState = PowerState.ORANGE;
        _powerOrangeEffect.SetActive(true);
        _targetsInARow = targetsInARow;

        // Audio SFX
        _sFX.PlayOneShot(_audioAssetManager.GetClip(AudioAssetManager.TAKE_ORANGE_SFX));
    }

    public void ApplyPowerUpRed(int shields)
    {
        // Disables previous powers (if any)
        PowersDown();

        // Apply Power Up
        _currentPowerState = PowerState.RED;
        _powerRedEffect.SetActive(true);
        _shields = shields;

        // Audio SFX
        _sFX.PlayOneShot(_audioAssetManager.GetClip(AudioAssetManager.TAKE_RED_SFX));
        _powerRedAudioCoroutine = PowerRedAudio();
        StartCoroutine(_powerRedAudioCoroutine);
    }

    public void ApplyPowerUpGreen(int slowDownCount)
    {
        // Disables previous powers (if any)
        PowersDown();

        // Apply Power Up
        _currentPowerState = PowerState.GREEN;
        _powerGreenEffect.SetActive(true);
        _applySlowDown = true;
        _slowDownCount = slowDownCount;

        // Audio SFX
        _sFX.PlayOneShot(_audioAssetManager.GetClip(AudioAssetManager.TAKE_GREEN_SFX));
        _powerGreenAudioCoroutine = PowerGreenAudio();
        StartCoroutine(_powerGreenAudioCoroutine);
    }

    public void ApplyPowerUpYellow(int potions)
    {
        // Disables previous powers (if any)
        PowersDown();

        // Apply Power Up
        _currentPowerState = PowerState.YELLOW;
        _powerYellowEffect.SetActive(true);
        _applyPotion = true;
        _potions = potions;

        // Audio SFX
        _sFX.PlayOneShot(_audioAssetManager.GetClip(AudioAssetManager.TAKE_YELLOW_SFX));
        _powerYellowAudioCoroutine = PowerYellowAudio();
        StartCoroutine(_powerYellowAudioCoroutine);
    }

    public void ApplyPowerUpPink(int tastyValue)
    {
        // Disables previous powers (if any)
        PowersDown();

        // Apply Power Up
        _currentPowerState = PowerState.PINK;
        _powerPinkEffect.SetActive(true);
        _applyTasty = true;
        _tastyValue = tastyValue;

        // Audio SFX
        _sFX.PlayOneShot(_audioAssetManager.GetClip(AudioAssetManager.TAKE_PINK_SFX));
        _powerPinkFinalizeCoroutine = PowerPinkFinalize();
        StartCoroutine(_powerPinkFinalizeCoroutine);
    }

    public void ApplyPowerUpBlue(int bombCount)
    {
        // Disables previous powers (if any)
        PowersDown();

        // Apply Power Up
        _currentPowerState = PowerState.BLUE;
        _powerBlueEffect.SetActive(true);
        _applyBomb = true;
        _bombCount = bombCount;

        // Audio SFX
        _sFX.PlayOneShot(_audioAssetManager.GetClip(AudioAssetManager.SCARY_SFX));
        _powerBlueFinalizeCoroutine = PowerBlueFinalize();
        StartCoroutine(_powerBlueFinalizeCoroutine);
    }

    public void TakeCandy()
    {
        _sFX.PlayOneShot(_crunchClips[Random.Range(0, _crunchClips.Count)]);
    }

    public void AimBackward()
    {
        _log.DebugLog(LOG_TAG, "AimBackward _currentState: " + _currentState + 
                               ", _currentPowerState: " + _currentPowerState +
                               ", _gameManager.IsProcessInput " + _gameManager.IsProcessInput,
                               gameObject);

        _sFX.PlayOneShot(_audioAssetManager.GetClip(AudioAssetManager.BOING_SFX));
        _currentState = State.AIM_BACKWARD;
        _shields--;
        _hud.UpdatePowerUp(ObjectPooler.POWER_UP_RED_KEY, _shields);

        _log.DebugLog(LOG_TAG, "Shields count " + _shields, gameObject);
    }

    public bool NoPower()
    {
        return _currentPowerState == PowerState.NORMAL;
    }

    public bool IsCurrentPowerOrange()
    {
        return _currentPowerState == PowerState.ORANGE;
    }

    public bool IsCurrentPowerRed()
    {
        return _currentPowerState == PowerState.RED;
    }

    public bool IsCurrentPowerGreen()
    {
        return _currentPowerState == PowerState.GREEN;
    }

    public bool IsCurrentPowerYellow()
    {
        return _currentPowerState == PowerState.YELLOW;
    }

    public bool IsCurrentPowerPink()
    {
        return _currentPowerState == PowerState.PINK;
    }

    public bool IsCurrentPowerBlue()
    {
        return _currentPowerState == PowerState.BLUE;
    }

    public bool IsDead()
    {
        return _currentState == State.DEAD ||
               _currentState == State.FALL ||
               _currentState == State.DISPOSE;
    }

    public bool IsIdle()
    {
        return _currentState == State.IDLE;
    }

    public bool IsAimForward()
    {
        return _currentState == State.AIM_FORWARD;
    }

    public bool IsJumpForward()
    {
        return _currentState == State.JUMP_FORWARD;
    }

    public bool IsAimBackward()
    {
        return _currentState == State.AIM_BACKWARD;
    }

    public bool IsJumpBackward()
    {
        return _currentState == State.JUMP_BACKWARD;
    }

    public void PauseAnimations(bool pause)
    {
        AnimatorUpdateMode animatorUpdateMode = pause ? AnimatorUpdateMode.Normal : AnimatorUpdateMode.UnscaledTime;
        _characterAnimator.updateMode = animatorUpdateMode;
        _crownAnimator.updateMode = animatorUpdateMode;
    }

    public void FireworkRandomAudio()
    {
        _sFX.PlayOneShot(_fireworkClips[Random.Range(0, _fireworkClips.Count)], FIREWORKS_VOLUME_SCALE);
    }
    #endregion
}
