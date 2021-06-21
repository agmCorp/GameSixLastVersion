using System;
using System.Collections.Generic;
using UnityEngine;

public class ChallengeManager : MonoBehaviour
{
    #region Public Constants
    public const string TAG_CHALLENGE_MANAGER = "ChallengeManager";
    #endregion

    #region Private Json structs (GC friendly)
    /* 
     * In C#, classes are always allocated on the heap. 
     * Structs are allocated on the stack, if a local function variable, or on the heap as 
     * part of a class if a class member.
     */
#pragma warning disable 0649 // Field 'field' is never assigned to, and will always have its default value 'value'
    [Serializable]
    private struct JsonSwingConfig
    {
        public bool enabled;
        public float minRadius;
        public float maxRadius;
        public float swingingSpeed;
        public float swingDuration;
        public float pauseDuration;
    }

    [Serializable]
    private struct JsonSpeedConfig
    {
        public float initialSpeed;
        public bool clockwise;
        public JsonAccelerationConfig accelerationConfig;
    }

    [Serializable]
    private struct JsonAccelerationConfig
    {
        public bool enabled;
        public float maxSpeed;
        public float acceleration;
        public bool bounce;
    }

    [Serializable]
    private struct JsonPlanetConfig
    {
        public float radius;
        public string[] moonKeyList;
        public JsonSwingConfig swingConfig;
        public JsonSpeedConfig speedConfig;
    }

    [Serializable]
    private struct JsonPolar
    {
        public float rho;
        public float phi;
    }

    [Serializable]
    private struct JsonFleeConfig
    {
        public float fleeSpeed;
        public float shrinkTime;
    }

    [Serializable]
    private struct JsonChallengeConfig
    {
        public bool loaded;
        public JsonPolar polarCoords;
        public JsonFleeConfig fleeConfig;
        public JsonPlanetConfig[] planetConfigList;
    }
#pragma warning restore 0649 // Field 'field' is never assigned to, and will always have its default value 'value'
    #endregion

    #region Private structs
    private struct Challenge
    {
        public bool Loaded { get; set; }
        public bool IsLevel { get; set; }
        public string ChallengeName { get; set; }
        public float DisposePlanetsAfter { get; set; }
        public GameObject Target { get; set; }
        public List<GameObject> PlanetList { get; set; }
    }

    private struct LevelInfo
    {
        public int DebugId { get; set; }
        public float Rho { get; set; }
        public float Phi { get; set; }
    }
    #endregion

    #region Private Constants
    private const string LOG_TAG = "ChallengeManager";
    private const string LEVEL_FLAG = "*";
    private const string LEVEL_NAME = "Level";
    private const string LEVEL_DEBUG_ID = "DebugId";
    private const string PLANET_NAME = "Planet";
    private const string MOON_NAME = "Moon";
    private const int MAX_TARGETS_AHEAD = 3;
    private const int MAX_TARGETS_BEHIND = 3;
    private const float INITIAL_POLE_Y = 10.0f;
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();
    private readonly GameMap _gameMap = GameMap.GetInstance();
    private readonly GPGS _gpgs = GPGS.GetInstance();

    private AudioAssetManager _audioAssetManager;
    private ILightManager _lightManager;
    private Hud _hud;
    private AudioSource _sFX;
    private ObjectPooler _objectPooler;
    private int _levelCount;
    private Queue<Challenge> _challengeQueue;
    private Queue<Challenge> _challengeDeallocatedQueue;
    private Queue<string> _challengeTokenQueue;
    private Queue<GameObject> _targetDeallocatedQueue;
    private Dictionary<int, LevelInfo> _levelInfoDictionary;
    private List<AudioClip> _targetReachedClips;
    private float _deviceWidth;
    private float _deviceHeight;
    private int _currentLevel;
    private Vector3 _lastPole;
    #endregion

    #region Properties
    public int Level
    {
        get { return _currentLevel; }
        set { _currentLevel = value; }
    }
    #endregion

