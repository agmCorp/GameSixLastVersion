using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GameManager : MonoBehaviour
{
    #region Public Constants
    public const string TAG_GAME_MANAGER = "GameManager";
    #endregion

    #region Private Constants
    private const string LOG_TAG = "GameManager";
    private const string TAG_GLOBAL_VOLUME = "GlobalVolume";
    private const float RELOAD_OFFSET = 10.0f;
    private const float GAME_OVER_PAUSE_TIME = 1.0f;
    private const float DEFALUT_FIXED_DELTA_TIME = 0.02f;
    private const float SLOW_DOWN_FACTOR = 0.02f;
    private const float SLOW_DOWN_LENGHT = 2.0f;
    private const float PARTICLE_MIN_OFFSET_X = -2.0f;
    private const float PARTICLE_MAX_OFFSET_X = 2.0f;
    private const float PARTICLE_MIN_OFFSET_Y = -4.0f;
    private const float PARTICLE_MAX_OFFSET_Y = 4.0f;
    private const float PARTICLE_MAX_DELAY = 0.2f;
    private const float GOAL_BOUNCE_DELAY = 3.0f;
    private const float STAGE_CLEARED_OFFSET_Y = 2.3f;
    private const float PRESS_BACK_OFFSET_Y = -2.3f;
    private const string OBJ_FIREWORKS_NAME = "Fireworks";
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();

    [SerializeField] private GameObject _wanderingSpirits = null;
    [SerializeField] private GameObject _stageCleared = null;
    [SerializeField] private GameObject _pressBack = null;

    private ObjectPooler _objectPooler;
    private ChallengeManager _challengeManager;
    private ILightManager _lightManager;
    private Vector3 _lastCamPosition;
    private Hud _hud;
    private PlayerController _playerController;
    private Volume _volume;
    private List<string> _fireworkKeyList;
    private ColorCurves _colorCurves;
    private bool _slowMotion;
    private bool _pause;
    private bool _grandFinale;
    private bool _processInput;
    private IEnumerator _fireworksCoroutine;
    private IEnumerator _goalBounceCoroutine;
    private GameObject _fireworks;
    #endregion

    #region Properties
    public bool IsGamePaused
    {
        get { return _pause; }
    }
    public bool IsGrandFinale
    {
        get { return _grandFinale; }
    }
    public bool IsProcessInput
    {
        get { return _processInput; }
        set { _processInput = value; }
    }
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        _challengeManager = GameObject.FindGameObjectWithTag(ChallengeManager.TAG_CHALLENGE_MANAGER).GetComponent<ChallengeManager>();
        _hud = GameObject.FindGameObjectWithTag(Hud.TAG_HUD).GetComponent<Hud>();
        _playerController = GameObject.FindGameObjectWithTag(PlayerController.TAG_PLAYER).GetComponent<PlayerController>();
        _volume = GameObject.FindGameObjectWithTag(TAG_GLOBAL_VOLUME).GetComponent<Volume>();
        _fireworkKeyList = new List<string>
        {
            { ObjectPooler.FIREWORK_01_KEY },
            { ObjectPooler.FIREWORK_02_KEY },
            { ObjectPooler.FIREWORK_03_KEY },
            { ObjectPooler.FIREWORK_04_KEY }
        };
    }

    private void Start()
    {
        _objectPooler = ObjectPooler.GetInstance();
        _volume.profile.TryGet(out _colorCurves);

        // Dinamic component. See GlobalLight.cs
        _lightManager = GameObject.FindGameObjectWithTag(GlobalConstants.TAG_LIGHTS).GetComponent<ILightManager>();
    }

    private void Update()
    {
        if (_slowMotion)
        {
            Time.timeScale += (1.0f / SLOW_DOWN_LENGHT) * Time.unscaledDeltaTime;
            Time.timeScale = Mathf.Clamp(Time.timeScale, 0.0f, 1.0f);
            Time.fixedDeltaTime = Time.timeScale * DEFALUT_FIXED_DELTA_TIME;
            _slowMotion = (Time.timeScale != 1);
        }
    }
    #endregion

    #region Utils
    private GameObject GetFireworkFromPool(string fireworkKey, Vector3 pos)
    {
        return _objectPooler.SpawnFromPool(fireworkKey, pos, Quaternion.identity); ;
    }

    private void ReturnFireworkToPool(GameObject firework)
    {
        _objectPooler.ReturnToPool(firework);
    }

    private string GetRandomFireworkKey()
    {
        return _fireworkKeyList[Random.Range(0, _fireworkKeyList.Count)];
    }

    private IEnumerator FinalizeGame()
    {
        _lightManager.PublicInit();
        BlackAndWhiteEffect(true);
        _hud.PlayHitSound();
        Pause(true);

        yield return new WaitForSecondsRealtime(GAME_OVER_PAUSE_TIME);

        _hud.PlayGameOverMusic();
        Pause(false);
        DoSlowMotion();
        _playerController.Fall();
    }

    private IEnumerator Fireworks(Vector3 posPlayer)
    {
        _fireworks = new GameObject(OBJ_FIREWORKS_NAME);
        _fireworks.transform.position = posPlayer;

        Vector3 pos;
        GameObject firework;
        while (true)
        {
            pos = posPlayer;
            pos.x += Random.Range(PARTICLE_MIN_OFFSET_X, PARTICLE_MAX_OFFSET_X);
            pos.y += Random.Range(PARTICLE_MIN_OFFSET_Y, PARTICLE_MAX_OFFSET_Y);
            firework = GetFireworkFromPool(GetRandomFireworkKey(), pos);
            firework.transform.parent = _fireworks.transform;
            _playerController.FireworkRandomAudio();
            yield return new WaitForSeconds(Random.Range(0.0f, PARTICLE_MAX_DELAY));
        }
    }

    private IEnumerator GoalBounce(TargetController goal)
    {
        while (true)
        {
            goal.BounceAnimation();
            yield return new WaitForSeconds(GOAL_BOUNCE_DELAY);
        }
    }
    #endregion

    #region API
    public void BuyPower(string powerKey, int powerParam)
    {
        _playerController.Awake(true);
        
        switch (powerKey)
        {
            case ObjectPooler.POWER_UP_RED_KEY:
                _playerController.ApplyPowerUpRed(powerParam);
                break;

            case ObjectPooler.POWER_UP_ORANGE_KEY:
                _playerController.ApplyPowerUpOrange(powerParam);
                break;

            case ObjectPooler.POWER_UP_GREEN_KEY:
                _playerController.ApplyPowerUpGreen(powerParam);
                break;

            case ObjectPooler.POWER_UP_YELLOW_KEY:
                _playerController.ApplyPowerUpYellow(powerParam);
                break;

            default:
                _log.DebugLogError(LOG_TAG, "Power not for sale", gameObject);
                break;
        }
    }

    public bool IsGameOver()
    {
        return _playerController.IsDead();
    }

    public void GameOver()
    {
        _hud.DisablePauseButton();
        _hud.DisableStoreButton();
        _playerController.RIP();
        StartCoroutine(FinalizeGame());
        _log.DebugLog(LOG_TAG, "Game Over", gameObject);
    }

    public void DisposeGame(Vector3 camPosition)
    {
        _lastCamPosition = camPosition;
        _challengeManager.Dispose();
        _log.DebugLog(LOG_TAG, "Game disposed", gameObject);
    }

    public void ReloadGame()
    {
        BlackAndWhiteEffect(false);
        int level = _challengeManager.Level;
        Vector3 pole = new Vector3(_lastCamPosition.x, _lastCamPosition.y + RELOAD_OFFSET, _lastCamPosition.z);
        _challengeManager.LoadFromLevel(level, pole);
        // The current level will be updated after the player makes contact with the target
        _challengeManager.Level = level - 1;
        _playerController.Reload();
        _hud.PlayGameMusic();
    }

    public void DoSlowMotion()
    {
        _slowMotion = true;
        Time.timeScale = SLOW_DOWN_FACTOR;
        Time.fixedDeltaTime *= Time.timeScale;
    }

    public void BlackAndWhiteEffect(bool blackAndWhiteEffect)
    {
        _colorCurves.active = blackAndWhiteEffect;
    }

    public void Pause(bool pause)
    {
        _pause = pause;
        Time.timeScale = pause ? 0 : 1;
        AudioListener.pause = pause;
    }

    public void PauseGameAnimations(bool pause)
    {
        _playerController.PauseAnimations(pause);
        _challengeManager.PauseAnimations(pause);
    }

    public void GrandFinale(TargetController goal)
    {
        _grandFinale = true;
        _lightManager.EndlessParty();
        _hud.PlayGrandFinaleMusic();

        Vector3 pos = _playerController.gameObject.transform.position;
        Instantiate(_wanderingSpirits, new Vector2(pos.x, pos.y + PARTICLE_MIN_OFFSET_Y), Quaternion.identity);
        Instantiate(_stageCleared, new Vector2(pos.x, pos.y + STAGE_CLEARED_OFFSET_Y), Quaternion.identity);
        Instantiate(_pressBack, new Vector2(pos.x, pos.y + PRESS_BACK_OFFSET_Y), Quaternion.identity);
        _fireworksCoroutine = Fireworks(pos);
        StartCoroutine(_fireworksCoroutine);
        _goalBounceCoroutine = GoalBounce(goal);
        StartCoroutine(_goalBounceCoroutine);
    }

    public void DisposeGrandFinale()
    {
        if (_grandFinale)
        {
            if (_fireworksCoroutine != null)
            {
                StopCoroutine(_fireworksCoroutine);
            }

            if (_goalBounceCoroutine != null)
            {
                StopCoroutine(_goalBounceCoroutine);
            }

            Transform fireworks = _fireworks.transform;
            while (fireworks.childCount > 0)
            {
                ReturnFireworkToPool(fireworks.GetChild(0).gameObject);
            }

            _challengeManager.Dispose();
        }
    }
    #endregion
}
