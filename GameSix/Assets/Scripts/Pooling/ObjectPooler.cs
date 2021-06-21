using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour
{
    #region Public Constants
    public const string PLANET_KEY = "PLANET";
    public const string TARGET_KEY = "TARGET";
    public const string NUMBER_KEY = "NUMBER";

    public const string SMALL_BLUE_KEY = "SMALL_BLUE";
    public const string SMALL_GREEN_KEY = "SMALL_GREEN";
    public const string SMALL_ORANGE_KEY = "SMALL_ORANGE";
    public const string SMALL_PINK_KEY = "SMALL_PINK";
    public const string SMALL_YELLOW_KEY = "SMALL_YELLOW";

    public const string MEDIUM_BLUE_KEY = "MEDIUM_BLUE";
    public const string MEDIUM_GREEN_KEY = "MEDIUM_GREEN";
    public const string MEDIUM_ORANGE_KEY = "MEDIUM_ORANGE";
    public const string MEDIUM_PINK_KEY = "MEDIUM_PINK";
    public const string MEDIUM_YELLOW_KEY = "MEDIUM_YELLOW";

    public const string BIG_BLUE_KEY = "BIG_BLUE";
    public const string BIG_GREEN_KEY = "BIG_GREEN";
    public const string BIG_ORANGE_KEY = "BIG_ORANGE";
    public const string BIG_PINK_KEY = "BIG_PINK";
    public const string BIG_YELLOW_KEY = "BIG_YELLOW";

    public const string POWER_UP_RED_KEY = "POWER_UP_RED";
    public const string POWER_UP_ORANGE_KEY = "POWER_UP_ORANGE";
    public const string POWER_UP_GREEN_KEY = "POWER_UP_GREEN";
    public const string POWER_UP_YELLOW_KEY = "POWER_UP_YELLOW";
    public const string POWER_UP_PINK_KEY = "POWER_UP_PINK";
    public const string POWER_UP_BLUE_KEY = "POWER_UP_BLUE";

    public const string CANDY_KEY = "CANDY";

    public const string FIREWORK_01_KEY = "FIREWORK_01";
    public const string FIREWORK_02_KEY = "FIREWORK_02";
    public const string FIREWORK_03_KEY = "FIREWORK_03";
    public const string FIREWORK_04_KEY = "FIREWORK_04";
    #endregion

    #region Private Constants
    private const string LOG_TAG = "ObjectPooler";
    private const int INITIAL_POOLSIZE_PLANET = 3;
    private const int INITIAL_POOLSIZE_TARGET = 3;
    private const int INITIAL_POOLSIZE_NUMBER = 3;

    private const int INITIAL_POOLSIZE_SMALL_MOON = 3;
    private const int INITIAL_POOLSIZE_MEDIUM_MOON = 3;
    private const int INITIAL_POOLSIZE_BIG_MOON = 3;

    private const int INITIAL_POOLSIZE_COLLECTIBLE = 3;

    private const int INITIAL_POOLSIZE_FIREWORKS = 3;
    #endregion

    #region Private structs (GC friendy)
    private struct Pool
    {
        public string Key { get; set; }
        public GameObject Prefab { get; set; }
        public int Size { get; set; }
    }
    #endregion

    #region Private Attributes
    private readonly Logging _log = Logging.GetInstance();

    // Singleton MonoBehaviour
    private static ObjectPooler _instance;

    [SerializeField] private GameObject _prefabPlanet = null;
    [SerializeField] private GameObject _prefabTarget = null;
    [SerializeField] private GameObject _prefabNumber = null;

    [SerializeField] private GameObject _prefabSmallMoonBlue = null;
    [SerializeField] private GameObject _prefabSmallMoonGreen = null;
    [SerializeField] private GameObject _prefabSmallMoonOrange = null;
    [SerializeField] private GameObject _prefabSmallMoonPink = null;
    [SerializeField] private GameObject _prefabSmallMoonYellow = null;

    [SerializeField] private GameObject _prefabMediumMoonBlue = null;
    [SerializeField] private GameObject _prefabMediumMoonGreen = null;
    [SerializeField] private GameObject _prefabMediumMoonOrange = null;
    [SerializeField] private GameObject _prefabMediumMoonPink = null;
    [SerializeField] private GameObject _prefabMediumMoonYellow = null;

    [SerializeField] private GameObject _prefabBigMoonBlue = null;
    [SerializeField] private GameObject _prefabBigMoonGreen = null;
    [SerializeField] private GameObject _prefabBigMoonOrange = null;
    [SerializeField] private GameObject _prefabBigMoonPink = null;
    [SerializeField] private GameObject _prefabBigMoonYellow = null;

    [SerializeField] private GameObject _prefabPowerUpRed = null;
    [SerializeField] private GameObject _prefabPowerUpOrange = null;
    [SerializeField] private GameObject _prefabPowerUpGreen = null;
    [SerializeField] private GameObject _prefabPowerUpYellow = null;
    [SerializeField] private GameObject _prefabPowerUpPink = null;
    [SerializeField] private GameObject _prefabPowerUpBlue = null;

    [SerializeField] private GameObject _prefabCandy = null;

    [SerializeField] private GameObject _prefabFirework01 = null;
    [SerializeField] private GameObject _prefabFirework02 = null;
    [SerializeField] private GameObject _prefabFirework03 = null;
    [SerializeField] private GameObject _prefabFirework04 = null;

    private List<Pool> _poolConfigList;
    private Dictionary<string, List<GameObject>> _poolDictionary;
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
        _poolConfigList = GetPoolConfigList();
        _poolDictionary = GetPoolDictionary(_poolConfigList);
    }
    #endregion

    #region Utils
    private Pool CreatePool(string key, GameObject prefab, int size)
    {
        Pool pool = new Pool
        {
            Key = key,
            Prefab = prefab,
            Size = size
        };

        return pool;
    }

    private List<Pool> GetPoolConfigList()
    {
        List<Pool> poolConfigList = new List<Pool>
        {
            CreatePool(PLANET_KEY, _prefabPlanet, INITIAL_POOLSIZE_PLANET),
            CreatePool(TARGET_KEY, _prefabTarget, INITIAL_POOLSIZE_TARGET),
            CreatePool(NUMBER_KEY, _prefabNumber, INITIAL_POOLSIZE_NUMBER),

            CreatePool(SMALL_BLUE_KEY, _prefabSmallMoonBlue, INITIAL_POOLSIZE_SMALL_MOON),
            CreatePool(SMALL_GREEN_KEY, _prefabSmallMoonGreen, INITIAL_POOLSIZE_SMALL_MOON),
            CreatePool(SMALL_ORANGE_KEY, _prefabSmallMoonOrange, INITIAL_POOLSIZE_SMALL_MOON),
            CreatePool(SMALL_PINK_KEY, _prefabSmallMoonPink, INITIAL_POOLSIZE_SMALL_MOON),
            CreatePool(SMALL_YELLOW_KEY, _prefabSmallMoonYellow, INITIAL_POOLSIZE_SMALL_MOON),

            CreatePool(MEDIUM_BLUE_KEY, _prefabMediumMoonBlue, INITIAL_POOLSIZE_MEDIUM_MOON),
            CreatePool(MEDIUM_GREEN_KEY, _prefabMediumMoonGreen, INITIAL_POOLSIZE_MEDIUM_MOON),
            CreatePool(MEDIUM_ORANGE_KEY, _prefabMediumMoonOrange, INITIAL_POOLSIZE_MEDIUM_MOON),
            CreatePool(MEDIUM_PINK_KEY, _prefabMediumMoonPink, INITIAL_POOLSIZE_MEDIUM_MOON),
            CreatePool(MEDIUM_YELLOW_KEY, _prefabMediumMoonYellow, INITIAL_POOLSIZE_MEDIUM_MOON),

            CreatePool(BIG_BLUE_KEY, _prefabBigMoonBlue, INITIAL_POOLSIZE_BIG_MOON),
            CreatePool(BIG_GREEN_KEY, _prefabBigMoonGreen, INITIAL_POOLSIZE_BIG_MOON),
            CreatePool(BIG_ORANGE_KEY, _prefabBigMoonOrange, INITIAL_POOLSIZE_BIG_MOON),
            CreatePool(BIG_PINK_KEY, _prefabBigMoonPink, INITIAL_POOLSIZE_BIG_MOON),
            CreatePool(BIG_YELLOW_KEY, _prefabBigMoonYellow, INITIAL_POOLSIZE_BIG_MOON),

            CreatePool(POWER_UP_RED_KEY, _prefabPowerUpRed, INITIAL_POOLSIZE_COLLECTIBLE),
            CreatePool(POWER_UP_ORANGE_KEY, _prefabPowerUpOrange, INITIAL_POOLSIZE_COLLECTIBLE),
            CreatePool(POWER_UP_GREEN_KEY, _prefabPowerUpGreen, INITIAL_POOLSIZE_COLLECTIBLE),
            CreatePool(POWER_UP_YELLOW_KEY, _prefabPowerUpYellow, INITIAL_POOLSIZE_COLLECTIBLE),
            CreatePool(POWER_UP_PINK_KEY, _prefabPowerUpPink, INITIAL_POOLSIZE_COLLECTIBLE),
            CreatePool(POWER_UP_BLUE_KEY, _prefabPowerUpBlue, INITIAL_POOLSIZE_COLLECTIBLE),

            CreatePool(CANDY_KEY, _prefabCandy, INITIAL_POOLSIZE_COLLECTIBLE),

            CreatePool(FIREWORK_01_KEY, _prefabFirework01, INITIAL_POOLSIZE_FIREWORKS),
            CreatePool(FIREWORK_02_KEY, _prefabFirework02, INITIAL_POOLSIZE_FIREWORKS),
            CreatePool(FIREWORK_03_KEY, _prefabFirework03, INITIAL_POOLSIZE_FIREWORKS),
            CreatePool(FIREWORK_04_KEY, _prefabFirework04, INITIAL_POOLSIZE_FIREWORKS)
        };

        return poolConfigList;
    }

    private Dictionary<string, List<GameObject>> GetPoolDictionary(List<Pool> poolConfigList)
    {
        Dictionary<string, List<GameObject>> poolDictionary = new Dictionary<string, List<GameObject>>();

        foreach (Pool poolConfig in poolConfigList)
        {
            List<GameObject> poolList = new List<GameObject>();

            for (int i = 0; i < poolConfig.Size; i++)
            {
                AddNewGameObjectToPool(poolList, poolConfig.Prefab);

            }

            poolDictionary.Add(poolConfig.Key, poolList);
        }
        return poolDictionary;
    }

    private GameObject AddNewGameObjectToPool(List<GameObject> poolList, GameObject prefab)
    {
        GameObject obj = Instantiate(prefab, transform) as GameObject;
        obj.SetActive(false);
        poolList.Add(obj);
        return obj;
    }

    private GameObject GetPoolConfigPrefab(string key)
    {
        GameObject prefab = null;
        foreach (Pool poolConfig in _poolConfigList)
        {
            if (poolConfig.Key.Equals(key))
            {
                prefab = poolConfig.Prefab;
                break;
            }
        }
        return prefab;
    }
    #endregion

    #region API
    // Singleton MonoBehaviour: retrieve instance
    public static ObjectPooler GetInstance()
    {
        return _instance;
    }

    public GameObject SpawnFromPool(string key, Vector3 position, Quaternion rotation)
    {
        GameObject objectToSpawn = null;

        if (_poolDictionary.ContainsKey(key))
        {
            foreach (GameObject obj in _poolDictionary[key])
            {
                // Object isn't active and belongs to gameObject.transform list
                if (!obj.activeSelf && obj.transform.IsChildOf(transform))
                {
                    objectToSpawn = obj;
                    break;
                }
            }

            if (objectToSpawn == null)
            {
                objectToSpawn = AddNewGameObjectToPool(_poolDictionary[key], GetPoolConfigPrefab(key));
            }

            objectToSpawn.SetActive(true);
            objectToSpawn.transform.SetParent(null);
            objectToSpawn.transform.position = position;
            objectToSpawn.transform.rotation = rotation;
        }
        else
        {
            _log.DebugLogError(LOG_TAG, "Pool with key " + key + " doesn't exist.", gameObject);
        }
        return objectToSpawn;
    }

    public void ReturnToPool(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(transform);
    }
    #endregion
}