    #region MonoBehaviour
    private void Awake()
    {
        _hud = GameObject.FindGameObjectWithTag(Hud.TAG_HUD).GetComponent<Hud>();
        _sFX = GetComponent<AudioSource>();

        _challengeQueue = new Queue<Challenge>();
        _challengeDeallocatedQueue = new Queue<Challenge>();
        _challengeTokenQueue = new Queue<string>();
        _targetDeallocatedQueue = new Queue<GameObject>();
        _levelInfoDictionary = new Dictionary<int, LevelInfo>();
    }

    private void Start()
    {
        // Audio and Clips
        _audioAssetManager = AudioAssetManager.GetInstance();
        _targetReachedClips = new List<AudioClip>
        {
            _audioAssetManager.GetClip(AudioAssetManager.YAY_SFX),
            _audioAssetManager.GetClip(AudioAssetManager.YIPEE_SFX),
            _audioAssetManager.GetClip(AudioAssetManager.YES_SFX),
            _audioAssetManager.GetClip(AudioAssetManager.YAMMY_SFX)
        };

        _objectPooler = ObjectPooler.GetInstance();
        AspectRatio();
        if (_gameMap.IsMapLoaded())
        {
            _levelCount = _gameMap.GetLevelCount();
            _currentLevel = PlayerPrefs.GetInt(GlobalConstants.PREF_FROM_LEVEL) - 1;
            Vector3 pole = new Vector3(0, INITIAL_POLE_Y, 0);
            LoadFromLevel(_currentLevel + 1, pole);
        }
        
        // Dinamic component. See GlobalLight.cs
        _lightManager = GameObject.FindGameObjectWithTag(GlobalConstants.TAG_LIGHTS).GetComponent<ILightManager>();
    }
    #endregion

    #region Utils
    private void AspectRatio()
    {
        // The vertical area of the game will always fit to the screen height.
        // And as the screen aspect ratio changes there will be extra space added to the left and right of the screen. 
        // It's the default behaviour of Unity camera.
        Camera cam = Camera.main;
        float aspectRatio = (cam.pixelWidth / GlobalConstants.PPM) / (cam.pixelHeight / GlobalConstants.PPM);
        _deviceWidth = aspectRatio * GlobalConstants.TARGET_APP_HEIGHT_PIXELS / GlobalConstants.PPM;
        _deviceHeight = GlobalConstants.TARGET_APP_HEIGHT_PIXELS / GlobalConstants.PPM;

        // Margin to prevent half of the player/target from leaving the screen 
        _deviceWidth -= GlobalConstants.TARGET_APP_PADDING_PIXELS / GlobalConstants.PPM;
        _deviceHeight -= GlobalConstants.TARGET_APP_PADDING_PIXELS / GlobalConstants.PPM;
    }

    private JsonChallengeConfig LoadJsonChallengeConfigFromFile(string challengeFilename)
    {
        JsonChallengeConfig jsonChallengeConfig = new JsonChallengeConfig();
        TextAsset txt = (TextAsset)Resources.Load(challengeFilename, typeof(TextAsset));
        if (txt != null)
        {
            jsonChallengeConfig = JsonUtility.FromJson<JsonChallengeConfig>(txt.text);
            jsonChallengeConfig.loaded = true;
        }
        else
        {
            jsonChallengeConfig.loaded = false;
        }
        return jsonChallengeConfig;
    }

    private Challenge LoadChallenge(JsonChallengeConfig jsonChallengeConfig, bool isLevel, int levelNumber, string challengeName, Vector3 pole)
    {
        Challenge challenge = new Challenge();
        challenge.Loaded = true;
        challenge.IsLevel = isLevel;
        challenge.ChallengeName = challengeName;
        challenge.DisposePlanetsAfter = jsonChallengeConfig.fleeConfig.shrinkTime;
        
        // Target
        Vector3 position = new Vector3(pole.x + jsonChallengeConfig.polarCoords.rho, pole.y, pole.z);
        challenge.Target = GetTargetFromPool(ObjectPooler.TARGET_KEY, position);
        challenge.Target.transform.RotateAround(pole, Vector3.back, NormalizeAngle(jsonChallengeConfig.polarCoords.phi));
        challenge.Target.transform.rotation = Quaternion.identity;
        challenge.Target.transform.position = ClampToDeviceScreen(pole, challenge.Target.transform.position);
        challenge.Target.transform.parent = gameObject.transform;
        TargetController target = challenge.Target.GetComponent<TargetController>();
        target.PublicInit(challengeName, pole, isLevel, levelNumber, _levelCount);

        // Planets
        List<GameObject> planetList = new List<GameObject>();
        int planetCount = 0;
        int fromSortingOrder = 0;
        foreach (JsonPlanetConfig jsonPlanetConfig in jsonChallengeConfig.planetConfigList)
        {
            PlanetController planetController = LoadPlanet(jsonPlanetConfig,
                                                challenge.Target.transform,
                                                PLANET_NAME +
                                                GlobalConstants.NAME_SEPARATOR + planetCount,
                                                MOON_NAME +
                                                GlobalConstants.NAME_SEPARATOR,
                                                jsonChallengeConfig.fleeConfig.fleeSpeed,
                                                jsonChallengeConfig.fleeConfig.shrinkTime,
                                                fromSortingOrder);
            planetList.Add(planetController.gameObject);
            planetCount++;
            fromSortingOrder -= planetController.MoonCount;
        }
        challenge.PlanetList = planetList;

        return challenge;
    }

    // Keeps targetPosition inside the screen 
    private Vector2 ClampToDeviceScreen(Vector3 pole, Vector3 targetPosition)
    {
        float clampX = targetPosition.x;
        float clampY = targetPosition.y;

        if (clampX > pole.x)
        {
            if (clampX > pole.x + _deviceWidth)
            {
                clampX = pole.x + _deviceWidth;
            }
        }
        else
        {
            if (clampX < pole.x - _deviceWidth)
            {
                clampX = pole.x - _deviceWidth;
            }
        }

        if (clampY > pole.y)
        {
            if (clampY > pole.y + _deviceHeight)
            {
                clampY = pole.y + _deviceHeight;
            }
        }
        else
        {
            if (clampY < pole.y - _deviceHeight)
            {
                clampY = pole.y - _deviceHeight;
            }

        }

        return new Vector3(clampX, clampY, targetPosition.z);
    }

    // Clockwise from 0 (left) to 360
    private float NormalizeAngle(float degrees)
    {
        return 180 + degrees;
    }

    private PlanetController LoadPlanet(JsonPlanetConfig jsonPlanetConfig, Transform parent, string planetName,
                                  string moonName, float fleeSpeed, float shrinkTime, int fromSortingOrder)
    {
        Queue<string> moonKeyQueue = new Queue<string>(jsonPlanetConfig.moonKeyList);
        GameObject obj = GetPlanetFromPool(ObjectPooler.PLANET_KEY, parent.position, jsonPlanetConfig.radius, moonKeyQueue, planetName, moonName, fromSortingOrder);
        obj.transform.parent = parent;
        PlanetController planetController = obj.GetComponent<PlanetController>();

        if (jsonPlanetConfig.speedConfig.accelerationConfig.enabled)
        {
            planetController.StartOrbiting(jsonPlanetConfig.speedConfig.initialSpeed,
                                           jsonPlanetConfig.speedConfig.clockwise,
                                           jsonPlanetConfig.speedConfig.accelerationConfig.maxSpeed,
                                           jsonPlanetConfig.speedConfig.accelerationConfig.acceleration,
                                           jsonPlanetConfig.speedConfig.accelerationConfig.bounce);
        }
        else
        {
            planetController.StartOrbiting(jsonPlanetConfig.speedConfig.initialSpeed, jsonPlanetConfig.speedConfig.clockwise);
        }

        if (jsonPlanetConfig.swingConfig.enabled)
        {
            if (jsonPlanetConfig.swingConfig.swingDuration > 0 && jsonPlanetConfig.swingConfig.pauseDuration > 0)
            {
                planetController.StartSwingingWithPause(jsonPlanetConfig.swingConfig.minRadius,
                                                        jsonPlanetConfig.swingConfig.maxRadius,
                                                        jsonPlanetConfig.swingConfig.swingingSpeed,
                                                        jsonPlanetConfig.swingConfig.swingDuration,
                                                        jsonPlanetConfig.swingConfig.pauseDuration);
            }
            else
            {
                planetController.StartSwinging(jsonPlanetConfig.swingConfig.minRadius,
                                               jsonPlanetConfig.swingConfig.maxRadius,
                                               jsonPlanetConfig.swingConfig.swingingSpeed);
            }
        }

        planetController.SetFlee(fleeSpeed, shrinkTime);

        return planetController;
    }

    private GameObject GetPlanetFromPool(string planetKey, Vector3 position, float radius, Queue<string> moonKeyQueue,
                                         string planetName, string moonName, int fromSortingOrder)
    {
        GameObject planet = _objectPooler.SpawnFromPool(planetKey, position, Quaternion.identity);
        planet.GetComponent<PlanetController>().PublicInit(radius, moonKeyQueue, planetName, moonName, fromSortingOrder);
        return planet;
    }

    private void ReturnPlanetToPool(GameObject planet)
    {
        planet.GetComponent<PlanetController>().Dispose();
        _objectPooler.ReturnToPool(planet);
    }

    private GameObject GetTargetFromPool(string targetKey, Vector3 position)
    {
        GameObject target = _objectPooler.SpawnFromPool(targetKey, position, Quaternion.identity);
        return target;
    }

    private void ReturnTargetToPool(GameObject target)
    {
        target.GetComponent<TargetController>().Dispose();
        _objectPooler.ReturnToPool(target);
    }

    private void DisposePlanets()
    {
        Challenge challenge = _challengeDeallocatedQueue.Dequeue();
        foreach (GameObject obj in challenge.PlanetList)
        {
            ReturnPlanetToPool(obj);
        }
    }

    private void DisposeTargets()
    {
        if (PeekNextTarget() != null)
        {
            // We add one to MAX_TARGETS_BEHIND because we don't want to count the target we are in.
            if (_targetDeallocatedQueue.Count > MAX_TARGETS_BEHIND + 1)
            {
                ReturnTargetToPool(_targetDeallocatedQueue.Dequeue());
            }
        }
        else
        {
            // Releases every previous Targets except the last one
            while (_targetDeallocatedQueue.Count > 1)
            {
                ReturnTargetToPool(_targetDeallocatedQueue.Dequeue());
            }

            // Removes dotted line from the last one
            _targetDeallocatedQueue.Peek().GetComponent<TargetController>().RemoveDottedLines();
        }
    }

    private void ReleaseResources(Challenge challenge)
    {
        _challengeDeallocatedQueue.Enqueue(challenge);
        Invoke(nameof(DisposePlanets), challenge.DisposePlanetsAfter);

        // Targets aren't released in sync with planets
        _targetDeallocatedQueue.Enqueue(challenge.Target);
        DisposeTargets();
    }

    private Challenge LoadNextChallenge(string challengeToken, Vector3 pole)
    {
        Challenge challenge = new Challenge();
        bool isLevel;
        JsonChallengeConfig jsonChallengeConfig;
        int levelNumber = -1;
        LevelInfo levelInfo;

        // A level is a fake challenge
        isLevel = IsLevel(challengeToken);
        if (isLevel)
        {
            jsonChallengeConfig = new JsonChallengeConfig();
            levelNumber = GetLevelNumber(challengeToken);
            if (levelNumber > 0)
            {
                levelInfo = _levelInfoDictionary[levelNumber];
                jsonChallengeConfig.loaded = true;
                jsonChallengeConfig.polarCoords = new JsonPolar
                {
                    rho = levelInfo.Rho,
                    phi = levelInfo.Phi
                };
                jsonChallengeConfig.fleeConfig = new JsonFleeConfig();
                jsonChallengeConfig.planetConfigList = new JsonPlanetConfig[0];

                challengeToken = LEVEL_NAME + GlobalConstants.NAME_SEPARATOR + levelNumber + 
                                 GlobalConstants.NAME_SEPARATOR +
                                 LEVEL_DEBUG_ID + GlobalConstants.NAME_SEPARATOR + levelInfo.DebugId;
            }
            else
            {
                jsonChallengeConfig.loaded = false;
            }
        }
        else
        {
            // Regular Challange
            jsonChallengeConfig = LoadJsonChallengeConfigFromFile(challengeToken);
        }

        // Finally loads the Challenge
        if (jsonChallengeConfig.loaded)
        {
            challenge = LoadChallenge(jsonChallengeConfig, isLevel, levelNumber, challengeToken, pole);
            _challengeQueue.Enqueue(challenge);

            _log.DebugLog(LOG_TAG, "Challenge '" + challenge.ChallengeName + "' loaded.", gameObject);
        }

        return challenge;
    }

    private bool IsLevel(string challengeName)
    {
        return challengeName.IndexOf(LEVEL_FLAG + GlobalConstants.NAME_SEPARATOR) >= 0;
    }

    private int GetLevelNumber(string levelName)
    {
        int levelNumber;
        string[] values = levelName.Split(GlobalConstants.NAME_SEPARATOR);
        bool isParsableLevelNumber = Int32.TryParse(values[1], out levelNumber);
        return isParsableLevelNumber ? levelNumber : -1;
    }

    private void LoadPath(int fromlevel)
    {
        // Clears queues
        _levelInfoDictionary.Clear();
        _challengeTokenQueue.Clear();

        for (int i = fromlevel; i <= _levelCount; i++)
        {
            GameMap.JsonLevelConfig jsonLevelConfig = _gameMap.GetJsonLevelConfig(i - 1);

            // A level is a fake challenge
            _challengeTokenQueue.Enqueue(LEVEL_FLAG + GlobalConstants.NAME_SEPARATOR + i);

            // The level information is stored for later reference 
            LevelInfo levelInfo = new LevelInfo
            {
                DebugId = jsonLevelConfig.levelDebugId,
                Rho = jsonLevelConfig.polarCoords.rho,
                Phi = jsonLevelConfig.polarCoords.phi
            };
            _levelInfoDictionary.Add(i, levelInfo);

            // Challenges of the level
            foreach (string challengeFilename in jsonLevelConfig.challengeFilenameList)
            {
                _challengeTokenQueue.Enqueue(challengeFilename);
            }
        }

        _log.DebugLog(LOG_TAG, "Path loaded from level " + fromlevel + " to " +  _levelCount + ".", gameObject);
    }

    private void LoadFirstChallenges(int fromLevel, Vector3 pole)
    {
        int i = fromLevel;
        int n = i + MAX_TARGETS_AHEAD;
        while (_challengeTokenQueue.Count > 0 && (i < n))
        {
            Challenge challenge = LoadNextChallenge(_challengeTokenQueue.Dequeue(), pole);
            if (challenge.Loaded)
            {
                pole = challenge.Target.transform.position;
            }
            i++;
        }
        _lastPole = pole;
    }

    private void RemoveFirstDottedLine()
    {
        GameObject target = PeekNextTarget();
        if (target != null)
        {
            target.GetComponent<TargetController>().RemoveDottedLines();
        }
    }

    private void ManageLevel(Challenge challengeCompleted)
    {
        // Increases _currentLevel if we reach a level challenge
        if (challengeCompleted.IsLevel)
        {
            // Audio SFX
            _sFX.PlayOneShot(_audioAssetManager.GetClip(AudioAssetManager.VICTORY_SFX));

            _currentLevel++;

            int highLevel = PlayerPrefs.GetInt(GlobalConstants.PREF_HIGH_LEVEL);
            if (_currentLevel > highLevel)
            {
                PlayerPrefs.SetInt(GlobalConstants.PREF_HIGH_LEVEL, _currentLevel);

                // Synchronizes score
                if (_gpgs.IsAuthenticated()) { 
                    _gpgs.PostHighLevel(_currentLevel);
                }
            }

            if (_currentLevel == 1)
            {
                _hud.ShowHelp();
            }
            else
            {
               _lightManager.Party();
            }
            _hud.ShowLevel(_currentLevel, _levelCount);

            _log.DebugLog(LOG_TAG, "Current level: " + _currentLevel + ".", gameObject);
        }
        else
        {
            // Audio SFX
            _sFX.PlayOneShot(_targetReachedClips[UnityEngine.Random.Range(0, _targetReachedClips.Count)]);
        }
    }

    private void SleepAnimation(Challenge challenge)
    {
        foreach (GameObject obj in challenge.PlanetList)
        {
            PlanetController planetController = obj.GetComponent<PlanetController>();
            planetController.SleepAnimation();
        }
    }

    private void IdleAnimation(Challenge challenge)
    {
        foreach (GameObject obj in challenge.PlanetList)
        {
            PlanetController planetController = obj.GetComponent<PlanetController>();
            planetController.IdleAnimation();
        }
    }

    private void SlowDown(Challenge challenge)
    {
        foreach (GameObject obj in challenge.PlanetList)
        {
            PlanetController planetController = obj.GetComponent<PlanetController>();
            planetController.SlowDown();
        }
    }

    private void PauseAnimations(Challenge challenge, bool pause)
    {
        foreach (GameObject obj in challenge.PlanetList)
        {
            PlanetController planetController = obj.GetComponent<PlanetController>();
            planetController.PauseAnimations(pause);
        }
    }
    #endregion

    #region API
    public GameObject PeekNextTarget()
    {
        return _challengeQueue.Count > 0 ? _challengeQueue.Peek().Target : null;
    }

    public void LoadFromLevel(int level, Vector3 pole)
    {
        LoadPath(level);
        LoadFirstChallenges(level, pole);
        RemoveFirstDottedLine();
    }

    public void ChallengeCompleted()
    {
        _log.DebugLog(LOG_TAG, "ChallengeManager.ChallengeCompleted", gameObject);

        // Gets the Challenge completed
        Challenge challengeCompleted = _challengeQueue.Dequeue();

        // Starts "Flee" state on each Planet
        foreach (GameObject obj in challengeCompleted.PlanetList)
        {
            PlanetController planetController = obj.GetComponent<PlanetController>();
            planetController.StartFlee();
        }

        // Take actions if we reach a level challenge
        ManageLevel(challengeCompleted);

        // Releases resources (Planets and Targets)
        ReleaseResources(challengeCompleted);

        // There are still Challenges to load
        if (_challengeTokenQueue.Count > 0)
        {
            // For performance reasons we mustn't exceed the maximum allowed
            if (_challengeQueue.Count < MAX_TARGETS_AHEAD)
            {
                // Loads a new Challenge from file
                Challenge newChallenge = LoadNextChallenge(_challengeTokenQueue.Dequeue(), _lastPole);
                if (newChallenge.Loaded)
                {
                    _lastPole = newChallenge.Target.transform.position;
                }
            }
        }
        else
        {
            _log.DebugLog(LOG_TAG, "Map is completed.", gameObject);
        }
    }

    public void SleepAnimation()
    {
        Challenge challenge;
        for (int i = 0; i < _challengeQueue.Count; i++)
        {
            challenge = _challengeQueue.Dequeue();
            SleepAnimation(challenge);
            _challengeQueue.Enqueue(challenge);
        }
    }

    public void IdleAnimation()
    {
        Challenge challenge;
        for (int i = 0; i < _challengeQueue.Count; i++)
        {
            challenge = _challengeQueue.Dequeue();
            IdleAnimation(challenge);
            _challengeQueue.Enqueue(challenge);
        }
    }

    public void SlowDown()
    {
        if (_challengeQueue.Count > 0)
        {
            SlowDown(_challengeQueue.Peek());
        }
    }

    public void Dispose()
    {
        // Releases every Challenge
        Challenge challenge;
        while (_challengeQueue.Count > 0)
        {
            challenge = _challengeQueue.Dequeue();
            foreach (GameObject obj in challenge.PlanetList)
            {
                ReturnPlanetToPool(obj);
            }
            ReturnTargetToPool(challenge.Target);
        }

        // Releases every previous Targets
        while (_targetDeallocatedQueue.Count > 0)
        {
            ReturnTargetToPool(_targetDeallocatedQueue.Dequeue());
        }

        _log.DebugLog(LOG_TAG, "Map Disposed", gameObject);
    }

    public void PauseAnimations(bool pause)
    {
        Challenge challenge;
        for (int i = 0; i < _challengeQueue.Count; i++)
        {
            challenge = _challengeQueue.Dequeue();
            PauseAnimations(challenge, pause);
            _challengeQueue.Enqueue(challenge);
        }

        for (int i = 0; i < _challengeDeallocatedQueue.Count; i++)
        {
            challenge = _challengeDeallocatedQueue.Dequeue();
            PauseAnimations(challenge, pause);
            _challengeDeallocatedQueue.Enqueue(challenge);
        }
    }
    #endregion
}
